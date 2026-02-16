namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    internal sealed class CalendarEvent
    {
        public CalendarEvent(string id, string subject, BodyStructure body, TimeStructure start, bool isCancelled, string type, TimeStructure end)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(body);

            this.Id = id;
            this.Subject = subject;
            this.Body = body;
            this.Start = start;
            this.IsCancelled = isCancelled;
            Type = type;
            End = end;
        }

        public string Id { get; }

        public string Subject { get; }

        public TimeStructure Start { get; set; }

        public BodyStructure Body { get; set; }

        public bool IsCancelled { get; }

        public string Type { get; }

        public TimeStructure End { get; set; }
    }

    internal sealed class BodyStructure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> is <see langword="null"/></exception>
        public BodyStructure(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.Content = content;
        }

        public string Content { get; set; }
    }

    internal sealed class TimeStructure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZone"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dateTime"/> or <paramref name="timeZone"/> is <see langword="null"/></exception>
        public TimeStructure(DateTime dateTime, string timeZone)
        {
            if (timeZone == null)
            {
                throw new ArgumentNullException(nameof(timeZone));
            }

            this.DateTime = dateTime;
            this.TimeZone = timeZone;
        }

        public DateTime DateTime { get; set; }

        public string TimeZone { get; set; }
    }
}
