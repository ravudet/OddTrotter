namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;

    using OddTrotter.Graph.CalendarEventsContext;

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
        private readonly DateTime startTime; //// TODO should you also add this to settings, defaulting to `now`?
        private readonly uint pageSize; //// TODO add settings
        private readonly TimeSpan firstInstanceInSeriesLookahead; //// TODO settings
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

            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetSeriesEvents()
        {
            var seriesEventMasters = await this.GetSeriesEventMasters().ConfigureAwait(false);
            var mastersWithInstances = seriesEventMasters
                .Select(
                    async seriesMasterOrTranslationError => await seriesMasterOrTranslationError
                        .SelectLeft(
                            async seriesMaster =>
                            {
                                var instances = await this.GetInstancesInSeries(seriesMaster.Id).ConfigureAwait(false);
                                return (SeriesMaster: seriesMaster, PotentialFirstInstance: instances.FirstOrDefault(new Nothing()));
                            })
                        .ConfigureAwait(false))
                .Select(
                    seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError // what we want is ieither<(seriesmaster+firstinsatnce), ieither<seriesmastertranslationerror, ieither<instancetranslationerror, instancepagingerror>; so we are returning here and either of *that* or *nothing*
                        .Apply(
                            seriesMasterPlusPotentialFirstInstance => seriesMasterPlusPotentialFirstInstance
                                .PotentialFirstInstance
                                .Apply(
                                    potentialFirstInstanceOrError => potentialFirstInstanceOrError
                                        .Apply(
                                            firstInstanceOrError => Either
                                                .Right<Nothing>()
                                                .Left(
                                                    firstInstanceOrError
                                                        .Apply(
                                                            firstInstance => Either
                                                                .Right<Either<Graph.CalendarEventTranslationException, Either<Graph.CalendarEventTranslationException, Graph.PagingException>>>()
                                                                .Left(
                                                                    (
                                                                        SeriesMaster: seriesMasterPlusPotentialFirstInstance.SeriesMaster,
                                                                        FirstInstance: firstInstance
                                                                    )),
                                                            instanceTranslationError => Either
                                                                .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                                                .Right(
                                                                    Either
                                                                        .Left<Graph.CalendarEventTranslationException>()
                                                                        .Right(
                                                                            Either
                                                                                .Right<Graph.PagingException>()
                                                                                .Left(instanceTranslationError))))),
                                            nothing => Either
                                                .Left<Either<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), Either<Graph.CalendarEventTranslationException, Either<Graph.CalendarEventTranslationException, Graph.PagingException>>>>()
                                                .Right(nothing)),
                                    instancePagingError => Either
                                            .Right<Nothing>()
                                            .Left(
                                                Either
                                                    .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                                    .Right(
                                                        Either
                                                            .Left<Graph.CalendarEventTranslationException>()
                                                            .Right(
                                                                Either
                                                                    .Left<Graph.CalendarEventTranslationException>()
                                                                    .Right(instancePagingError))))),
                            seriesTranslationError => Either
                                .Right<Nothing>()
                                .Left(
                                    Either
                                        .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                        .Right(
                                            Either
                                                .Right<Either<Graph.CalendarEventTranslationException, Graph.PagingException>>()
                                                .Left(seriesTranslationError)))))
                .TrySelect( //// TODO any way to get type inference here?
                    (IEither<IEither<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), IEither<Graph.CalendarEventTranslationException, IEither<Graph.CalendarEventTranslationException, Graph.PagingException>>>, Nothing> potentialSeriesMasterPlusFirstInstanceOrError, [MaybeNullWhen(false)] out IEither<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), IEither<Graph.CalendarEventTranslationException, IEither<Graph.CalendarEventTranslationException, Graph.PagingException>>> seriesMasterWithInstanceOrError) =>
                    {
                        return potentialSeriesMasterPlusFirstInstanceOrError.TryGetLeft(out seriesMasterWithInstanceOrError);
                    });
            /*.Select(
                seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError
                    .Apply(
                        seriesMasterPlusPotentialFirstInstance => seriesMasterPlusPotentialFirstInstance
                            .PotentialFirstInstance
                            .Apply(
                                firstInstanceOrDefault => firstInstanceOrDefault
                                    .Decompose(out var firstInstance, out var nothing) ?
                                        Either
                                            .Right<Nothing>()
                                            .Left(
                                                Either
                                                    .Right<Graph.CalendarEventTranslationException>()
                                                    .Left()*/
            /*.TrySelect
                <
                    IEither<(Graph.CalendarEvent SeriesMaster, IEither<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException> Instance), Graph.CalendarEventTranslationException>,
                    Graph.PagingException,
                    IEither<(Graph.CalendarEvent SeriesMaster, IEither<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException> Instance), Graph.CalendarEventTranslationException>
                >(
                (IEither<IEither<(Graph.CalendarEvent SeriesMaster, IEither<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException> Instance), Graph.CalendarEventTranslationException>, Nothing> seriesMasterPlusPontentialFirstInstanceOrTranslationError, [MaybeNullWhen(false)] out IEither<(Graph.CalendarEvent SeriesMaster, IEither<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException> Instance), Graph.CalendarEventTranslationException> seriesMasterWithInstance) =>
                    seriesMasterPlusPontentialFirstInstanceOrTranslationError.Decompose(out seriesMasterWithInstance, out _))*/

            return mastersWithInstances;
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstancesInSeries(string seriesMasterId)
        {
            var pageStartTime = this.startTime;
            var pageEndTime = pageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < pageEndTime)
            {
                pageEndTime = this.endTime.Value;
            }

            return await this.GetInstancesInSeries(seriesMasterId, pageStartTime, pageEndTime).ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstancesInSeries(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var initial = await GetInstancesInSeriesWithinTimeSlice(seriesMasterId, pageStartTime, pageEndTime).ConfigureAwait(false);

            var newPageStartTime = this.startTime;
            var newPageEndTime = newPageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < newPageEndTime)
            {
                newPageEndTime = this.endTime.Value;
            }

            return initial.Concat(this.GetInstancesInSeries(seriesMasterId, newPageStartTime, newPageEndTime));
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingException>> GetInstancesInSeriesWithinTimeSlice(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var context = this.calendarSource.Events().Get(seriesMasterId).Instances(pageStartTime, pageEndTime).Get();
            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
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

    internal static class Extensions
    {
        internal static IQueryResult<TResult, TError> Select<TValue, TError, TResult>(
            this IQueryResult<TValue, TError> queryResult,
            Func<TValue, Task<TResult>> selector)
        {
        }
    }
}
