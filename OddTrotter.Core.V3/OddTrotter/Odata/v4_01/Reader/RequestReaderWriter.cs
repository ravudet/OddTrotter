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
            //// TODO add the "apply" methods to the tokens before implementing this

            ////requestReader.Move()
            throw new System.Exception("TODO");
        }
    }
}
