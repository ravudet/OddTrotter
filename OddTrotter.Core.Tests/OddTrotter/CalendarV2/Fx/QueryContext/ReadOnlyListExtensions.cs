namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;

    using Fx.Either;

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

                this.isInitialized = true;
            }

            public IQueryResult<TValue, TError> WithError<TError>(TError error)
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException(
                        $"This instance of '{typeof(ToQueryResultBuilder<TValue>).FullName}' was initialized as a default instance and is in an invalid state.");
                }

                return ToQueryResult(this.list, new RealNullable<TError>(error));
            }

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

        private static IQueryResult<TValue, TError> ToQueryResult<TValue, TError>(
            IReadOnlyList<TValue> list,
            RealNullable<TError> error)
        {
            return new ToQueryResultQueryResult<TValue, TError>(ToQueryResultNode(list, 0, error));
        }

        private sealed class ToQueryResultQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            public ToQueryResultQueryResult(IQueryResultNode<TValue, TError> nodes)
            {
                Nodes = nodes;
            }

            public IQueryResultNode<TValue, TError> Nodes { get; }
        }

        private static IQueryResultNode<TValue, TError> ToQueryResultNode<TValue, TError>(IReadOnlyList<TValue> list, int index, RealNullable<TError> possibleError)
        {
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
                this.list = list;
                this.index = index;
                this.possibleError = possibleError;
            }

            public TValue Value
            {
                get
                {
                    return this.list[this.index];
                }
            }

            public IQueryResultNode<TValue, TError> Next()
            {
                return ToQueryResultNode<TValue, TError>(this.list, this.index + 1, this.possibleError);
            }
        }
    }
}
