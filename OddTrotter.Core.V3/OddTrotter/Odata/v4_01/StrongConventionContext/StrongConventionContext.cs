namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.WeakConventionContext;

    internal sealed class StrongConventionContext<T> : IStrongConventionContext<T>
    {
        private readonly IWeakConventionContext weakConventionContext;

        public StrongConventionContext(IWeakConventionContext weakConventionContext)
        {
            this.weakConventionContext = weakConventionContext;
        }

        public async Task<GetCollectionResponse<T>> GetCollection(GetCollectionRequest<T> request)
        {
            var weakConventionRequest = new GetCollectionRequest(
                request.Url,
                request.Headers);
            var weakConventionResponse = await this.weakConventionContext.GetCollection(weakConventionRequest).ConfigureAwait(false);


        }
    }
}
