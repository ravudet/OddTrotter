namespace OddTrotter.CalendarEventsContext
{
    using System;

    internal sealed class ContextException : Exception
    {
        public ContextException(string message)
            : base(message)
        {
        }

        public ContextException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
