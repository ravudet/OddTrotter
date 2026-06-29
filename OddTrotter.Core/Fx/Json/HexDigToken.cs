namespace Fx.Json
{
    public readonly ref struct HexDigToken
    {
        public static bool TryCreate(byte digit, out HexDigToken hexDigToken)
        {
            if (
                (digit >= 0x30 && digit <= 0x39) || 
                (digit >= 0x41 && digit <= 0x46) ||
                (digit >= 0x61 && digit <= 0x66))
            {
                hexDigToken = new HexDigToken(digit);
                return true;
            }

            hexDigToken = default;
            return false;
        }

        private HexDigToken(byte digit)
        {
            Digit = digit;
        }

        public byte Digit { get; }
    }
}
