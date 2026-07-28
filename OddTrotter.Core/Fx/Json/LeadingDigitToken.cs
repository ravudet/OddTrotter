namespace Fx.Json
{
    public readonly struct LeadingDigitToken
    {
        public static bool TryCreate(byte digit, out LeadingDigitToken leadingDigitToken)
        {
            if (digit < '1' || digit > '9')
            {
                leadingDigitToken = default;
                return false;
            }

            leadingDigitToken = new LeadingDigitToken(digit);
            return true;
        }

        private LeadingDigitToken(byte digit)
        {
            Digit = digit;
        }
        public byte Digit { get; }
    }
}
