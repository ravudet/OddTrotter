namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System.Threading.Tasks;

    using OddTrotter.Calendar;

    internal sealed class HttpClientConventionContext : IConventionContext //// TODO call this one weak and the other one strong? (as in weak typing and strong typing?)
    {
        private readonly IHttpClient httpClient;

        internal HttpClientConventionContext(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
