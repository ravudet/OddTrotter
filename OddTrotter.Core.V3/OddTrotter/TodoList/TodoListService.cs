namespace OddTrotter.TodoListService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public async Task<TodoListResult<CalendarTodoListErrors>> Retrieve()
        {
            var todoListEvents = await this
                .calendarEventsContext
                .Where(calendarEvent => calendarEvent.Start < DateTime.UtcNow)
                .Where(calendarEvent => calendarEvent.IsCancelled == false)
                .Where(calendarEvent => calendarEvent.Subject == "todo list")
                .Evaluate()
                .ConfigureAwait(false);

            var lastRecordedEventTimeStamp = DateTime.UtcNow; //// TODO use the correct timestamp

            var builder = await Convert(todoListEvents, lastRecordedEventTimeStamp).ConfigureAwait(false);

            var todoList = new TodoList(
                builder.TodoList.ToString(),
                lastRecordedEventTimeStamp,
                builder.EndTimestamp);
            var errors = new CalendarTodoListErrors(
                builder.PagingError?.ToString(), //// TODO is an exception string really expected here?
                Enumerable.Empty<CalendarEvent>(), //// TODO
                Enumerable.Empty<(CalendarEvent, Exception)>(), //// TODO
                Enumerable.Empty<CalendarEvent>(), //// TODO
                Enumerable.Empty<(CalendarEvent, Exception)>() //// TODO
                );
            var result = new TodoListResult<CalendarTodoListErrors>()
        }

        private static async Task<TodoListResultBuilder> Convert(IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException> queryResult, DateTime lastRecordedEventTimeStamp)
        {
            var builder = new TodoListResultBuilder(lastRecordedEventTimeStamp);

            ConvertIterator(queryResult.Nodes, builder);

            return builder;
        }

        private static void ConvertIterator(IQueryResultNode<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException> queryResultNode, TodoListResultBuilder builder)
        {
            bool @continue;
            while (((queryResultNode, @continue) = ConvertApply(queryResultNode, builder)).@continue)
            {
            }
        }

        private static (IQueryResultNode<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>, bool) ConvertApply(IQueryResultNode<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException> queryResultNode, TodoListResultBuilder builder)
        {
            return queryResultNode.Apply(
                element =>
                {
                    element.Value.Apply(
                        (left, context) =>
                        {
                            if (left.Start < context.EndTimestamp)
                            {
                                context.EndTimestamp = left.Start.DateTime; //// TODO why did you choose datetimeoffset some places and datetime others?
                            }

                            IEnumerable<string> parsedBody;
                            try
                            {
                                parsedBody = ParseEventBody(left.Body);
                            }
                            catch (Exception exception)
                            {
                                context.BodyParseErrors.Add(exception);
                                return new Nothing();
                            }

                            context.TodoList.AppendJoin(Environment.NewLine, parsedBody).AppendLine();
                            return new Nothing();
                        },
                        (right, context) =>
                        {
                            //// TODO do you want to stop recording the endtimestamp if a translation error occurred, or should the user be expected to handle it at that point? if the user is expected to handle it, it'd probably be good to put the errors in a more permanent storage so that a browser window mishap doesn't cause data loss
                            //// TODO i think you should let the user handle it because otherwise a calendar error will get them permanently stuck at a certain timestamp and they will need to actually go to the calendar event and fix it, instead of just checking that the error can be skipped and ignoring it, letting the next refresh remove it; you *will* want a way to persist the errors though for the browser mishap reason
                            context.TranslationErrors.Add(right);
                            return new Nothing();
                        },
                        builder);

                    return (element.Next(), true); //// TODO something is very wrong, because at some point `next` will need to get the next *page* and make a network call, but there's no task being used...
                },
                terminal =>
                {
                    if (terminal.TryGetLeft(out var error))
                    {
                        builder.PagingError = error.Value;
                    }

                    return (null!, false);
                });
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
