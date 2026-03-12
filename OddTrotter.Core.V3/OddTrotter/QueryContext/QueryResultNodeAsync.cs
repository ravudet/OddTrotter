/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public sealed class QueryResultNodeAsync<TValue, TError> : IQueryResultNode<TValue, TError>
    {
        private readonly IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> source;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public QueryResultNodeAsync(IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            this.source = source;
        }

        public Realizable.Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TValue, TError>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return this.source.ApplyAsync(leftMap, rightMap, ref context);
        }
    }
}
