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

        public Realizable(Realizable<Realizable<T>> realizable)
        {
            realizable.ContinueWith<Realizable<T>>(
                inner =>
                {
                    var thing = inner.ContinueWith(
                       source => source,
                       _ => throw _,
                       _ => throw _);
                    if (thing.AsEither.Decompose(out var value, out var future))
                    {

                    }

                    return thing;
                }, 
                _ => throw _, 
                _ => throw _);
        }

        public TypeHolder<Realizable<T>, T> AsContinuable()
        {
            return new TypeHolder<Realizable<T>, T>(this);
        }

        public TypeHolder<Realizable<T>, T, ITask<T>> AsEither
        {
            get
            {
                return new TypeHolder<Realizable<T>, T, ITask<T>>(this);
            }
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
        {
            if (either.AsEither.Decompose(out var value, out var future))
            {
                TResult result;
                try
                {
                    result = sourceContinuation(value);
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
                    sourceContinuation,
                    exceptionContinuation,
                    canceledContinuation);
            }
        }

        public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<T, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<ITask<T>, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return either.ApplyAsync(leftMap, rightMap, ref context);
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct
        {
            if (DecomposeMixin.TryCreate<TCasted, Realizable<T>, T, ITask<T>>(
                this,
                out casted))
            {
                return true;
            }

            casted = default;
            return false;
        }

        bool IDecomposeMixin<Realizable<T>, T, ITask<T>>.Decompose([MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right)
        {
            return either.Decompose(out left, out right);
        }
    }
}
