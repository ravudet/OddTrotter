namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Schema;

    using Fx.Either;
    using Fx.QueryContext;

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
                }
            }

            private sealed class QueryResultNode : IQueryResultNode<TElement, TException>
            {
                private readonly IEnumerable<TElement> enumerable;

                public QueryResultNode(IEnumerable<TElement> enumerable)
                {
                    this.enumerable = enumerable;
                }

                public Fx.Realizable.Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TElement, TException>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TException>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    throw new NotImplementedException();
                }
            }
        }

        internal static IQueryResult<TElement, TException> Concat<TElement, TException>(
            this IQueryResult<TElement, TException> queryResult,
            Task<IQueryResult<TElement, TException>> next)
        {

        }
    }
}
