namespace OddTrotter.Graph.CalendarEventsSource
{
    using OddTrotter.Graph.CalendarEventsContext;

    internal sealed class CalendarEventsSource : ICalendarEventsSource<CalendarEventsContext>
    {
        private readonly CalendarEventsContext calendarEventsContext;

        internal CalendarEventsSource(
            CalendarEventsContext calendarEventsContext)
        {
            this.calendarEventsContext = calendarEventsContext;
        }

        public CalendarEventsContext GenerateGetRequest()
        {
            return this.calendarEventsContext;
        }
    }
}
