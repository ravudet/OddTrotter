namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ValueReader<TNextReader> : ICategoryReader<ValueReader<TNextReader>, ValueCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out ValueCategory<TNextReader> category)
        {
            throw new System.NotImplementedException();
        }
    }

    public readonly ref struct ValueCategory<TNextReader>
    {
        private enum Type
        {
            False = 1, // since 0 is the default value, and we need to be able to distinguish an uninitialized instance
            Null,
            True,
            Object,
            Array,
            Number,
            String,
        }

        private Type type { get; init; }

        public static ValueCategory<TNextReader> False()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.False,
            };
        }

        public static ValueCategory<TNextReader> Null()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.Null,
            };
        }

        public static ValueCategory<TNextReader> True()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.True,
            };
        }

        public static ValueCategory<TNextReader> Object()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.Object,
            };
        }

        public static ValueCategory<TNextReader> Array()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.Array,
            };
        }

        public static ValueCategory<TNextReader> Number()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.Number,
            };
        }

        public static ValueCategory<TNextReader> String()
        {
            return new ValueCategory<TNextReader>()
            {
                type = Type.String,
            };
        }

        public TResult Apply<TResult>
            
    }
}
