/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public sealed class QueryResultNodeAsync<TValue, TError> : IQueryResultNodeAsync<TValue, TError>
    {
        private readonly IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public QueryResultNodeAsync(IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            this.source = source;
        }

        /// <inheritdoc/>
        public TResult Apply<TResult, TContext>(
            Func<IElementAsync<TValue, TError>, TContext, TResult> leftMap, 
            Func<IEither<IError<TError>, IEmpty>, TContext, TResult> rightMap,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return this.source.Apply(leftMap, rightMap, context);
        }

        /// <inheritdoc/>
        public Task<TResult> Apply<TResult, TContext>(
            Func<IElementAsync<TValue, TError>, TContext, Task<TResult>> leftMap, 
            Func<IEither<IError<TError>, IEmpty>, TContext, Task<TResult>> rightMap,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return this.source.Apply(leftMap, rightMap, context);
        }
    }
}
