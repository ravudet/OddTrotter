namespace Fx.QueryContext
{
    using System.Collections.Generic;

    using Fx.Either;

    internal static class ReadOnlyListExtensions
    {
        public readonly ref struct ToQueryResultBuilder<TValue>
        {
            private readonly IReadOnlyList<TValue> list;

            public ToQueryResultBuilder(IReadOnlyList<TValue> list)
            {
                this.list = list;
            }

            public IQueryResult<TValue, TError> WithError<TError>(TError error)
            {
                return ToQueryResult<TValue, TError>(this.list, new RealNullable<TError>(error));
            }

            public IQueryResult<TValue, TError> WithoutError<TError>()
            {
                return ToQueryResult<TValue, TError>(this.list, new RealNullable<TError>());
            }
        }

        public static ToQueryResultBuilder<TValue> ToQueryResult<TValue>(this IReadOnlyList<TValue> list)
        {
            return new ToQueryResultBuilder<TValue>(list);
        }

        private static IQueryResult<TValue, TError> ToQueryResult<TValue, TError>(
            IReadOnlyList<TValue> list,
            RealNullable<TError> error)
        {
            //// TODO make this "production"?
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
