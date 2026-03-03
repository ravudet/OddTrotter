namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class GetCollectionRequest //// TODO you need to start using the proper "multivalued" term when referring to collections
    {
        internal GetCollectionRequest(Uri url, IEnumerable<HttpHeader> headers)
        {
            this.Url = url;
            Headers = headers;
        }

        internal Uri Url { get; }

        internal IEnumerable<HttpHeader> Headers { get; }
    }
}
