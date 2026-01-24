namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class RequestReaderWriter : IRequestWriter
    {
        private readonly IRequestReader requestReader;

        public RequestReaderWriter(IRequestReader requestReader)
        {
            this.requestReader = requestReader;
        }

        public IVerbWriter Write()
        {
            requestReader.Move()
        }
    }
}
