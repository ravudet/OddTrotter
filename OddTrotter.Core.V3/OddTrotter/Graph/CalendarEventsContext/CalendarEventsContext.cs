namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.Realizable;

    using OddTrotter.Calendar;

    using StrongConventionContext = OddTrotter.Odata.v4_01.StrongConventionContext;


    internal interface ICollectionContext<out TElement, out TTranslationException, out TPagingException>
    {
        ITask<IQueryResult<IEither<TElement, TTranslationException>, TPagingException>> Evaluate();
    }
    
    /// <summary>
    /// TODO does this implementation still work when query parameters are added to the interface?
    /// TODO plus, does the interface even make sense? not every collection supports every query parameter, and that's not *really* known until you make the network call
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TTranslationException"></typeparam>
    /// <typeparam name="TPagingException"></typeparam>
    internal sealed class CollectionContext<TElement, TTranslationException, TPagingException> : ICollectionContext<TElement, TTranslationException, TPagingException>
    {
        private readonly StrongConventionContext.IStrongConventionContext<TElement> strongConventionContext;
        private readonly Uri calendarRoot;
        private readonly string accessToken;
        private readonly Func<string, Exception, TTranslationException> translationExceptionFactory;

        internal CollectionContext(
            StrongConventionContext.IStrongConventionContext<TElement> strongConventionContext,
            Uri calendarRoot,
            string accessToken,
            Func<string, Exception, TTranslationException> translationExceptionFactory)
        {
            this.strongConventionContext = strongConventionContext;
            this.calendarRoot = calendarRoot;
            this.accessToken = accessToken;
            this.translationExceptionFactory = translationExceptionFactory;
        }

        public async ITask<IQueryResult<IEither<TElement, TTranslationException>, TPagingException>> Evaluate()
        {
            return await this.EvaluatePage(this.strongConventionContext, this.calendarRoot, this.accessToken, true).ConfigureAwait(false);


            //// TODO should this be a query result, or should this just do the query parameters thing, and let the layer above do the query result?

        }

        private async Task<IQueryResult<IEither<TElement, TTranslationException>, TPagingException>> EvaluatePage(
            StrongConventionContext.IStrongConventionContext<TElement> strongConventionContext,
            Uri uri,
            string accessToken,
            bool throwOnFailureResponse)
        {
            var getCollectionRequest = new StrongConventionContext.GetCollectionRequest<TElement>(
                uri.ToString(),
                new[]
                {
                    new HttpHeader("Authorization", accessToken), //// TODO i think you need an exception specifically for this, you can do something like this to accomplish that:
                    /*
                    internal interface IGraphVersion : ICalendarEventsContext
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        /// <inheritdoc cref="ICalendarEventsContext.Evaluate"/>
        /// <exception cref="System.IO.IOException"></exception>
        Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate();
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    }
                    */
                });
            StrongConventionContext.GetCollectionResponse<TElement> getCollectionResponse;
            try
            {
                getCollectionResponse = await strongConventionContext.GetCollection(getCollectionRequest).ConfigureAwait(false);
            }
            catch (StrongConventionContext.ReadException readException)
            {
                throw new ReadException("TODO", readException);
            }
            catch (StrongConventionContext.WriteException writeException)
            {
                throw new WriteException("TODO", writeException);
            }
            catch (StrongConventionContext.StrongConventionException strongConventionException)
            {
                throw new ContextException("TODO", strongConventionException);
            }

            return getCollectionResponse.Apply(
                success =>
                {
                    var graphCalendarEvents = success
                        .Elements
                        .Select(element => element
                            .Element
                            .SelectRight(deserializationError =>
                                 this.translationExceptionFactory("TODO", deserializationError.Exception)))
                        .ToQueryResult<IEither<TElement, TTranslationException>, TPagingException>(); //// TODO bad type inference

                    if (success.NextLink != null)
                    {
                        graphCalendarEvents = graphCalendarEvents.Concat(EvaluatePage(strongConventionContext, new Uri(success.NextLink), accessToken, false));
                    }

                    return graphCalendarEvents;
                },
                failure =>
                {
                    if (throwOnFailureResponse)
                    {
                        throw new ContextException("TODO");
                    }
                    else
                    {
                        return Enumerable.Empty<IEither<TElement, TTranslationException>>().ToQueryResult<IEither<TElement, TTranslationException>, TPagingException>(); //// TODO bad type inference //// TODO put the failure in there
                    }
                });
        }
    }


    internal sealed class CalendarEventsContext : ICalendarEventsContext<CalendarEventsContext>
    {
        private readonly StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri calendarRoot;
        private readonly string accessToken;

        private readonly string? filter;
        private readonly string? orderBy;
        private readonly string? top;

        internal CalendarEventsContext(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri calendarRoot,
            string accessToken)
            : this(strongConventionContext, calendarRoot, accessToken, null, null, null)
        {
        }

        private CalendarEventsContext(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri calendarRoot,
            string accessToken,
            string? filter,
            string? orderBy,
            string? top)
        {
            this.strongConventionContext = strongConventionContext;
            this.calendarRoot = calendarRoot;
            this.accessToken = accessToken; //// TODO access token really should be baked into `strongConventionContext`, especially to abstract things like token expiration

            this.filter = filter;
            this.orderBy = orderBy;
            this.top = top;
        }

        public async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            return await EvaluatePage(this.strongConventionContext, this.calendarRoot, this.accessToken, true).ConfigureAwait(false);


            //// TODO should this be a query result, or should this just do the query parameters thing, and let the layer above do the query result?
        }

        private static async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> EvaluatePage(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri uri, 
            string accessToken,
            bool throwOnFailureResponse)
        {
            var getCollectionRequest = new StrongConventionContext.GetCollectionRequest<CalendarEvent>(
                uri.ToString(),
                new[]
                {
                    new HttpHeader("Authorization", accessToken), //// TODO i think you need an exception specifically for this, you can do something like this to accomplish that:
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
                });
            StrongConventionContext.GetCollectionResponse<CalendarEvent> getCollectionResponse;
            try
            {
                getCollectionResponse = await strongConventionContext.GetCollection(getCollectionRequest).ConfigureAwait(false);
            }
            catch (StrongConventionContext.ReadException readException)
            {
                throw new ReadException("TODO", readException);
            }
            catch (StrongConventionContext.WriteException writeException)
            {
                throw new WriteException("TODO", writeException);
            }
            catch (StrongConventionContext.StrongConventionException strongConventionException)
            {
                throw new ContextException("TODO", strongConventionException);
            }

            return getCollectionResponse.Apply(
                success =>
                {
                    var graphCalendarEvents = success
                        .Elements
                        .Select(element => element
                            .Element
                            .SelectRight(deserializationError =>
                                new CalendarEventTranslationException("TODO", deserializationError.Exception)))
                        .ToQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>(); //// TODO bad type inference

                    if (success.NextLink != null)
                    {
                        graphCalendarEvents = graphCalendarEvents.Concat(EvaluatePage(strongConventionContext, new Uri(success.NextLink), accessToken, false));
                    }

                    return graphCalendarEvents;
                },
                failure =>
                {
                    if (throwOnFailureResponse)
                    {
                        throw new ContextException("TODO");
                    }
                    else
                    {
                        return Enumerable.Empty<IEither<CalendarEvent, CalendarEventTranslationException>>().ToQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>(); //// TODO bad type inference //// TODO put the failure in there
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
                    this.accessToken,
                    filterExpression,
                    this.orderBy,
                    this.top);
            }
            else
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    this.accessToken,
                    this.filter + " and " + filterExpression,
                    this.orderBy,
                    this.top);
            }
        }

        public CalendarEventsContext Top(int top)
        {
            if (this.top != null)
            {
                throw new Exception("TODO invalidoperationexception");
            }

            return new CalendarEventsContext(
                this.strongConventionContext,
                this.calendarRoot,
                this.accessToken,
                this.filter,
                this.orderBy,
                top.ToString());
        }

        public CalendarEventsContext OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy)
        {
            string orderByExpression;
            if (orderBy is Expression<Func<CalendarEvent, string>> asString && asString == StartTime)
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
                    this.accessToken,
                    this.filter,
                    orderByExpression,
                    this.top);
            }
            else
            {
                return new CalendarEventsContext(
                    this.strongConventionContext,
                    this.calendarRoot,
                    this.accessToken,
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

        internal static Expression<Func<CalendarEvent, bool>> IsCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == true;

        internal static Expression<Func<CalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == false;

        internal static Expression<Func<CalendarEvent, string>> StartTime { get; } = calendarEvent => calendarEvent.Start.DateTime;
    }

    internal static class Extensions2
    {
        internal static IQueryResult<TElement, TException> ToQueryResult<TElement, TException>(
            this IEnumerable<TElement> enumerable)
        {
            return new QueryResult<TElement, TException>(enumerable);
        }

        private sealed class QueryResult<TElement, TException> : IQueryResult<TElement, TException>
        {
            private readonly IEnumerable<TElement> enumerable;

            public QueryResult(IEnumerable<TElement> enumerable)
            {
                this.enumerable = enumerable;
            }

            public IQueryResultNode<TElement, TException> Nodes
            {
                get
                {
                    return new QueryResultNode(this.enumerable.GetEnumerator()); //// TODO disposable
                }
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

                    public IQueryResultNode<TElement, TException> Next()
                    {
                        return new QueryResultNode(this.enumerator);
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

        internal static IQueryResult<TElement, TException> Concat<TElement, TException>(
            this IQueryResult<TElement, TException> queryResult,
            Task<IQueryResult<TElement, TException>> next)
        {
            //// TODO in a previous iteration, you took several overloads to aggregate the possible terminal errors; but you *could* have one aggregator that takes two `optional<texception>` parameters
            return new ConcatQueryResult<TElement, TException>(queryResult, next);
        }

        private sealed class ConcatQueryResult<TElement, TException> : IQueryResult<TElement, TException>
        {
            private readonly IQueryResult<TElement, TException> queryResult;
            private readonly Task<IQueryResult<TElement, TException>> next;

            public ConcatQueryResult(
                IQueryResult<TElement, TException> queryResult,
                Task<IQueryResult<TElement, TException>> next)
            {
                this.queryResult = queryResult;
                this.next = next;
            }

            public IQueryResultNode<TElement, TException> Nodes
            {
                get
                {
                    return new QueryResultNode(this.queryResult.Nodes, this.next);
                }
            }

            private sealed class QueryResultNode : IQueryResultNode<TElement, TException>
            {
                private readonly IQueryResultNode<TElement, TException> queryResultNode;
                private readonly Task<IQueryResult<TElement, TException>> next;

                public QueryResultNode(
                    IQueryResultNode<TElement, TException> queryResultNode,
                    Task<IQueryResult<TElement, TException>> next)
                {
                    this.queryResultNode = queryResultNode;
                    this.next = next;
                }

                public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TElement, TException>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TException>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    var realizable = queryResultNode
                        .ApplyAsync<TContinuable, TContext, Realizable<TContinuable>>( //// TODO why doesn't type inference work?
                            (element, ref context) => Realizable.FromResult(leftMap(new FirstElement(element, this.next), ref context)),
                            (terminal, ref context) =>
                            {
                                return this
                                    .next
                                    .ToTaskWrapper() //// TODO how to handle `configureawait`?
                                    .ContinueWith(
                                        nextQueryResult =>
                                        {
                                            unsafe
                                            {
                                                var fakeContext = default(TContext)!;
                                                ref TContext toPass = ref Unsafe.AsRef(ref fakeContext); //// TODO use the real context here...

                                                //// TODO you shouldn't need to decompose
                                                if (nextQueryResult.Nodes.Decompose(out var left, out var right))
                                                {
                                                    return leftMap(left, ref toPass);
                                                }
                                                else
                                                {
                                                    return rightMap(right, ref toPass);
                                                }
                                            }
                                        },
                                        _ => throw _,
                                        _ => throw _);
                            },
                            ref context);

                    //// TODO it really feels like a tcontinuable should be adaptable to a realizable as an extension or something
                    return realizable
                        .ContinueWith(
                            continuable => continuable.ContinueWith(_ => _, _ => throw _, _ => throw _),
                            _ => throw _,
                            _ => throw _)
                        .Unwrap();
                }

                private sealed class FirstElement : IElement<TElement, TException>
                {
                    private readonly IElement<TElement, TException> element;
                    private readonly Task<IQueryResult<TElement, TException>> next;

                    public FirstElement(
                        IElement<TElement, TException> element,
                        Task<IQueryResult<TElement, TException>> next)
                    {
                        this.element = element;
                        this.next = next;
                    }

                    public TElement Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    public IQueryResultNode<TElement, TException> Next()
                    {
                        //// TODO you don't account for `this.element.next` returning an error
                        return new QueryResultNode(this.element.Next(), this.next);
                    }
                }
            }
        }
    }
}
