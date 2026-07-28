namespace Fx.Json
{
    using System;

    public readonly struct ExpCategory<TNextReader>
    {
        private enum Type
        {
            Absent = 1,
            Present,
        }

        private Type type { get; init; }

        public static ExpCategory<TNextReader> Absent()
        {
            return new ExpCategory<TNextReader>()
            {
                type = Type.Absent,
            };
        }

        public static ExpCategory<TNextReader> Present()
        {
            return new ExpCategory<TNextReader>()
            {
                type = Type.Present,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> absent,
            Func<EReader<ExpSignReader<DigitReader<DigitsReader<TNextReader>>>>?, TResult> present)
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

        public bool TryPresent(out EReader<ExpSignReader<DigitReader<DigitsReader<TNextReader>>>>? eReader)
        {
            eReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
