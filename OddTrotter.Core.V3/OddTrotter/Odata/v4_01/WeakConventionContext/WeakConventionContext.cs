namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal sealed class WeakConventionContext : IWeakConventionContext
    {
        private readonly IProtocolContext protocolContext;

        public WeakConventionContext(IProtocolContext protocolContext)
        {
            this.protocolContext = protocolContext;
        }

        public async Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            var odataRequest = new OdataRequest(
                "GET",
                request.Url,
                request.Headers);
            var odataResponse = await this.protocolContext.Send(odataRequest).ConfigureAwait(false);

            return new GetCollectionResponse();
        }
    }
}
