/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Try;

    public static partial class QueryResultExtensions
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/></exception>
        public static async Task<IQueryResult<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this Task<IQueryResult<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return (await source.ConfigureAwait(false)).Select(selector);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="try"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="try"/> is <see langword="null"/></exception>
        public static async Task<IQueryResult<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this Task<IQueryResult<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return (await source.ConfigureAwait(false)).TrySelect(@try);
        }
    }
}
