namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;

    internal sealed class HttpClientConventionContext : IConventionContext //// TODO call this one weak and the other one strong? (as in weak typing and strong typing?)
    {
        private readonly Func<HttpRequestMessage, IRequestReader> requestReaderFactory;

        internal HttpClientConventionContext(Func<HttpRequestMessage, IRequestReader> requestReaderFactory)
        {
            this.requestReaderFactory = requestReaderFactory;
        }

        public Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            //// TODO protocol should mean syntax and convention should mean semantics (i.e. it was syntactically an odata response (protocol), but semantically, it was supposed to be a collection, and it wasn't (convention))


            //// TODO do you want a level below this that has an odatarequest and an odataresponse?

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.Url))
            {
                //// TODO headers
                



                var requestReader = this.requestReaderFactory(httpRequestMessage);

            }
        }
    }
}
