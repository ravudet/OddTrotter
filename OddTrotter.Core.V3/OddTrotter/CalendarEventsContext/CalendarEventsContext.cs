namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using Graph = OddTrotter.Graph.CalendarEventsContext;

    internal sealed class CalendarEventsContext : 
        IQueryContext
            <
                IEither
                    <
                        CalendarEvent, 
                        CalendarEventTranslationException
                    >, 
                CalendarEvent, 
                PagingException
            >, 
        IWhereQueryContextMixin
            <
                IEither
                    <
                        CalendarEvent,
                        CalendarEventTranslationException
                    >,
                CalendarEvent,
                PagingException,
                CalendarEventsContext
            >
    {
        private readonly Graph.ICalendarSource calendarSource;
        private readonly DateTime startTime;
        private readonly uint pageSize; //// TODO add settings
        private readonly bool? isCancelled; //// TODO implement `where`
        private readonly DateTime? endTime; //// TODO implement `where`

        internal CalendarEventsContext(Graph.ICalendarSource calendarSource, DateTime startTime)
        {
            this.calendarSource = calendarSource;
            this.startTime = startTime;
        }

        public async ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            var events = await this.GetEvents().ConfigureAwait(false);
            return Translate(events);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetEvents()
        {
            var instanceEvents = await this.GetInstanceEvents().ConfigureAwait(false);
            var seriesEvents = await this.GetSeriesEvents().ConfigureAwait(false);

            return instanceEvents.Concat(
                seriesEvents,
                firstError => firstError,
                secondError => secondError,
                (firstError, secondError) =>
                    new Graph.PagingException(
                            "TODO an error occurred while paging both instances events and series events",
                            new AggregateException(firstError, secondError)));
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstanceEvents()
        {
            var context = this
                .calendarSource
                .Events()
                .Get()
                .Filter(calendarEvent => calendarEvent.Type == "singleInstance")
                .Filter(calendarEvent => calendarEvent.Start.DateTime > this.startTime) //// TODO i can't decide if `timestructure.datetime` should be a string and we should call `this.startTime.ToString()` here, or if `timestructure.datetime` is supposed to be a datetime; look at the csdl probably...
                .Top(this.pageSize)
                .OrderBy(calendarEvent => calendarEvent.Start.DateTime);

            if (this.endTime != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.End.DateTime < this.endTime.Value);
            }

            if (this.isCancelled.HasValue)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetSeriesEvents()
        {
            var seriesEventMasters = await this.GetSeriesEventMasters().ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstancesInSeries()
        {
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstancesInSeriesWithinTimeSlice()
        {
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetSeriesEventMasters()
        {
            var context = this
                .calendarSource
                .Events()
                .Get()
                .Filter(calendarEvent => calendarEvent.Type == "seriesMaster")
                .OrderBy(calendarEvent => calendarEvent.Start.DateTime)
                .Top(this.pageSize);

            if (this.isCancelled.HasValue)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private static IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException> Translate(IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException> graphQueryResult)
        {
            return graphQueryResult
                .Select(element => element
                    .SelectRight(translationException => new CalendarEventTranslationException("TODO", translationException))
                    .SelectLeft(calendarEvent => CalendarEventsContext.Translate(calendarEvent))
                    .SelectManyLeft())
                .SelectError(pagingException => new PagingException("TODO", pagingException));
        }

        private static IEither<CalendarEvent, CalendarEventTranslationException> Translate(Graph.CalendarEvent calendarEvent)
        {
            DateTimeOffset start;
            try
            {
                start = DateTimeOffset.Parse(calendarEvent.Start.DateTime);
            }
            catch (Exception exception)
            {
                return Either.Left<CalendarEvent>().Right(new CalendarEventTranslationException("tODO", exception));
            }

            return Either
                .Right<CalendarEventTranslationException>()
                .Left(
                    new CalendarEvent(
                        calendarEvent.Id,
                        calendarEvent.Subject, 
                        calendarEvent.Body.Content,
                        start, 
                        calendarEvent.IsCancelled));
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
