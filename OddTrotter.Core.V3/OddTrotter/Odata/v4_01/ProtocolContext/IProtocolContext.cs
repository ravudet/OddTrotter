namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "protocol" is around syntax (e.g. odata doesn't allow top-level collections)
    /// </summary>
    internal interface IProtocolContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        Task<OdataResponse> Send(OdataRequest request);
    }
}
