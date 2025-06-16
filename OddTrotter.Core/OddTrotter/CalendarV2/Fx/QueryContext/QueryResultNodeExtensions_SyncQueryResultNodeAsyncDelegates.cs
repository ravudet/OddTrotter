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


        //// TODO are these operations all "lifts"? as in, we have `ToQueryResultNodeAsync`, so we should also be able to do it on `itask<input>` and get back an `itask<result>`

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
    }
}
