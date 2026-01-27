namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class FalseToken
    {
        private FalseToken()
        {
        }

        public static FalseToken Instance { get; } = new FalseToken();
    }
}
