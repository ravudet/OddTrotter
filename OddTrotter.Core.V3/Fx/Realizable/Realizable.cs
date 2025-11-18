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
            if (either.TypeHolder.Decompose(out var value, out var future))
            {
                return new Realizable<TResult>(source(value));
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
            //// TODO can you put this code into a single place? you've duplicated it a few times
            if (typeof(TCasted) == typeof(DecomposeMixin<Realizable<T>, T, ITask<T>>))
            {
                var mixin = new DecomposeMixin<Realizable<T>, T, ITask<T>>(
                    this,
                    (Realizable<T> either, [MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right) => either.Decompose(out left, out right));
                casted = Unsafe.As<DecomposeMixin<Realizable<T>, T, ITask<T>>, TCasted>(ref mixin);
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
    }













    //// TODO then split this into files
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
}
