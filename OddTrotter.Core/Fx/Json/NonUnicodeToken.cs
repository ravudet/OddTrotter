namespace Fx.Json
{
    public readonly ref struct NonUnicodeToken
    {
        public static bool TryCreate(byte @char, out NonUnicodeToken nonUnicodeToken)
        {
            switch (@char)
            {
                case 0x22:
                case 0x5C:
                case 0x2F:
                case 0x62:
                case 0x66:
                case 0x6E:
                case 0x72:
                case 0x74:
                    nonUnicodeToken = new NonUnicodeToken(@char);
                    return true;
                default:
                    nonUnicodeToken = default;
                    return false;
            }
        }

        private NonUnicodeToken(byte @char)
        {
            Char = @char;
        }

        public byte Char { get; }
    }
}
