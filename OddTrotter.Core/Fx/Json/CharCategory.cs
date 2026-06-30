namespace Fx.Json
{
    using System;

    public readonly ref struct CharCategory<TNextReader>
    {
        private enum Type
        {
            Escaped = 1,
            Unescaped,
        }

        private Type type { get; init; }

        public static CharCategory<TNextReader> Escaped()
        {
            return new CharCategory<TNextReader>()
            {
                type = Type.Escaped,
            };
        }

        public static CharCategory<TNextReader> Unescaped()
        {
            return new CharCategory<TNextReader>()
            {
                type = Type.Unescaped,
            };
        }

        public TResult Apply<TResult>(
            Func<EscapedCharReader<TNextReader>?, TResult> escaped,
            Func<UnescapedCharReader<TNextReader>?, TResult> unescaped)
        {
            switch (this.type)
            {
                case Type.Escaped:
                    return escaped(default);
                case Type.Unescaped:
                    return unescaped(default);
                default:
                    throw new Exception("TODO bug");
            }
        }

        public bool TryEscaped(out EscapableCharReader<TNextReader>? escaped)
        {
            escaped = default;
            return this.Apply(
                _ => true,
                _ => false);
        }

        public bool TryUnescaped(out UnescapedCharReader<TNextReader>? unescaped)
        {
            unescaped = default;
            return this.Apply(
                _ => false,
                _ => true);
        }
    }
}
