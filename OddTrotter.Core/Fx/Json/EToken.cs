namespace Fx.Json
{
    public readonly ref struct EToken
    {
        public static bool TryCreate(byte e, out EToken eToken)
        {
            if (e != 'e' && e != 'E')
            {
                eToken = default;
                return false;
            }

            eToken = new EToken(e);
            return true;
        }

        private EToken(byte e)
        {
            E = e;
        }

        public byte E { get; }
    }
}
