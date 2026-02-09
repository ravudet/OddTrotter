namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.Realizable;

    using OddTrotter.Calendar;

    using StrongConventionContext = OddTrotter.Odata.v4_01.StrongConventionContext;

    internal sealed class CalendarEventsContext : ICalendarEventsContext
    {
        private readonly StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri uri;
        private readonly string accessToken;

        internal CalendarEventsContext(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri uri,
            string accessToken)
        {
            this.strongConventionContext = strongConventionContext;
            this.uri = uri;
            this.accessToken = accessToken;
        }

        public async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            return await EvaluatePage(this.strongConventionContext, this.uri, this.accessToken, true);


            //// TODO should this be a query result, or should this just do the query parameters thing, and let the layer above do the query result?


            //// TODO do you want to try making the interface more general by using type parameters?
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
                    new HttpHeader("Authorization", accessToken),
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
                        return Enumerable.Empty<IEither<CalendarEvent, CalendarEventTranslationException>>().ToQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>(); //// TODO bad type inference
                    }
                });
        }
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
                    return queryResultNode
                        .ApplyAsync<TResult, TContext, TContinuable>( //// TODO why doesn't type inference work?
                            (element, ref context) => leftMap(new FirstElement(element, this.next), ref context),
                            (terminal, ref context) => rightMap(terminal, ref context), //// TODO you need to continue by traversing `next`
                            ref context);
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

                private sealed class SecondElement : IElement<TElement, TException>
                {
                    public SecondElement()
                }
            }
        }
    }
}
