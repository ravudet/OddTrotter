namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    internal sealed class PagingException : Exception
    {
        public PagingException(string message)
            : base(message)
        {
        }

        public PagingException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
