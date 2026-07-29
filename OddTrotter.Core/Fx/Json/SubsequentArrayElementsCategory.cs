namespace Fx.Json
{
    using System;

    public readonly struct SubsequentArrayElementsCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static SubsequentArrayElementsCategory<TNextReader> None()
        {
            return new SubsequentArrayElementsCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static SubsequentArrayElementsCategory<TNextReader> Some()
        {
            return new SubsequentArrayElementsCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> noneReader,
            Func<ValueReader<SubsequentArrayElementsReader<TNextReader>>?, TResult> someReader)
        {
            switch (this.type)
            {
                case Type.None:
                    return noneReader(default);
                case Type.Some:
                    return someReader(default);
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

        public bool TrySome(out ValueReader<SubsequentArrayElementsReader<TNextReader>>? valueReader)
        {
            valueReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
