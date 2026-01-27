namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class StringToken
    {
        internal StringToken(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
