namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Threading.Tasks;

    internal interface IProtocolContext
    {
        Task<OdataResponse> GetCollection(OdataRequest request);
    }
}
