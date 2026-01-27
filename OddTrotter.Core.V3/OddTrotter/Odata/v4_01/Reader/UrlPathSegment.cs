namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class UrlPathSegment
    {
        internal UrlPathSegment(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }
}
