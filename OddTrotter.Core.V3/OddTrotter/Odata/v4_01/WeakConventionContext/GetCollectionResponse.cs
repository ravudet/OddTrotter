namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;
    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal sealed class GetCollectionResponse
    {
        internal GetCollectionResponse(string httpStatusCode, IEnumerable<HttpHeader> headers, IEnumerable<CollectionElement> elements)
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
            Elements = elements;
        }

        public string HttpStatusCode { get; }
        public IEnumerable<HttpHeader> Headers { get; }
        public IEnumerable<CollectionElement> Elements { get; }
    }

    internal sealed class CollectionElement
    {
    }
}
