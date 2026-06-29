namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
    {
        public static bool TryMove(Context context, out StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }

    public sealed class HexDigReader<TNextReader> : IValueReader<HexDigReader<TNextReader>, TNextReader, HexDigToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out HexDigToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!HexDigToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            return true;
        }
    }

    public readonly ref struct HexDigToken
    {
        public static bool TryCreate(byte digit, out HexDigToken hexDigToken)
        {
            if (
                (digit >= 0x30 && digit <= 0x39) || 
                (digit >= 0x41 && digit <= 0x46) ||
                (digit >= 0x61 && digit <= 0x66))
            {
                hexDigToken = new HexDigToken(digit);
                return true;
            }

            hexDigToken = default;
            return false;
        }

        private HexDigToken(byte digit)
        {
            Digit = digit;
        }

        public byte Digit { get; }
    }
}
