namespace Fx.Json
{
    using System;
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

        public TResult Apply<TResult>(
            Func<FalseReader<TNextReader>?, TResult> falseReader,
            Func<NullReader<TNextReader>?, TResult> nullReader,
            Func<TrueReader<TNextReader>?, TResult> trueReader,
            Func<ObjectReader<TNextReader>?, TResult> objectReader,
            Func<ArrayReader<TNextReader>?, TResult> arrayReader,
            Func<NumberReader<TNextReader>?, TResult> numberReader,
            Func<StringReader<TNextReader>?, TResult> stringReader)
        {
            switch (this.type)
            {
                case Type.False:
                    return falseReader(default);
                case Type.Null:
                    return nullReader(default);
                case Type.True:
                    return trueReader(default);
                case Type.Object:
                    return objectReader(default);
                case Type.Array:
                    return arrayReader(default);
                case Type.Number:
                    return numberReader(default);
                case Type.String:
                    return stringReader(default);
                default:
                    throw new Exception("TODO bug");
            }
        }

        public bool TryFalse(out FalseReader<TNextReader>? falseReader)
        {
            falseReader = default;
            return this.type == Type.False;
        }

        public bool TryNull(out NullReader<TNextReader>? nullReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`
            
            nullReader = default;
            return this.Apply(
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }

        public bool TryTrue(out TrueReader<TNextReader>? trueReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`

            trueReader = default;
            return this.Apply(
                _ => false,
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }

        public bool TryNull(out NullReader<TNextReader>? nullReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`

            nullReader = default;
            return this.Apply(
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }

        public bool TryNull(out NullReader<TNextReader>? nullReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`

            nullReader = default;
            return this.Apply(
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }

        public bool TryNull(out NullReader<TNextReader>? nullReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`

            nullReader = default;
            return this.Apply(
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }

        public bool TryNull(out NullReader<TNextReader>? nullReader)
        {
            //// TODO you can optimize this method by making it more like `tryfalse`

            nullReader = default;
            return this.Apply(
                _ => false,
                _ => true,
                _ => false,
                _ => false,
                _ => false,
                _ => false,
                _ => false);
        }
    }
}
