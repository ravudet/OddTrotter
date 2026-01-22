/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public sealed class QueryResultNode<TValue, TError> : IQueryResultNode<TValue, TError>
    {
        private readonly IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> node;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
        public QueryResultNode(IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> node)
        {
            ArgumentNullException.ThrowIfNull(node);

            this.node = node;
        }

        /// <inheritdoc/>
        public TResult Apply<TResult, TContext>(
            Func<IElement<TValue, TError>, TContext, TResult> leftMap, 
            Func<IEither<IError<TError>, IEmpty>, TContext, TResult> rightMap, 
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return this.node.Apply(leftMap, rightMap, context);
        }

        /// <inheritdoc/>
        public async Task<TResult> Apply<TResult, TContext>(
            Func<IElement<TValue, TError>, TContext, Task<TResult>> leftMap, 
            Func<IEither<IError<TError>, IEmpty>, TContext, Task<TResult>> rightMap, 
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return await this.node.Apply(leftMap, rightMap, context).ConfigureAwait(false);
        }
    }
}
