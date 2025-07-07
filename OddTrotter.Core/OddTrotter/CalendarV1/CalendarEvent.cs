/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace OddTrotter.Calendar
{
    using System;

    public sealed class CalendarEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="start"></param>
        /// <param name="isCancelled"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="id"/> or <paramref name="subject"/> or <paramref name="body"/> is <see langword="null"/>
        /// </xception>
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

            Foo.Bar.DoWork();
        }

        public string Id { get; }

        public string Subject { get; }

        public string Body { get; }

        public DateTimeOffset Start { get; }

        public bool IsCancelled { get; }
    }
}
