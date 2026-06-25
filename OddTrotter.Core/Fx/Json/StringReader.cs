namespace Fx.Json
{
    using System;

    public sealed class StringReader<TNextReader>
    {
    }

    public sealed class CharsReader<TNextReader>
    {
    }

    public readonly ref struct CharsCategory<TNextReader>
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
            Func<CharReader<SubsequentCharsReader<TNextReader>>?, TResult> some)
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
    }

    public sealed class CharReader<TNextReader>
    {
    }

    public sealed class SubsequentCharsReader<TNextReader>
    {
    }
}
