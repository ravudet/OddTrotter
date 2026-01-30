namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "convention" is around semantics (e.g. we expected a collection but didn't receive one)
    /// 
    /// TODO "weak" means that we use .net types with odata language (e.g. nextlink, property names, etc)
    /// 
    /// TODO when do you want to apply an iedmmodel? i think you could have a model-based weakconventioncontext which recognizes that, e.g., the getcollectionrequest is a request to a single-valued endpoint; so maybe "model-based" is actually an implementation-specific construct and not present in the interface; double check this rationale with `istrongconventioncontext` as well (though at the "strong" level you would need something that maps the edmmodel to the generic type parameter probably)
    /// </summary>
    internal interface IWeakConventionContext
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
        Task<GetCollectionResponse> GetCollection(GetCollectionRequest request);
    }
}
