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
            var verbReader = this.requestReader.Read();
            verbReader.Read(out var verb)


            ////requestReader.Move()
            ////throw new System.Exception("TODO");
        }
    }
}
