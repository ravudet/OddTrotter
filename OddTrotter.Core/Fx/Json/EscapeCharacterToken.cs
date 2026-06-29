namespace Fx.Json
{
    public readonly ref struct EscapeCharacterToken
    {
        public static bool TryCreate(byte escapeCharacter, out EscapeCharacterToken escapeCharacterToken)
        {
            if (escapeCharacter != 0x5C)
            {
                return false;
            }

            return true;
        }
    }
}
