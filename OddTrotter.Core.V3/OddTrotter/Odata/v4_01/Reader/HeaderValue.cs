namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class HeaderValue
    {
        internal HeaderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
