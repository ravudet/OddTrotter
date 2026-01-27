namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System.Net.Http;

    internal sealed class ResponseReader : IResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal ResponseReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public IHeadersReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeadersReader : IHeadersReader
    {
        public HeadersToken Read()
        {
            throw new System.NotImplementedException();
        }
    }
}
