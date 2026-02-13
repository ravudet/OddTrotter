namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using Graph = OddTrotter.Graph.CalendarEventsContext;

    internal sealed class CalendarEventsContext : 
        IQueryContext
            <
                IEither
                    <
                        CalendarEvent, 
                        CalendarEventTranslationException
                    >, 
                CalendarEvent, 
                PagingException
            >, 
        IWhereQueryContextMixin
            <
                IEither
                    <
                        CalendarEvent,
                        CalendarEventTranslationException
                    >,
                CalendarEvent,
                PagingException,
                CalendarEventsContext
            >
    {
        private readonly Graph.ICalendarEventsContext calendarEventsContext;

        internal CalendarEventsContext(Graph.ICalendarEventsContext calendarEventsContext)
        {
            this.calendarEventsContext = calendarEventsContext;
        }

        public async ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            //// TODO you should be combining instance and series events here
            var queryResult = await this.calendarEventsContext.Evaluate().ConfigureAwait(false);
            return queryResult
                .Select(element => element
                    .SelectRight(translationException => new CalendarEventTranslationException("TODO", translationException))
                    .SelectLeft(calendarEvent => CalendarEventsContext.Translate(calendarEvent))
                    .SelectManyLeft())
                .SelectError(pagingException => new PagingException("TODO", pagingException));
        }

        private static IEither<CalendarEvent, CalendarEventTranslationException> Translate(Graph.CalendarEvent calendarEvent)
        {
            DateTimeOffset start;
            try
            {
                start = DateTimeOffset.Parse(calendarEvent.Start.DateTime);
            }
            catch (Exception exception)
            {
                return Either.Left<CalendarEvent>().Right(new CalendarEventTranslationException("tODO", exception));
            }

            return Either
                .Right<CalendarEventTranslationException>()
                .Left(
                    new CalendarEvent(
                        calendarEvent.Id,
                        calendarEvent.Subject, 
                        calendarEvent.Body.Content,
                        start, 
                        calendarEvent.IsCancelled));
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
