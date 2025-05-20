namespace Fx.QueryContext
{
    using Fx.Either;

    public static partial class QueryResultNodeExtensions
    {
        public static QueryResultNodeAsync<TValue, TError> ToQueryResultNodeAsync<TValue, TError>(
            this IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            return new QueryResultNodeAsync<TValue, TError>(source);
        }
    }
}
