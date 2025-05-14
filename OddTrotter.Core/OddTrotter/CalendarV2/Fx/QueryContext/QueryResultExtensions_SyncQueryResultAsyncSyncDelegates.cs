/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using Fx.Either;
    using System;
    using System.Threading.Tasks;

    public static partial class QueryResultExtensions
    {
        public static async Task<IQueryResult<TValueResult, TError>> SelectAsync<TValueSource, TError, TValueResult>(
            this IQueryResult<TValueSource, TError> source,
            Func<TValueSource, Task<TValueResult>> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            //// TODO do this "right"
            ////return await Task.FromResult(source.Select(element => selector(element).ConfigureAwait(false).GetAwaiter().GetResult())).ConfigureAwait(false);
            return new SelectAsyncQueryResult<TValueSource, TError, TValueResult>(await source.Nodes.SelectAsync(selector).ConfigureAwait(false));
        }

        /*private sealed class SelectAsyncQueryResult<TValueSource, TError, TValueResult> : IQueryResult<TValueResult, TError>
        {
            private readonly IQueryResult<TValueSource, TError> source;
            private readonly Func<TValueSource, Task<TValueResult>> selector;

            public SelectAsyncQueryResult(IQueryResult<TValueSource, TError> source, Func<TValueSource, Task<TValueResult>> selector)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(selector);

                this.source = source;
                this.selector = selector;
            }

            public IQueryResultNode<TValueResult, TError> Nodes
            {
                get
                {
                    return this.source.Nodes.SelectAsync(selector);
                }
            }
        }*/

        private sealed class SelectAsyncQueryResult<TValueSource, TError, TValueResult> : IQueryResult<TValueResult, TError>
        {
            public SelectAsyncQueryResult(IQueryResultNode<TValueResult, TError> source)
            {
                ArgumentNullException.ThrowIfNull(source);

                this.Nodes = source;
            }

            public IQueryResultNode<TValueResult, TError> Nodes { get; }
        }

        private static async Task<IQueryResultNode<TValueResult, TError>> SelectAsync<TValueSource, TError, TValueResult>(
            this IQueryResultNode<TValueSource, TError> source,
            Func<TValueSource, Task<TValueResult>> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            /*return (await source
                .SelectLeftAsync(
                    async element =>
                        new SelectAsyncElement<TValueSource, TError, TValueResult>(
                            await selector(element.Value).ConfigureAwait(false),
                            await element.NextAsync().ConfigureAwait(false),
                            selector))
                .ConfigureAwait(false))
                .ToQueryResultNode();*/

            return await Task.FromResult(new Node<TValueSource, TError, TValueResult>(source, selector)).ConfigureAwait(false);
        }

        private sealed class Node<TValueSource, TError, TValueResult> : IQueryResultNode<TValueResult, TError>
        {
            private readonly IQueryResultNode<TValueSource, TError> source;
            private readonly Func<TValueSource, Task<TValueResult>> selector;

            public Node(
                IQueryResultNode<TValueSource, TError> source,
                Func<TValueSource, Task<TValueResult>> selector)
            {
                this.source = source;
                this.selector = selector;
            }

            public TResult Apply<TResult, TContext>(Func<IElement<TValueResult, TError>, TContext, TResult> leftMap, Func<IEither<IError<TError>, IEmpty>, TContext, TResult> rightMap, TContext context)
            {
                return this
                    .Apply(
                        async (left, context) => await Task.FromResult(leftMap(left, context)).ConfigureAwait(false), 
                        async (right, context) => await Task.FromResult(rightMap(right, context)).ConfigureAwait(false),
                        context)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }

            public async Task<TResult> Apply<TResult, TContext>(Func<IElement<TValueResult, TError>, TContext, Task<TResult>> leftMap, Func<IEither<IError<TError>, IEmpty>, TContext, Task<TResult>> rightMap, TContext context)
            {
                var selected = (await this.SelectAsync().ConfigureAwait(false));
                
                var applied = await selected.Apply(leftMap, rightMap, context).ConfigureAwait(false);

                return applied;
            }

            private async Task<IQueryResultNode<TValueResult, TError>> SelectAsync()
            {
                return (await source
                    .SelectLeft(
                        async element =>
                            new SelectAsyncElement<TValueSource, TError, TValueResult>(
                                await selector(element.Value).ConfigureAwait(false),
                                await element.NextAsync().ConfigureAwait(false),
                                selector))
                    .ConfigureAwait(false))
                    .ToQueryResultNode();
            }
        }

        private sealed class SelectAsyncElement<TValueSource, TError, TValueResult> : IElement<TValueResult, TError>
        {
            private readonly IQueryResultNode<TValueSource, TError> next;
            private readonly Func<TValueSource, Task<TValueResult>> selector;

            public SelectAsyncElement(
                TValueResult value,
                IQueryResultNode<TValueSource, TError> next,
                Func<TValueSource, Task<TValueResult>> selector)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(selector);

                this.Value = value;
                this.next = next;
                this.selector = selector;
            }

            public TValueResult Value { get; }

            public IQueryResultNode<TValueResult, TError> Next()
            {
                //// TODO implement this
                return this.NextAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            public async Task<IQueryResultNode<TValueResult, TError>> NextAsync()
            {
                return await this.next.SelectAsync(selector).ConfigureAwait(false);
            }
        }
    }
}
