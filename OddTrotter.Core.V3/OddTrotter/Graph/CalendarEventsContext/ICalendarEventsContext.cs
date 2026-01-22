namespace OddTrotter.Graph.CalendarEventsContext
{
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    internal interface ICalendarEventsContext
    {
        Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate();
    }
}
