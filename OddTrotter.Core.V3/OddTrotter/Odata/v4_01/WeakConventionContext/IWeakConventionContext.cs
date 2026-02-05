namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "convention" is around semantics (e.g. we expected a collection but didn't receive one)
    /// 
    /// TODO "weak" means that we use .net types with odata language (e.g. nextlink, property names, etc)
    /// 
    /// TODO when do you want to apply an iedmmodel? i think you could have a model-based weakconventioncontext which recognizes that, e.g., the getcollectionrequest is a request to a single-valued endpoint; so maybe "model-based" is actually an implementation-specific construct and not present in the interface; double check this rationale with `istrongconventioncontext` as well (though at the "strong" level you would need something that maps` the edmmodel to the generic type parameter probably)
    /// </summary>
    internal interface IWeakConventionContext
    {
        /// <summary>
        /// TODO at what point should failure responses be surfaces as exceptions?
        /// 
        /// TODO at what point do we check if the request is actually to a collection? i.e. when do we require the model?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="WeakConventionContext">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        Task<GetCollectionResponse> GetCollection(GetCollectionRequest request);
    }
}
