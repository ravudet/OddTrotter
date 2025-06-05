/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    using Fx.Try;

    public static class Either
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <returns></returns>
        public static Empty<TLeft> Left<TLeft>()
        {
            return new Empty<TLeft>();
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Full<TLeft> Left<TLeft>(TLeft value)
        {
            return new Full<TLeft>(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <remarks>
        /// This class was named so that it does not conflict with intellisense's ability to find the <see cref="Left"/> method
        /// overloads
        /// </remarks>
        public readonly ref struct Empty<TLeft>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <typeparam name="TRight"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public Either<TLeft, TRight> Right<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <remarks>
        /// This class was named so that it does not conflict with intellisense's ability to find the <see cref="Left"/> method
        /// overloads
        /// </remarks>
        public readonly ref struct Full<TLeft>
        {
            private readonly TLeft value;

            private readonly bool initialized;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Always thrown; a default instance of <see cref="Full{TLeft}"/> is invalid
            /// </exception>
            public Full()
            {
                throw new InvalidOperationException(
                    $"Initializing a default instance of '{typeof(Full<TLeft>).Namespace}.{typeof(Full<TLeft>).Name}' results in an invalid state.");
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            public Full(TLeft value)
            {
                this.value = value;
                this.initialized = true;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <typeparam name="TRight"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown if this instance of <see cref="Full{TLeft}"/> is a default instance
            /// </exception>
            public Either<TLeft, TRight> Right<TRight>()
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(Full<TLeft>).Namespace}.{typeof(Full<TLeft>).Name}' was initialized as a default instance and is in an invalid state.");
                }

                return new Either<TLeft, TRight>.Left(this.value);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="value"></param>
        /// <param name="discriminator">assumed to not throw exceptions</param>
        /// <param name="leftFactory">assumed to not throw exceptions</param>
        /// <param name="rightFactory">assumed to not throw exceptions</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="discriminator"/> or <paramref name="leftFactory"/> or <paramref name="rightFactory"/> is
        /// <see langword="null"/>
        /// </exception>
        public static IEither<TLeft, TRight> Create<TValue, TLeft, TRight>(
            TValue value, 
            Func<TValue, bool> discriminator, 
            Func<TValue, TLeft> leftFactory, 
            Func<TValue, TRight> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value))
            {
                return Either.Left(leftFactory(value)).Right<TRight>();
            }
            else
            {
                return Either.Left<TLeft>().Right(rightFactory(value));
            }
        }

        public interface IMaybe<out TValue> : IEither<TValue, Nothing>
        {
        }

        public static Attempt<TInput, TOutput> ToAttempt<TInput, TOutput>(this Try<TInput, TOutput> @try)
        {
            return input => @try(input, out var output) ? Maybe.Value(output) : Maybe.Nothing<TOutput>();
        }

        public delegate IMaybe<TOutput> Attempt<in TInput, out TOutput>(TInput input);

        private static class Maybe
        {
            public static Maybe<T> Value<T>(T value)
            {
                return new Maybe<T>(Either.Left(value).Right<Nothing>());
            }

            public static Maybe<T> Nothing<T>()
            {
                //// TODO this could be a singleton //// TODO you could have static analysis automation that recognizes singletons and propagates them
                return new Maybe<T>(Either.Left<T>().Right(new Nothing()));
            }
        }

        private sealed class Maybe<T> : IMaybe<T>
        {
            private readonly IEither<T, Nothing> either;

            public Maybe(IEither<T, Nothing> either)
            {
                this.either = either;
            }

            public TResult Apply<TResult, TContext>(Func<T, TContext, TResult> leftMap, Func<Nothing, TContext, TResult> rightMap, TContext context)
            {
                throw new NotImplementedException();
            }

            public System.Threading.Tasks.Task<TResult> Apply<TResult, TContext>(Func<T, TContext, System.Threading.Tasks.Task<TResult>> leftMap, Func<Nothing, TContext, System.Threading.Tasks.Task<TResult>> rightMap, TContext context)
            {
                throw new NotImplementedException();
            }
        }

        public static IMaybe<TOutput> ToMaybe<TInput, TOutput>(this TInput input, Attempt<TInput, TOutput> attempt)
        {
            return attempt(input);
        }

        public static IMaybe<TOutput> ToMaybe<TInput, TOutput>(this TInput input, Try<TInput, TOutput> @try)
        {
            if (@try(input, out var output))
            {
                return new Maybe<TOutput>(Either.Left(output).Right<Nothing>());
            }
            else
            {
                return new Maybe<TOutput>(Either.Left<TOutput>().Right(new Nothing()));
            }
        }

        public static IEither<TLeft, Nothing> ToEither<TLeft>(this TLeft value, Func<TLeft, bool> discriminator)
        {
            if (discriminator(value))
            {
                return Either.Left(value).Right<Nothing>();
            }
            else
            {
                return Either.Left<TLeft>().Right(new Nothing());
            }
        }

        public static IEither<TLeft, TResult> ToEither<TLeft, TResult>(this TLeft value, Try<TLeft, TResult> @try)
        {
            if (@try(value, out var result))
            {
                return Either.Left<TLeft>().Right(result);
            }
            else
            {
                return Either.Left(value).Right<TResult>();
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="value"></param>
        /// <param name="discriminator"></param>
        /// <param name="leftFactory"></param>
        /// <param name="rightFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="discriminator"/> or <paramref name="leftFactory"/> or <paramref name="rightFactory"/> is
        /// <see langword="null"/>
        /// </exception>
        public static IEither<TLeft, TRight> TryCreate<TValue, TResult, TLeft, TRight>(
            TValue value,
            Try<TValue, TResult> discriminator,
            Func<TValue, TResult, TLeft> leftFactory,
            Func<TValue, TRight> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value, out var result))
            {
                return Either.Left(leftFactory(value, result)).Right<TRight>();
            }
            else
            {
                return Either.Left<TLeft>().Right(rightFactory(value));
            }
        }
    }
}
