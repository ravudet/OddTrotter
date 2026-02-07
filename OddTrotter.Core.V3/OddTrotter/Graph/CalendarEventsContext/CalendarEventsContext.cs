namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Threading.Tasks;

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
            var getCollectionRequest = new StrongConventionContext.GetCollectionRequest<CalendarEvent>(
                this.uri.ToString(),
                new[]
                {
                    new HttpHeader("Authorization", this.accessToken),
                });
            StrongConventionContext.GetCollectionResponse<CalendarEvent> getCollectionResponse;
            try
            {
                getCollectionResponse = await this.strongConventionContext.GetCollection(getCollectionRequest).ConfigureAwait(false);
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

            //// TODO do you want to try making the interface more general?
        }

        private static async Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> EvaluatePage(
            StrongConventionContext.IStrongConventionContext<CalendarEvent> strongConventionContext, 
            Uri uri, 
            string accessToken)
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

            //// TODO do you want to try making the interface more general?
        }
    }
}
