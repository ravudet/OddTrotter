/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        private Either()
        {
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="left"></param>
        [Obsolete("If you are seeing this message, it means that you are using an implicit conversion from something to an `Either`. That demonstrates that there is value in having the implicit conversion.")]
        public static implicit operator Either<TLeft, TRight>(TLeft left)
        {
            return new Either<TLeft, TRight>.Left(left);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="right"></param>
        [Obsolete("If you are seeing this message, it means that you are using an implicit conversion from something to an `Either`. That demonstrates that there is value in having the implicit conversion.")]
        public static implicit operator Either<TLeft, TRight>(TRight right)
        {
            return new Either<TLeft, TRight>.Right(right);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/></exception>
        /// <exception cref="LeftMapException">
        /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Left"/> node
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Right"/> node
        /// </exception>
        protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

        public abstract class Visitor<TResult, TContext>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="LeftMapException">
            /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Left"/> node
            /// </exception>
            /// <exception cref="RightMapException">
            /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Right"/> node
            /// </exception>
            public TResult Visit(Either<TLeft, TRight> node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return node.Dispatch(this, context);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="LeftMapException">
            /// Thrown if an error occurred while processing <paramref name="node"/>
            /// </exception>
            protected internal abstract TResult Accept(Either<TLeft, TRight>.Left node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="RightMapException">
            /// Thrown if an error occurred while processing <paramref name="node"/>
            /// </exception>
            protected internal abstract TResult Accept(Either<TLeft, TRight>.Right node, TContext context);
        }

        public sealed class Left : Either<TLeft, TRight>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            public Left(TLeft value)
            {
                this.Value = value;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            public TLeft Value { get; }

            /// <inheritdoc/>
            protected sealed override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
            {
                ArgumentNullException.ThrowIfNull(visitor);

                return visitor.Accept(this, context);
            }
        }

        public sealed class Right : Either<TLeft, TRight>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            public Right(TRight value)
            {
                this.Value = value;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            public TRight Value { get; }

            /// <inheritdoc/>
            protected sealed override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
            {
                ArgumentNullException.ThrowIfNull(visitor);

                return visitor.Accept(this, context);
            }
        }

        /// <inheritdoc/>
        public TResult Apply<TResult, TContext>(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return new DelegateVisitor<TResult, TContext>(leftMap, rightMap).Visit(this, context);
        }

        private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
        {
            private readonly Func<TLeft, TContext, TResult> leftAccept;
            private readonly Func<TRight, TContext, TResult> rightAccept;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="leftAccept"></param>
            /// <param name="rightAccept"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="leftAccept"/> or <paramref name="rightAccept"/> is <see langword="null"/>
            /// </exception>
            public DelegateVisitor(
                Func<TLeft, TContext, TResult> leftAccept, 
                Func<TRight, TContext, TResult> rightAccept)
            {
                ArgumentNullException.ThrowIfNull(leftAccept);
                ArgumentNullException.ThrowIfNull(rightAccept);

                this.leftAccept = leftAccept;
                this.rightAccept = rightAccept;
            }

            /// <inheritdoc/>
            protected internal sealed override TResult Accept(Left node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                try
                {
                    return this.leftAccept(node.Value, context);
                }
                catch (Exception exception)
                {
                    throw new LeftMapException(
                        $"An error occurred while process the left value of an {nameof(Either<TLeft, TRight>)}.",
                        exception);
                }
            }

            /// <inheritdoc/>
            protected internal sealed override TResult Accept(Right node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                try
                {
                    return this.rightAccept(node.Value, context);
                }
                catch (Exception exception)
                {
                    throw new RightMapException(
                        $"An error occurred while process the right value of an {nameof(Either<TLeft, TRight>)}.",
                        exception);
                }
            }
        }

    }
}
