/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public static partial class QueryResultNodeAsyncExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValueSource"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TValueResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
        /// </exception>
        public static async ITask<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNodeAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

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

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectElementAsync(
                TValueResult value,
                IQueryResultNodeAsync<TValueSource, TError> next,
                Func<TValueSource, TValueResult> selector)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(selector);

                this.Value = value;
                this.next = next;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public TValueResult Value { get; }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValueResult, TError>> Next()
            {
                return await this.next.Select(this.selector).ConfigureAwait(false);
            }
        }
    }
}
