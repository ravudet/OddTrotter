namespace OddTrotter.Graph.CalendarEventsSource
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using OddTrotter.Graph.CalendarEventsContext;
    using OddTrotter.Odata.v4_01.StrongConventionContext;

    internal sealed class CalendarSource : ICalendarSource
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri rootCalendarUri;

        internal CalendarSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarUri)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarUri = rootCalendarUri;
        }

        public ICalendarEventsSource Events()
        {
            return new CalendarEventsSource(
                this.strongConventionContext, 
                new Uri(this.rootCalendarUri, "events"));
        }
    }

    internal sealed class CalendarEventsSource : ICalendarEventsSource
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri rootCalendarEventsUri;

        internal CalendarEventsSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventsUri)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarEventsUri = rootCalendarEventsUri;
        }

        public ICalendarEventsContext Get()
        {
            return new CalendarEventsContext(this.strongConventionContext, this.rootCalendarEventsUri);
        }

        public ICalendarEventSource Get(string id)
        {
            return new CalendarEventSource(
                this.strongConventionContext, 
                new Uri(this.rootCalendarEventsUri, id));
        }
    }

    internal sealed class CalendarEventSource : ICalendarEventSource
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri rootCalendarEventUri;

        internal CalendarEventSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventUri)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarEventUri = rootCalendarEventUri;
        }

        public ICalendarEventContext Get()
        {
            return new CalendarEventContext(this.strongConventionContext, this.rootCalendarEventUri);
        }

        public ICalendarEventsSource Instances(DateTime startDateTime, DateTime endTime)
        {
            return new CalendarEventsSource(
                this.strongConventionContext,
                new Uri(this.rootCalendarEventUri, $"instances?startDateTime={startDateTime.ToString()}&endTime={endTime.ToString()}"));
        }
    }

    internal sealed class CalendarEventContext : ICalendarEventContext
    {
        internal CalendarEventContext(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventUri)
        {
        }

        public ICalendarEventsContext Compute<TResult>(Expression<Func<CalendarEvent, TResult>> computation, string dynamicPropertyName)
        {
            throw new NotImplementedException();
        }

        public Task<CalendarEvent> Evaluate()
        {
            throw new NotImplementedException();
        }
    }
}
