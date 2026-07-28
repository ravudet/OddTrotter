namespace Fx.Json
{
    public readonly struct SignToken
    {
        private enum Type
        {
            Absent = 1,
            Negative,
        }

        private Type type { get; init; }

        public static SignToken Absent()
        {
            return new SignToken()
            {
                type = Type.Absent,
            };
        }

        public static SignToken Negative()
        {
            return new SignToken()
            {
                type = Type.Negative,
            };
        }
    }
}
