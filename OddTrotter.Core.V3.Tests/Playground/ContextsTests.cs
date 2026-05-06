namespace OddTrotter.Graph.CalendarEventsSource.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    //// TODO you can have a roslyn analyzer that notices that the model is out of sync with the interfaces; you could also have quick actions like "i don't want a post on this collection" and it will annotate the model to say that post isn't allowed (or whatever other things like this there are)

    internal interface ICalendarSource
    {
        ICalendarContext Get();

        ICalendarEventsSource Events();
    }

    internal interface ICalendarContext
    {
        Task<Calendar> Evaluate();

        ICalendarContext Select<TResult>(Expression<Func<Calendar, TResult>> selector);
    }

    internal sealed class Calendar //// TODO use the "DTO" suffix (data transfer object) or the "OTW" suffix (over the wire)
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Calendar()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public IEnumerable<CalendarEvent> Events { get; }
    }

    internal interface ICalendarEventsSource
    {
        ICalendarEventsContext Get();

        ICalendarEventSource Key(string id); //// TODO this could conflict with a property, but i think that's fine because the property wouldn't take a parameter; it could also conflict with a function or something, which isn't good; try to figure that out //// TODO you could have icollectionsource which has `key(string)` (which could work with a `ascollectionsource` extension so that if there *is* a conflict, the caller has some way to disambiguate), but then what about composite keys? //// TODO i actually really like this because you can do similar things for the query options and the developer can "implement" whatever interfaces they want (so if the collection isn't indexable, that interface isn't implemented, the same way if filter is supported, that interface wouldn't be implemented)
    }

    internal sealed class CalendarEventTranslationError
    {
        private CalendarEventTranslationError()
        {
        }
    }

    internal sealed class PagingError
    {
        private PagingError()
        {
        }
    }

    internal interface ICalendarEventsContext
    {
        ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationError>, PagingError>> Evaluate();

        ICalendarEventsContext Filter(Expression<Func<CalendarEvent, bool>> filter);

        ICalendarEventsContext Top(uint top);

        ICalendarEventsContext OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy);
    }

    internal sealed class CalendarEvent
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private CalendarEvent()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public string Id { get; }

        public string Type { get; }

        public TimeStructure Start { get; }

        public TimeStructure End { get; }

        public bool IsCancelled { get; }

        //// public IEnumerable<CalendarEvent> Instances { get; }

        public PatternedRecurrence Recurrence { get; }

        public sealed class PatternedRecurrence
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            private PatternedRecurrence()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            {
            }

            public RecurrenceRange Range { get; }

            public sealed class RecurrenceRange
            {
                private RecurrenceRange()
                {
                }

                public DateOnly Date { get; }
            }
        }

        public sealed class TimeStructure
        {
            private TimeStructure()
            {
            }

            public DateTime DateTime { get; }
        }
    }

    internal interface ICalendarEventSource
    {
        Task<CalendarEvent> Evaluate();

        ICalendarContext Select<TResult>(Expression<Func<CalendarEvent, TResult>> selector);

        ICalendarEventsSource Instances(DateTime startDateTime, DateTime endTime);
    }
}

namespace OddTrotter.NonGraph.CalendarEventsSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    internal sealed class Calendar
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Calendar()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public IEnumerable<CalendarEvent> Events { get; }
    }

    internal sealed class CalendarEvent
    {
        public CalendarEvent(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    internal interface ICalendarSource
    {
        ICalendarContext Get();

        ICalendarEventsSource Events();
    }

    internal interface ICalendarContext
    {
        Task<Calendar> Evalaute();

        ICalendarContext Select<TResult>(Expression<Func<Calendar, TResult>> selector);
    }

    internal interface ICalendarEventsSource
    {
        ICalendarEventsContext Get();
    }

    internal sealed class CalendarEventTranslationError
    {
        public CalendarEventTranslationError()
        {
        }
    }

    internal sealed class PagingError
    {
        public PagingError()
        {
        }
    }

    internal interface ICalendarEventsContext
    {
        ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationError>, PagingError>> Evaluate();

        ICalendarEventsContext Filter(Expression<Func<CalendarEvent, bool>> filter);

        ICalendarEventsContext Top(uint top);

        ICalendarEventsContext OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy);
    }
}

