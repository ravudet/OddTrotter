/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public static partial class QueryResultNodeExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public static QueryResultNodeAsync<TValue, TError> ToQueryResultNodeAsync<TValue, TError>(
            this IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new QueryResultNodeAsync<TValue, TError>(source);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public static async ITask<QueryResultNodeAsync<TValue, TError>> ToQueryResultNodeAsync<TValue, TError>(
            this ITask<IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>>> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new QueryResultNodeAsync<TValue, TError>(await source.ConfigureAwait(false));
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryResultNode"></param>
        /// <param name="elementMap"></param>
        /// <param name="terminalMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResultNode"/> or <paramref name="elementMap"/> or <paramref name="terminalMap"/> is <see langword="null"/></exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="elementMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="elementMap"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="terminalMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="terminalMap"/> threw.
        /// </exception>
        public static async ITask<TResult> Apply<TValue, TError, TResult>(
            this IQueryResultNode<TValue, TError> queryResultNode,
            Func<IElement<TValue, TError>, TResult> elementMap,
            Func<IEither<IError<TError>, IEmpty>, ITask<TResult>> terminalMap)
        {
            ArgumentNullException.ThrowIfNull(queryResultNode);
            ArgumentNullException.ThrowIfNull(elementMap);
            ArgumentNullException.ThrowIfNull(terminalMap);

            return await
                queryResultNode
                    .Apply(
                        async (element, nothing) => await Task.FromResult(elementMap(element)).ConfigureAwait(false),
                        async (terminal, nothing) => await terminalMap(terminal).ConfigureAwait(false),
                        new Nothing())
                    .ConfigureAwait(false);
        }

    }
}
