namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class TrueToken
    {
        private TrueToken()
        {
        }

        public static TrueToken Instance { get; } = new TrueToken();
    }
}
