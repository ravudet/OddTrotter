using System.Net.Http;

namespace OddTrotter.Graph.CalendarEventsContext
{
    internal abstract class PagingError
    {
        private PagingError()
        {
        }

        internal sealed class Read : PagingError
        {
            internal Read(ReadException exception)
            {
                Exception = exception;
            }

            public ReadException Exception { get; }
        }

        internal sealed class Write : PagingError
        {
            internal Write(WriteException exception)
            {
                Exception = exception;
            }

            public WriteException Exception { get; }
        }

        internal sealed class Http : PagingError
        {
            internal Http(HttpRequestException exception)
            {
                Exception = exception;
            }

            public HttpRequestException Exception { get; }
        }

        internal sealed class Context : PagingError
        {
            internal Context(ContextException exception)
            {
                Exception = exception;
            }

            public ContextException Exception { get; }
        }

        internal sealed class Unauthorized : PagingError
        {
            internal Unauthorized(UnauthorizedAccessTokenException exception)
            {
                Exception = exception;
            }

            public UnauthorizedAccessTokenException Exception { get; }
        }
    }
}
