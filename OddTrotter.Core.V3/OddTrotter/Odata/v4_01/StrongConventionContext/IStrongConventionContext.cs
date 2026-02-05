namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "convention" is around semantics (e.g. we expected a collection but didn't receive one)
    /// 
    /// TODO "strong" means that we use .net types with model based names (e.g. strongly typed properties); this could also be considered "deserialization"
    /// </summary>
    internal interface IStrongConventionContext<T>
    {
        /// <summary>
        /// TODO at what point should failure responses be surfaces as exceptions?
        /// 
        /// TODO at what point do we check if the request is actually to a collection? i.e. when do we require the model?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetCollectionResponse<T>> GetCollection(GetCollectionRequest<T> request);
    }
}
