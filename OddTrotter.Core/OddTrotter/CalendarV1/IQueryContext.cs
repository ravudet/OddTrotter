////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace OddTrotter.Calendar
{
    using OddTrotter.TodoList;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using static OddTrotter.Calendar.QueryResultExtensions;

    using Fx.Either;
    using Fx.QueryContext;
    using System.Net.Http.Headers;
    using Fx.Try;
    using static OddTrotter.Calendar.QueryResultExtensions.FirstOrDefaultResult<TElement, TError, TDefault>;
    using System.Linq;

    public interface IQueryContext<TValue, TError>
    {
        /// <summary>
        /// TODO should this really not throw exceptions? "partial" was intended to indicate that *some* results came back, but an intermediate network error occurred; is it ok for "partial" to just mean any error at all?
        /// </summary>
        /// <returns></returns>
        Task<QueryResult<TValue, TError>> Evaluate();
    }

    /// <summary>
    /// TODO is it *really* that this is modeling queryresult*nodes* and that there are really 2 types of "queryresults": one that has just values, and one that has values and an error? this would let you mostly have ienumerables running around
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    public abstract class QueryResult<TValue, TError>
    {
        private QueryResult()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that the <see cref="Visitor{TResult, TContext}.Dispatch"/> overloads can throw</exception> //// TODO is this good?
        protected abstract TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that the <see cref="Visitor{TResult, TContext}.Dispatch"/> overloads can throw</exception> //// TODO is this good?
        protected abstract Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context);

        public abstract class Visitor<TResult, TContext>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Throws any of the exceptions that the <see cref="Dispatch"/> overloads can throw</exception> //// TODO is this good?
            public TResult Visit(QueryResult<TValue, TError> node, TContext context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return node.Accept(this, context);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract TResult Dispatch(Final node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract TResult Dispatch(Element node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract TResult Dispatch(Partial node, TContext context);
        }

        public abstract class AsyncVisitor<TResult, TContext>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Throws any of the exceptions that the <see cref="DispatchAsync"/> overloads can throw</exception> //// TODO is this good?
            public async Task<TResult> VisitAsync(QueryResult<TValue, TError> node, TContext context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return await node.AcceptAsync(this, context).ConfigureAwait(false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract Task<TResult> DispatchAsync(Final node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract Task<TResult> DispatchAsync(Element node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract Task<TResult> DispatchAsync(Partial node, TContext context);
        }

        public sealed class Final : QueryResult<TValue, TError>
        {
            /// <summary>
            /// 
            /// </summary>
            public Final()
            {
            }

            /// <inheritdoc/>
            protected override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return visitor.Dispatch(this, context);
            }

            /// <inheritdoc/>
            protected override async Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return await visitor.DispatchAsync(this, context).ConfigureAwait(false);
            }
        }

        public abstract class Element : QueryResult<TValue, TError>
        {
            public Element(TValue value)
            {
                //// TODO this value needs to be realized for the first element of the query result to be returned, and since the first element of the query result is the same object as the one returned, this means that we lose laziness; for example, if i have a method Foo that returns a query result that's pulled from a service and I do something like Foo().Concat(Foo()), both queries need to be executed before we can even return; the developer who is calling this concat *could* implement their own derived type of Element, but that is a significant burden over the concat call
                this.Value = value;
            }

            public TValue Value { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// this method should not throw; cases where throwing might make sense should instead be handled by returning <see cref="QueryResult{TValue, TError}.Partial"/>
            /// </remarks>
            public abstract QueryResult<TValue, TError> Next(); //// TODO you previously tried using `task`s here, but you realized that the tasks were all running in the background, taking away any laziness that might be useful; could you have something like a `lazytask` that doesn't start until awaited or something? make it a struct?

            /// <inheritdoc/>
            protected override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return visitor.Dispatch(this, context);
            }

            /// <inheritdoc/>
            protected override async Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return await visitor.DispatchAsync(this, context).ConfigureAwait(false);
            }
        }

        public sealed class Partial : QueryResult<TValue, TError>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="error"></param>
            public Partial(TError error)
            {
                this.Error = error;
            }

            public TError Error { get; }

            /// <inheritdoc/>
            protected override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return visitor.Dispatch(this, context);
            }

            /// <inheritdoc/>
            protected override async Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return await visitor.DispatchAsync(this, context).ConfigureAwait(false);
            }
        }
    }

    public static class ExceptionExtensions
    {
        public static Exception AsException<TException>(this TException exception) where TException : Exception
        {
            return exception;
        }
    }

    public static class QueryResultAsyncExtensions
    {
        public static IQueryResult<TResult, TError> TrySelect<TValue, TError, TResult>(this IQueryResult<TValue, TError> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return new TrySelectQueryResult<TResult, TError>(queryResult.Nodes.TrySelectIterator(@try));
        }

        private sealed class TrySelectQueryResult<TResult, TError> : IQueryResult<TResult, TError>
        {
            public TrySelectQueryResult(IQueryResultNode<TResult, TError> nodes)
            {
                Nodes = nodes;
            }


            public IQueryResultNode<TResult, TError> Nodes { get; }
        }

        private static IQueryResultNode<TResult, TError> TrySelectIterator<TValue, TError, TResult>(this IQueryResultNode<TValue, TError> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return queryResult.SelectLeft(
                element =>
                    TryCreate(
                        element,
                        Lift<TValue, TResult, IElement<TValue, TError>>(@try, element => element.Value), ///// TODO not sure that this is a life, and not sure that the lack of the type inference makes this useful in any way
                        (element, result) => new TrySelectElement<TValue, TError, TResult>(result, element.Next(), @try),
                        element => element.Next().TrySelectIterator(@try))
                    .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private static Try<TOther, TResult> Lift<TSource, TResult, TOther>(Try<TSource, TResult> @try, Func<TOther, TSource> selector)
        {
            return (TOther other, [MaybeNullWhen(false)] out TResult result) => @try(selector(other), out result);
        }

        private static IEither<TLeft, TRight> TryCreate<TValue, TResult, TLeft, TRight>( //// TODO this should go in the `either` factory methods class, if you choose to keep it
            TValue value,
            Try<TValue, TResult> discriminator,
            Func<TValue, TResult, TLeft> leftFactory,
            Func<TValue, TRight> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value, out var result))
            {
                return Either.Left(leftFactory(value, result)).Right<TRight>();
            }
            else
            {
                return Either.Left<TLeft>().Right(rightFactory(value));
            }
        }

        private sealed class TrySelectElement<TValue, TError, TResult> : IElement<TResult, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Try<TValue, TResult> @try;

            public TrySelectElement(TResult value, IQueryResultNode<TValue, TError> next, Fx.Try.Try<TValue, TResult> @try)
            {
                Value = value;
                this.next = next;
                this.@try = @try;
            }

            public TResult Value { get; }

            public IQueryResultNode<TResult, TError> Next()
            {
                return this.next.TrySelectIterator(this.@try);
            }
        }

        public static async Task<IQueryResult<TValue, TError>> TrySelectAsync<TValue, TError>(this Task<IQueryResult<IEither<TValue, Nothing>, TError>> queryResult)
        {
            //// TODO is there a way to avoid all of this type specification through some sort of type inference? what is missing that makes type inference not work?
            return await queryResult.TrySelectAsync((IEither<TValue, Nothing> either, [MaybeNullWhen(false)] out TValue result) => either.TryGet(out result)).ConfigureAwait(false);
        }

        public static async Task<IQueryResult<TResult, TError>> TrySelectAsync<TValue, TError, TResult>(this Task<IQueryResult<TValue, TError>> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return (await queryResult).TrySelect(@try);
        }

        public static async Task<QueryResult<TValue, TError>> TrySelectAsync<TValue, TError>(this Task<QueryResult<IEither<TValue, Nothing>, TError>> queryResult)
        {
            //// TODO is there a way to avoid all of this type specification through some sort of type inference? what is missing that makes type inference not work?
            return await queryResult.TrySelectAsync((IEither<TValue, Nothing> either, [MaybeNullWhen(false)] out TValue result) => either.TryGet(out result)).ConfigureAwait(false);
        }

        public static async Task<QueryResult<TResult, TError>> TrySelectAsync<TValue, TError, TResult>(this Task<QueryResult<TValue, TError>> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return await TrySelectVisitor<TValue, TError, TResult>.Instance.VisitAsync(await queryResult.ConfigureAwait(false), @try).ConfigureAwait(false);
        }

        private sealed class TrySelectResult<TValue, TError, TResult> : QueryResult<TResult, TError>.Element
        {
            private readonly QueryResult<TValue, TError>.Element queryResult;
            private readonly Fx.Try.Try<TValue, TResult> @try;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <param name="queryResult"></param>
            /// <param name="try"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="try"/> is <see langword="null"/></exception>
            public TrySelectResult(TResult value, QueryResult<TValue, TError>.Element queryResult, Fx.Try.Try<TValue, TResult> @try)
                : base(value)
            {
                ArgumentNullException.ThrowIfNull(queryResult, nameof(queryResult));
                ArgumentNullException.ThrowIfNull(@try, nameof(@try));

                this.queryResult = queryResult;
                this.@try = @try;
            }

            /// <inheritdoc/>
            public override QueryResult<TResult, TError> Next()
            {
                //// TODO i don't think you documented a single `queryresult.element.next` method
                return TrySelectVisitor<TValue, TError, TResult>.Instance.VisitAsync(this.queryResult.Next(), this.@try).ConfigureAwait(false).GetAwaiter().GetResult(); //// TODO async query result
            }
        }

        private sealed class TrySelectVisitor<TValue, TError, TResult> : QueryResult<TValue, TError>.AsyncVisitor<QueryResult<TResult, TError>, Fx.Try.Try<TValue, TResult>>
        {
            /// <summary>
            /// 
            /// </summary>
            private TrySelectVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static TrySelectVisitor<TValue, TError, TResult> Instance { get; } = new TrySelectVisitor<TValue, TError, TResult>();

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TValue, TError>.Final node, Fx.Try.Try<TValue, TResult> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return await Task.FromResult(new QueryResult<TResult, TError>.Final()).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Throws any of the exceptions that <paramref name="context"/> can throw</exception> //// TODO is this good?
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TValue, TError>.Element node, Fx.Try.Try<TValue, TResult> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (context(node.Value, out var value)) //// TODO you have an unstated convention that interfaces and delegates and methods you define won't return null unless explicitly documented or marked up with `?`; do you want to keep doing that, or state explicitly stating that null can't be returned?
                {
                    return new TrySelectResult<TValue, TError, TResult>(value, node, context);
                }
                else
                {
                    return await this.VisitAsync(node.Next(), context).ConfigureAwait(false);
                }
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TValue, TError>.Partial node, Fx.Try.Try<TValue, TResult> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return await Task.FromResult(new QueryResult<TResult, TError>.Partial(node.Error)).ConfigureAwait(false);
            }
        }

        public static async Task<QueryResult<TValue, TError>> WhereAsync<TValue, TError>(this Task<QueryResult<TValue, TError>> queryResult, Func<TValue, bool> predicate)
        {
            return (await queryResult.ConfigureAwait(false)).Where(predicate);
        }


        /*public static async Task<FirstOrDefaultResult<TElement, TError, TDefault>> FirstOrDefault<TElement, TError, TDefault>(this Task<QueryResult<TElement, TError>> queryResult)
        {
            //// TODO in `select`, for convenience, you have a `select` overload that *does* use a task queryresult, but *doesn't* use a task selector; that's not really relevenat for first; do you still want the "convenience method"?
            //// TODO actually, you seem to have two dimensions: is `this` a task + is the `func` a task? and you seem to want (for convenience) all 4 variations
            return (await queryResult.ConfigureAwait(false)).FirstOrDefault();
        }*/

        public static async Task<QueryResultExtensions.FirstOrDefaultResult<TElement, TError, TDefault>> FirstOrDefaultAsync<TElement, TError, TDefault>(this Task<QueryResult<TElement, TError>> queryResult, TDefault defaultValue)
        {
            return (await queryResult.ConfigureAwait(false)).FirstOrDefault(defaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryResult"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
        public static async Task<QueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<QueryResult<TSource, TError>> queryResult,
            Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(queryResult);
            ArgumentNullException.ThrowIfNull(selector);

            return (await queryResult.ConfigureAwait(false)).Select(selector);
        }

        public static async Task<QueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<QueryResult<TSource, TError>> queryResult,
            Func<TSource, Task<TResult>> selector)
        {
            return await (await queryResult.ConfigureAwait(false)).SelectAsync(selector).ConfigureAwait(false);
        }

        public static async Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<IQueryResult<TSource, TError>> queryResult,
            Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(queryResult);
            ArgumentNullException.ThrowIfNull(selector);

            return (await queryResult.ConfigureAwait(false)).Select(selector);
        }

        public static async Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<IQueryResult<TSource, TError>> queryResult,
            Func<TSource, Task<TResult>> selector)
        {
            return await (await queryResult.ConfigureAwait(false)).SelectAsync(selector).ConfigureAwait(false);
        }

        public static Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this IQueryResult<TSource, TError> queryResult,
            Func<TSource, Task<TResult>> selector)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return
                Task.FromResult(
                        queryResult
                            .Select(
                                element => selector(element).ConfigureAwait(false).GetAwaiter().GetResult())); //// TODO make this actually async
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryResult"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
        public static async Task<QueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this QueryResult<TSource, TError> queryResult,
            Func<TSource, Task<TResult>> selector)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return await SelectAsyncVisitor<TSource, TError, TResult>.Instance.VisitAsync(queryResult, selector).ConfigureAwait(false);
        }

        private sealed class SelectAsyncResult<TSource, TError, TResult> : QueryResult<TResult, TError>.Element
        {
            private readonly QueryResult<TSource, TError>.Element queryResult;
            private readonly Func<TSource, Task<TResult>> selector;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="queryResult"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> or <paramref name="queryResult"/> is <see langword="null"/></exception>
            public SelectAsyncResult(
                QueryResult<TSource, TError>.Element queryResult,
                Func<TSource, Task<TResult>> selector)
                :base((selector ?? throw new ArgumentNullException(nameof(selector)))((queryResult ?? throw new ArgumentNullException(nameof(queryResult))).Value).ConfigureAwait(false).GetAwaiter().GetResult()) //// TODO you should getresult here, you should have an async queryresult variant... //// TODO does this mean that `queryresult` should *not* have an async visitor, and `asyncqueryresult` should not have a synchronous visitor?
            {
                this.queryResult = queryResult;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public override QueryResult<TResult, TError> Next()
            {
                return SelectAsyncVisitor<TSource, TError, TResult>.Instance.VisitAsync(this.queryResult.Next(), this.selector).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private class SelectAsyncVisitor<TSource, TError, TResult> : QueryResult<TSource, TError>.AsyncVisitor<QueryResult<TResult, TError>, Func<TSource, Task<TResult>>>
        {
            /// <summary>
            /// 
            /// </summary>
            protected SelectAsyncVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static SelectAsyncVisitor<TSource, TError, TResult> Instance { get; } = new SelectAsyncVisitor<TSource, TError, TResult>();

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/>  is <see langword="null"/></exception> 
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TSource, TError>.Final node, Func<TSource, Task<TResult>> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return await Task.FromResult(new QueryResult<TResult, TError>.Final()).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/>  is <see langword="null"/></exception> 
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TSource, TError>.Element node, Func<TSource, Task<TResult>> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return await Task.FromResult(new SelectAsyncResult<TSource, TError, TResult>(node, context)).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/>  is <see langword="null"/></exception> 
            public override async Task<QueryResult<TResult, TError>> DispatchAsync(QueryResult<TSource, TError>.Partial node, Func<TSource, Task<TResult>> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return await Task.FromResult(new QueryResult<TResult, TError>.Partial(node.Error)).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Either
    ///     <
    ///         (
    ///             CalendarEvent SeriesMaster, 
    ///             FirstOrDefault
    ///                 <
    ///                     Either
    ///                         <
    ///                             CalendarEvent, 
    ///                             CalendarEventsContextTranslationException
    ///                         >,
    ///                     CalendarEventsContextPagingException,
    ///                     Void
    ///                 >
    ///         ),
    ///         CalendarEventsContextTranslationException
    ///     >
    ///     
    /// Either
    ///     <
    ///         (
    ///             CalendarEvent SeriesMaster,
    ///             Either
    ///                 <
    ///                     Either
    ///                         <
    ///                             CalendarEvent,
    ///                             CalendarEventsContextTranslationException
    ///                         >,
    ///                     CalendarEVentsContextPagingException
    ///                 >
    ///         ),
    ///         CalendarEventsContextTranslationException
    ///     >
    ///     
    /// 
    /// 
    /// 
    /// Either
    ///     <
    ///         (
    ///             CalendarEvent, 
    ///             Either
    ///                 <
    ///                     Either
    ///                         <
    ///                             CalendarEvent, 
    ///                             CalendarEventsContextTranslationEcxeption
    ///                         >, 
    ///                     CAlendarEventsContextPAgingException
    ///                 >
    ///         ), 
    ///         CalendarEventsContextTranslationException
    ///     >
    ///     
    /// 
    /// Either<CalendarEvent, CalendarEventsContextTranslationException>
    /// </summary>

    public static class QueryResultExtensions
    {
        public static bool IsElement<TElement, TError, TDefault>(this FirstOrDefaultResult<TElement, TError, TDefault> result)
        {
            return result is FirstOrDefaultResult<TElement, TError, TDefault>.First;
        }

        public static bool IsError<TElement, TError, TDefault>(this FirstOrDefaultResult<TElement, TError, TDefault> result)
        {
            return result is FirstOrDefaultResult<TElement, TError, TDefault>.Error;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="firstOrDefaultResult"></param>
        /// <param name="elementSelector"></param>
        /// <param name="errorSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="firstOrDefaultResult"/> or <paramref name="elementSelector"/> or <paramref name="errorSelector"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that <paramref name="elementSelector"/> or <paramref name="errorSelector"/> can throw</exception> //// TODO is this good?
        public static IEither<TResult, Nothing> TryNotDefault<TElement, TError, TDefault, TResult>(
            this FirstOrDefaultResult<TElement, TError, TDefault> firstOrDefaultResult,
            Func<TElement, TResult> elementSelector,
            Func<TError, TResult> errorSelector)
        {
            //// TODO compare writing each firstordefaultresult variant with converting firstordefaultresult to either and using either.tryleft/tryright
            //// TODO do you like how you named things regarding the "try" stuff?
            ArgumentNullException.ThrowIfNull(firstOrDefaultResult);
            ArgumentNullException.ThrowIfNull(elementSelector);
            ArgumentNullException.ThrowIfNull(errorSelector);

            //// TODO normalize your naming of "visit" and "select"

            return firstOrDefaultResult
                .Visit(
                    element =>
                        Either
                            .Left(
                                elementSelector(element))
                            .Right<Nothing>(),
                    error =>
                        Either
                            .Left(
                                errorSelector(error))
                            .Right<Nothing>(),
                    @default =>
                        Either
                            .Left<TResult>()
                            .Right(new Nothing()));
        }

        public static IEither<TResult, Nothing> TryDefault<TElement, TError, TDefault, TResult>(
            this FirstOrDefaultResult<TElement, TError, TDefault> firstOrDefaultResult,
            Func<TElement, TResult> elementSelector,
            Func<TError, TResult> errorSelector,
            Func<TDefault, TResult> defaultSelector)
        {
            if (firstOrDefaultResult.TryIsDefault(elementSelector, errorSelector, defaultSelector, out var result))
            {
                return Either.Left(result).Right<Nothing>();
            }
            else
            {
                return Either.Left<TResult>().Right(new Nothing());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <see langword="null"/></exception>
        public static bool TryIsDefault<TElement, TError, TDefault, TResult>(
            this FirstOrDefaultResult<TElement, TError, TDefault> firstOrDefaultResult,
            Func<TElement, TResult> elementSelector,
            Func<TError, TResult> errorSelector,
            Func<TDefault, TResult> defaultSelector,
            out TResult result)
        {
            if (firstOrDefaultResult == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var selected = firstOrDefaultResult.Visit(
                element => (Result: elementSelector(element), IsDefault: false),
                error => (Result: errorSelector(error), IsDefault: false),
                @default => (Result: defaultSelector(@default), IsDefault: true));

            result = selected.Result;
            return selected.IsDefault;
        }

        public static IEither<TElement, IEither<TError, TDefault>> ToEither<TElement, TError, TDefault>(this FirstOrDefaultResult<TElement, TError, TDefault> firstOrDefaultResult)
        {
            return firstOrDefaultResult.Visit(
                element => Either.Left(element).Right<Fx.Either.Either<TError, TDefault>>(),
                error => Either.Left<TElement>().Right(Either.Left(error).Right<TDefault>()),
                @default => Either.Left<TElement>().Right(Either.Left<TError>().Right(@default)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="elementSelector"></param>
        /// <param name="errorSelector"></param>
        /// <param name="defaultSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="elementSelector"/> or <paramref name="errorSelector"/> or <paramref name="defaultSelector"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that <paramref name="elementSelector"/> or <paramref name="errorSelector"/> or <paramref name="defaultSelector"/> can throw</exception> //// TODO is this good?
        public static TResult Visit<TElement, TError, TDefault, TResult>(
            this FirstOrDefaultResult<TElement, TError, TDefault> result,
            Func<TElement, TResult> elementSelector,
            Func<TError, TResult> errorSelector,
            Func<TDefault, TResult> defaultSelector)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(elementSelector);
            ArgumentNullException.ThrowIfNull(errorSelector);
            ArgumentNullException.ThrowIfNull(defaultSelector);

            return
                new FirstOrDefaultResultDelegateVisitor<TElement, TError, TDefault, TResult, Nothing>(
                    (element, context) => elementSelector(element),
                    (error, context) => errorSelector(error),
                    (@default, context) => defaultSelector(@default))
                .Visit(result, new Nothing());
        }

        private sealed class FirstOrDefaultResultDelegateVisitor<TElement, TError, TDefault, TResult, TContext> : FirstOrDefaultResult<TElement, TError, TDefault>.Visitor<TResult, TContext>
        {
            private readonly Func<TElement, TContext, TResult> elementSelector;
            private readonly Func<TError, TContext, TResult> errorSelector;
            private readonly Func<TDefault, TContext, TResult> defaultSelector;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="elementSelector"></param>
            /// <param name="errorSelector"></param>
            /// <param name="defaultSelector"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="elementSelector"/> or <paramref name="errorSelector"/> or <paramref name="defaultSelector"/> is <see langword="null"/></exception>
            public FirstOrDefaultResultDelegateVisitor(
                Func<TElement, TContext, TResult> elementSelector,
                Func<TError, TContext, TResult> errorSelector,
                Func<TDefault, TContext, TResult> defaultSelector)
            {
                ArgumentNullException.ThrowIfNull(elementSelector);
                ArgumentNullException.ThrowIfNull(errorSelector);
                ArgumentNullException.ThrowIfNull(defaultSelector);

                this.elementSelector = elementSelector;
                this.errorSelector = errorSelector;
                this.defaultSelector = defaultSelector;
            }

            /// <inheritdoc/>
            protected internal override TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.First node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return this.elementSelector(node.Value, context);
            }

            /// <inheritdoc/>
            protected internal override TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.Error node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return this.errorSelector(node.Value, context);
            }

            /// <inheritdoc/>
            protected internal override TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.Default node, TContext context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return this.defaultSelector(node.Value, context);
            }
        }

        public abstract class FirstOrDefaultResult<TElement, TError, TDefault>
        {
            /// <summary>
            /// 
            /// </summary>
            private FirstOrDefaultResult()
            {
                //// TODO do you really like this approach over using nested eithers? you will likely follow this pattern in other `queryresult` extensions
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TResult"></typeparam>
            /// <typeparam name="TContext"></typeparam>
            /// <param name="visitor"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Throws any of the exceptions that the <see cref="Visitor{TResult, TContext}.Accept"/> overloads can throw</exception> //// TODO is this good?
            protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

            public abstract class Visitor<TResult, TContext>
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <param name="context"></param>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                /// <exception cref="Exception">Throws any of the exceptions that the <see cref="Accept"/> overloads can throw</exception> //// TODO is this good?
                public TResult Visit(FirstOrDefaultResult<TElement, TError, TDefault> node, TContext context)
                {
                    ArgumentNullException.ThrowIfNull(node);

                    return node.Dispatch(this, context);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <param name="context"></param>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
                protected internal abstract TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.First node, TContext context);

                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <param name="context"></param>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
                protected internal abstract TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.Error node, TContext context);

                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <param name="context"></param>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
                protected internal abstract TResult Accept(FirstOrDefaultResult<TElement, TError, TDefault>.Default node, TContext context);
            }

            public sealed class First : FirstOrDefaultResult<TElement, TError, TDefault>
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public First(TElement value)
                {
                    this.Value = value;
                }

                public TElement Value { get; }

                /// <inheritdoc/>
                protected sealed override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                {
                    ArgumentNullException.ThrowIfNull(visitor);

                    return visitor.Accept(this, context);
                }
            }

            public sealed class Error : FirstOrDefaultResult<TElement, TError, TDefault>
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public Error(TError value)
                {
                    this.Value = value;
                }

                public TError Value { get; }

                /// <inheritdoc/>
                protected sealed override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                {
                    ArgumentNullException.ThrowIfNull(visitor);

                    return visitor.Accept(this, context);
                }
            }

            public sealed class Default : FirstOrDefaultResult<TElement, TError, TDefault>
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public Default(TDefault value)
                {
                    this.Value = value;
                }

                public TDefault Value { get; }

                /// <inheritdoc/>
                protected sealed override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                {
                    ArgumentNullException.ThrowIfNull(visitor);

                    return visitor.Accept(this, context);
                }
            }
        }

        private sealed class FirstOrDefaultVisitor<TElement, TError, TDefault> : QueryResult<TElement, TError>.Visitor<FirstOrDefaultResult<TElement, TError, TDefault>, TDefault>
        {
            /// <summary>
            /// 
            /// </summary>
            private FirstOrDefaultVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static FirstOrDefaultVisitor<TElement, TError, TDefault> Instance { get; } = new FirstOrDefaultVisitor<TElement, TError, TDefault>();

            /// <inheritdoc/>
            public override FirstOrDefaultResult<TElement, TError, TDefault> Dispatch(QueryResult<TElement, TError>.Final node, TDefault context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return new FirstOrDefaultResult<TElement, TError, TDefault>.Default(context);
            }

            /// <inheritdoc/>
            public override FirstOrDefaultResult<TElement, TError, TDefault> Dispatch(QueryResult<TElement, TError>.Element node, TDefault context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return new FirstOrDefaultResult<TElement, TError, TDefault>.First(node.Value);
            }

            /// <inheritdoc/>
            public override FirstOrDefaultResult<TElement, TError, TDefault> Dispatch(QueryResult<TElement, TError>.Partial node, TDefault context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }
                return new FirstOrDefaultResult<TElement, TError, TDefault>.Error(node.Error);
            }
        }

        public static IEither<System.Linq.FirstOrDefault<TElement, Nothing>, TError> FirstOrDefault<TElement, TError>(this IQueryResult<TElement, TError> queryResult)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            return queryResult
                .FirstOrDefault(
                    new Nothing());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> is <see langword="null"/></exception>
        public static FirstOrDefaultResult<TElement, TError, Nothing> FirstOrDefault<TElement, TError>(this QueryResult<TElement, TError> queryResult)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            return queryResult
                .FirstOrDefault(
                    new Nothing());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="queryResult"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> is <see langword="null"/></exception>
        public static FirstOrDefaultResult<TElement, TError, TDefault> FirstOrDefault<TElement, TError, TDefault>(this QueryResult<TElement, TError> queryResult, TDefault defaultValue)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            return FirstOrDefaultVisitor<TElement, TError, TDefault>.Instance.Visit(queryResult, defaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        public static QueryResult<TValue, TError> AsBase<TValue, TError>(this QueryResult<TValue, TError> queryResult)
        {
            return queryResult;
        }

        private sealed class SplitVisitor<TValue, TError, TRight> : QueryResult<IEither<TValue, TRight>, TError>.Visitor<TError?, (Action<TValue>, Action<TRight>)>
        {
            private SplitVisitor()
            {
            }

            public static SplitVisitor<TValue, TError, TRight> Instance { get; } = new SplitVisitor<TValue, TError, TRight>();

            public override TError? Dispatch(QueryResult<IEither<TValue, TRight>, TError>.Final node, (Action<TValue>, Action<TRight>) context)
            {
                return default;
            }

            public override TError? Dispatch(QueryResult<IEither<TValue, TRight>, TError>.Element node, (Action<TValue>, Action<TRight>) context)
            {
                node.Value.Apply(
                    (left, @void) =>
                    {
                        context.Item1(left);
                        return new Nothing();
                    },
                    (right, @void) =>
                    {
                        context.Item2(right);
                        return new Nothing();
                    },
                    new Nothing()); //// TODO shouldn't you pass in `context` instead of creating closures?
                return default;
            }

            public override TError? Dispatch(QueryResult<IEither<TValue, TRight>, TError>.Partial node, (Action<TValue>, Action<TRight>) context)
            {
                return node.Error;
            }
        }

        public static TError? Split<TValue, TError, TRight>(
            this QueryResult<IEither<TValue, TRight>, TError> queryResult,
            Action<TValue> leftAction,
            Action<TRight> rightAction)
        {
            return SplitVisitor<TValue, TError, TRight>.Instance.Visit(queryResult, (leftAction, rightAction));
        }

        public static IEnumerable<TValue> ToEnumerable<TValue, TError>(this QueryResult<TValue, TError> queryResult)
        {
            while (queryResult is QueryResult<TValue, TError>.Element element)
            {
                yield return element.Value;
                queryResult = element.Next();
            }

            //// TODO throw if queryresult is now partial?
        }

        private sealed class Pointer<T>
        {
            public Pointer(T value)
            {
                this.Value = value;
            }

            public T Value { get; set; }
        }

        private static IEnumerable<TValue> ToEnumerable<TValue, TError>(Pointer<QueryResult<TValue, TError>> pointer)
        {
            while (pointer.Value is QueryResult<TValue, TError>.Element element)
            {
                yield return element.Value;
                pointer.Value = element.Next();
            }
        }

        private sealed class ErrorResult<TValue, TErrorStart, TErrorEnd> : QueryResult<TValue, TErrorEnd>.Element
        {
            private readonly QueryResult<TValue, TErrorStart>.Element queryResult;
            private readonly Func<TErrorStart, TErrorEnd> selector;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="queryResult"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
            public ErrorResult(QueryResult<TValue, TErrorStart>.Element queryResult, Func<TErrorStart, TErrorEnd> selector)
                : base((queryResult ?? throw new ArgumentNullException(nameof(queryResult))).Value)
            {
                if (selector == null)
                {
                    throw new ArgumentNullException(nameof(selector));
                }

                this.queryResult = queryResult;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public override QueryResult<TValue, TErrorEnd> Next()
            {
                return ErrorVisitor<TValue, TErrorStart, TErrorEnd>.Instance.Visit(this.queryResult.Next(), this.selector);
            }
        }

        private sealed class ErrorVisitor<TValue, TErrorStart, TErrorEnd> : QueryResult<TValue, TErrorStart>.Visitor<QueryResult<TValue, TErrorEnd>, Func<TErrorStart, TErrorEnd>>
        {
            /// <summary>
            /// 
            /// </summary>
            private ErrorVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static ErrorVisitor<TValue, TErrorStart, TErrorEnd> Instance { get; } = new ErrorVisitor<TValue, TErrorStart, TErrorEnd>();

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TErrorEnd> Dispatch(QueryResult<TValue, TErrorStart>.Final node, Func<TErrorStart, TErrorEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new QueryResult<TValue, TErrorEnd>.Final();
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TErrorEnd> Dispatch(QueryResult<TValue, TErrorStart>.Element node, Func<TErrorStart, TErrorEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new ErrorResult<TValue, TErrorStart, TErrorEnd>(node, context);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TErrorEnd> Dispatch(QueryResult<TValue, TErrorStart>.Partial node, Func<TErrorStart, TErrorEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new QueryResult<TValue, TErrorEnd>.Partial(context(node.Error));
            }
        }

        public static Fx.QueryContext.IQueryResult<TValue, TErrorEnd> ErrorSelect<TValue, TErrorStart, TErrorEnd>(this IQueryResult<TValue, TErrorStart> queryResult, Func<TErrorStart, TErrorEnd> selector)
        {
            return new ErrorSelectQueryResult<TValue, TErrorEnd>(queryResult.Nodes.ErrorSelectIterator(selector));
        }

        private static Fx.QueryContext.IQueryResultNode<TValue, TErrorEnd> ErrorSelectIterator<TValue, TErrorStart, TErrorEnd>(this IQueryResultNode<TValue, TErrorStart> queryResult, Func<TErrorStart, TErrorEnd> selector)
        {
            return queryResult
                .Select(
                    element => new ErrorSelectElement<TValue, TErrorStart, TErrorEnd>(element, selector),
                    terminal => terminal.SelectLeft(error => new ErrorSelectError<TErrorEnd>(selector(error.Value))))
                .ToQueryResultNode();
        }

        private sealed class ErrorSelectElement<TValue, TErrorStart, TErrorEnd> : IElement<TValue, TErrorEnd>
        {
            private readonly IElement<TValue, TErrorStart> element;
            private readonly Func<TErrorStart, TErrorEnd> selector;

            public ErrorSelectElement(IElement<TValue, TErrorStart> element, Func<TErrorStart, TErrorEnd> selector)
            {
                this.element = element;
                this.selector = selector;
            }

            public TValue Value
            {
                get
                {
                    return this.element.Value;
                }
            }

            public IQueryResultNode<TValue, TErrorEnd> Next()
            {
                return this.element.Next().ErrorSelectIterator(this.selector);
            }
        }

        private sealed class ErrorSelectError<TErrorEnd> : IError<TErrorEnd>
        {
            public ErrorSelectError(TErrorEnd value)
            {
                Value = value;
            }

            public TErrorEnd Value { get; }
        }

        private sealed class ErrorSelectQueryResult<TValue, TErrorEnd> : Fx.QueryContext.IQueryResult<TValue, TErrorEnd>
        {
            public ErrorSelectQueryResult(IQueryResultNode<TValue, TErrorEnd> nodes)
            {
                Nodes = nodes;
            }

            public IQueryResultNode<TValue, TErrorEnd> Nodes { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TErrorStart"></typeparam>
        /// <typeparam name="TErrorEnd"></typeparam>
        /// <param name="queryResult"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
        public static QueryResult<TValue, TErrorEnd> ErrorSelect<TValue, TErrorStart, TErrorEnd>(this QueryResult<TValue, TErrorStart> queryResult, Func<TErrorStart, TErrorEnd> selector)
        {
            //// TODO do you like this method name? do you want to normalize with names used in `either`?

            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return ErrorVisitor<TValue, TErrorStart, TErrorEnd>.Instance.Visit(queryResult, selector);
        }

        public static _OfType<TValueStart, TError> OfType<TValueStart, TError>(this QueryResult<TValueStart, TError> queryResult)
        {
            return new _OfType<TValueStart, TError>(queryResult);
        }

        public sealed class _OfType<TValueStart, TError>
        {
            private readonly QueryResult<TValueStart, TError> queryResult;

            public _OfType(QueryResult<TValueStart, TError> queryResult)
            {
                this.queryResult = queryResult;
            }

            public QueryResult<TValueEnd, TError> Invoke<TValueEnd>()
            {
                //// TODO is there anything you can do to make this more smooth?
                return OfTypeVisitor<TValueStart, TError, TValueEnd>.Instance.Visit(this.queryResult, default);
            }
        }

        private sealed class OfTypeVisitor<TValueStart, TError, TValueEnd> : QueryResult<TValueStart, TError>.Visitor<QueryResult<TValueEnd, TError>, Nothing>
        {
            private OfTypeVisitor()
            {
            }

            public static OfTypeVisitor<TValueStart, TError, TValueEnd> Instance { get; } = new OfTypeVisitor<TValueStart, TError, TValueEnd>();

            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Final node, Nothing context)
            {
                return new QueryResult<TValueEnd, TError>.Final();
            }

            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Element node, Nothing context)
            {
                if (node.Value is TValueEnd valueEnd)
                {
                    return new OfTypeResult<TValueStart, TError, TValueEnd>(node, valueEnd);
                }
                else
                {
                    return this.Visit(node.Next(), context);
                }
            }

            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Partial node, Nothing context)
            {
                return new QueryResult<TValueEnd, TError>.Partial(node.Error);
            }
        }

        private sealed class OfTypeResult<TValueStart, TError, TValueEnd> : QueryResult<TValueEnd, TError>.Element
        {
            private readonly QueryResult<TValueStart, TError>.Element queryResult;

            public OfTypeResult(QueryResult<TValueStart, TError>.Element queryResult, TValueEnd valueEnd)
                : base(valueEnd)
            {
                this.queryResult = queryResult;
            }

            /// <inheritdoc/>
            public override QueryResult<TValueEnd, TError> Next()
            {
                return OfTypeVisitor<TValueStart, TError, TValueEnd>.Instance.Visit(this.queryResult.Next(), default);
            }
        }

        public static IEither<TValue, TError> First<TValue, TError>(this QueryResult<TValue, TError> queryResult)
        {
            return FirstVisitor<TValue, TError>.Instance.Visit(queryResult, default);
        }

        private sealed class FirstVisitor<TValue, TError> : QueryResult<TValue, TError>.Visitor<IEither<TValue, TError>, Nothing>
        {
            private FirstVisitor()
            {
            }

            public static FirstVisitor<TValue, TError> Instance { get; } = new FirstVisitor<TValue, TError>();

            public override IEither<TValue, TError> Dispatch(QueryResult<TValue, TError>.Final node, Nothing context)
            {
                throw new InvalidOperationException("TODO");
            }

            public override IEither<TValue, TError> Dispatch(QueryResult<TValue, TError>.Element node, Nothing context)
            {
                return Either.Left(node.Value).Right<TError>();
            }

            public override IEither<TValue, TError> Dispatch(QueryResult<TValue, TError>.Partial node, Nothing context)
            {
                return Either.Left<TValue>().Right(node.Error);
            }
        }

        private sealed class WhereResult<TValue, TError> : QueryResult<TValue, TError>.Element
        {
            private readonly Element queryResult;
            private readonly Func<TValue, bool> predicate;

            public WhereResult(QueryResult<TValue, TError>.Element queryResult, Func<TValue, bool> predicate)
                : base(queryResult.Value)
            {
                this.queryResult = queryResult;
                this.predicate = predicate;
            }

            /// <inheritdoc/>
            public sealed override QueryResult<TValue, TError> Next()
            {
                return WhereVisitor<TValue, TError>.Instance.Visit(this.queryResult.Next(), this.predicate);
            }
        }

        private sealed class WhereVisitor<TValue, TError> : QueryResult<TValue, TError>.Visitor<QueryResult<TValue, TError>, Func<TValue, bool>>
        {
            private WhereVisitor()
            {
            }

            public static WhereVisitor<TValue, TError> Instance { get; } = new WhereVisitor<TValue, TError>();

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Final node, Func<TValue, bool> context)
            {
                return node;
            }

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Element node, Func<TValue, bool> context)
            {
                if (context(node.Value))
                {
                    return new WhereResult<TValue, TError>(node, context);
                }
                else
                {
                    return this.Visit(node.Next(), context);
                }
            }

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Partial node, Func<TValue, bool> context)
            {
                return node;
            }
        }

        public static QueryResult<TValue, TError> Where<TValue, TError>(this QueryResult<TValue, TError> queryResult, Func<TValue, bool> predicate)
        {
            return WhereVisitor<TValue, TError>.Instance.Visit(queryResult, predicate);
        }

        private sealed class ToQueryResultResult<TValue, TError> : QueryResult<TValue, TError>.Element
        {
            private readonly IEnumerator<TValue> enumerator;

            public ToQueryResultResult(IEnumerator<TValue> enumerator)
                : base(enumerator.Current)
            {
                //// TODO make queryresult.final disposable?
                this.enumerator = enumerator;
            }

            /// <inheritdoc/>
            public override QueryResult<TValue, TError> Next()
            {
                if (!this.enumerator.MoveNext())
                {
                    return new QueryResult<TValue, TError>.Final();
                }

                //// TODO is this ok? it might not be properly immutable this way
                return new ToQueryResultResult<TValue, TError>(this.enumerator);
            }
        }

        public static QueryResult<TValue, TError> ToQueryResult<TValue, TError>(this IEnumerable<TValue> enumerable)
        {
            IEnumerator<TValue>? enumerator = null;
            try
            {
                enumerator = enumerable.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return new QueryResult<TValue, TError>.Final();
                }

                return new ToQueryResultResult<TValue, TError>(enumerator);
            }
            catch
            {
                enumerator?.Dispose();
                throw;
            }
        }

        private sealed class ConcatResult<TValue, TError> : QueryResult<TValue, TError>.Element
        {
            private readonly QueryResult<TValue, TError>.Element element;
            private readonly QueryResult<TValue, TError> second;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="element"></param>
            /// <param name="second"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> or <paramref name="second"/> is <see langword="null"/></exception>
            public ConcatResult(QueryResult<TValue, TError>.Element element, QueryResult<TValue, TError> second)
                : base((element ?? throw new ArgumentNullException(nameof(element))).Value)
            {
                ArgumentNullException.ThrowIfNull(second);

                this.element = element;
                this.second = second;
            }

            /// <inheritdoc/>
            public override QueryResult<TValue, TError> Next()
            {
                return ConcatVisitor<TValue, TError>.Instance.Visit(this.element.Next(), second);
            }
        }

        private sealed class ConcatVisitor<TValue, TError> : QueryResult<TValue, TError>.Visitor<QueryResult<TValue, TError>, QueryResult<TValue, TError>>
        {
            /// <summary>
            /// 
            /// </summary>
            private ConcatVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static ConcatVisitor<TValue, TError> Instance { get; } = new ConcatVisitor<TValue, TError>();

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Final node, QueryResult<TValue, TError> context)
            {
                ArgumentNullException.ThrowIfNull(node);
                ArgumentNullException.ThrowIfNull(context);

                return context;
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Element node, QueryResult<TValue, TError> context)
            {
                ArgumentNullException.ThrowIfNull(node);
                ArgumentNullException.ThrowIfNull(context);

                return new ConcatResult<TValue, TError>(node, context);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Partial node, QueryResult<TValue, TError> context)
            {
                ArgumentNullException.ThrowIfNull(node);
                ArgumentNullException.ThrowIfNull(context);

                //// TODO what if there was in error in first *and* second?
                return node;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="first"/> or <paramref name="second"/> is <see langword="null"/></exception>
        public static QueryResult<TValue, TError> Concat<TValue, TError>(this QueryResult<TValue, TError> first, QueryResult<TValue, TError> second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            return ConcatVisitor<TValue, TError>.Instance.Visit(first, second);
        }

        private sealed class SelectResult<TValueStart, TValueEnd, TError> : QueryResult<TValueEnd, TError>.Element
        {
            private readonly QueryResult<TValueStart, TError>.Element queryResult;
            private readonly Func<TValueStart, TValueEnd> selector;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="queryResult"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
            public SelectResult(QueryResult<TValueStart, TError>.Element queryResult, Func<TValueStart, TValueEnd> selector)
                : base((selector ?? throw new ArgumentNullException(nameof(selector)))((queryResult ?? throw new ArgumentNullException(nameof(queryResult))).Value))
            {
                this.queryResult = queryResult;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public override QueryResult<TValueEnd, TError> Next()
            {
                return SelectVisitor<TValueStart, TValueEnd, TError>.Instance.Visit(this.queryResult.Next(), this.selector);
            }
        }

        private sealed class SelectVisitor<TValueStart, TValueEnd, TError> : QueryResult<TValueStart, TError>.Visitor<QueryResult<TValueEnd, TError>, Func<TValueStart, TValueEnd>>
        {
            /// <summary>
            /// 
            /// </summary>
            private SelectVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static SelectVisitor<TValueStart, TValueEnd, TError> Instance { get; } = new SelectVisitor<TValueStart, TValueEnd, TError>();

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Final node, Func<TValueStart, TValueEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new QueryResult<TValueEnd, TError>.Final();
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Element node, Func<TValueStart, TValueEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new SelectResult<TValueStart, TValueEnd, TError>(node, context);
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
            public override QueryResult<TValueEnd, TError> Dispatch(QueryResult<TValueStart, TError>.Partial node, Func<TValueStart, TValueEnd> context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                return new QueryResult<TValueEnd, TError>.Partial(node.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValueStart"></typeparam>
        /// <typeparam name="TValueEnd"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="queryResult"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryResult"/> or <paramref name="selector"/> is <see langword="null"/></exception>
        public static QueryResult<TValueEnd, TError> Select<TValueStart, TValueEnd, TError>(this QueryResult<TValueStart, TError> queryResult, Func<TValueStart, TValueEnd> selector)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return SelectVisitor<TValueStart, TValueEnd, TError>.Instance.Visit(queryResult, selector);
        }

        private sealed class DistinctByResult<TValue, TError, TKey> : QueryResult<TValue, TError>.Element
        {
            private readonly Element queryResult;
            private readonly DistinctByContext<TValue, TKey> context;

            public DistinctByResult(QueryResult<TValue, TError>.Element queryResult, DistinctByContext<TValue, TKey> context)
                : base(queryResult.Value)
            {
                this.queryResult = queryResult;
                this.context = context;
            }

            /// <inheritdoc/>
            public override QueryResult<TValue, TError> Next()
            {
                return DistinctByVisitor<TValue, TError, TKey>.Instance.Visit(this.queryResult.Next(), this.context);
            }
        }

        private sealed class DistinctByContext<TValue, TKey>
        {
            public DistinctByContext(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
            {
                this.KeySelector = keySelector;
                this.Comparer = comparer;
            }

            public Func<TValue, TKey> KeySelector { get; }

            public IEqualityComparer<TKey> Comparer { get; }
        }

        private sealed class DistinctByVisitor<TValue, TError, TKey> : QueryResult<TValue, TError>.Visitor<QueryResult<TValue, TError>, DistinctByContext<TValue, TKey>>
        {
            private DistinctByVisitor()
            {
            }

            public static DistinctByVisitor<TValue, TError, TKey> Instance { get; } = new DistinctByVisitor<TValue, TError, TKey>();

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Final node, DistinctByContext<TValue, TKey> context)
            {
                return node;
            }

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Element node, DistinctByContext<TValue, TKey> context)
            {
                throw new NotImplementedException();
            }

            public override QueryResult<TValue, TError> Dispatch(QueryResult<TValue, TError>.Partial node, DistinctByContext<TValue, TKey> context)
            {
                return node;
            }
        }

        public static QueryResult<TValue, TError> DistinctBy<TValue, TError, TKey>(this QueryResult<TValue, TError> queryResult, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var context = new DistinctByContext<TValue, TKey>(keySelector, comparer);
            return DistinctByVisitor<TValue, TError, TKey>.Instance.Visit(queryResult, context);
        }
    }
}
