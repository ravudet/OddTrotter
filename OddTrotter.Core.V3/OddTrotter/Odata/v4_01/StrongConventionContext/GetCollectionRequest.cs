namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    internal sealed class GetCollectionRequest<T>
    {
        internal GetCollectionRequest(string url)
        {
            this.Url = url;
        }

        internal string Url { get; }
    }
}
