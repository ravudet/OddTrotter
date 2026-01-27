namespace OddTrotter.Odata.v4_01.Reader
{
    internal sealed class NullToken
    {
        private NullToken()
        {
        }

        public static NullToken Instance { get; } = new NullToken();
    }
}
