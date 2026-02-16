namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;
    using Fx.Realizable;

    using OddTrotter.Graph.CalendarEventsContext;

    using Graph = OddTrotter.Graph.CalendarEventsContext;

    internal sealed class CalendarEventsContextSettings
    {
        private CalendarEventsContextSettings(uint pageSize, TimeSpan firstInstanceInSeriesLookahead)
        {
            PageSize = pageSize;
            FirstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
        }

        public static CalendarEventsContextSettings Default { get; } = new CalendarEventsContextSettings(
            10,
            TimeSpan.FromDays(14));

        public uint PageSize { get; }
        public TimeSpan FirstInstanceInSeriesLookahead { get; }
    }

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
        private readonly uint pageSize;
        private readonly TimeSpan firstInstanceInSeriesLookahead;
        private readonly bool? isCancelled;
        private readonly DateTime? endTime;
        private readonly Func<CalendarEvent, bool>? where;

        internal CalendarEventsContext(Graph.ICalendarSource calendarSource, DateTime startTime)
            : this(calendarSource, startTime, CalendarEventsContextSettings.Default)
        {
        }

        internal CalendarEventsContext(Graph.ICalendarSource calendarSource, DateTime startTime, CalendarEventsContextSettings settings)
            : this(
                  calendarSource,
                  startTime,
                  settings.PageSize,
                  settings.FirstInstanceInSeriesLookahead,
                  null,
                  null,
                  null)
        {
        }

        private CalendarEventsContext(
            Graph.ICalendarSource calendarSource, 
            DateTime startTime,
            uint pageSize,
            TimeSpan firstInstanceInSeriesLookahead,
            bool? isCancelled,
            DateTime? endTime,
            Func<CalendarEvent, bool>? where)
        {
            this.calendarSource = calendarSource;
            this.startTime = startTime;
            this.pageSize = pageSize;
            this.firstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
            this.isCancelled = isCancelled;
            this.endTime = endTime;
            this.where = where;
        }

        public async ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate()
        {
            var events = await this.GetEvents().ConfigureAwait(false);
            var translatedEvents = Translate(events);
            if (where != null)
            { 
                translatedEvents = translatedEvents
                    .Where(
                        calendarEventOrError => calendarEventOrError.Apply(calendarEvent => this.where(calendarEvent), error => true));
            }

            return translatedEvents;
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
                .SelectAsync(
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
                        return potentialSeriesMasterPlusFirstInstanceOrError.Decompose(out seriesMasterWithInstanceOrError, out _);
                    })
                .Select(
                    seriesMasterWithInstanceOrError => seriesMasterWithInstanceOrError
                        .SelectLeft(
                            seriesMasterPlusInstance => new Graph.CalendarEvent(
                                seriesMasterPlusInstance.SeriesMaster.Id,
                                seriesMasterPlusInstance.SeriesMaster.Subject,
                                seriesMasterPlusInstance.SeriesMaster.Body,
                                seriesMasterPlusInstance.FirstInstance.Start,
                                seriesMasterPlusInstance.SeriesMaster.IsCancelled,
                                seriesMasterPlusInstance.SeriesMaster.Type,
                                seriesMasterPlusInstance.FirstInstance.End))
                        .SelectRight(
                            errors => errors.SelectManyRight())
                        .SelectRight(
                            translationErrorOrInstancePagingError => translationErrorOrInstancePagingError
                                .SelectRight(
                                    instancePagingError => new Graph.CalendarEventTranslationException("TODO", instancePagingError))
                                .Coalesce()));

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

            return initial.Concat2(this.GetInstancesInSeries(seriesMasterId, newPageStartTime, newPageEndTime));
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
            /*DateTimeOffset start;
            try
            {
                start = DateTimeOffset.Parse(calendarEvent.Start.DateTime);
            }
            catch (Exception exception)
            {
                return Either.Left<CalendarEvent>().Right(new CalendarEventTranslationException("tODO", exception));
            }*/

            return Either
                .Right<CalendarEventTranslationException>()
                .Left(
                    new CalendarEvent(
                        calendarEvent.Id,
                        calendarEvent.Subject, 
                        calendarEvent.Body.Content,
                        calendarEvent.Start.DateTime, 
                        calendarEvent.IsCancelled));
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            if (object.ReferenceEquals(predicate, StartLessThanNow))
            {
                var now = DateTime.UtcNow;
                if (this.endTime != null && this.endTime < now)
                {
                    // we logically can see that this will always happen (they can only call set `endTime` to `DateTime.UtcNow`, so `now` will always been more in the future than `endTime`) 
                    return this;
                }

                return new CalendarEventsContext(this.calendarSource, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, this.isCancelled, now,
                    this.where);
            }
            else if (object.ReferenceEquals(predicate, IsNotCancelled))
            {
                if (this.isCancelled != null)
                {
                    // the caller can only provide `IsNotCancelled` right now, so if `isCancelled` is already set, it won't be changing
                    return this;
                }

                return new CalendarEventsContext(this.calendarSource, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, false, this.endTime, this.where);
            }

            var compiledPredicate = predicate.Compile();
            return new CalendarEventsContext(
                this.calendarSource,
                this.startTime,
                this.pageSize,
                this.firstInstanceInSeriesLookahead,
                this.isCancelled,
                this.endTime,
                this.where == null ? compiledPredicate : calendarEvent => this.where(calendarEvent) && compiledPredicate(calendarEvent));
        }

        public static Expression<Func<CalendarEvent, bool>> StartLessThanNow { get; } = calendarEvent => calendarEvent.Start < DateTime.UtcNow; //// TODO will "now" constantly change?

        public static Expression<Func<CalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => !calendarEvent.IsCancelled;
    }

    internal static class Extensions
    {
        internal static IQueryResult<TResult, TError> SelectAsync<TValue, TError, TResult>(
            this IQueryResult<TValue, TError> queryResult,
            Func<TValue, Task<TResult>> selector)
        {
            return new SelectQueryResult<TValue, TError, TResult>(queryResult, selector);
        }

        private sealed class SelectQueryResult<TValue, TError, TResult> : IQueryResult<TResult, TError>
        {
            private readonly IQueryResult<TValue, TError> queryResult;
            private readonly Func<TValue, Task<TResult>> selector;

            public SelectQueryResult(
                IQueryResult<TValue, TError> queryResult,
                Func<TValue, Task<TResult>> selector)
            {
                this.queryResult = queryResult;
                this.selector = selector;
            }

            public IQueryResultNode<TResult, TError> Nodes
            {
                get
                {
                    return new QueryResultNode(this.queryResult.Nodes, this.selector);
                }
            }

            private sealed class QueryResultNode : IQueryResultNode<TResult, TError>
            {
                private readonly IQueryResultNode<TValue, TError> queryResult;
                private readonly Func<TValue, Task<TResult>> selector;

                public QueryResultNode(
                    IQueryResultNode<TValue, TError> queryResult,
                    Func<TValue, Task<TResult>> selector)
                {
                    this.queryResult = queryResult;
                    this.selector = selector;
                }

                public Realizable<TResult1> ApplyAsync<TResult1, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TResult, TError>, TContext, TContinuable, TResult1> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult1> rightMap, ref TContext context)
                    where TResult1 : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult1>, allows ref struct
                {
                    if (this.queryResult.Decompose(out var element, out var terminal)) //// TODO you shouldn't need to use decompose
                    {
                        return this.selector(element.Value)
                            .ToTaskWrapper()
                            .ContinueWith(
                                selected => new Element(selected, element, this.selector),
                                _ => throw _,
                                _ => throw _)
                            .ContinueWith(
                                element =>
                                {
                                    var fakeContext = default(TContext)!;
                                    ref TContext toPass = ref Unsafe.AsRef(ref fakeContext); //// TODO use the real context here...

                                    return leftMap(element, ref toPass).ContinueWith(_ => _, _ => throw _, _ => throw _);
                                },
                                _ => throw _,
                                _ => throw _)
                            .Unwrap();
                    }
                    else
                    {
                        return rightMap(terminal, ref context).ContinueWith(_ => _, _ => throw _, _ => throw _);
                    }
                }

                private sealed class Element : IElement<TResult, TError>
                {
                    private readonly IElement<TValue, TError> element;
                    private readonly Func<TValue, Task<TResult>> selector;

                    public Element(
                        TResult value,
                        IElement<TValue, TError> element,
                        Func<TValue, Task<TResult>> selector)
                    {
                        Value = value;
                        this.element = element;
                        this.selector = selector;
                    }

                    public TResult Value { get; }

                    public IQueryResultNode<TResult, TError> Next()
                    {
                        return new QueryResultNode(this.element.Next(), this.selector);
                    }
                }
            }
        }
    }
}
