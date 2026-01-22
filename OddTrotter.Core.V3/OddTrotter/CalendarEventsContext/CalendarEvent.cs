namespace OddTrotter.CalendarEventsContext
{
    using System;

    public sealed class CalendarEvent
    {
        private CalendarEvent()
        {
        }

        public DateTimeOffset Start { get; }
    }
}
