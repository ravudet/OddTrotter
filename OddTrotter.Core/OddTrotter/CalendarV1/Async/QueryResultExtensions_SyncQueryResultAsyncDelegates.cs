/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using Fx.Either;
    using System;
    using System.Threading.Tasks;

    public static partial class QueryResultExtensions
    {
        public static IQueryResultNodeAsync<TValue, TError> ToQueryResultNodeAsync<TValue, TError>(
            this IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
        {
            return new QueryResultNodeAsync<TValue, TError>(source);
        }

        public sealed class QueryResultNodeAsync<TValue, TError> : IQueryResultNodeAsync<TValue, TError>
        {
            private readonly IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source;

            public QueryResultNodeAsync(IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>> source)
            {
                this.source = source;
            }

            public TResult Apply<TResult, TContext>(Func<IElementAsync<TValue, TError>, TContext, TResult> leftMap, Func<IEither<IError<TError>, IEmpty>, TContext, TResult> rightMap, TContext context)
            {
                return this.source.Apply(leftMap, rightMap, context);
            }

            public Task<TResult> Apply<TResult, TContext>(Func<IElementAsync<TValue, TError>, TContext, Task<TResult>> leftMap, Func<IEither<IError<TError>, IEmpty>, TContext, Task<TResult>> rightMap, TContext context)
            {
                return this.source.Apply(leftMap, rightMap, context);
            }
        }

        public static IQueryResultAsync<TValueResult, TError> Select<TValueSource, TError, TValueResult>(
            this IQueryResult<TValueSource, TError> source,
            Func<TValueSource, Task<TValueResult>> selector)
        {
            return new SelectQueryResultAsync<TValueSource, TError, TValueResult>(source, selector);
        }

        private sealed class SelectQueryResultAsync<TValueSource, TError, TValueResult> : IQueryResultAsync<TValueResult, TError>
        {
            private readonly IQueryResult<TValueSource, TError> source;
            private readonly Func<TValueSource, Task<TValueResult>> selector;

            public SelectQueryResultAsync(
                IQueryResult<TValueSource, TError> source,
                Func<TValueSource, Task<TValueResult>> selector)
            {
                this.source = source;
                this.selector = selector;
            }

            public ITask<IQueryResultNodeAsync<TValueResult, TError>> GetNodes()
            {
                //// TODO task
                return new TaskWrapper<IQueryResultNodeAsync<TValueResult, TError>>(this.source.Nodes.Select(this.selector));
            }
        }

        public static async Task<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNode<TValueSource, TError> source,
            Func<TValueSource, Task<TValueResult>> selector)
        {
            var future = source
                .SelectLeft(
                    async element =>
                        new SelectElementAsync<TValueSource, TError, TValueResult>(
                            await selector(element.Value).ConfigureAwait(false),
                            element.Next(),
                            selector));
            var result = await future.ConfigureAwait(false);
            return result.ToQueryResultNodeAsync();
        }

        private sealed class SelectElementAsync<TValueSource, TError, TValueResult> : IElementAsync<TValueResult, TError>
        {
            private readonly IQueryResultNode<TValueSource, TError> next;
            private readonly Func<TValueSource, Task<TValueResult>> selector;

            public SelectElementAsync(
                TValueResult value,
                IQueryResultNode<TValueSource, TError> next,
                Func<TValueSource, Task<TValueResult>> selector)
            {
                Value = value;
                this.next = next;
                this.selector = selector;
            }

            public TValueResult Value { get; }

            public ITask<IQueryResultNodeAsync<TValueResult, TError>> NextAsync()
            {
                //// TODO task
                return new TaskWrapper<IQueryResultNodeAsync<TValueResult, TError>>(this.next.Select(this.selector));
            }
        }

        //// TODO update the asyncresult + syncdelegate variants to use the async interfaces
    }
}
