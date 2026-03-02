namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using OddTrotter.Calendar;

    internal sealed class OdataRequest
    {
        internal OdataRequest(HttpMethod httpMethod, Uri uri, IEnumerable<HttpHeader> headers)
        {
            HttpMethod = httpMethod;
            Uri = uri;
            Headers = headers;
        }

        internal HttpMethod HttpMethod { get; }       
        internal Uri Uri { get; }
        public IEnumerable<HttpHeader> Headers { get; }
    }
}
