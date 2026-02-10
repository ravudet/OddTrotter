namespace OddTrotter.CalendarEventsContext
{
    using System;

    internal sealed class ReadException : Exception
    {
        public ReadException(string message)
            : base(message)
        {
        }

        public ReadException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
