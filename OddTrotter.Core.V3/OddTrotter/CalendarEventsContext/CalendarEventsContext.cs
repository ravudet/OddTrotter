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

        public ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            throw new System.NotImplementedException();
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
