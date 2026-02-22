namespace OddTrotter.TodoListService
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.CalendarEventsContext;

    internal class CalendarTodoListErrors
    {
        public CalendarTodoListErrors( //// TODO this whole type is wrong, look at the todo list service to find what the *types* are that you can service; you shouldn't lose overall fidelity (i.e. the UI should still have text that has useful and readable messages), but still not leak abstractions //// TODO this might mean that `todolistservice` is *not* "abstract" and just directly takes an instance of (the non-graph) `calendareventscontext` so that it knows directly what to do for each error
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

        public string? BrokenNextLink { get; }

        public IEnumerable<CalendarEvent> EventsWithoutStarts { get; }

        public IEnumerable<(CalendarEvent, Exception)> EventsWithStartParseFailures { get; }

        public IEnumerable<CalendarEvent> EventsWithoutBodies { get; }

        public IEnumerable<(CalendarEvent, Exception)> EventsWithBodyParseFailures { get; }
    }
}
