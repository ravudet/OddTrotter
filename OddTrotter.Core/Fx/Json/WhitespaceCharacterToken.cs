namespace Fx.Json
{
    public readonly struct WhitespaceCharacterToken
    {
        public static bool TryCreate(byte @char, out WhitespaceCharacterToken whitespaceToken)
        {
            switch (@char)
            {
                case 0x20:
                case 0x09:
                case 0x0A:
                case 0x0D:
                    whitespaceToken = new WhitespaceCharacterToken(@char);
                    return true;
                default:
                    whitespaceToken = default;
                    return false;
            }
        }

        private WhitespaceCharacterToken(byte @char)
        {
            this.Char = @char;
        }

        public byte Char { get; }
    }
}
