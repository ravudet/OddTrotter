/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

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
    }
}
