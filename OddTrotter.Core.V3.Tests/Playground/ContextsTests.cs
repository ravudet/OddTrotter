namespace OddTrotter.Graph.CalendarEventsSource.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

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

    internal sealed class Calendar
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
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

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
                private readonly bool? isCancelled;
                private readonly Func<OddTrotter.CalendarEvent, bool>? seriesMasterPredicate;

                public CalendarEventsContext(Graph.ICalendarEventsContext graphCalendarEventsContext)
                    : this(
                          graphCalendarEventsContext,
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
                    bool? isCancelled, 
                    Func<OddTrotter.CalendarEvent, bool>? seriesMasterPredicate)
                {
                    this.graphCalendarEventsContext = graphCalendarEventsContext;

                    this.startTime = startTime;
                    this.endTime = endTime;
                    this.isCancelled = isCancelled;
                    this.seriesMasterPredicate = seriesMasterPredicate;
                }

                public async ITask<IQueryResult<IEither<OddTrotter.CalendarEvent, OddTrotter.CalendarEventTranslationError>, OddTrotter.PagingError>> Evaluate()
                {
                    //// TODO you are here
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

                private async ITask<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetInstanceEvents()
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

                    if (this.isCancelled != null)
                    {
                        calendarEvents = calendarEvents
                            .Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled);
                    }

                    calendarEvents = calendarEvents
                        .Top(pageSize) //// TODO should this even be part of the chain? should you just preserve if `top` was called on you?
                        .OrderBy(calendarEvent => calendarEvent.Start.DateTime);

                    return await calendarEvents.Evaluate().ConfigureAwait(false);
                }

                private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetSeriesEvents()
                {
                    //// TODO you could actually use recurrence.range.startdate for series events to find the "earliest" instance; or, if `filter(event => event.starttime > {foo})` has been called, just use `{foo}`
                    //// TODO the querycontext needs to call this with `.Filter(CalendarSource.EndTimeLessThan(this.endTime.Value))` for it to work right now
                    

                }

                private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationError>, Graph.PagingError>> GetSeriesEventMasters()
                {
                    //// TODO use the predicate *here* if avaialbe instead of the caller (better for potential re-use)
                }

                public OddTrotter.ICalendarEventsContext Filter(Expression<Func<OddTrotter.CalendarEvent, bool>> filter)
                {
                    DateTime? startTime = null;
                    if (filter.Parameters.Count == 1)
                    {
                        var parameterName = filter.Parameters[0].Name;
                        if (parameterName != null)
                        {
                            if (parameterName.StartsWith(nameof(StartTimeGreaterThan)))
                            {
                                if (long.TryParse(parameterName.Substring(nameof(StartTimeGreaterThan).Length), out var startTimeTicks))
                                {
                                    startTime = new DateTime(startTimeTicks);
                                }
                            }
                        }
                    }

                    if (startTime == null)
                    {
                        throw new NotImplementedException("TODO");
                    }

                    //// TODO you need to check if the filter applies to series masters in a special way; you need to apply that to series masters and instance events
                    //// TODO you need to passthrough other filters

                    return new CalendarEventsContext(
                        this.graphCalendarEventsContext,
                        startTime,
                        this.endTime,
                        this.isCancelled,
                        this.seriesMasterPredicate);
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
    }
}
