namespace Fx.Json
{
    using System;

    public readonly ref struct EscapableCharCategory<TNextReader>
    {
        private enum Type
        {
            Unicode = 1,
            NonUnicode,
        }

        private Type type { get; init; }

        public static EscapableCharCategory<TNextReader> Unicode()
        {
            return new EscapableCharCategory<TNextReader>()
            {
                type = Type.Unicode,
            };
        }

        public static EscapableCharCategory<TNextReader> NonUnicode()
        {
            return new EscapableCharCategory<TNextReader>()
            {
                type = Type.NonUnicode,
            };
        }

        public TResult Apply<TResult>(
            Func<UnicodeReader<TNextReader>?, TResult> unicode,
            Func<NonUnicodeReader<TNextReader>?, TResult> nonUnicode)
        {
            switch (this.type)
            {
                case Type.Unicode:
                    return unicode(default);
                case Type.NonUnicode:
                    return nonUnicode(default);
                default:
                    throw new Exception("TODO bug");
            }
        }

        public bool TryUnicode(out UnicodeReader<TNextReader>? unicode)
        {
            unicode = default;
            return this.Apply(
                _ => true,
                _ => false);
        }

        public bool TryNonUnicode(out NonUnicodeReader<TNextReader>? nonUnicode)
        {
            nonUnicode = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
