namespace OddTrotter.CalendarV1.Tokenization.ReaderImplementations
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Readers;

    public sealed class GetResponseReader : IGetResponseReader
    {
        public GetResponseReader(HttpResponseMessage httpResponseMessage)
        {
        }

        public ValueTask Read()
        {
            throw new System.NotImplementedException();
        }

        public IGetResponseHeadersReader TryMoveNext(out bool moved)
        {
            throw new System.NotImplementedException();
        }
    }
}
