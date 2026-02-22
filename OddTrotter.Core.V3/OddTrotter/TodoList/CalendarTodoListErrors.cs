namespace OddTrotter.TodoListService
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.CalendarEventsContext;

    internal class CalendarTodoListErrors
    {
        public CalendarTodoListErrors(
            string? brokenNextLink,
            IEnumerable<CalendarEvent> eventsWithoutStarts,
            IEnumerable<(CalendarEvent, Exception)> eventsWithStartParseFailures,
            IEnumerable<CalendarEvent> eventsWithoutBodies,
            IEnumerable<(CalendarEvent, Exception)> eventsWithBodyParseFailures)
        {
            if (eventsWithoutStarts == null)
            {
                throw new ArgumentNullException(nameof(eventsWithoutStarts));
            }

            if (eventsWithStartParseFailures == null)
            {
                throw new ArgumentNullException(nameof(eventsWithStartParseFailures));
            }

            if (eventsWithoutBodies == null)
            {
                throw new ArgumentNullException(nameof(eventsWithoutBodies));
            }

            if (eventsWithBodyParseFailures == null)
            {
                throw new ArgumentNullException(nameof(eventsWithBodyParseFailures));
            }

            this.BrokenNextLink = brokenNextLink;
            EventsWithoutStarts = eventsWithoutStarts;
            EventsWithStartParseFailures = eventsWithStartParseFailures;
            EventsWithoutBodies = eventsWithoutBodies;
            EventsWithBodyParseFailures = eventsWithBodyParseFailures;
        }

        /// <summary>
        /// Gets the URI of one of three values:
        /// 1. <see langword="null"/> if no errors occurred retrieve any of the data
        /// 2. The URL of series master entity for which an error occurred while retrieving the instance events
        /// 3. The URL of the nextLink for which an error occurred while retrieving the that URL's page
        /// </summary>
        public string? BrokenNextLink { get; }

        public IEnumerable<CalendarEvent> EventsWithoutStarts { get; }

        public IEnumerable<(CalendarEvent, Exception)> EventsWithStartParseFailures { get; }

        public IEnumerable<CalendarEvent> EventsWithoutBodies { get; }

        public IEnumerable<(CalendarEvent, Exception)> EventsWithBodyParseFailures { get; }
    }
}
