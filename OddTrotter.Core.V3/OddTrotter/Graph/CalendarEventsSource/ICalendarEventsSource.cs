namespace OddTrotter.Graph.CalendarEventsSource
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using OddTrotter.Graph.CalendarEventsContext;

    internal interface ICalendarEventsSource<TGetContext>
        where TGetContext : ICalendarEventsContext<TGetContext>
    {
        TGetContext GenerateGetRequest();
    }

    internal interface ICalendarEventsSource
    {
        ICalendarEventsContext Get();

        ICalendarEventSource Get(string id);
    }

    internal interface ICalendarEventSource
    {
        ICalendarEventContext Get();

        ICalendarEventsSource Instances(DateTime startDateTime, DateTime endTime);
    }

    internal interface ICalendarEventContext
    {
        /// <summary>
        ///  //// TODO do you want to split httpexception into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// </summary>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception>
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="ContextException">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        /// <exception cref="CalendarEventTranslationException"></exception>
        Task<CalendarEvent> Evaluate();

        ICalendarEventsContext Compute<TResult>(Expression<Func<CalendarEvent, TResult>> computation, string dynamicPropertyName); //// TODO this doesn't really make sense as written, but the point is to demonstrate that the single-valued query options should be available here
    }
}
