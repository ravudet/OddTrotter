namespace OddTrotter.Graph.CalendarEventsSource
{
    using OddTrotter.Graph.CalendarEventsContext;

    internal interface ICalendarEventsSource
    {
        ICalendarEventsContext GenerateGetRequest();
    }

    internal interface ICalendarEventsContext : ICalendarEventsContext<ICalendarEventsContext>
    {
    }
}
