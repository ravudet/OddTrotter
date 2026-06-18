namespace Fx.Json
{
    using System;

    public readonly ref struct ArrayElementsCategory<TNextReader>
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
            Func<ArrayElementReader<ArrayElementsReader<TNextReader>>?, TResult> someReader)
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
    }

}
