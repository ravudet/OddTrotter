namespace Fx.Json
{
    using System;

    public readonly struct ArrayElementsCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static ArrayElementsCategory<TNextReader> None()
        {
            return new ArrayElementsCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static ArrayElementsCategory<TNextReader> Some()
        {
            return new ArrayElementsCategory<TNextReader>()
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
