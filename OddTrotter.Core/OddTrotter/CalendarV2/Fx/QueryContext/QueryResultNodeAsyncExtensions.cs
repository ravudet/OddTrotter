namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public static partial class QueryResultNodeAsyncExtensions
    {
        public static async ITask<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNodeAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            return await source
                .SelectLeft(
                    async element =>
                        new SelectElementAsync<TValueSource, TError, TValueResult>(
                            selector(element.Value),
                            await element.Next().ConfigureAwait(false),
                            selector))
                .ToQueryResultNodeAsync()
                .ConfigureAwait(false);
        }

        private sealed class SelectElementAsync<TValueSource, TError, TValueResult> : IElementAsync<TValueResult, TError>
        {
            private readonly IQueryResultNodeAsync<TValueSource, TError> next;
            private readonly Func<TValueSource, TValueResult> selector;

            public SelectElementAsync(
                TValueResult value,
                IQueryResultNodeAsync<TValueSource, TError> next,
                Func<TValueSource, TValueResult> selector)
            {
                Value = value;
                this.next = next;
                this.selector = selector;
            }

            public TValueResult Value { get; }

            public async ITask<IQueryResultNodeAsync<TValueResult, TError>> Next()
            {
                return await this.next.Select(this.selector).ConfigureAwait(false);
            }
        }
    }
}
