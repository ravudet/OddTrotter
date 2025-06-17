/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;
    using Fx.Try;

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
            this ITask<IQueryResultNodeAsync<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return await (await source.ConfigureAwait(false)).Select(selector).ConfigureAwait(false);
        }

        public static async ITask<IQueryResultNodeAsync<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this ITask<IQueryResultNodeAsync<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            return await (await source.ConfigureAwait(false)).TrySelect(@try).ConfigureAwait(false);
        }
    }
}
