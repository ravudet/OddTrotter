/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections.Generic
{
    using Fx;
    using Fx.Either;
    using Fx.QueryContext;

    internal static class ReadOnlyListExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
        public static ToQueryResultBuilder<TValue> ToQueryResult<TValue>(this IReadOnlyList<TValue> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultBuilder<TValue>(list);
        }

        public readonly ref struct ToQueryResultBuilder<TValue>
        {
            private readonly IReadOnlyList<TValue> list;

            private readonly bool isInitialized;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Always thrown; a default instance of <see cref="ToQueryResultBuilder{TValue}"/> is invalid
            /// </exception>
            public ToQueryResultBuilder()
            {
                throw new InvalidOperationException(
                    $"Initializing a default instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' results in an invalid state.");
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="list"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
            public ToQueryResultBuilder(IReadOnlyList<TValue> list)
            {
                ArgumentNullException.ThrowIfNull(list);

                this.list = list;

                this.isInitialized = true;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <typeparam name="TError"></typeparam>
            /// <param name="error"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown if this instance of <see cref="ToQueryResultBuilder{TValue}"/> is a default instance
            /// </exception>
            public IQueryResult<TValue, TError> WithError<TError>(TError error)
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.");
                }

                return ToQueryResult(this.list, new RealNullable<TError>(error));
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <typeparam name="TError"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown if this instance of <see cref="ToQueryResultBuilder{TValue}"/> is a default instance
            /// </exception>
            public IQueryResult<TValue, TError> WithoutError<TError>()
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.");
                }

                return ToQueryResult(this.list, new RealNullable<TError>());
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="list"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
        private static IQueryResult<TValue, TError> ToQueryResult<TValue, TError>(
            IReadOnlyList<TValue> list,
            RealNullable<TError> error)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultQueryResult<TValue, TError>(ToQueryResultNode(list, 0, error));
        }

        private sealed class ToQueryResultQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="nodes"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <see langword="null"/></exception>
            public ToQueryResultQueryResult(IQueryResultNode<TValue, TError> nodes)
            {
                ArgumentNullException.ThrowIfNull(nodes);

                this.Nodes = nodes;
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Nodes { get; }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="possibleError"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or greater than the <see cref="IReadOnlyCollection{T}.Count"/> of <paramref name="list"/></exception>
        private static IQueryResultNode<TValue, TError> ToQueryResultNode<TValue, TError>(IReadOnlyList<TValue> list, int index, RealNullable<TError> possibleError)
        {
            ArgumentNullException.ThrowIfNull(list);
            //// TODO new version of .NET have more factories for argumentoutofrange
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.");
            }

            if (index > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"'{nameof(index)}' must not be greater than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.");
            }

            if (index < list.Count)
            {
                return
                    Either
                        .Left(
                            new ToQueryResultNodeElement<TValue, TError>(list, index, possibleError))
                        .Right<IEither<IError<TError>, IEmpty>>()
                        .ToQueryResultNode();
            }
            else
            {
                if (possibleError.TryGetValue(out var error))
                {
                    return
                        Either
                            .Left<IElement<TValue, TError>>()
                            .Right(
                                Either
                                    .Left(new Error<TError>(error))
                                    .Right<IEmpty>())
                            .ToQueryResultNode();
                }
                else
                {
                    return
                        Either
                            .Left<IElement<TValue, TError>>()
                            .Right(
                                Either
                                    .Left<IError<TError>>()
                                    .Right(MockEmpty.Instance))
                            .ToQueryResultNode();
                }
            }
        }

        private sealed class ToQueryResultNodeElement<TValue, TError> : IElement<TValue, TError>
        {
            private readonly IReadOnlyList<TValue> list;

            private readonly int index;

            private readonly RealNullable<TError> possibleError;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="list"></param>
            /// <param name="index"></param>
            /// <param name="possibleError"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or greater than or equal to the <see cref="IReadOnlyCollection{T}.Count"/> of <paramref name="list"/></exception>
            public ToQueryResultNodeElement(IReadOnlyList<TValue> list, int index, RealNullable<TError> possibleError)
            {
                ArgumentNullException.ThrowIfNull(list);
                //// TODO new version of .NET have more factories for argumentoutofrange
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.");
                }

                if (index >= list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"'{nameof(index)}' must be less than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.");
                }

                this.list = list;
                this.index = index;
                this.possibleError = possibleError;
            }

            /// <inheritdoc/>
            public TValue Value
            {
                get
                {
                    return this.list[index];
                }
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Next()
            {
                return ToQueryResultNode(this.list, this.index + 1, this.possibleError);
            }
        }
    }
}
