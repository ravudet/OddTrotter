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

    public sealed class NonUnicodeReader<TNextReader> : IValueReader<NonUnicodeReader<TNextReader>, TNextReader, NonUnicodeToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out NonUnicodeToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!NonUnicodeToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            return true;
        }
    }

    public readonly ref struct NonUnicodeToken
    {
        public static bool TryCreate(byte @char, out NonUnicodeToken nonUnicodeToken)
        {
            switch (@char)
            {
                case 0x22:
                case 0x5C:
                case 0x2F:
                case 0x62:
                case 0x66:
                case 0x6E:
                case 0x72:
                case 0x74:
                    nonUnicodeToken = new NonUnicodeToken(@char);
                    return true;
                default:
                    nonUnicodeToken = default;
                    return false;
            }
        }

        private NonUnicodeToken(byte @char)
        {
            Char = @char;
        }

        public byte Char { get; }
    }

    public sealed class UnicodeReader<TNextReader> : IMoveReader<UnicodeReader<TNextReader>, UReader<HexDigReader<HexDigReader<HexDigReader<HexDigReader<TNextReader>>>>>>
    {
        public static bool TryMove(Context context, out UReader<HexDigReader<HexDigReader<HexDigReader<HexDigReader<TNextReader>>>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }

    public sealed class UReader<TNextReader> : IValueReader<UReader<TNextReader>, TNextReader, UToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out UToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!UToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            return true;
        }
    }

    public readonly ref struct UToken
    {
        public static bool TryCreate(byte u, out UToken uToken)
        {
            uToken = default;
            return u == 0x75;
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
