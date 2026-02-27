namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Net.Http;

    internal abstract class PagingError
    {
        private PagingError(Uri nextLink)
        {
            NextLink = nextLink;
        }

        Uri NextLink { get; }

        internal sealed class Read : PagingError
        {
            internal Read(Uri nextLink, ReadException exception)
                : base(nextLink)
            {
                Exception = exception;
            }

            public ReadException Exception { get; }
        }

        internal sealed class Write : PagingError
        {
            internal Write(Uri nextLink, WriteException exception)
                : base(nextLink)
            {
                Exception = exception;
            }

            public WriteException Exception { get; }
        }

        internal sealed class Http : PagingError
        {
            internal Http(Uri nextLink, HttpRequestException exception)
                : base(nextLink)
            {
                Exception = exception;
            }

            public HttpRequestException Exception { get; }
        }

        internal sealed class Context : PagingError
        {
            internal Context(Uri nextLink, ContextException exception)
                : base(nextLink)
            {
                Exception = exception;
            }

            public ContextException Exception { get; }
        }

        internal sealed class Unauthorized : PagingError
        {
            internal Unauthorized(Uri nextLink, UnauthorizedAccessTokenException exception)
                : base(nextLink)
            {
                Exception = exception;
            }

            public UnauthorizedAccessTokenException Exception { get; }
        }
    }
}
