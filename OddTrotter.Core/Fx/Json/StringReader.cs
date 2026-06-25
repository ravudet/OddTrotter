namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http.Headers;

    using Fx.Parsing.Reader;

    public sealed class StringReader<TNextReader>
    {
    }

    public sealed class CharsReader<TNextReader> : ICategoryReader<CharsReader<TNextReader>, CharsCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out CharsCategory<TNextReader> category)
        {
            if (context.ValidBytes == 0)
            {
                category = CharsCategory<TNextReader>.None();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            var currentByte = context.Buffer[context.CurrentByteIndex];
            if (UnescapedCharToken.TryCreate(context.Buffer[context.CurrentByteIndex], out _))
            {
                // unescaped
                category = CharsCategory<TNextReader>.Some();
            }
            else if (EscapeCharacterToken.TryCreate(currentByte, out _))
            {
                // escaped
                category = CharsCategory<TNextReader>.Some();
            }
            else
            {
                category = CharsCategory<TNextReader>.None();
            }

            return true;
        }
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

    public sealed class CharReader<TNextReader> : ICategoryReader<CharReader<TNextReader>, CharCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out CharCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            var currentByte = context.Buffer[context.CurrentByteIndex];
            if (EscapeCharacterToken.TryCreate(currentByte, out _))
            {
                category = CharCategory<TNextReader>.Escaped();
            }
            else if (UnescapedCharToken.TryCreate(currentByte, out _))
            {
                category = CharCategory<TNextReader>.Unescaped();
            }
            else
            {
                // we are here because someone has a `charreader` at a byte that is an unescaped `"` character, which should never happen if the caller started with a `fx.json.reader`, but if they are trying to just reader the current character as though it is a "char", then it's an invalid payload
                throw new InvalidPayloadException("TODO");
            }

            return true;
        }
    }

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
    }

    public sealed class UnescapedCharReader<TNextReader> : IValueReader<UnescapedCharReader<TNextReader>, TNextReader, UnescapedCharToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out UnescapedCharToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!UnescapedCharToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            return true;
        }
    }

    public readonly ref struct UnescapedCharToken
    {
        public static bool TryCreate(byte @char, out UnescapedCharToken charToken)
        {
            if (!UnescapedCharToken.IsValid(@char))
            {
                charToken = default;
                return false;
            }

            charToken = new UnescapedCharToken(@char);
            return true;
        }

        private static bool IsValid(byte @char)
        {
            return
                (@char >= 0x20 && @char <= 0x21) ||
                (@char >= 0x23 && @char <= 0x5B) ||
                (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
        }

        private UnescapedCharToken(byte @char)
        {
            Char = @char;
        }

        public byte Char { get; }
    }

    public sealed class EscapedCharReader<TNextReader>
    {
    }

    public readonly ref struct EscapeCharacterToken
    {
        public static bool TryCreate(byte escapeCharacter, out EscapeCharacterToken escapeCharacterToken)
        {
        }
    }

    public sealed class SubsequentCharsReader<TNextReader>
    {
    }
}
