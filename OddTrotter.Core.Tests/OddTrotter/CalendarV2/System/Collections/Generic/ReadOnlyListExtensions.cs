/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections.Generic
{
    using Fx;
    using Fx.Either;
    using Fx.QueryContext;

    internal static class ReadOnlyListExtensions
    {
        public static ToQueryResultBuilder<TValue> ToQueryResult<TValue>(this IReadOnlyList<TValue> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultBuilder<TValue>(list);
        }

        public readonly ref struct ToQueryResultBuilder<TValue>
        {
            private readonly IReadOnlyList<TValue> list;

            private readonly bool isInitialized;

            public ToQueryResultBuilder()
            {
                throw new InvalidOperationException(
                    $"Initializing a default instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' results in an invalid state.");
            }

            public ToQueryResultBuilder(IReadOnlyList<TValue> list)
            {
                ArgumentNullException.ThrowIfNull(list);

                this.list = list;

                isInitialized = true;
            }

            public IQueryResult<TValue, TError> WithError<TError>(TError error)
            {
                if (!isInitialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.");
                }

                return ToQueryResult(list, new RealNullable<TError>(error));
            }

            public IQueryResult<TValue, TError> WithoutError<TError>()
            {
                if (!isInitialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.");
                }

                return ToQueryResult(list, new RealNullable<TError>());
            }
        }

        private static IQueryResult<TValue, TError> ToQueryResult<TValue, TError>(
            IReadOnlyList<TValue> list,
            RealNullable<TError> error)
        {
            ArgumentNullException.ThrowIfNull(list);

            return new ToQueryResultQueryResult<TValue, TError>(ToQueryResultNode(list, 0, error));
        }

        private sealed class ToQueryResultQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            public ToQueryResultQueryResult(IQueryResultNode<TValue, TError> nodes)
            {
                ArgumentNullException.ThrowIfNull(nodes);

                Nodes = nodes;
            }

            public IQueryResultNode<TValue, TError> Nodes { get; }
        }

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

            public TValue Value
            {
                get
                {
                    return list[index];
                }
            }

            public IQueryResultNode<TValue, TError> Next()
            {
                return ToQueryResultNode(list, index + 1, possibleError);
            }
        }
    }
}
