namespace Fx.Json
{
    using System;

    public readonly ref struct ExpSignToken
    {
        private enum Type
        {
            Absent = 1,
            Positive,
            Negative,
        }

        private Type type { get; init; }

        public static ExpSignToken Absent()
        {
            return new ExpSignToken()
            {
                type = Type.Absent,
            };
        }

        public static ExpSignToken Positive()
        {
            return new ExpSignToken()
            {
                type = Type.Positive,
            };
        }

        public static ExpSignToken Negative()
        {
            return new ExpSignToken()
            {
                type = Type.Negative,
            };
        }

        public TResult Apply<TResult>(
            Func<TResult> absent,
            Func<TResult> positive,
            Func<TResult> negative)
        {
            switch (this.type)
            {
                case Type.Absent:
                    return absent();
                case Type.Positive:
                    return positive();
                case Type.Negative: 
                    return negative();
                default:
                    throw new Exception("TODO bug");
            }
        }
    }
}
