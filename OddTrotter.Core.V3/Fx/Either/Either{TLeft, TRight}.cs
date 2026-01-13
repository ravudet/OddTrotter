/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Fx.Either.Mixins;
    using Fx.Realizable;

    public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private Either()
        {
        }

        protected abstract Realizable<TResult> Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;

        public abstract class Visitor<TResult, TContext>
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            public Realizable<TResult> Visit(Either<TLeft, TRight> node, ref TContext context)
            {
                return node.Dispatch(this, ref context);
            }

            protected internal abstract Realizable<TResult> Accept(Either<TLeft, TRight>.Left node, ref TContext context);

            protected internal abstract Realizable<TResult> Accept(Either<TLeft, TRight>.Right node, ref TContext context);
        }

        public sealed class Left : Either<TLeft, TRight>, IDecomposeMixin<Either<TLeft, TRight>.Left, TLeft, TRight>
        {
            public Left(TLeft value)
            {
                Value = value;
            }

            public TLeft Value { get; }

            public bool Decompose([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
            {
                left = this.Value;
                right = default;
                return true;
            }

            protected override Realizable<TResult> Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, ref TContext context)
            {
                return visitor.Accept(this, ref context);
            }
        }

        public sealed class Right : Either<TLeft, TRight>, IDecomposeMixin<Either<TLeft, TRight>.Right, TLeft, TRight>
        {
            public Right(TRight value)
            {
                Value = value;
            }

            public TRight Value { get; }

            public bool Decompose([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
            {
                left = default;
                right = this.Value;
                return false;
            }

            protected override Realizable<TResult> Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, ref TContext context)
            {
                return visitor.Accept(this, ref context);
            }
        }

        public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return new DelegateVisitor<TResult, TContext, TContinuable>(leftMap, rightMap).Visit(this, ref context);
        }

        private sealed class DelegateVisitor<TResult, TContext, TContinuable> : Visitor<TResult, TContext>
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            private readonly AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap;
            private readonly AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap;

            public DelegateVisitor(
                AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap,
                AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap)
            {
                this.leftMap = leftMap;
                this.rightMap = rightMap;
            }

            protected internal override Realizable<TResult> Accept(Left node, ref TContext context)
            {
                return
                    this.leftMap(node.Value, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new LeftMapException(exception),
                        canceled => throw canceled);
            }

            protected internal override Realizable<TResult> Accept(Right node, ref TContext context)
            {
                return
                    this.rightMap(node.Value, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new RightMapException(exception),
                        canceled => throw canceled);
            }
        }
    }
}