namespace Adapter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.Realizable;

    using OddTrotter.Graph.CalendarEventsContext;

    using Graph = OddTrotter.Graph.CalendarEventsSource.V2;
    using OddTrotter = OddTrotter.NonGraph.CalendarEventsSource;

    internal sealed class CalendarSource : OddTrotter.ICalendarSource
    {
        private readonly Graph.ICalendarSource graphCalendarSource;

        public CalendarSource(Graph.ICalendarSource graphCalendarSource)
        {
            this.graphCalendarSource = graphCalendarSource;
        }

        public OddTrotter.ICalendarEventsSource Events()
        {
            return new CalendarEventsSource(this.graphCalendarSource.Events());
        }

        private sealed class CalendarEventsSource : OddTrotter.ICalendarEventsSource
        {
            private readonly Graph.ICalendarEventsSource graphCalendarEventsSource;

            public CalendarEventsSource(Graph.ICalendarEventsSource graphCalendarEventsSource)
            {
                this.graphCalendarEventsSource = graphCalendarEventsSource;
            }

            public OddTrotter.ICalendarEventsContext Get()
            {
                return new CalendarEventsContext(this.graphCalendarEventsSource.Get());
            }

            private sealed class CalendarEventsContext : OddTrotter.ICalendarEventsContext
            {
                private readonly Graph.ICalendarEventsContext graphCalendarEventsContext;

                private readonly DateTime? startTime;
                private readonly DateTime? endTime;

                /// <summary>
                /// a filter was applied that matches a property that has consistent values across all instances in a series and the filter is known to be supported by graph (e.g. iscancelled)
                /// </summary>
                private readonly Expression<Func<Graph.CalendarEvent, bool>>? filterConsistentAcrossInstancesAndSupportedByGraph;

                /// <summary>
                /// a filter was applied that matches a property that has consistent values across all instances in a series and the filter is known to be not supported by graph (e.g. subject)
                /// </summary>
                private readonly Func<Graph.CalendarEvent, bool>? filterConsistentAcrossInstancesAndNotSupportedByGraph;

                /// <summary>
                /// a filter was applied that matches a property that does not have consistent values across all instances in a series (regardless of whether the filter is known to be supported by graph) (e.g. start) //// TODO you don't actually have anything that should reach here if the caller is just matching on a single property; this should only be reached right now when they are using a more complex expression (like using binary operators or comparing properties to other properties)
                /// </summary>
                private readonly Func<OddTrotter.CalendarEvent, bool>? filterNotConsistentAcrossInstances;

                public CalendarEventsContext(Graph.ICalendarEventsContext graphCalendarEventsContext)
                    : this(
                          graphCalendarEventsContext,
                          null,
                          null,
                          null,
                          null,
                          null)
                {
                }

                public CalendarEventsContext(
                    Graph.ICalendarEventsContext graphCalendarEventsContext,
                    DateTime? startTime,
                    DateTime? endTime,
                    Expression<Func<Graph.CalendarEvent, bool>>? filterConsistentAcrossInstancesAndSupportedByGraph,
                    Func<Graph.CalendarEvent, bool>? filterConsistentAcrossInstancesAndNotSupportedByGraph,
                    Func<OddTrotter.CalendarEvent, bool>? filterNotConsistentAcrossInstances)
                {
                    this.graphCalendarEventsContext = graphCalendarEventsContext;

                    this.startTime = startTime;
                    this.endTime = endTime;
                    this.filterConsistentAcrossInstancesAndSupportedByGraph = filterConsistentAcrossInstancesAndSupportedByGraph;
                    this.filterConsistentAcrossInstancesAndNotSupportedByGraph = filterConsistentAcrossInstancesAndNotSupportedByGraph;
                    this.filterNotConsistentAcrossInstances = filterNotConsistentAcrossInstances;
                }

                public async ITask<IQueryResult<IEither<OddTrotter.CalendarEvent, OddTrotter.CalendarEventTranslationError>, OddTrotter.PagingError>> Evaluate()
                {
                    //// TODO you are here potentially
                    //// TODO i don't remember if there's something in this method you still need to do, but you are actually in the seriesevent masters stuff, and you're thinking about what it should look like to filter (look at the todos there)

                    var instanceEvents = await this.GetInstanceEvents().ConfigureAwait(false);
                    var seriesEvents = await this.GetSeriesEvents().ConfigureAwait(false);

                    var combined = instanceEvents.Concat(seriesEvents, _ => _, _ => _, (_, _) => throw new Exception("tODO"));

                    return combined
                        .Select(
                            graphCalendarEventOrTranslationError => graphCalendarEventOrTranslationError
                                .Select(
                                    graphCalendarEvent => new OddTrotter.CalendarEvent(graphCalendarEvent.Id),
                                    translationError => new OddTrotter.CalendarEventTranslationError() //// TODO
                                    ))
                        .SelectError(
                            pagingError => new OddTrotter.PagingError());
                }

                private async ITask<IQueryResult<IEither<OddTrotter.CalendarEvent, OddTrotter.CalendarEventTranslationError>, OddTrotter.PagingError>> GetInstanceEvents()
                {
                    var instanceEvents = await this
                        .GetGraphInstanceEvents()
                        .Select(
                            graphCalendarEventOrTranslationError => graphCalendarEventOrTranslationError
                                .Select(
                                    graphCalendarEvent => new OddTrotter.CalendarEvent(graphCalendarEvent.Id),
                                    translationError => new OddTrotter.CalendarEventTranslationError() //// TODO
                                    ))
                        .SelectError(
                            pagingError => new OddTrotter.PagingError())
                        .ConfigureAwait(false);

                    if (this.filterNotConsistentAcrossInstances != null)
                    {
                        instanceEvents = instanceEvents.Where(this.filterNotConsistentAcrossInstances); //// TODO it would be best to break this filter into those things that are supported by graph and those that aren't so that you can use a `filter` instead; but it's also possible that there is nothing supported by grpah for those things not consistent across instances //// TODO however, this could lead to behavior where a filter (that isn't supported by graph) is applied to series events (because it's applied in-memory), but the instance events are all errors; that'd be pretty weird
                    }

                    return instanceEvents;
                }

                private async ITask<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetGraphInstanceEvents()
                {
                    //// TODO consider what it means to have infrastructure which has this interface injected so that a service is implemented; particularly, how do skiptokens work?

                    var pageSize = 100U; //// TODO configure this

                    var calendarEvents = this
                        .graphCalendarEventsContext
                        .Filter(calendarEvent => calendarEvent.Type == "singleInstance");

                    if (this.startTime != null)
                    {
                        calendarEvents = calendarEvents
                            .Filter(calendarEvent => calendarEvent.Start.DateTime > this.startTime); //// TODO i can't decide if `timestructure.datetime` should be a string and we should call `this.startTime.ToString()` here, or if `timestructure.datetime` is supposed to be a datetime; look at the csdl probably...
                    }

                    if (this.endTime != null)
                    {
                        calendarEvents = calendarEvents
                            .Filter(calendarEvent => calendarEvent.End.DateTime < this.endTime);
                    }

                    if (this.filterConsistentAcrossInstancesAndSupportedByGraph != null)
                    {
                        calendarEvents = calendarEvents
                            .Filter(this.filterConsistentAcrossInstancesAndSupportedByGraph);
                    }

                    //// TODO make sure iscancelled can be called by the consumer

                    calendarEvents = calendarEvents
                        .Top(pageSize) //// TODO should this even be part of the chain? should you just preserve if `top` was called on you?
                        .OrderBy(calendarEvent => calendarEvent.Start.DateTime); //// TODO do we care about the order? because we don't have a way to get the order in the instances of series masters, right? without retrieving all of the master data?

                    var instanceEvents = await calendarEvents.Evaluate().ConfigureAwait(false);

                    if (this.filterConsistentAcrossInstancesAndNotSupportedByGraph != null)
                    {
                        instanceEvents = instanceEvents.Where(this.filterConsistentAcrossInstancesAndNotSupportedByGraph);
                    }

                    return instanceEvents;
                }

                private async Task<IQueryResult<IEither<OddTrotter.CalendarEvent, OddTrotter.CalendarEventTranslationError>, OddTrotter.PagingError>> GetSeriesEvents()
                {
                    //// TODO the querycontext needs to call this with `.Filter(CalendarSource.EndTimeLessThan(this.endTime.Value))` for it to work right now

                    var seriesEventMasters = await this.GetSeriesEventMasters().ConfigureAwait(false);
                    var mastersWithInstances = seriesEventMasters
                        .SelectAsync(
                            async seriesMasterOrTranslationError => await seriesMasterOrTranslationError
                                .SelectLeft(
                                    async seriesMaster =>
                                    {
                                        // this selector is trying to accomplish a lot; ultimately, the entire *method* is trying to get a series master and its first instance, so that we can see if the series has any instances in the time period that's been configured by the caller
                                        // by the time we get to this selector, we have the series masters, so we are now trying to get the first instance
                                        // once we "have" the instances (it is lazily evaluated), we are going to try to get the first one; *but* what if the first one has a translation error? well, we could just try to get the second one; *but*, what if there's a bug in our translation code? then *all* of the instances will have errors, and if its an unending series, we will loop forever trying to find the an instance that will never exist
                                        // so, we can't take the first non-error instance of *all* of the instances; we need to put a cap on it, so we use a `take`
                                        // then, we go ahead and get the first instance whether or not it has an error; we will use this in the case where *all* of the instances have an error
                                        // now that we have that in our back pocket just in case, we try to see if there are any *non-error* instances using a `where`; we get the first instance of *those*
                                        // if there are none, then we go back to the first error instance
                                        // and now we have the potential first instance, so we may return
                                        var instances = await this
                                            .GetInstancesInSeries(seriesMaster)
                                            .Take(100) //// TODO configure this;
                                            .ConfigureAwait(false);
                                        var potentialFirstInstance = await instances.FirstOrDefault(new Nothing()).ConfigureAwait(false);

                                        //// TODO perf-wise, this is no different from enumerable, but maybe you could do better
                                        var nonErrorInstance = await instances
                                            .Where(potentialInstance => potentialInstance.Apply(instance => true, error => false))
                                            .FirstOrDefault(new Nothing()).ConfigureAwait(false);
                                        if (!nonErrorInstance.TryGetRight(out _))
                                        {
                                            potentialFirstInstance = nonErrorInstance;
                                        }

                                        return
                                            (
                                                SeriesMaster: seriesMaster,
                                                PotentialFirstInstance: potentialFirstInstance
                                            );
                                    })
                                .ConfigureAwait(false))
                        .Select(
                            seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError
                                // we want to filter out series masters that don't have future instances (we *don't* want to filter errors, since they *might* represent future instances); we will do this later with a `tryselect`, so we need to get the `nothing` instances to the "right" side of the either; we are also looking to get non-error cases to the left side of the either; so, we should end up with something like `ieither<ieither<...<ieither<(seriesmaster, firstinstance), error>, error>,...> nothing>`
                                .LiftSequence() // pull the paging error out of the tuple
                                .Associate() // move the tuple left
                                .LiftSequence() // pull the nothing out of the tuple
                                .Associate() // move the tuple left
                                .LiftSequence() // pull the translation error out of the tuple
                                .Associate() // move the tuple left
                                .SelectRight(
                                    errorCases => errorCases
                                        .SelectRight(
                                            nothingOrErrors => nothingOrErrors
                                                .Swap() // move nothing to the right side
                                            ))
                                .Unassociate() // move nothing to the right
                                .Unassociate() // move nothing to the right
                                .SelectLeft( // get all of the eithers nested on the left
                                    seriesMasterPlusPontentialFirstInstanceOrErrorCases => seriesMasterPlusPontentialFirstInstanceOrErrorCases
                                        .Associate())
                                )
                        .TrySelect()
                        .Select(
                            seriesMasterPlusInstanceOrError => seriesMasterPlusInstanceOrError
                                .SelectLeft(
                                    seriesMasterPlusInstance =>
                                        // combine the series master and the first instance into a "canonical" calendar event; this allows the caller to see meaningful timestamps while preserving the "series" nature of the event (for things like canceling and accepting the event);
                                        // NOTE: there's an argument to be made that this class should actually return all future instances of the series event, and not preserve the data about the series, but i'm not clear what the design of the (non-graph) `calendarevent` class would look like in that case, for situations like canceling, declining, or accepting a series
                                        new OddTrotter.CalendarEvent( //// TODO
                                            seriesMasterPlusInstance.Item1.Id/*,
                                            seriesMasterPlusInstance.Item1.Subject,
                                            seriesMasterPlusInstance.Item1.Body,
                                            seriesMasterPlusInstance.Item2.Start,
                                            seriesMasterPlusInstance.Item1.IsCancelled,
                                            seriesMasterPlusInstance.Item1.Type,
                                            seriesMasterPlusInstance.Item2.End*/
                                            ))
                                .SelectRight(
                                    // reorder the error cases so that you can combine the different translation errors
                                    errorCases => errorCases
                                        .SelectRight(
                                            pagingOrTranslation => pagingOrTranslation
                                                .Swap()))
                                .SelectRight(
                                    errors => errors
                                        .SelectManyRight())
                                .SelectRight(
                                    translationErrorOrInstancePagingError => translationErrorOrInstancePagingError
                                        .Select(
                                            translationError => new OddTrotter.CalendarEventTranslationError(), //// TODO
                                            instancePagingError => new OddTrotter.CalendarEventTranslationError() //("TODO include the paging error and include everything we know about the series master")
                                        )
                                        .Coalesce()))
                        .SelectError(
                            seriesPagingError => new OddTrotter.PagingError() //// TODO
                            );

                    if (this.filterNotConsistentAcrossInstances != null)
                    {
                        mastersWithInstances = mastersWithInstances.Where(this.filterNotConsistentAcrossInstances);
                    }

                    return mastersWithInstances;
                }

                private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetInstancesInSeries(Graph.CalendarEvent seriesMaster)
                {
                    var startTime = this.startTime;
                    if (startTime == null)
                    {
                        //// TODO who should know to call the series event masters with `select=recurrence/range/startDate`?
                        startTime = seriesMaster.Recurrence.Range.Date.ToDateTime(TimeOnly.MinValue) - TimeSpan.FromDays(1); // we go backwards 1 day to account for any time zone (1 day is sufficient because there are no time zones more than 1 day apart)
                    }

                    var endTime = this.endTime;
                    if (endTime == null)
                    {
                        endTime = startTime + TimeSpan.FromDays(365); //// TODO make this configurable //// TODO you could also use the `recurrence` property to determine the end time, but the API is just awful...
                    }

                    return await this.GetInstancesInSeries(seriesMaster.Id, startTime.Value, endTime.Value);
                }

                private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetInstancesInSeries(string seriesMasterId, DateTime startTime, DateTime endTime)
                {
                    if (startTime == endTime)
                    {
                        return Enumerable.Empty<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>>().ToQueryResultAsync();
                    }

                    var sliceEndTime = Min(endTime, startTime + TimeSpan.FromDays(7)); //// TODO make the slice size configurable
                    return await this
                        .GetInstancesInSeriesSlice(seriesMasterId, startTime, sliceEndTime)
                        .Concat(
                            this.GetInstancesInSeries(seriesMasterId, sliceEndTime, endTime));
                }

                private async ITask<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetInstancesInSeriesSlice(string seriesMasterId, DateTime startTime, DateTime endTime)
                {
                    //// TODO you are going to use the `filterConsistentAcrossInstancesAndNotSupportedByGraph` when you get the instances of the series; there is a bit of magic here that, if you are given filters you don't understand, you are basically passing them to graph; so, let's say that the instance filter has something about start time, but it's nested or something, so you don't understand it in the `filter` method to pull out the `starttime` field; in that case, you will "simply" be slow, but still function, because you will get *all* the series mastsers, and then do the start time filtering on the instances themselves; the same will apply for anything else that could have been useful for performance (like endtime, or something that graph doesn't support, like subject filtering (actually, the subject filtering case will be more like "if the oddtrotter one understands it, we can do better performance, but if it doesn't, we will pass it through and graph won't understand it, so the call will fail", which isn't necessarily great, but the whole point is that we need to support *at least* what graph supports, and if you give stuff to us in a format that we understand, we do better)
                }

                private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetSeriesEventMasters()
                {
                    var calendarEvents = this
                        .graphCalendarEventsContext
                        .Filter(calendarEvent => calendarEvent.Type == "seriesMaster");

                    //// TODO make sure iscancelled can be called by the consumer

                    if (this.filterConsistentAcrossInstancesAndSupportedByGraph != null)
                    {
                        calendarEvents = calendarEvents.Filter(this.filterConsistentAcrossInstancesAndSupportedByGraph);
                    }

                    var seriesMasters = await calendarEvents.Evaluate().ConfigureAwait(false);

                    if (this.filterConsistentAcrossInstancesAndNotSupportedByGraph != null)
                    {
                        seriesMasters = seriesMasters.Where(this.filterConsistentAcrossInstancesAndNotSupportedByGraph);
                    }

                    return seriesMasters;
                }

                public OddTrotter.ICalendarEventsContext Filter(Expression<Func<OddTrotter.CalendarEvent, bool>> filter)
                {
                    Expression<Func<OddTrotter.CalendarEvent, bool>>? remainingFilter = filter;

                    ExtractStartTime(
                        remainingFilter, 
                        out var startTime,
                        out remainingFilter);
                    ExtractEndTime(
                        remainingFilter, 
                        out var endTime, 
                        out remainingFilter);
                    ExtractSeriesMasterFilter(
                        remainingFilter, 
                        out var filterConsistentAcrossInstancesAndSupportedByGraph, 
                        out var filterConsistentAcrossInstancesAndNotSupportedByGraph, 
                        out var filterNotConsistentAcrossInstances);

                    if (startTime == null)
                    {
                        startTime = this.startTime;
                    }
                    else if (this.startTime != null)
                    {
                        // they have called something like:
                        //
                        // ```
                        // events
                        //      .Filter(calendarEvent => calendarEvent.Start > DateTime.Parse("2026-04-24")
                        //      .Filter(calendarEvent => calendarEvent.Start > DateTime.Parse("2026-03-24");
                        // ```
                        //
                        // Since multiple calls to `Filter` are treated as a logical "and", we should take the greater value,
                        // since events that match the larger value also match the smaller value.
                        startTime = Max(startTime.Value, this.startTime.Value);
                    }

                    if (endTime == null)
                    {
                        endTime = this.endTime;
                    }
                    else if (this.endTime != null)
                    {
                        // they have called something like:
                        //
                        // ```
                        // events
                        //      .Filter(calendarEvent => calendarEvent.Start < DateTime.Parse("2026-04-24")
                        //      .Filter(calendarEvent => calendarEvent.Start < DateTime.Parse("2026-03-24");
                        // ```
                        //
                        // Since multiple calls to `Filter` are treated as a logical "and", we should take the lesser value,
                        // since events that match the smaller value also match the larger value.
                        endTime = Min(endTime.Value, this.endTime.Value);
                    }

                    if (filterConsistentAcrossInstancesAndSupportedByGraph == null)
                    {
                        filterConsistentAcrossInstancesAndSupportedByGraph = this.filterConsistentAcrossInstancesAndSupportedByGraph;
                    }
                    else if (this.filterConsistentAcrossInstancesAndSupportedByGraph != null)
                    {
                        //// TODO
                        /*filterConsistentAcrossInstancesAndSupportedByGraph = Expression.And(this.filterConsistentAcrossInstancesAndSupportedByGraph, filterConsistentAcrossInstancesAndSupportedByGraph);*/
                    }

                    if (filterConsistentAcrossInstancesAndNotSupportedByGraph == null)
                    {
                        filterConsistentAcrossInstancesAndNotSupportedByGraph = this.filterConsistentAcrossInstancesAndNotSupportedByGraph;
                    }
                    else if (this.filterConsistentAcrossInstancesAndNotSupportedByGraph != null)
                    {
                        filterConsistentAcrossInstancesAndNotSupportedByGraph = calendarEvent => this.filterConsistentAcrossInstancesAndNotSupportedByGraph(calendarEvent) && filterConsistentAcrossInstancesAndNotSupportedByGraph(calendarEvent);
                    }

                    if (filterNotConsistentAcrossInstances == null)
                    {
                        filterNotConsistentAcrossInstances = this.filterNotConsistentAcrossInstances;
                    }
                    else if (this.filterNotConsistentAcrossInstances != null)
                    {
                        //// TODO
                    }

                    return new CalendarEventsContext(
                        this.graphCalendarEventsContext,
                        startTime,
                        endTime,
                        filterConsistentAcrossInstancesAndSupportedByGraph,
                        filterConsistentAcrossInstancesAndNotSupportedByGraph,
                        filterNotConsistentAcrossInstances);
                }

                private static DateTime Max(DateTime first, DateTime second)
                {
                    if (first > second)
                    {
                        return first;
                    }
                    else
                    {
                        return second;
                    }
                }

                private static DateTime Min(DateTime first, DateTime second)
                {
                    if (first < second)
                    {
                        return first;
                    }
                    else
                    {
                        return second;
                    }
                }

                private static void ExtractStartTime(
                    Expression<Func<OddTrotter.CalendarEvent, bool>>? currentFilter, 
                    out DateTime? startTime, 
                    out Expression<Func<OddTrotter.CalendarEvent, bool>>? remainingFilter)
                {
                    if (currentFilter == null)
                    {
                        startTime = null;
                        remainingFilter = null;
                        return;
                    }

                    if (currentFilter.Parameters.Count == 1)
                    {
                        var parameterName = currentFilter.Parameters[0].Name;
                        if (parameterName != null)
                        {
                            if (parameterName.StartsWith(nameof(StartTimeGreaterThan)))
                            {
                                if (long.TryParse(parameterName.Substring(nameof(StartTimeGreaterThan).Length), out var startTimeTicks))
                                {
                                    startTime = new DateTime(startTimeTicks);
                                    remainingFilter = null;
                                    return;
                                }
                            }
                        }
                    }

                    startTime = null;
                    remainingFilter = currentFilter;
                    return;
                }

                private static void ExtractEndTime(
                    Expression<Func<OddTrotter.CalendarEvent, bool>>? currentFilter, 
                    out DateTime? endTime, 
                    out Expression<Func<OddTrotter.CalendarEvent, bool>>? remainingFilter)
                {
                }

                private static void ExtractSeriesMasterFilter(
                    Expression<Func<OddTrotter.CalendarEvent, bool>>? currentFilter,
                    out Expression<Func<Graph.CalendarEvent, bool>> filterConsistentAcrossInstancesAndSupportedByGraph,
                    out Func<Graph.CalendarEvent, bool> filterConsistentAcrossInstancesAndNotSupportedByGraph,
                    out Func<OddTrotter.CalendarEvent, bool>? filterNotConsistentAcrossInstances
                    )
                {
                    //// TODO as a result of the below, you should really rename the parameters to describe what they are instead of how they are used (i.e. supportedandconsistent isntead of seriesmasterfilter) because seriesmasterpredicate will need to be applied to instances as well
                    //// TODO if subject is tested and in a format that you can extract, you need to also apply it to the instances even though graph doesn't understand it; this is true for anything that you will filter series masters by, but that graph doesn't understand
                }

                public OddTrotter.ICalendarEventsContext OrderBy<TOrder>(Expression<Func<OddTrotter.CalendarEvent, TOrder>> orderBy)
                {
                    throw new NotImplementedException();
                }

                public OddTrotter.ICalendarEventsContext Top(uint top)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public OddTrotter.ICalendarContext Get()
        {
            return new CalendarContext(this.graphCalendarSource.Get());
        }

        private sealed class CalendarContext : OddTrotter.ICalendarContext
        {
            private readonly Graph.ICalendarContext graphCalendarContext;

            public CalendarContext(Graph.ICalendarContext graphCalendarContext)
            {
                this.graphCalendarContext = graphCalendarContext;
            }

            public Task<OddTrotter.Calendar> Evalaute()
            {
                throw new NotImplementedException();
            }

            public OddTrotter.ICalendarContext Select<TResult>(Expression<Func<OddTrotter.Calendar, TResult>> selector)
            {
                throw new NotImplementedException();
            }
        }

        internal static Expression<Func<CalendarEvent, bool>> StartTimeGreaterThan(DateTime dateTime)
        {
            Expression<Func<CalendarEvent, bool>> foo = calendarEvent => true;
            var ticks = Expression.Parameter(typeof(CalendarEvent), nameof(StartTimeGreaterThan) + dateTime.Ticks.ToString());
            foo.Update(foo.Body, new[] { ticks });

            return foo;
        }
    }

    internal static partial class Extensions
    {
        public static ITask<IQueryResult<TValue, TErrorResult>> SelectError<TValue, TErrorSource, TErrorResult>(
            this ITask<IQueryResult<TValue, TErrorSource>> queryResult,
            Func<TErrorSource, TErrorResult> selector)
        {
            throw new Exception("TODO");
        }

        public static IQueryResult<IEither<TValue, TDeserializationError>, TPagingError> Where<TValue, TDeserializationError, TPagingError>(
            this IQueryResult<IEither<TValue, TDeserializationError>, TPagingError> queryResult,
            Func<TValue, bool> predicate)
        {
            return queryResult.Where(either => !either.TryGetLeft(out var value) || predicate(value));
        }

        internal static async ITask<IQueryResult<TValue, TError>> Take<TValue, TError>(
            this Task<IQueryResult<TValue, TError>> queryResult,
            int count)
        {
            //// TODO note somewhere that if you find `count` elements before getting to the end, you don't end up preserving any `terror` that might have occurred

            return new TakeQueryResult<TValue, TError>(await queryResult.ConfigureAwait(false), count);
        }

        private sealed class TakeQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            private readonly IQueryResult<TValue, TError> queryResult;
            private readonly int count;

            public TakeQueryResult(
                IQueryResult<TValue, TError> queryResult,
                int count)
            {
                this.queryResult = queryResult;
                this.count = count;
            }

            public async ITask<IQueryResultNode<TValue, TError>> GetNodes()
            {
                return new Node(await this.queryResult.GetNodes().ConfigureAwait(false), this.count);
            }

            private sealed class Node : IQueryResultNode<TValue, TError>
            {
                private readonly IQueryResultNode<TValue, TError> queryResult;
                private readonly int count;

                public Node(
                    IQueryResultNode<TValue, TError> queryResult,
                    int count)
                {
                    this.queryResult = queryResult;
                    this.count = count;
                }

                public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TValue, TError>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    return this.queryResult.ApplyAsync<TResult, TContext, TContinuable>(
                        (element, ref context) =>
                        {
                            return leftMap(new Element(element, this.count), ref context);
                        },
                        (terminal, ref context) =>
                        {
                            return rightMap(terminal, ref context);
                        },
                        ref context);
                }

                private sealed class Element : IElement<TValue, TError>
                {
                    private readonly IElement<TValue, TError> element;
                    private readonly int count;

                    public Element(IElement<TValue, TError> element, int count)
                    {
                        this.element = element;
                        this.count = count;
                    }

                    public TValue Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    public async ITask<IQueryResultNode<TValue, TError>> Next()
                    {
                        return new Node(await this.element.Next().ConfigureAwait(false), count - 1);
                    }
                }
            }
        }


        internal static IQueryResult<TValue, TError> TrySelect<TValue, TError>(
            this IQueryResult<IEither<TValue, Nothing>, TError> queryResult)
        {
            return queryResult.TrySelect<IEither<TValue, Nothing>, TError, TValue>((either, [MaybeNullWhen(false)] out left) => either.TryGetLeft(out left));
        }

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

            public async ITask<IQueryResultNode<TResult, TError>> GetNodes()
            {
                return new QueryResultNode(await this.queryResult.GetNodes().ConfigureAwait(false), this.selector);
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

                    public async ITask<IQueryResultNode<TResult, TError>> Next()
                    {
                        return new QueryResultNode(await this.element.Next().ConfigureAwait(false), this.selector);
                    }
                }
            }
        }
    }
}
