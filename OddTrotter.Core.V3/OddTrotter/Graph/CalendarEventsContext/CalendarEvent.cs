namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    internal sealed class CalendarEvent
    {
        public CalendarEvent(string id, string subject, string body, DateTimeOffset start, bool isCancelled)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(body);

            this.Id = id;
            this.Subject = subject;
            this.Body = body;
            this.Start = start;
            this.IsCancelled = isCancelled;
        }

        public string Id { get; }

        public string Subject { get; }

        public string Body { get; }

        public DateTimeOffset Start { get; }

        public bool IsCancelled { get; }
    }
}
