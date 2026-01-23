namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System.Threading.Tasks;

    internal interface IConventionContext
    {
        /// <summary>
        /// TODO error response //// TODO these should be individual exceptions (that *maybe* have a base type)
        /// TODO not odata response
        /// TODO not collection response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetCollectionResponse> GetCollection(GetCollectionRequest request);
    }
}
