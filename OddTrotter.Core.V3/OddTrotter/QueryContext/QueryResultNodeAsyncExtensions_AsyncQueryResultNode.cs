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
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="try"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="try"/> is <see langword="null"/>
        /// </exception>
        public static async ITask<IQueryResultNode<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this ITask<IQueryResultNode<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return await (await source.ConfigureAwait(false)).TrySelect(@try).ConfigureAwait(false);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
        /// </exception>
        public static async ITask<IQueryResultNode<TValue, TError>> Where<TValue, TError>(
            this ITask<IQueryResultNode<TValue, TError>> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return await (await source.ConfigureAwait(false)).Where(predicate).ConfigureAwait(false);
        }
    }
}
