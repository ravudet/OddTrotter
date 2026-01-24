namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;

    internal sealed class HttpClientConventionContext : IConventionContext //// TODO call this one weak and the other one strong? (as in weak typing and strong typing?)
    {
        private readonly Func<HttpRequestMessage, RequestReader> requestReaderFactory;

        internal HttpClientConventionContext(Func<HttpRequestMessage, RequestReader> requestReaderFactory)
        {
            this.requestReaderFactory = requestReaderFactory;
        }

        public Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            //// TODO do you want a level below this that has an odatarequest and an odataresponse?

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.Url))
            {
                //// TODO headers
                var requestReader = this.requestReaderFactory(httpRequestMessage);

            }
        }
    }
}
