namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class GetCollectionRequest<T>
    {
        internal GetCollectionRequest(string url, IEnumerable<HttpHeader> headers)
        {
            this.Url = url;
            Headers = headers;
        }

        internal string Url { get; }
        public IEnumerable<HttpHeader> Headers { get; }
    }
}
