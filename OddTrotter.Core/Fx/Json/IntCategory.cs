namespace Fx.Json
{
    using System;

    public readonly ref struct IntCategory<TNextReader>
    {
        private enum Type
        {
            Zero = 1,
            NonZero,
        }

        private Type type { get; init; }

        public static IntCategory<TNextReader> Zero()
        {
            return new IntCategory<TNextReader>()
            {
                type = Type.Zero,
            };
        }

        public static IntCategory<TNextReader> NonZero()
        {
            return new IntCategory<TNextReader>()
            {
                type = Type.NonZero,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> zero,
            Func<LeadingDigitReader<DigitsReader<TNextReader>>?, TResult> nonZero)
        {
            switch (this.type)
            {
                case Type.Zero:
                    return zero(default);
                case Type.NonZero:
                    return nonZero(default);
                default:
                    throw new Exception("TODO bug");
            }
        }
    }
}
