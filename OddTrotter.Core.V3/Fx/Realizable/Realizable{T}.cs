/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx;
    using Fx.Either;
    using Fx.Either.Mixins;

    //// TODO better name
    public readonly ref struct Realizable<T> : IContinuable<T>, IEither<Realizable<T>, T, ITask<T>>, ICastable, IDecomposeMixin<Realizable<T>, T, ITask<T>>
        where T : allows ref struct
    {
        private readonly RefEither<T, ITask<T>> either;

        public Realizable(T value)
        {
            either = new RefEither<T, ITask<T>>(value);
        }

        public Realizable(ITask<T> future)
        {
            //// TODO do you really want `future` to be `itask` specifically, or should this be a generic on `realizable`?
            //// TODO you're asking this because `realizable` only needs `future` to be `icontinuable`, but `icontinuable` is pretty useless for the caller since it has no way to actually get the value; so, a generic would let us receive what we require (icontinuable) without the caller needing to implement `getawaiter` if they have some other way to get the value
            either = new RefEither<T, ITask<T>>(future);
        }

        public TypeHolder<Realizable<T>, T, ITask<T>> TypeHolder
        {
            get
            {
                return new TypeHolder<Realizable<T>, T, ITask<T>>(this);
            }
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> source, Func<Exception, TResult> exception, Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
        {
            //// TODO rename the parameters so they are actually descriptive; too many naming conflicts with local variables
            
            if (either.TypeHolder.Decompose(out var value, out var future))
            {
                TResult result;
                try
                {
                    result = source(value);
                }
                catch (Exception sourceException)
                {
                    return Realizable.FromException<TResult>(sourceException);
                }

                return new Realizable<TResult>(result);
            }
            else
            {
                return future.ContinueWith(
                    source,
                    exception,
                    canceled);
            }
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<T, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<ITask<T>, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return either.Apply(leftMap, rightMap, ref context);
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct
        {
            //// TODO you are here
            //// TODO can you put this code into a single place? you've duplicated it a few times
            
            if (TryDecompose(
                this,
                (Realizable<T> either, [MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right) => either.Decompose(out left, out right),
                out casted))
            {
                return true;
            }

            casted = default;
            return false;
        }

        public bool Decompose([MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right)
        {
            //// TODO can you make this an implicit interface implementation?
            return either.Decompose(out left, out right);
        }

        public static bool TryDecompose<TCasted, TEither, TLeft, TRight>(TEither either, DecomposeDelegate<TEither, TLeft, TRight> decomposeDelegate, out TCasted casted)
            where TCasted : struct, allows ref struct
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            if (typeof(TCasted) == typeof(DecomposeMixin<TEither, TLeft, TRight>))
            {
                var mixin = new DecomposeMixin<TEither, TLeft, TRight>(
                    either,
                    decomposeDelegate);
                casted = Unsafe.As<DecomposeMixin<TEither, TLeft, TRight>, TCasted>(ref mixin);
                return true;
            }

            casted = default;
            return false;
        }
    }
}
