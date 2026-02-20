namespace OddTrotter.TodoList
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using OddTrotter.CalendarEventsContext;

    internal sealed class TodoList<TCalendarEventsContext> : ITodoList<TodoListErrors>
        where TCalendarEventsContext : IQueryContext<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, PagingException>,
        IWhereQueryContextMixin<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, PagingException, CalendarEventsContext>
    {
        private readonly TCalendarEventsContext calendarEventsContext;

        public TodoList(TCalendarEventsContext calendarEventsContext)
        {
            this.calendarEventsContext = calendarEventsContext;
        }

        public Task<(TodoListResult, TodoListErrors)> Retrieve()
        {
            var todoListEvents = this
                .calendarEventsContext
                .Where(calendarEvent => calendarEvent.Start < DateTime.UtcNow)
                .Where(calendarEvent => calendarEvent.IsCancelled == false)
                .Where(calendarEvent => calendarEvent.Subject == "todo list");


        }
    }
}
