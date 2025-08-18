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
