/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    using Fx.Realizable;

    public sealed class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private readonly BetterNullable<TLeft> left;
        private readonly BetterNullable<TRight> right;

        public Either(TLeft left)
        {
            this.left = new BetterNullable<TLeft>(left);

            right = default;
        }

        public Either(TRight right)
        {
            this.right = new BetterNullable<TRight>(right);

            left = default;
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, ref TContext context)
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
    }
}
