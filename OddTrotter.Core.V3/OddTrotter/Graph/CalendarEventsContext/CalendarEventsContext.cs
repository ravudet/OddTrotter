namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    using OddTrotter.Calendar;
    using OddTrotter.Odata.v4_01.StrongConventionContext;

    internal sealed class CalendarEventsContext : ICalendarEventsContext
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri uri;

        internal CalendarEventsContext(IStrongConventionContext<CalendarEvent> strongConventionContext, Uri uri)
        {
            this.strongConventionContext = strongConventionContext;
            this.uri = uri;
        }

        public async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            var getCollectionRequest = new GetCollectionRequest<CalendarEvent>(this.uri.ToString(), System.Linq.Enumerable.Empty<HttpHeader>()); //// TODO add access token

            var getCollectionResponse = await this.strongConventionContext.GetCollection(getCollectionRequest).ConfigureAwait(false);
        }
    }
}
