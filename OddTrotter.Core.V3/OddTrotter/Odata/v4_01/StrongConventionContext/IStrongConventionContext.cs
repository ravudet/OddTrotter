namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "convention" is around semantics (e.g. we expected a collection but didn't receive one)
    /// 
    /// TODO "strong" means that we use .net types with model based names (e.g. strongly typed properties); this could also be considered "deserialization"
    /// </summary>
    internal interface IStrongConventionContext
    {
        /// <summary>
        /// TODO error response //// TODO these should be individual exceptions (that *maybe* have a base type)
        /// TODO not odata response
        /// TODO not collection response
        /// 
        /// TODO at what point do we check if the request is actually to a collection? i.e. when do we require the model?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetCollectionResponse<T>> GetCollection<T>(GetCollectionRequest<T> request);
    }
}
