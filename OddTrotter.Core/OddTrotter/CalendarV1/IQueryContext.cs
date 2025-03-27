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
    using System.Linq;

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

        //// TODO do you really want a tryselect overload that pretends ieithers are trys

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


        //// TODO in `select`, for convenience, you have a `select` overload that *does* use a task queryresult, but *doesn't* use a task selector; that's not really relevenat for first; do you still want the "convenience method"?
        //// TODO actually, you seem to have two dimensions: is `this` a task + is the `func` a task? and you seem to want (for convenience) all 4 variations



        public static async Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<IQueryResult<TSource, TError>> queryResult,
            Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(queryResult);
            ArgumentNullException.ThrowIfNull(selector);

            return (await queryResult.ConfigureAwait(false)).Select(selector);
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
    }

   
    public static class QueryResultExtensions
    {




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
            //// TODO do you like this method name? do you want to normalize with names used in `either`?
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




        //// TODO you previously had an `oftype` extension that went unused, likely because it had the type inference problem; maybe your `type<T>` solution can work here?







        //// TODO you are here




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
