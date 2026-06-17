using System;
using System.Diagnostics.CodeAnalysis;

using Fx.Parsing.Reader;

namespace Fx.Json
{
    public sealed class WhitespaceReader<TNextReader> : ICategoryReader<WhitespaceReader<TNextReader>, WhitespaceCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out WhitespaceCategory<TNextReader> category)
        {
            throw new System.NotImplementedException();
        }
    }

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
            Func<WhitespaceCharaceterReader<TNextReader>?, TResult> someReader)
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
    }

    public sealed class WhitespaceCharaceterReader<TNextReader>
    {
    }

    public readonly struct WhitespaceCharacterToken
    {
        public static bool TryCreate(byte @char, out WhitespaceCharacterToken whitespaceToken)
        {
            switch (@char)
            {
                case 0x20:
                case 0x09:
                case 0x0A:
                case 0x0D:
                    whitespaceToken = new WhitespaceCharacterToken(@char);
                    return true;
                default:
                    whitespaceToken = default;
                    return false;
            }
        }

        private WhitespaceCharacterToken(byte @char)
        {
            this.Char = @char;
        }

        public byte Char { get; }
    }

}
