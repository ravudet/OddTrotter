namespace OddTrotter.Graph.CalendarEventsSource
{
    using OddTrotter.Graph.CalendarEventsContext;

    internal interface ICalendarEventsSource<TGetContext>
        where TGetContext : ICalendarEventsContext<TGetContext>
    {
        TGetContext GenerateGetRequest();
    }
}
