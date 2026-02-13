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
        private readonly string accessToken;

        internal CalendarSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarUri,
            string accessToken)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarUri = rootCalendarUri;
            this.accessToken = accessToken;
        }

        public ICalendarEventsSource Events()
        {
            return new CalendarEventsSource(
                this.strongConventionContext, 
                new Uri(this.rootCalendarUri, "events"), 
                this.accessToken);
        }
    }

    internal sealed class CalendarEventsSource : ICalendarEventsSource
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri rootCalendarEventsUri;
        private readonly string accessToken;

        internal CalendarEventsSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventsUri,
            string accessToken)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarEventsUri = rootCalendarEventsUri;
            this.accessToken = accessToken; //// TODO this should really be abstracted in the strong convention context somehow
        }

        public ICalendarEventsContext Get()
        {
            return new CalendarEventsContext(this.strongConventionContext, this.rootCalendarEventsUri, this.accessToken);
        }

        public ICalendarEventSource Get(string id)
        {
            return new CalendarEventSource(
                this.strongConventionContext, 
                new Uri(this.rootCalendarEventsUri, id), 
                this.accessToken);
        }
    }

    internal sealed class CalendarEventSource : ICalendarEventSource
    {
        private readonly IStrongConventionContext<CalendarEvent> strongConventionContext;
        private readonly Uri rootCalendarEventUri;
        private readonly string accessToken;

        internal CalendarEventSource(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventUri,
            string accessToken)
        {
            this.strongConventionContext = strongConventionContext;
            this.rootCalendarEventUri = rootCalendarEventUri;
            this.accessToken = accessToken;
        }

        public ICalendarEventContext Get()
        {
            return new CalendarEventContext(this.strongConventionContext, this.rootCalendarEventUri, this.accessToken);
        }

        public ICalendarEventsSource Instances(DateTime startDateTime, DateTime endTime)
        {
            return new CalendarEventsSource(
                this.strongConventionContext,
                new Uri(this.rootCalendarEventUri, $"instances?startDateTime={startDateTime.ToString()}&endTime={endTime.ToString()}"),
                this.accessToken);
        }
    }

    internal sealed class CalendarEventContext : ICalendarEventContext
    {
        internal CalendarEventContext(
            IStrongConventionContext<CalendarEvent> strongConventionContext,
            Uri rootCalendarEventUri,
            string accessToken)
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
