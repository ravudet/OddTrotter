namespace Fx.Json
{
    using System;

    public readonly ref struct WhitespaceCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static WhitespaceCategory<TNextReader> None()
        {
            return new WhitespaceCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static WhitespaceCategory<TNextReader> Some()
        {
            return new WhitespaceCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> noneReader,
            Func<WhitespaceCharacterReader<TNextReader>?, TResult> someReader)
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

        public bool TrySome(out WhitespaceCharacterReader<WhitespaceReader<TNextReader>>? whitespaceCharacterReader)
        {
            whitespaceCharacterReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
