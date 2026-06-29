namespace Fx.Json
{
    public readonly ref struct UnescapedCharToken
    {
        public static bool TryCreate(byte @char, out UnescapedCharToken charToken)
        {
            if (!UnescapedCharToken.IsValid(@char))
            {
                charToken = default;
                return false;
            }

            charToken = new UnescapedCharToken(@char);
            return true;
        }

        private static bool IsValid(byte @char)
        {
            return
                (@char >= 0x20 && @char <= 0x21) ||
                (@char >= 0x23 && @char <= 0x5B) ||
                (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
        }

        private UnescapedCharToken(byte @char)
        {
            Char = @char;
        }

        public byte Char { get; }
    }
}
