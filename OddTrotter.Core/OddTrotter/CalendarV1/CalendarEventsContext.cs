/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace OddTrotter.Calendar
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    /// <summary>
    /// This represents all of the calendar event invites that have an occurrence after a given timestamp. This can't be done on
    /// Graph with a simple `$filter` because series event masters often preserve the timestamp of the first instance which will
    /// usually be in the past.
    /// </summary>
    public sealed class CalendarEventsContext : 
        Fx.QueryContext.IQueryContext
            <
                IEither
                    <
                        CalendarEvent,
                        CalendarEventsContextTranslationException
                    >,
                CalendarEvent, 
                CalendarEventsContextPagingException
            >,
        Fx.QueryContext.Mixins.IWhereQueryContextMixin //// TODO rename to iquerycontextwheremixin?
            <
                IEither
                    <
                        CalendarEvent,
                        CalendarEventsContextTranslationException
                    >,
                CalendarEvent,
                CalendarEventsContextPagingException,
                CalendarEventsContext
            >
    {
        private readonly IGraphCalendarEventsEvaluator graphCalendarEventsContext;

        private readonly UriPath calendarUriPath;

        /// <summary>
        /// This is required because the `/instances` call on series event masters requires both a start time and an end time;
        /// we can continually look further in the future for `endTime`, but we have no way (as far as I can tell) to pick a 
        /// `startTime` that guarantees we don't miss any instance events. So, we need the caller to tell us the start time in
        /// order to handle the series event instances.
        /// </summary>
        private readonly DateTime startTime;

        private readonly int pageSize;

        private readonly TimeSpan firstInstanceInSeriesLookahead;

        /// <summary>
        /// TODO FUTURE you will want to handle this better in the future when you have full support for different query options.
        /// For now, `null` means that they've not filtered on `startTime < {something}`, and not `null` means that this value is
        /// `{something}`
        /// </summary>
        private readonly DateTime? endTime;

        /// <summary>
        /// TODO FUTURE you will want to handle this better in the future when you have full support for different query options.
        /// For now, `null` means that they've not filtered on `isCancelled`, and not `null` means that this value is the value 
        /// they are searching for
        /// </summary>
        private readonly bool? isCancelled;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="calendarUri"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="settings"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="graphCalendarEventsContext"/> or <paramref name="calendarUriPath"/> or 
        /// <paramref name="settings"/> is <see langword="null"/>
        /// </exception>
        public CalendarEventsContext(
            IGraphCalendarEventsEvaluator graphCalendarEventsContext,
            UriPath calendarUriPath,
            DateTime startTime,
            CalendarEventsContextSettings settings)
            : this(
                  ArgumentNullInline.ThrowIfNull(graphCalendarEventsContext), //// TODO shouldn't this assertion just go in the primary constructor?
                  ArgumentNullInline.ThrowIfNull(calendarUriPath), //// TODO shouldn't this assertion just go in the primary constructor?
                  startTime,
                  ArgumentNullInline.ThrowIfNull(settings).PageSize,
                  settings.FirstInstanceInSeriesLookahead, //// TODO you only have to do the inline assertion once
                  null,
                  null)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCalendarEventsContext"></param>
        /// <param name="calendarUriPath"></param>
        /// <param name="startTime"></param>
        /// <param name="pageSize"></param>
        /// <param name="endTime"></param>
        /// <param name="isCancelled"></param>
        private CalendarEventsContext(
            IGraphCalendarEventsEvaluator graphCalendarEventsContext,
            UriPath calendarUriPath,
            DateTime startTime,
            int pageSize,
            TimeSpan firstInstanceInSeriesLookahead,
            DateTime? endTime,
            bool? isCancelled)
        {
            this.graphCalendarEventsContext = graphCalendarEventsContext;
            this.calendarUriPath = calendarUriPath;
            this.startTime = startTime;
            this.pageSize = pageSize;
            this.firstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
            this.endTime = endTime;
            this.isCancelled = isCancelled;
        }

        public ITask<IQueryResult<IEither
                                <
                                    CalendarEvent,
                                    CalendarEventsContextTranslationException
                                >, CalendarEventsContextPagingException>> Evaluate()
        {
            //// TODO have an extension method so you don't need to write out the type
            return 
                new TaskWrapper
                    <
                        IQueryResult
                            <
                                IEither
                                    <
                                        CalendarEvent,
                                        CalendarEventsContextTranslationException
                                    >, 
                                CalendarEventsContextPagingException
                            >
                    >(
                        this.EvaluateImpl());
        }

        private async Task<IQueryResult<IEither
                                <
                                    CalendarEvent,
                                    CalendarEventsContextTranslationException
                                >, CalendarEventsContextPagingException>> EvaluateImpl()
        {
            var instanceEvents = await this.GetInstanceEvents().ConfigureAwait(false);
            var seriesEvents = await this.GetSeriesEvents().ConfigureAwait(false);
            //// TODO FUTURE merge the sorted sequences instead of concat //// TODO these are not necessarily sorted because the
            /// series events will come back in the order of their master start times, not the first instance start times
            var allEvents = instanceEvents
                .Concat(
                    seriesEvents,
                    firstError => firstError,
                    secondError => secondError,
                    (firstError, secondError) => 
                        new CalendarEventsContextPagingException(
                            "TODO an error occurred while paging both instances events and series events", 
                            new AggregateException(firstError, secondError)));
            return allEvents;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphResponse"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="graphResponse"/> is <see langword="null"/>
        /// </exception>
        private static
            IQueryResult
                <
                    IEither
                        <
                            CalendarEvent, 
                            CalendarEventsContextTranslationException
                        >, 
                    CalendarEventsContextPagingException
                >
            Adapt(
                IQueryResult
                    <
                        IEither
                            <
                                OddTrotter.Calendar.GraphCalendarEvent, 
                                GraphCalendarEventsContextTranslationException
                            >, 
                        GraphPagingException
                    > graphResponse)
        {
            ArgumentNullException.ThrowIfNull(graphResponse);

            return graphResponse
                .ErrorSelect(
                    graphPagingException =>
                        new CalendarEventsContextPagingException(
                            "An error occurred while paging through all of the calendar events.", 
                            graphPagingException))
                .Select(
                    graphCalendarEvent =>
                        graphCalendarEvent
                            .Select(
                                left => 
                                    ToCalendarEvent(left),
                                right =>
                                    new CalendarEventsContextTranslationException(
                                        $"An error occurred while translating the OData response into a Graph calendar event",
                                        right))
                            .SelectManyLeft());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private async 
            Task
                <
                    IQueryResult
                        <
                            IEither
                                <
                                    CalendarEvent,
                                    CalendarEventsContextTranslationException
                                >, 
                            CalendarEventsContextPagingException
                        >
                > 
            GetInstanceEvents()
        {
            //// TODO you are here
            //// TODO a bunch of what is still in this class should probably be in graphcalendareventscontext; the graphcalendareventscontext right now only does the translation from odata types to graph types, but it doesn't do any of the "wisdom" about urls (like the root calendar uri) and such
            //// TODO have a "query builder" nested in this class that takes what the "graphcontext" is currently doing
            //// TODO rename the "graphevaluator" back to "graphcontext"
            var context = new GraphCalendarEventsContext(this.graphCalendarEventsContext, this.calendarUriPath)
                .Filter(GraphCalendarEventsContext.TypeEqualsSingleInstance)
                .Filter(GraphCalendarEventsContext.StartTimeGreaterThan(this.startTime))
                .Top(this.pageSize)
                .OrderBy(GraphCalendarEventsContext.StartTime);
            if (this.endTime != null)
            {
                context = context.Filter(GraphCalendarEventsContext.EndTimeLessThan(this.endTime.Value));
            }

            if (this.isCancelled != null)
            {
                if (this.isCancelled.Value)
                {
                    context = context.Filter(GraphCalendarEventsContext.IsCancelled);
                }
                else
                {
                    context = context.Filter(GraphCalendarEventsContext.IsNotCancelled);
                }
            }

            var response = await context.Evaluate().ConfigureAwait(false);
            return Adapt(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private async Task<IQueryResult<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException>> GetSeriesEvents()
        {
            var seriesEventMasters =
                await this.GetSeriesEventMasters().ConfigureAwait(false);
            var mastersWithInstances = await seriesEventMasters
                //// TODO you added these async variants for `either` and `queryresult` but you didn't actually put thought into if these work as expected; for example, does it actually make sense to await the `queryresult.selectasync` call here? or does that imply something about the lazy evaluation that you don't want to actually do; would it make sense to have a `selectasync` method overload that is on `task<queryresult>` as well so that you can chain them? //// TODO you wrote a "convenience" overload for now, but you should reall revisit this and think it all the way through; i know you have doing that, but probably unit tests will help
                //// TODO and is it also ok to pass the tasks returned without awaiting them until the "very end" so to speak? https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD003.md
                .SelectAsync(
                    seriesMasterOrTranslationError => seriesMasterOrTranslationError
                        .SelectLeftAsync(
                            async seriesMaster =>
                            {
                                var instances = await GetInstancesInSeries(seriesMaster).ConfigureAwait(false);
                                return (SeriesMaster: seriesMaster, FirstInstance: instances.FirstOrDefault(new Nothing())); //// TODO should there be a parameterless overload for firstordefault
                            }/*,
                            translationError => Task.FromResult(translationError))) //// TODO go through all of your lambda code and make sure they have meaningful names*/
                            ))
                .Select(
                    seriesPlusInstanceOrTranslationError =>
                        seriesPlusInstanceOrTranslationError
                            .Apply(
                                seriesPlusInstance =>
                                    seriesPlusInstance.FirstInstance.Apply( //// TODO this was `trynotdefault` in the old way; i think we might want to abandon that approach, but you had this note previously incase you stick with that approach: //// TODO i *think* both delegates here are just doing identity operations; do you want to have a `trynotdefault` convenience overload that returns bool with and out parameter of the either of the other two?
                                        firstOrDefault =>
                                            firstOrDefault.TryGetLeft(out var instance) ?
                                                Either
                                                    .Left(
                                                        Either
                                                            .Left(
                                                                (
                                                                    SeriesMaster: seriesPlusInstance.SeriesMaster,
                                                                    Instance: Either
                                                                        .Left(instance)
                                                                        .Right<CalendarEventsContextPagingException>()
                                                                ))
                                                            .Right<CalendarEventsContextTranslationException>())
                                                    .Right<Nothing>() :
                                                Either
                                                    .Left<Either<(CalendarEvent SeriesMaster, Either<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException> Instance), CalendarEventsContextTranslationException>>()
                                                    .Right(new Nothing()),
                                        instancePagingError =>
                                            Either
                                                .Left(
                                                    Either
                                                        .Left(
                                                            (
                                                                SeriesMaster: seriesPlusInstance.SeriesMaster,
                                                                Instance: Either
                                                                    .Left<IEither<CalendarEvent, CalendarEventsContextTranslationException>>()
                                                                    .Right(instancePagingError)
                                                            ))
                                                        .Right<CalendarEventsContextTranslationException>())
                                                .Right<Nothing>()),
                                translationError => Either
                                    .Left(
                                        Either
                                            .Left
                                                <
                                                    (
                                                        CalendarEvent SeriesMaster,
                                                        Fx.Either.Either
                                                            <
                                                                IEither
                                                                    <
                                                                        CalendarEvent,
                                                                        CalendarEventsContextTranslationException
                                                                    >,
                                                                CalendarEventsContextPagingException
                                                            > Instance
                                                    )
                                                >()
                                            .Right(translationError))
                                    .Right<Nothing>()))
                .TrySelect(
                    (Either<Either<(CalendarEvent SeriesMaster, Either<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException> Instance), CalendarEventsContextTranslationException>, Nothing> either, [MaybeNullWhen(false)] out Either<(CalendarEvent SeriesMaster, Either<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException> Instance), CalendarEventsContextTranslationException> left) => 
                        either.TryGetLeft(out left)) //// TODO this amount of generics is really bad, see if you can fix it
                .Select(
                    seriesPlusInstanceOrTranslationError =>
                        seriesPlusInstanceOrTranslationError.SelectLeft(
                            seriesPlusInstance =>
                                seriesPlusInstance
                                    .Instance
                                    .Apply(
                                        instanceOrTranslationError =>
                                            instanceOrTranslationError
                                                .Apply( //// TODO isn't this just a select?
                                                    instance => 
                                                        Either
                                                            .Left(
                                                                new CalendarEvent(
                                                                    seriesPlusInstance.SeriesMaster.Id,
                                                                    seriesPlusInstance.SeriesMaster.Subject,
                                                                    seriesPlusInstance.SeriesMaster.Body,
                                                                    instance.Start,
                                                                    seriesPlusInstance.SeriesMaster.IsCancelled))
                                                            .Right<CalendarEventsContextTranslationException>(),
                                                    translationError =>
                                                        Either
                                                            .Left<CalendarEvent>()
                                                            .Right(
                                                                //// TODO should the translation error be a discriminated union? that way callers can differentiate? or does that leak too many details?
                                                                new CalendarEventsContextTranslationException($"A future instance was found for the series master with ID '{seriesPlusInstance.SeriesMaster.Id}' but the instance could not be translated correctly. The series master was '{JsonSerializer.Serialize(seriesPlusInstance.SeriesMaster)}'. TODO is the underlying error giving us the time range?", translationError))), //// TODO assuming because it got deserialized it can be serialized again...
                                        pagingError => 
                                            Either
                                                .Left<CalendarEvent>()
                                                .Right(
                                                    new CalendarEventsContextTranslationException($"A future instance was not found for the series master with ID '{seriesPlusInstance.SeriesMaster.Id}' because an error occurred while retrieving the instances for that master. It is possible that a future instanace exists.", pagingError))))
                        .SelectManyLeft())
                .ConfigureAwait(false);
            return mastersWithInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesMaster"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="seriesMaster"/> is <see langword="null"/></exception>
        private async Task<IQueryResult<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException>> GetInstancesInSeries(CalendarEvent seriesMaster)
        {
            if (seriesMaster == null)
            {
                throw new ArgumentNullException(nameof(seriesMaster));
            }

            var pageStartTime = this.startTime;
            var pageEndTime = pageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null)
            {
                if (this.endTime.Value < pageEndTime)
                {
                    //// TODO this might be confusing that we override the `firstInstanceInSeriesLookahead` if `endTime` is specified; take the case where the endtime is 6 months from now, but the lookahead is 2 months from now; from the perspective of the constructor caller, their lookahead is not honored; from the perspective of the where caller, their endtime is not honored; leveraging the `recurrence` property (https://learn.microsoft.com/en-us/graph/api/resources/patternedrecurrence?view=graph-rest-1.0) to compute when the last possible instance is might be the best solution
                    pageEndTime = this.endTime.Value;
                }
            }

            var context = new GetInstancesInSeriesContext(this.calendarUriPath, this.isCancelled, this.graphCalendarEventsContext, seriesMaster, pageStartTime, pageEndTime, this.endTime, this.firstInstanceInSeriesLookahead);
            var instancesInSeries = await GetInstancesInSeriesWithinTimeSlice(context).ConfigureAwait(false);

            return instancesInSeries;
        }

        private sealed class GetInstancesInSeriesContext
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="calendarUriPath"></param>
            /// <param name="isCancelled"></param>
            /// <param name="graphCalendarEventsContext"></param>
            /// <param name="seriesMaster"></param>
            /// <param name="pageStartTime"></param>
            /// <param name="pageEndTime"></param>
            /// <param name="globalEndTime"></param>
            /// <param name="firstInstanceInSeriesLookahead"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="calendarUriPath"/> or <paramref name="graphCalendarEventsContext"/> or <paramref name="seriesMaster"/> is <see langword="null"/></exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="pageEndTime"/> is before <paramref name="pageStartTime"/> or <paramref name="globalEndTime"/> is before <paramref name="pageEndTime"/> or <paramref name="globalEndTime"/> is before <paramref name="pageStartTime"/> or <paramref name="firstInstanceInSeriesLookahead"/> is not a positive value</exception>
            public GetInstancesInSeriesContext(UriPath calendarUriPath, bool? isCancelled, IGraphCalendarEventsEvaluator graphCalendarEventsContext, CalendarEvent seriesMaster, DateTime pageStartTime, DateTime pageEndTime, DateTime? globalEndTime, TimeSpan firstInstanceInSeriesLookahead)
            {
                if (calendarUriPath == null)
                {
                    throw new ArgumentNullException(nameof(calendarUriPath));
                }

                if (graphCalendarEventsContext == null)
                {
                    throw new ArgumentNullException(nameof(graphCalendarEventsContext));
                }

                if (seriesMaster == null)
                {
                    throw new ArgumentNullException(nameof(seriesMaster));
                }

                if (pageEndTime < pageStartTime)
                {
                    throw new ArgumentOutOfRangeException(nameof(pageEndTime), $"'{nameof(pageEndTime)}' cannot be before '{nameof(pageStartTime)}'. The provided value for '{nameof(pageEndTime)}' was '{pageEndTime}'. The provided value for '{nameof(pageStartTime)}' was '{pageStartTime}'.");
                }

                if (globalEndTime < pageStartTime)
                {
                    throw new ArgumentOutOfRangeException(nameof(globalEndTime), $"'{nameof(globalEndTime)}' cannot be before '{nameof(pageStartTime)}'. The provided value for '{nameof(globalEndTime)}' was '{globalEndTime}'. The provided value for '{nameof(pageStartTime)}' was '{pageStartTime}'.");
                }

                if (globalEndTime < pageEndTime)
                {
                    throw new ArgumentOutOfRangeException(nameof(globalEndTime), $"'{nameof(globalEndTime)}' cannot be before '{nameof(pageEndTime)}'. The provided value for '{nameof(globalEndTime)}' was '{globalEndTime}'. The provided value for '{nameof(pageEndTime)}' was '{pageEndTime}'.");
                }

                if (firstInstanceInSeriesLookahead <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(firstInstanceInSeriesLookahead), $"'{nameof(firstInstanceInSeriesLookahead)}' must be a positive value. The provided value was '{firstInstanceInSeriesLookahead}'.");
                }

                this.CalendarUriPath = calendarUriPath;
                this.IsCancelled = isCancelled;
                this.GraphCalendarEventsContext = graphCalendarEventsContext;
                this.SeriesMaster = seriesMaster;
                this.PageStartTime = pageStartTime;
                this.PageEndTime = pageEndTime;
                this.GlobalEndTime = globalEndTime;
                this.FirstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
            }

            public UriPath CalendarUriPath { get; }
            public bool? IsCancelled { get; }
            public IGraphCalendarEventsEvaluator GraphCalendarEventsContext { get; }
            public CalendarEvent SeriesMaster { get; }
            public DateTime PageStartTime { get; }
            public DateTime PageEndTime { get; }
            public DateTime? GlobalEndTime { get; }
            public TimeSpan FirstInstanceInSeriesLookahead { get; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/></exception>
        private static async Task<IQueryResult<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException>> GetInstancesInSeriesWithinTimeSlice(GetInstancesInSeriesContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var url =
                $"{context.CalendarUriPath.Path}/events/{context.SeriesMaster.Id}/instances?startDateTime={context.PageStartTime}&endTime={context.PageEndTime}&$select=id,start,subject,body,isCancelled";

            if (context.IsCancelled != null)
            {
                url += $"&$filter=isCancelled eq {(context.IsCancelled.Value ? "true" : "false")}";
            }

            var graphRequest = new GraphQuery.GetEvents(new Uri(url, UriKind.Relative).ToRelativeUri());

            var graphResponse = await context
                .GraphCalendarEventsContext
                .Page(
                    graphRequest,
                    nextLink => 
                    //// TODO maybe everything about your below comment is wrong; according to the standard, this URL is opaque, but can be trusted to give back the next page; that means that we need a way to call the URL directly *without* anything like adding a `.Filter` and whatnot, and just get back the strongly-type collection of `graphcalendarevent`
                        nextLink.StartsWith(context.GraphCalendarEventsContext.ServiceRoot) ? context.GraphCalendarEventsContext : throw new Exception("TODO you need a new exception type for this probably?")) //// TODO this contextgenerator stuff was because you didn't have serviceroot on the context interface, so you wanted the caller to pass it in; now that you have it in the interface, instead of a generator, you should probably just take in the "dictionary"; the reason this is coming up is because otherwise the `page` method needs to describe how the generator should return (or throw) in the even that a context cannot be found by the caller; you really don't want the generator to throw because that defeats the purpose of the queryresult stuff
                .ConfigureAwait(false);

            var pageStartTime = context.PageEndTime;
            var pageEndTime = pageStartTime + context.FirstInstanceInSeriesLookahead;
            if (context.GlobalEndTime != null)
            {
                if (context.GlobalEndTime.Value < pageEndTime)
                {
                    pageEndTime = context.GlobalEndTime.Value;
                }
            }

            var nextContext = new GetInstancesInSeriesContext(
                context.CalendarUriPath,
                context.IsCancelled,
                context.GraphCalendarEventsContext,
                context.SeriesMaster,
                pageStartTime,
                pageEndTime,
                context.GlobalEndTime,
                context.FirstInstanceInSeriesLookahead);
            return Adapt(graphResponse).Concat(await GetInstancesInSeriesWithinTimeSlice(nextContext).ConfigureAwait(false), _ => _, _ => _, (_, __) => new CalendarEventsContextPagingException("TODO", _)); //// TODO you probably should have a "concat if no errors in first" overload
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCalendarEvent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEvent"/> is <see langword="null"/></exception>
        private static IEither<CalendarEvent, CalendarEventsContextTranslationException> ToCalendarEvent(OddTrotter.Calendar.GraphCalendarEvent graphCalendarEvent)
        {
            if (graphCalendarEvent == null)
            {
                throw new ArgumentNullException(nameof(graphCalendarEvent));
            }

            DateTime start;
            try
            {
                //// TODO handle timezone... //// TODO previously you simply considered events without a utc timezone as invalid events; this was the code:
                /*
!string.Equals(@event?.Start?.TimeZone, "utc", StringComparison.OrdinalIgnoreCase) ? throw new InvalidOperationException("the event did not have a known time zone in its start time") :  DateTime.SpecifyKind(DateTime.Parse(
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            @event
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
                                .Start
                                .DateTime),
                            DateTimeKind.Utc)));
                */
                start = DateTime.Parse(graphCalendarEvent.Start.DateTime);
            }
            catch (FormatException formatException)
            {
                return Either
                    .Left<CalendarEvent>()
                    .Right(
                        new CalendarEventsContextTranslationException(
                            $"An error occurred while parsing the start time of the event: '{JsonSerializer.Serialize(graphCalendarEvent)}'.", // we are trusting that we can serialize without exception since we have a non-null instance of a highly structured type
                            formatException));
            }

            return Either
                .Left(
                    new CalendarEvent(
                        graphCalendarEvent.Id,
                        graphCalendarEvent.Subject,
                        graphCalendarEvent.Body.Content,
                        start,
                        graphCalendarEvent.IsCancelled))
                .Right<CalendarEventsContextTranslationException>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private async Task<IQueryResult<IEither<CalendarEvent, CalendarEventsContextTranslationException>, CalendarEventsContextPagingException>> GetSeriesEventMasters()
        {
            var url =
                $"{this.calendarUriPath.Path}/events?" +
                $"$select=body,start,subject,isCancelled&" +
                $"$top={this.pageSize}&" + // the graph API does not implement `$top` correctly; it returns a `@nextLink` even if it gives you all `pageSize` elements that are requested; for this reason, we can use `$top` for page size here
                $"$orderBy=start/dateTime&" +
                "$filter=type eq 'seriesMaster'";

            if (this.isCancelled != null)
            {
                url += $"and isCancelled eq {this.isCancelled.Value.ToString().ToLower()}";
            }

            var graphRequest = new GraphQuery.GetEvents(new Uri(url, UriKind.Relative).ToRelativeUri());
            var graphResponse = await this
                .graphCalendarEventsContext
                .Page(
                    graphRequest,
                    nextLink => nextLink.StartsWith(this.graphCalendarEventsContext.ServiceRoot) ? this.graphCalendarEventsContext : throw new Exception("TODO you need a new exception type for this probably?")) //// TODO this contextgenerator stuff was because you didn't have serviceroot on the context interface, so you wanted the caller to pass it in; now that you have it in the interface, instead of a generator, you should probably just take in the "dictionary"; the reason this is coming up is because otherwise the `page` method needs to describe how the generator should return (or throw) in the even that a context cannot be found by the caller; you really don't want the generator to throw because that defeats the purpose of the queryresult stuff
                .ConfigureAwait(false);
            return Adapt(graphResponse);
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (object.ReferenceEquals(predicate, StartLessThanNow))
            {
                var now = DateTime.UtcNow;
                if (this.endTime != null && this.endTime < now)
                {
                    // we logically can see that this will always happen (they can only call set `endTime` to `DateTime.UtcNow`, so `now` will always been more in the future than `endTime`) 
                    return this;
                }

                return new CalendarEventsContext(this.graphCalendarEventsContext, this.calendarUriPath, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, now, this.isCancelled);
            }
            else if (object.ReferenceEquals(predicate, IsNotCancelled))
            {
                if (this.isCancelled != null)
                {
                    // the caller can only provide `IsNotCancelled` right now, so if `isCancelled` is already set, it won't be changing
                    return this;
                }

                return new CalendarEventsContext(this.graphCalendarEventsContext, this.calendarUriPath, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, this.endTime, false);
            }

            throw new NotImplementedException("TODO");
        }

        public static Expression<Func<CalendarEvent, bool>> StartLessThanNow { get; } = calendarEvent => calendarEvent.Start < DateTime.UtcNow; //// TODO will "now" constantly change?

        public static Expression<Func<CalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => !calendarEvent.IsCancelled;

        //// TODO please figure out how you want to do all this newline formatting, you can't seem to get it right and be consistent; make sur ethat you consider having too many generic type arguments
        //// TODO normalize all of your visitors with the pattern (naming, internal protected in the visitor, etc) that you are using in other repos
    }
}
