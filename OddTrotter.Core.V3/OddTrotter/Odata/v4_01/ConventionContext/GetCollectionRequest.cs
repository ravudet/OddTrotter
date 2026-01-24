namespace OddTrotter.Odata.v4_01.ConventionContext
{
    internal sealed class GetCollectionRequest
    {
        internal GetCollectionRequest(string url)
        {
            this.Url = url;
        }

        internal string Url { get; }
    }
}
