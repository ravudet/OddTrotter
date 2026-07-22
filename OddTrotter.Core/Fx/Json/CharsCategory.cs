namespace Fx.Json
{
    using System;

    public readonly struct CharsCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static CharsCategory<TNextReader> None()
        {
            return new CharsCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static CharsCategory<TNextReader> Some()
        {
            return new CharsCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> none,
            Func<CharReader<CharsReader<TNextReader>>?, TResult> some)
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

        public bool TrySome(out CharReader<CharsReader<TNextReader>>? charReader)
        {
            charReader = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
