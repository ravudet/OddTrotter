/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

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
        public static QueryResultNode<TValue, TError> ToQueryResultNode<TValue, TError>(
            this IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            //// TODO this doesn't belong in queryresultnodeextensions, but it also doesn't belong in the fx.either.eitherextensions; this should be in fx.querycontext.eitherextensions probably

            ArgumentNullException.ThrowIfNull(source);

            return new QueryResultNode<TValue, TError>(source);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public static async ITask<QueryResultNode<TValue, TError>> ToQueryResultNode<TValue, TError>(
            this ITask<IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>>> source)
        {
            //// TODO this doesn't belong in queryresultnodeextensions, but it also doesn't belong in the fx.either.eitherextensions; this should be in fx.querycontext.eitherextensions probably

            ArgumentNullException.ThrowIfNull(source);

            return new QueryResultNode<TValue, TError>(await source.ConfigureAwait(false));
        }
    }
}
