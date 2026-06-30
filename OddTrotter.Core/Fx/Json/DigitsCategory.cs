namespace Fx.Json
{
    using System;

    public readonly ref struct DigitsCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static DigitsCategory<TNextReader> None()
        {
            return new DigitsCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static DigitsCategory<TNextReader> Some()
        {
            return new DigitsCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> none,
            Func<DigitReader<DigitsReader<TNextReader>>?, TResult> some)
        {
            switch (this.type)
            {
                case Type.None:
                    return none(default);
                case Type.Some:
                    return some(default);
                default:
                    throw new Exception("TODO bug");
            }
        }

        public bool TryNone(out TNextReader? nextReader)
        {
            nextReader = default;
            return this.Apply(
                _ => true,
                _ => false);
        }

        public bool TrySome(out DigitReader<DigitsReader<TNextReader>>? digitReader)
        {
            digitReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
