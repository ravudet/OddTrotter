/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections.Generic
{
    using System.Threading.Tasks;

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
        public static ToQueryResultAsyncBuilder<TValue> ToQueryResultAsync<TValue>(this IReadOnlyList<TValue> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultAsyncBuilder<TValue>(list);
        }

        public readonly ref struct ToQueryResultAsyncBuilder<TValue>
        {
            private readonly IReadOnlyList<TValue> list;

            private readonly bool isInitialized;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Always thrown; a default instance of <see cref="ToQueryResultBuilder{TValue}"/> is invalid
            /// </exception>
            public ToQueryResultAsyncBuilder()
            {
                var message = $"Initializing a default instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' results in an invalid state.";
                throw new InvalidOperationException(message);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="list"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
            public ToQueryResultAsyncBuilder(IReadOnlyList<TValue> list)
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
            public IQueryResultAsync<TValue, TError> WithError<TError>(TError error)
            {
                if (!this.isInitialized)
                {
                    var message = $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.";
                    throw new InvalidOperationException(message);
                }

                return ToQueryResultAsync(this.list, new Optional<TError>(error));
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <typeparam name="TError"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown if this instance of <see cref="ToQueryResultBuilder{TValue}"/> is a default instance
            /// </exception>
            public IQueryResultAsync<TValue, TError> WithoutError<TError>()
            {
                if (!this.isInitialized)
                {
                    var message = $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.";
                    throw new InvalidOperationException(message);
                }

                return ToQueryResultAsync(this.list, new Optional<TError>());
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
        private static IQueryResultAsync<TValue, TError> ToQueryResultAsync<TValue, TError>(
            IReadOnlyList<TValue> list,
            Optional<TError> error)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultQueryResultAsync<TValue, TError>(ToQueryResultNodeAsync(list, 0, error));
        }

        private sealed class ToQueryResultQueryResultAsync<TValue, TError> : IQueryResultAsync<TValue, TError>
        {
            private readonly IQueryResultNodeAsync<TValue, TError> nodes;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="nodes"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <see langword="null"/></exception>
            public ToQueryResultQueryResultAsync(IQueryResultNodeAsync<TValue, TError> nodes)
            {
                ArgumentNullException.ThrowIfNull(nodes);

                this.nodes = nodes;
            }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValue, TError>> GetNodes()
            {
                return await Task
                    .FromResult(this.nodes)
                    .ConfigureAwait(false);
            }
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is negative or greater than the <see cref="IReadOnlyCollection{T}.Count"/> of
        /// <paramref name="list"/>
        /// </exception>
        private static IQueryResultNodeAsync<TValue, TError> ToQueryResultNodeAsync<TValue, TError>(
            IReadOnlyList<TValue> list,
            int index,
            Optional<TError> possibleError)
        {
            ArgumentNullException.ThrowIfNull(list);
            //// TODO new version of .NET have more factories for argumentoutofrange
            if (index < 0)
            {
                var message = $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.";
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }

            if (index > list.Count)
            {
                var message = $"'{nameof(index)}' must not be greater than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.";
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }

            if (index < list.Count)
            {
                return
                    Either
                        .Left(
                            new ToQueryResultNodeElementAsync<TValue, TError>(list, index, possibleError))
                        .Right<IEither<IError<TError>, IEmpty>>()
                        .ToQueryResultNodeAsync();
            }
            else
            {
                if (possibleError.TryGetValue(out var error))
                {
                    return
                        Either
                            .Left<IElementAsync<TValue, TError>>()
                            .Right(
                                Either
                                    .Left(new Error<TError>(error))
                                    .Right<IEmpty>())
                            .ToQueryResultNodeAsync();
                }
                else
                {
                    return
                        Either
                            .Left<IElementAsync<TValue, TError>>()
                            .Right(
                                Either
                                    .Left<IError<TError>>()
                                    .Right(MockEmpty.Instance))
                            .ToQueryResultNodeAsync();
                }
            }
        }

        private sealed class ToQueryResultNodeElementAsync<TValue, TError> : IElementAsync<TValue, TError>
        {
            private readonly IReadOnlyList<TValue> list;

            private readonly int index;

            private readonly Optional<TError> possibleError;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="list"></param>
            /// <param name="index"></param>
            /// <param name="possibleError"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Thrown if <paramref name="index"/> is negative or greater than or equal to the
            /// <see cref="IReadOnlyCollection{T}.Count"/> of <paramref name="list"/>
            /// </exception>
            public ToQueryResultNodeElementAsync(IReadOnlyList<TValue> list, int index, Optional<TError> possibleError)
            {
                ArgumentNullException.ThrowIfNull(list);
                //// TODO new version of .NET have more factories for argumentoutofrange
                if (index < 0)
                {
                    var message = $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.";
                    throw new ArgumentOutOfRangeException(nameof(index), message);
                }

                if (index >= list.Count)
                {
                    var message = $"'{nameof(index)}' must be less than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.";
                    throw new ArgumentOutOfRangeException(nameof(index), message);
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
            public async ITask<IQueryResultNodeAsync<TValue, TError>> Next()
            {
                return await Task
                    .FromResult(
                        ToQueryResultNodeAsync(this.list, this.index + 1, this.possibleError))
                    .ConfigureAwait(false);
            }
        }

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
                var message = $"Initializing a default instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' results in an invalid state.";
                throw new InvalidOperationException(message);
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
                    var message = $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.";
                    throw new InvalidOperationException(message);
                }

                return ToQueryResult(this.list, new Optional<TError>(error));
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
                    var message = $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.";
                    throw new InvalidOperationException(message);
                }

                return ToQueryResult(this.list, new Optional<TError>());
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
            Optional<TError> error)
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is negative or greater than the <see cref="IReadOnlyCollection{T}.Count"/> of
        /// <paramref name="list"/>
        /// </exception>
        private static IQueryResultNode<TValue, TError> ToQueryResultNode<TValue, TError>(
            IReadOnlyList<TValue> list, 
            int index,
            Optional<TError> possibleError)
        {
            ArgumentNullException.ThrowIfNull(list);
            //// TODO new version of .NET have more factories for argumentoutofrange
            if (index < 0)
            {
                var message = $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.";
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }

            if (index > list.Count)
            {
                var message = $"'{nameof(index)}' must not be greater than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.";
                throw new ArgumentOutOfRangeException(nameof(index), message);
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

            private readonly Optional<TError> possibleError;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="list"></param>
            /// <param name="index"></param>
            /// <param name="possibleError"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/></exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Thrown if <paramref name="index"/> is negative or greater than or equal to the
            /// <see cref="IReadOnlyCollection{T}.Count"/> of <paramref name="list"/>
            /// </exception>
            public ToQueryResultNodeElement(IReadOnlyList<TValue> list, int index, Optional<TError> possibleError)
            {
                ArgumentNullException.ThrowIfNull(list);
                //// TODO new version of .NET have more factories for argumentoutofrange
                if (index < 0)
                {
                    var message = $"'{nameof(index)}' cannot be a negative value. The provided value was '{index}'.";
                    throw new ArgumentOutOfRangeException(nameof(index), message);
                }

                if (index >= list.Count)
                {
                    var message = $"'{nameof(index)}' must be less than the length of '{nameof(list)}'. The provided value for '{nameof(index)}' was '{index}'. The length of '{nameof(list)}' was '{list.Count}'.";
                    throw new ArgumentOutOfRangeException(nameof(index), message);
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
