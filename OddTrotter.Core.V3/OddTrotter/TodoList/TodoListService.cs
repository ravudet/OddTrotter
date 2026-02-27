namespace OddTrotter.TodoListService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using OddTrotter.CalendarEventsContext;

    using static Fx.Either.EitherExtensions;

    internal sealed class TodoListService : ITodoListService<CalendarTodoListErrors>
    {
        private readonly CalendarEventsContext calendarEventsContext;

        public TodoListService(CalendarEventsContext calendarEventsContext)
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
            var result = new TodoListResult<CalendarTodoListErrors>(
                todoList,
                errors);

            return result;
        }

        private static async Task<TodoListResultBuilder> Convert(IQueryResultAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.CalendarEventsContext.PagingError> queryResult, DateTime lastRecordedEventTimeStamp)
        {
            var builder = new TodoListResultBuilder(lastRecordedEventTimeStamp);

            await ConvertIterator(await queryResult.GetNodes().ConfigureAwait(false), builder).ConfigureAwait(false);

            return builder;
        }

        private static async Task ConvertIterator(IQueryResultNodeAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.CalendarEventsContext.PagingError> queryResultNode, TodoListResultBuilder builder)
        {
            bool @continue;
            while (((queryResultNode, @continue) = await ConvertApply(queryResultNode, builder).ConfigureAwait(false)).@continue)
            {
            }
        }

        private static async Task<(IQueryResultNodeAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.CalendarEventsContext.PagingError>, bool)> ConvertApply(IQueryResultNodeAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.CalendarEventsContext.PagingError> queryResultNode, TodoListResultBuilder builder)
        {
            return await queryResultNode.Apply(
                async element =>
                {
                    element.Value.Apply(
                        (left, ref context) =>
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
                        (right, ref context) =>
                        {
                            //// TODO do you want to stop recording the endtimestamp if a translation error occurred, or should the user be expected to handle it at that point? if the user is expected to handle it, it'd probably be good to put the errors in a more permanent storage so that a browser window mishap doesn't cause data loss
                            //// TODO i think you should let the user handle it because otherwise a calendar error will get them permanently stuck at a certain timestamp and they will need to actually go to the calendar event and fix it, instead of just checking that the error can be skipped and ignoring it, letting the next refresh remove it; you *will* want a way to persist the errors though for the browser mishap reason
                            context.TranslationErrors.Add(right);
                            return new Nothing();
                        },
                        ref builder);

                    return (await element.Next().ConfigureAwait(false), true);
                },
                async terminal =>
                {
                    if (terminal.TryGetLeft(out var error))
                    {
                        builder.PagingError = error.Value;
                    }

                    return await Task.FromResult(((IQueryResultNodeAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.CalendarEventsContext.PagingError>)null!, false)).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }

        private static IEnumerable<string> ParseEventBody(string body)
        {
            //// TODO do you need to document anything here?
            body = body.Replace("&nbsp;", string.Empty);

            // the calendar api returns html bodies that are malformed xml; the head element contains a meta element that doesn't close
            var bodyElement = "<body>";
            var bodyCloseElement = "</body>";
            body = $"<html>{body.Substring(0, body.IndexOf(bodyCloseElement)).Substring(body.IndexOf(bodyElement) + bodyElement.Length)}</html>";

            var document = XDocument.Parse(body);
            var links = document.Descendants("a").Reverse();
            foreach (var link in links)
            {
                link.ReplaceWith(link.Value);
            }

            return document.Descendants("p").Select(element => element.Value);
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

            public Graph.CalendarEventsContext.PagingError? PagingError { get; set; }
        }
    }

    internal static partial class Extensions
    {
        internal static TResult Apply<TLeft, TRight, TContext, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            if (either.Decompose(out var left, out var right)) //// TODO you shouldn't need `decompose` for this
            {
                return leftMap(left, context);
            }
            else
            {
                return rightMap(right, context);
            }
        }
    }
}
