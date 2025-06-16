namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    public static partial class QueryResultNodeAsyncExtensions
    {
        public static async ITask<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this ITask<IQueryResultNodeAsync<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            return await (await source.ConfigureAwait(false)).Select(selector).ConfigureAwait(false);
        }
    }
}
