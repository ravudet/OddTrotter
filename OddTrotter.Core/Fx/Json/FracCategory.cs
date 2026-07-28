namespace Fx.Json
{
    using System;

    public readonly struct FracCategory<TNextReader>
    {
        private enum Type
        {
            Absent = 1,
            Present,
        }

        private Type type { get; init; }

        public static FracCategory<TNextReader> Absent()
        {
            return new FracCategory<TNextReader>()
            {
                type = Type.Absent,
            };
        }

        public static FracCategory<TNextReader> Present()
        {
            return new FracCategory<TNextReader>()
            {
                type = Type.Present,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> absent,
            Func<DigitReader<DigitsReader<TNextReader>>?, TResult> present)
        {
            switch (this.type)
            {
                case Type.Absent:
                    return absent(default);
                case Type.Present:
                    return present(default);
                default:
                    throw new Exception("TODO bug");
            }
        }

        public bool TryAbsent(out TNextReader? nextReader)
        {
            nextReader = default;
            return this.Apply(
                _ => true,
                _ => false);
        }

        public bool TryPresent(out DigitReader<DigitsReader<TNextReader>>? digitReader)
        {
            digitReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
