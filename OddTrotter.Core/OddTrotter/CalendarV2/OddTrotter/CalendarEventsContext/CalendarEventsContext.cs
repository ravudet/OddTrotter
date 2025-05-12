namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;
    using OddTrotter.Calendar;

    public sealed class CalendarEventsContext : 
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
                        CalendarEventsContextTranslationException
                    >,
                CalendarEvent,
                CalendarEventsContextPagingException,
                CalendarEventsContext
            >
    {
        private CalendarEventsContext()
        {
        }

        public ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            throw new System.NotImplementedException();
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        ITask<IQueryResult<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException>> IQueryContext<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEvent, CalendarEventsContextPagingException>.Evaluate()
        {
            throw new NotImplementedException();
        }
    }
}
