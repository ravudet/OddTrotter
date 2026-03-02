namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using OddTrotter.Calendar;

    internal sealed class OdataRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpMethod"/> or <paramref name="uri"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        internal OdataRequest(HttpMethod httpMethod, Uri uri, IEnumerable<HttpHeader> headers)
        {
            ArgumentNullException.ThrowIfNull(httpMethod);
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(headers);

            HttpMethod = httpMethod;
            Uri = uri;
            Headers = headers;
        }

        internal HttpMethod HttpMethod { get; }       
        internal Uri Uri { get; }
        public IEnumerable<HttpHeader> Headers { get; }
    }
}
