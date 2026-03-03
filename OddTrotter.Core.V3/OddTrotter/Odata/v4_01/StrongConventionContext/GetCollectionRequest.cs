namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class GetCollectionRequest<T>
    {
        internal GetCollectionRequest(Uri url, IEnumerable<HttpHeader> headers)
        {
            this.Url = url;
            Headers = headers;
        }

        internal Uri Url { get; }
        public IEnumerable<HttpHeader> Headers { get; }
    }
}
