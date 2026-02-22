namespace OddTrotter.TodoListService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using OddTrotter.CalendarEventsContext;

    internal sealed class TodoListService<TCalendarEventsContext> : ITodoListService<CalendarTodoListErrors>
        where TCalendarEventsContext : 
            IQueryContext<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, PagingException>,
            IWhereQueryContextMixin<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, PagingException, CalendarEventsContext>
    {
        private readonly TCalendarEventsContext calendarEventsContext;

        public TodoListService(TCalendarEventsContext calendarEventsContext)
        {
            this.calendarEventsContext = calendarEventsContext;
        }

        public Task<TodoListResult<CalendarTodoListErrors>> Retrieve()
        {
            var todoListEvents = this
                .calendarEventsContext
                .Where(calendarEvent => calendarEvent.Start < DateTime.UtcNow)
                .Where(calendarEvent => calendarEvent.IsCancelled == false)
                .Where(calendarEvent => calendarEvent.Subject == "todo list");

            throw new Exception("TODO");
        }

        private sealed class TodoListResultBuilder
        {
            public TodoListResultBuilder(DateTime lastRecordedEventTimeStamp)
            {
                this.TodoList = new StringBuilder();
                this.TranslationErrors = new List<CalendarEventTranslationException>();
                this.BodyParseErrors = new List<Exception>();
                this.EndTimestamp = lastRecordedEventTimeStamp;
            }

            public StringBuilder TodoList { get; set; }

            public DateTime EndTimestamp { get; set; }

            public List<CalendarEventTranslationException> TranslationErrors { get; set; }

            public List<Exception> BodyParseErrors { get; set; } //// TODO use the right tpye of elements

            public PagingException? PagingError { get; set; }
        }
    }
}
