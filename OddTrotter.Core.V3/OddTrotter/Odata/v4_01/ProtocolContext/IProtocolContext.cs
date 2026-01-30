namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Threading.Tasks;

    /// <summary>
    /// TODO "protocol" is around syntax (e.g. odata doesn't allow top-level collections)
    /// </summary>
    internal interface IProtocolContext
    {
        Task<OdataResponse> Send(OdataRequest request);
    }
}
