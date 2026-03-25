namespace OddTrotter.Graph.CalendarEventsSource.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

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

    internal interface ICalendarEventsContext
    {
        Task<Fx.QueryContext.IQueryResult<CalendarEvent, CalendarEventTranslationError>> Evaluate();

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

        //// public IEnumerable<CalendarEvent> Instances { get; }
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private CalendarEvent()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
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
        private CalendarEventTranslationError()
        {
        }
    }

    internal interface ICalendarEventsContext
    {
        Task<Fx.QueryContext.IQueryResult<CalendarEvent, CalendarEventTranslationError>> Evaluate();

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

    using Fx.QueryContext;

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

                public CalendarEventsContext(Graph.ICalendarEventsContext graphCalendarEventsContext)
                {
                    this.graphCalendarEventsContext = graphCalendarEventsContext;
                }

                public Task<IQueryResult<OddTrotter.CalendarEvent, OddTrotter.CalendarEventTranslationError>> Evaluate()
                {
                    //// TODO these need to return queryresults
                    return this.graphCalendarEventsContext.Evaluate();
                }

                public OddTrotter.ICalendarEventsContext Filter(Expression<Func<OddTrotter.CalendarEvent, bool>> filter)
                {
                    throw new NotImplementedException();
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
    }
}
