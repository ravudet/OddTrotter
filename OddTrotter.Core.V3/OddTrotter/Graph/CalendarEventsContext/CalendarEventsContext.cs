namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.Realizable;

    using OddTrotter.Calendar;
    using OddTrotter.CalendarEventsContext;

    using static System.Runtime.InteropServices.JavaScript.JSType;

    using StrongConventionContext = OddTrotter.Odata.v4_01.StrongConventionContext;

    internal sealed class CalendarEventsContext : 
        ICalendarEventsContext<CalendarEventsContext>, 
        ICalendarEventsContext
    {
        private readonly StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri calendarRoot;

        private readonly string? filter;
        private readonly string? orderBy;
        private readonly string? top;

        internal CalendarEventsContext(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri calendarRoot)
            : this(strongConventionContext, calendarRoot, null, null, null)
        {
        }

        private CalendarEventsContext(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri calendarRoot,
            string? filter,
            string? orderBy,
            string? top)
        {
            this.strongConventionContext = strongConventionContext;
            this.calendarRoot = calendarRoot;

            this.filter = filter;
            this.orderBy = orderBy;
            this.top = top;
        }

        public async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingError>> Evaluate()
        {
            return await EvaluatePage(this.strongConventionContext, this.calendarRoot, true).ConfigureAwait(false);


            //// TODO should this be a query result, or should this just do the query parameters thing, and let the layer above do the query result? //// TODO i like the ecision you've made here; it makes the interfaces map the csdl, which doesn't refer at all to paging and just views things as collections of elements
        }

        private static async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingError>> EvaluatePage(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri uri, 
            bool throwOnFailureResponse)
        {
            var getCollectionRequest = new StrongConventionContext.GetCollectionRequest<CalendarEvent>(
                uri,
                Enumerable.Empty<HttpHeader>());
                    /*
                    internal interface IGraphVersion : IStrongConventionContext
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        /// <inheritdoc cref="ICalendarEventsContext.Evaluate"/>
        /// <exception cref="System.IO.IOException"></exception>
        ... GetCollection();
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    }
                    */
                
            StrongConventionContext.GetCollectionResponse<CalendarEvent> getCollectionResponse;
            try
            {
                getCollectionResponse = await strongConventionContext.GetCollection(getCollectionRequest).ConfigureAwait(false);
            }
            catch (HttpRequestException httpRequestException)
            {
                if (throwOnFailureResponse)
                {
                    throw;
                }
                else
                {
                    return Enumerable
                        .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                        .ToQueryResultAsync()
                        .SelectError(_ => new PagingError.Http(uri, httpRequestException));
                }
            }
            catch (StrongConventionContext.ReadException readException)
            {
                var exception = new ReadException("TODO", readException);
                if (throwOnFailureResponse)
                {
                    throw exception;
                }
                else
                {
                    return Enumerable
                        .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                        .ToQueryResultAsync()
                        .SelectError(_ => new PagingError.Read(uri, exception));
                }
            }
            catch (StrongConventionContext.WriteException writeException)
            {
                var exception = new WriteException("TODO", writeException);
                if (throwOnFailureResponse)
                {
                    throw exception;
                }
                else
                {
                    return Enumerable
                        .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                        .ToQueryResultAsync()
                        .SelectError(_ => new PagingError.Write(uri, exception));
                }
            }
            catch (StrongConventionContext.StrongConventionException strongConventionException)
            {
                var exception = new ContextException("TODO", strongConventionException);
                if (throwOnFailureResponse)
                {
                    throw exception;
                }
                else
                {
                    return Enumerable
                        .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                        .ToQueryResultAsync()
                        .SelectError(_ => new PagingError.Context(uri, exception));
                }
            }

            return getCollectionResponse.Apply(
                success =>
                {
                    var graphCalendarEvents = success
                        .Elements
                        .Select(element => element
                            .Element
                            .SelectRight(deserializationError =>
                                new CalendarEventTranslationException("TODO include the odata object too, but not as 'odata', probably just a string", deserializationError.Exception)))
                        .ToQueryResultAsync()
                        .SelectError<PagingError>();

                    if (success.NextLink != null)
                    {
                        graphCalendarEvents = graphCalendarEvents.Concat(
                            EvaluatePage(strongConventionContext, new Uri(success.NextLink), false),
                            firstError => firstError,
                            secondError => secondError,
                            (firstError, secondError) => throw new Exception("TODO we should at this point know that we didn't receive an error for the first sequence..."));
                    }

                    return graphCalendarEvents;
                },
                failure =>
                {
                    if (false) //// TODO 501 or 503
                    {
#pragma warning disable CS0162 // Unreachable code detected
                        var exception = new UnauthorizedAccessTokenException("TODO", "TODO", "TODO");
#pragma warning restore CS0162 // Unreachable code detected
                        if (throwOnFailureResponse)
                        {
                            throw exception;
                        }
                        else
                        {
                            return Enumerable
                                .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                                .ToQueryResultAsync()
                                .SelectError(_ => new PagingError.Unauthorized(uri, exception));
                        }
                    }
                    else
                    {
                        var exception = new ContextException("TODO");
                        if (throwOnFailureResponse)
                        {
                            throw exception;
                        }
                        else
                        {
                            return Enumerable
                                .Empty<IEither<CalendarEvent, CalendarEventTranslationException>>()
                                .ToQueryResultAsync()
                                .SelectError(_ => new PagingError.Context(uri, exception));
                        }
                    }
                });
        }

        public CalendarEventsContext Filter(Expression<Func<CalendarEvent, bool>> filter)
        {
            string? filterExpression = null;
            if (filter == TypeEqualsSingleInstance)
            {
                filterExpression = "type eq 'singleInstance'";
            }
            else if (filter == IsCancelled)
            {
                filterExpression = "isCancelled eq true";
            }
            else if (filter == IsNotCancelled)
            {
                filterExpression = "isCancelled eq false";
            }
            else if (filter.Parameters.Count == 1)
            {
                var parameterName = filter.Parameters[0].Name;
                if (parameterName != null)
                {
                    if (parameterName.StartsWith(nameof(StartTimeGreaterThan)))
                    {
                        if (long.TryParse(parameterName.Substring(nameof(StartTimeGreaterThan).Length), out var startTimeTicks))
                        {
                            var startTime = new DateTime(startTimeTicks);
                            filterExpression = $"start/dateTime gt '{startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.000000")}'"; //// TODO does touniversal time mess up if you're already in utc?
                        }
                    }
                    else if (parameterName.StartsWith(nameof(EndTimeLessThan)))
                    {
                        if (long.TryParse(parameterName.Substring(nameof(EndTimeLessThan).Length), out var endTimeTicks))
                        {
                            var endTime = new DateTime(endTimeTicks);
                            filterExpression = $"end/dateTime lt '{endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.000000")}'";
                        }
                    }
                }
            }

            if (filterExpression == null)
            {
                throw new NotImplementedException("TODO");
            }

            if (this.filter == null)
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    filterExpression,
                    this.orderBy,
                    this.top);
            }
            else
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    this.filter + " and " + filterExpression,
                    this.orderBy,
                    this.top);
            }
        }

        public CalendarEventsContext Top(uint top)
        {
            if (this.top != null)
            {
                throw new Exception("TODO invalidoperationexception");
            }

            return new CalendarEventsContext(
                this.strongConventionContext,
                this.calendarRoot,
                this.filter,
                this.orderBy,
                top.ToString());
        }

        public CalendarEventsContext OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy)
        {
            string orderByExpression;
            if (orderBy is Expression<Func<CalendarEvent, DateTime>> asString && asString == StartTime)
            {
                orderByExpression = "start/dateTime";
            }
            else
            {
                throw new NotImplementedException("TODO");
            }

            if (this.orderBy == null)
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    this.filter,
                    orderByExpression,
                    this.top);
            }
            else
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    this.filter,
                    this.orderBy + "," + orderByExpression,
                    this.top);
            }
        }

        internal static Expression<Func<CalendarEvent, bool>> TypeEqualsSingleInstance { get; } = calendarEvent => true; //// TODO how should you handle the fact that `calendarEvent/type` won't get selected? it still needs to be a property on `graphcalendarevent` so that you can write this expression

        internal static Expression<Func<CalendarEvent, bool>> StartTimeGreaterThan(DateTime dateTime)
        {
            Expression<Func<CalendarEvent, bool>> foo = calendarEvent => true;
            var ticks = Expression.Parameter(typeof(CalendarEvent), nameof(StartTimeGreaterThan) + dateTime.Ticks.ToString());
            foo.Update(foo.Body, new[] { ticks });

            return foo;
        }

        internal static Expression<Func<CalendarEvent, bool>> EndTimeLessThan(DateTime dateTime)
        {
            Expression<Func<CalendarEvent, bool>> foo = calendarEvent => true;
            var ticks = Expression.Parameter(typeof(CalendarEvent), nameof(EndTimeLessThan) + dateTime.Ticks.ToString());
            foo.Update(foo.Body, new[] { ticks });

            return foo;
        }

        ICalendarEventsContext ICalendarEventsContext<ICalendarEventsContext>.Filter(Expression<Func<CalendarEvent, bool>> filter)
        {
            return Filter(filter);
        }

        ICalendarEventsContext ICalendarEventsContext<ICalendarEventsContext>.Top(uint top)
        {
            return Top(top);
        }

        ICalendarEventsContext ICalendarEventsContext<ICalendarEventsContext>.OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy)
        {
            return OrderBy(orderBy);
        }

        internal static Expression<Func<CalendarEvent, bool>> IsCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == true;

        internal static Expression<Func<CalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == false;

        internal static Expression<Func<CalendarEvent, DateTime>> StartTime { get; } = calendarEvent => calendarEvent.Start.DateTime;
    }

    internal static class Extensions2
    {
        internal static QueryResultAsync<TElement, Nothing> ToQueryResultAsync<TElement>(
            this IEnumerable<TElement> enumerable)
        {
            return new QueryResultAsync<TElement, Nothing>(enumerable);
        }

        internal sealed class QueryResultAsync<TElement, TException> : IQueryResult<TElement, TException>
        {
            private readonly IEnumerable<TElement> enumerable;

            public QueryResultAsync(IEnumerable<TElement> enumerable)
            {
                this.enumerable = enumerable;
            }

            public IQueryResult<TElement, TException2> SelectError<TException2>()
            {
                return new QueryResultAsync<TElement, TException2>(this.enumerable);
            }

            public async ITask<IQueryResultNode<TElement, TException>> GetNodes()
            {
                return await Task.FromResult(new QueryResultNode(this.enumerable.GetEnumerator())).ConfigureAwait(false); //// TODO disposable
            }

            private sealed class QueryResultNode : IQueryResultNode<TElement, TException>
            {
                private readonly IEnumerator<TElement> enumerator;

                public QueryResultNode(IEnumerator<TElement> enumerator)
                {
                    this.enumerator = enumerator;
                }

                public Fx.Realizable.Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TElement, TException>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TException>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    RefEither<IElement<TElement, TException>, IEither<IError<TException>, IEmpty>> either;
                    if (this.enumerator.MoveNext())
                    {
                        either = RefEither.Right<IEither<IError<TException>, IEmpty>>().Left((IElement<TElement, TException>)new Element(this.enumerator.Current, this.enumerator)); //// TODO shouldn't need the cast
                    }
                    else
                    {
                        either = RefEither.Left<IElement<TElement, TException>>().Right((IEither<IError<TException>, IEmpty>)Either.Left<IError<TException>>().Right(Empty.Instance)); //// TODO shouldn't need the cast
                    }

                    return either.ApplyAsync(leftMap, rightMap, ref context);
                }

                private sealed class Element : IElement<TElement, TException>
                {
                    private readonly IEnumerator<TElement> enumerator;

                    public Element(TElement value, IEnumerator<TElement> enumerator)
                    {
                        Value = value;
                        this.enumerator = enumerator;
                    }

                    public TElement Value { get; }

                    public async ITask<IQueryResultNode<TElement, TException>> Next()
                    {
                        return await Task.FromResult(new QueryResultNode(this.enumerator)).ConfigureAwait(false);
                    }
                }

                private sealed class Empty : IEmpty
                {
                    private Empty()
                    {
                    }

                    public static Empty Instance { get; } = new Empty();
                }
            }
        }

        internal static bool TryGetRight<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TRight right)
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return !either.Decompose(out _, out right);
        }

        internal static IQueryResult<TElement, TErrorResult> Concat<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResult<TElement, TErrorFirst> queryResult,
            IQueryResult<TElement, TErrorSecond> next,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            return new ConcatQueryResult<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
                queryResult,
                Task.FromResult(next), //// TODO do better
                firstErrorSelector,
                secondErrorSelector,
                errorAggregator);
        }

        internal static async ITask<IQueryResult<TElement, TErrorResult>> Concat<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
            this ITask<IQueryResult<TElement, TErrorFirst>> queryResult,
            Task<IQueryResult<TElement, TErrorSecond>> next,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            return new ConcatQueryResult<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
                await queryResult.ConfigureAwait(false),
                next,
                firstErrorSelector,
                secondErrorSelector,
                errorAggregator);
        }

        internal static IQueryResult<TElement, TErrorResult> Concat<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResult<TElement, TErrorFirst> queryResult,
            Task<IQueryResult<TElement, TErrorSecond>> next,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            return new ConcatQueryResult<TElement, TErrorFirst, TErrorSecond, TErrorResult>(
                queryResult, 
                next, 
                firstErrorSelector,
                secondErrorSelector,
                errorAggregator);
        }

        private sealed class ConcatQueryResult<TElement, TErrorFirst, TErrorSecond, TErrorResult> : IQueryResult<TElement, TErrorResult>
        {
            private readonly IQueryResult<TElement, TErrorFirst> queryResult;
            private readonly Task<IQueryResult<TElement, TErrorSecond>> next;
            private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
            private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
            private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

            public ConcatQueryResult(
                IQueryResult<TElement, TErrorFirst> queryResult,
                Task<IQueryResult<TElement, TErrorSecond>> next,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
            {
                this.queryResult = queryResult;
                this.next = next;
                this.firstErrorSelector = firstErrorSelector;
                this.secondErrorSelector = secondErrorSelector;
                this.errorAggregator = errorAggregator;
            }

            public async ITask<IQueryResultNode<TElement, TErrorResult>> GetNodes()
            {
                return await Node.Create(
                    await this.queryResult.GetNodes().ConfigureAwait(false), 
                    this.next, 
                    this.firstErrorSelector,
                    this.secondErrorSelector,
                    this.errorAggregator).ConfigureAwait(false);
            }

            private sealed class Node : IQueryResultNode<TElement, TErrorResult>
            {
                public static async ITask<IQueryResultNode<TElement, TErrorResult>> Create(
                    IQueryResultNode<TElement, TErrorFirst> queryResult,
                    Task<IQueryResult<TElement, TErrorSecond>> next,
                    Func<TErrorFirst, TErrorResult> firstErrorSelector,
                    Func<TErrorSecond, TErrorResult> secondErrorSelector,
                    Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                {
                    if (queryResult.TryGetRight(out var terminal))
                    {
                        var firstError = new BetterNullable<TErrorFirst>();
                        if (terminal.TryGetLeft(out var error))
                        {
                            firstError = new BetterNullable<TErrorFirst>(error.Value);
                        }

                        return new NextNode(
                            firstError,
                            await (await next.ConfigureAwait(false)).GetNodes().ConfigureAwait(false), 
                            firstErrorSelector,
                            secondErrorSelector,
                            errorAggregator);
                    }
                    else
                    {
                        return new Node(
                            queryResult, 
                            next, 
                            firstErrorSelector,
                            secondErrorSelector,
                            errorAggregator);
                    }
                }

                private readonly IQueryResultNode<TElement, TErrorFirst> queryResult;
                private readonly Task<IQueryResult<TElement, TErrorSecond>> next;
                private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
                private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
                private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

                private Node(
                    IQueryResultNode<TElement, TErrorFirst> queryResult,
                    Task<IQueryResult<TElement, TErrorSecond>> next,
                    Func<TErrorFirst, TErrorResult> firstErrorSelector,
                    Func<TErrorSecond, TErrorResult> secondErrorSelector,
                    Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                {
                    this.queryResult = queryResult;
                    this.next = next;
                    this.firstErrorSelector = firstErrorSelector;
                    this.secondErrorSelector = secondErrorSelector;
                    this.errorAggregator = errorAggregator;
                }

                public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TElement, TErrorResult>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TErrorResult>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    return this.queryResult.ApplyAsync<TResult, TContext, Realizable<TResult>>(
                        (element, ref context) =>
                        {
                            return leftMap(new Element(element, this.next, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator), ref context).ContinueWith(_ => _, _ => throw _, _ => throw _);
                        },
                        (terminal, ref context) =>
                        {
                            throw new Exception("TODO you really shouldn't get here because the terminal node is handled in the instantiating caller");
                        },
                        ref context);
                }

                private sealed class Element : IElement<TElement, TErrorResult>
                {
                    private readonly IElement<TElement, TErrorFirst> element;
                    private readonly Task<IQueryResult<TElement, TErrorSecond>> next;
                    private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
                    private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
                    private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

                    public Element(
                        IElement<TElement, TErrorFirst> element,
                        Task<IQueryResult<TElement, TErrorSecond>> next,
                        Func<TErrorFirst, TErrorResult> firstErrorSelector,
                        Func<TErrorSecond, TErrorResult> secondErrorSelector,
                        Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                    {
                        this.element = element;
                        this.next = next;
                        this.firstErrorSelector = firstErrorSelector;
                        this.secondErrorSelector = secondErrorSelector;
                        this.errorAggregator = errorAggregator;
                    }

                    public TElement Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    public async ITask<IQueryResultNode<TElement, TErrorResult>> Next()
                    {
                        return await Node.Create(
                            await this.element.Next().ConfigureAwait(false), 
                            this.next, 
                            this.firstErrorSelector,
                            this.secondErrorSelector,
                            this.errorAggregator).ConfigureAwait(false);
                    }
                }
            }

            private sealed class NextNode : IQueryResultNode<TElement, TErrorResult>
            {
                private readonly BetterNullable<TErrorFirst> firstError;
                private readonly IQueryResultNode<TElement, TErrorSecond> next;
                private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
                private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
                private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

                public NextNode(
                    BetterNullable<TErrorFirst> firstError,
                    IQueryResultNode<TElement, TErrorSecond> next,
                    Func<TErrorFirst, TErrorResult> firstErrorSelector,
                    Func<TErrorSecond, TErrorResult> secondErrorSelector,
                    Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                {
                    this.firstError = firstError;
                    this.next = next;
                    this.firstErrorSelector = firstErrorSelector;
                    this.secondErrorSelector = secondErrorSelector;
                    this.errorAggregator = errorAggregator;
                }

                public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TElement, TErrorResult>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TErrorResult>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    return this.next.ApplyAsync<TResult, TContext, TContinuable>(
                        (element, ref context) =>
                        {
                            return leftMap(new Element(element, this.firstError, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator), ref context);
                        },
                        (terminal, ref context) =>
                        {
                            return rightMap(new Terminal(terminal, this.firstError, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator), ref context);
                        },
                        ref context);
                }

                private sealed class Terminal : IEither<IError<TErrorResult>, IEmpty>
                {
                    private readonly IEither<IError<TErrorSecond>, IEmpty> terminal;
                    private readonly BetterNullable<TErrorFirst> firstError;
                    private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
                    private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
                    private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

                    public Terminal(
                        IEither<IError<TErrorSecond>, IEmpty> terminal,
                        BetterNullable<TErrorFirst> firstError,
                        Func<TErrorFirst, TErrorResult> firstErrorSelector,
                        Func<TErrorSecond, TErrorResult> secondErrorSelector,
                        Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                    {
                        this.terminal = terminal;
                        this.firstError = firstError;
                        this.firstErrorSelector = firstErrorSelector;
                        this.secondErrorSelector = secondErrorSelector;
                        this.errorAggregator = errorAggregator;
                    }

                    public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IError<TErrorResult>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEmpty, TContext, TContinuable, TResult> rightMap, ref TContext context)
                        where TResult : allows ref struct
                        where TContext : allows ref struct
                        where TContinuable : IContinuable<TResult>, allows ref struct
                    {
                        return this.terminal.ApplyAsync<TResult, TContext, TContinuable>(
                            (error, ref context) =>
                            {
                                TErrorResult resultError;
                                if (this.firstError.TryGetValue(out var firstError))
                                {
                                    resultError = this.errorAggregator(firstError, error.Value);
                                }
                                else
                                {
                                    resultError = this.secondErrorSelector(error.Value);
                                }

                                return leftMap(new Error(resultError), ref context);
                            },
                            (empty, ref context) =>
                            {
                                if (this.firstError.TryGetValue(out var firstError))
                                {
                                    return leftMap(
                                        new Error(this.firstErrorSelector(firstError)),
                                        ref context);
                                }
                                else
                                {
                                    return rightMap(empty, ref context);
                                }
                            },
                            ref context);
                    }

                    private sealed class Error : IError<TErrorResult>
                    {
                        public Error(TErrorResult value)
                        {
                            Value = value;
                        }

                        public TErrorResult Value { get; }
                    }
                }

                private sealed class Element : IElement<TElement, TErrorResult>
                {
                    private readonly IElement<TElement, TErrorSecond> element;
                    private readonly BetterNullable<TErrorFirst> firstError;
                    private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
                    private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
                    private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

                    public Element(
                        IElement<TElement, TErrorSecond> element,
                        BetterNullable<TErrorFirst> firstError,
                        Func<TErrorFirst, TErrorResult> firstErrorSelector,
                        Func<TErrorSecond, TErrorResult> secondErrorSelector,
                        Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
                    {
                        this.element = element;
                        this.firstError = firstError;
                        this.firstErrorSelector = firstErrorSelector;
                        this.secondErrorSelector = secondErrorSelector;
                        this.errorAggregator = errorAggregator;
                    }

                    public TElement Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    public async ITask<IQueryResultNode<TElement, TErrorResult>> Next()
                    {
                        return new NextNode(this.firstError, await this.element.Next().ConfigureAwait(false), this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator);
                    }
                }
            }
        }
    }
}
