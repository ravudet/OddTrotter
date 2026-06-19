namespace Fx.Json
{
    public readonly struct DigitToken //// TODO should all of your non-continuable tokens be ref struct?
    {
        public static bool TryCreate(byte digit, out DigitToken digitToken)
        {
            if (digit < '0' || digit > '9')
            {
                digitToken = default;
                return false;
            }

            digitToken = new DigitToken(digit);
            return true;
        }

        private DigitToken(byte digit)
        {
            Digit = digit;
        }

        public byte Digit { get; }
    }
}
