namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    internal sealed class WriteException : Exception
    {
        public WriteException(string message)
            : base(message)
        {
        }

        public WriteException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
