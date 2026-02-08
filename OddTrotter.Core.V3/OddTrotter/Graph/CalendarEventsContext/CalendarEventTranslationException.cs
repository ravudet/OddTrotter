namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    internal sealed class CalendarEventTranslationException : Exception
    {
        public CalendarEventTranslationException(string message)
            : base(message)
        {
        }

        public CalendarEventTranslationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
