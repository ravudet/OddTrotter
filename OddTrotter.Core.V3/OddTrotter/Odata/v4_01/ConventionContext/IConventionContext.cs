namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System.Threading.Tasks;

    internal interface IConventionContext
    {
        Task<GetCollectionResponse> GetCollection(GetCollectionRequest request);
    }
}
