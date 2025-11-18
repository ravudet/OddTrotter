/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx;
    using Fx.Realizable;

    public readonly ref struct RefEither<TLeft, TRight> : IEither<RefEither<TLeft, TRight>, TLeft, TRight>, ICastable, IDecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        private readonly RefNullable<TLeft> left;

        private readonly RefNullable<TRight> right;

        public RefEither(TLeft left)
        {
            this.left = new RefNullable<TLeft>(left);

            right = new RefNullable<TRight>();
        }

        public TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight> TypeHolder
        {
            get
            {
                return new TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight>(this);
            }
        }

        public RefEither(TRight right)
        {
            this.right = new RefNullable<TRight>(right);

            left = new RefNullable<TLeft>();
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, 
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            if (this.left.TryGetValue(out var left))
            {
                return
                    leftMap(left, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new LeftMapException(exception),
                        canceled => throw canceled);
            }
            else if (this.right.TryGetValue(out var right))
            {
                return
                    rightMap(right, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new RightMapException(exception),
                        canceled => throw canceled);
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        public bool Decompose([MaybeNullWhen(false)] out TLeft value, [MaybeNullWhen(true)] out TRight future)
        {
            if (left.TryGetValue(out value))
            {
                future = default;
                return true;
            }
            else if (right.TryGetValue(out future))
            {
                value = default;
                return false;
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted)
            where TCasted : struct, allows ref struct
        {
            if (typeof(TCasted) == typeof(DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>))
            {
                var mixin = new DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>(
                    this,
                    (RefEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => either.Decompose(out left, out right));
                casted = Unsafe.As<DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>, TCasted>(ref mixin);
                return true;
            }

            casted = default;
            return false;
        }
    }













    //// TODO then split this into files
    //// TODO go through files and remove the comment block
    //// TODO implement a test with mapping exceptions being throw
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
}
