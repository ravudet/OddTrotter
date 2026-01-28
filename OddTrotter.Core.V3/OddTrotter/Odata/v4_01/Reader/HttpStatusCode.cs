namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class HttpStatusCode
    {
        internal HttpStatusCode(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
