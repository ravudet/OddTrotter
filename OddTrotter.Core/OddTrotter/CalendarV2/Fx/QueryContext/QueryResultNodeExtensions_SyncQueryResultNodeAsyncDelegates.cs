namespace Fx.QueryContext
{
    using System;

    using Fx.Either;

    public static partial class QueryResultNodeExtensions
    {
        public static QueryResultNodeAsync<TValue, TError> ToQueryResultNode<TValue, TError>(
            this IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> node)
        {
            ArgumentNullException.ThrowIfNull(node);

            return new QueryResultNodeAsync<TValue, TError>(node);
        }
    }
}
