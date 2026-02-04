namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class OdataRequest
    {
        internal OdataRequest(string httpVerb, string url, IEnumerable<HttpHeader> headers)
        {
            HttpVerb = httpVerb;
            Url = url;
            Headers = headers;
        }

        internal string HttpVerb { get; }       
        internal string Url { get; }
        public IEnumerable<HttpHeader> Headers { get; }
    }
}
