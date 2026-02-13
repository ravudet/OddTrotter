namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    using OddTrotter.Graph.CalendarEventsSource;

    internal interface ICalendarSource
    {
        ICalendarEventsSource Events();
    }

    internal interface ICalendarEventsContext<out TCalendarEventsContext> where TCalendarEventsContext : ICalendarEventsContext<TCalendarEventsContext>
    {
        /// <summary>
        ///  //// TODO do you want to split httpexception into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// </summary>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception>
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="ContextException">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate();

        //// TODO you're really avoiding doing the `select` stuff because of the type system issues (like, what should the "not present" properties look like in the return value? and should the client be expected to check that stuff? (i.e. can you strongly type it somehow))

        TCalendarEventsContext Filter(Expression<Func<CalendarEvent, bool>> filter);

        TCalendarEventsContext Top(int top);

        TCalendarEventsContext OrderBy<TOrder>(Expression<Func<CalendarEvent, TOrder>> orderBy);
    }

    internal interface ICalendarEventsContext : ICalendarEventsContext<ICalendarEventsContext>
    {
    }
}
