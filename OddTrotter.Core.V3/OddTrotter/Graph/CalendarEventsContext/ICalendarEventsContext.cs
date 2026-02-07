namespace OddTrotter.Graph.CalendarEventsContext
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    internal interface ICalendarEventsContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="ContextException">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        Task<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, PagingException>> Evaluate();
    }
}
