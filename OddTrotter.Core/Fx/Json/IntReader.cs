namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class IntReader<TNextReader> : ICategoryReader<IntReader<TNextReader>, IntCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out IntCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (context.Buffer[context.CurrentByteIndex] == '0')
            {
                category = IntCategory<TNextReader>.Zero();
                ++context.CurrentByteIndex;
            }
            else
            {
                category = IntCategory<TNextReader>.NonZero();
            }

            return true;
        }
    }

    public readonly ref struct IntCategory<TNextReader>
    {
        private enum Type
        {
            Zero = 1,
            NonZero,
        }

        private Type type { get; init; }

        public static IntCategory<TNextReader> Zero()
        {
            return new IntCategory<TNextReader>()
            {
                type = Type.Zero,
            };
        }

        public static IntCategory<TNextReader> NonZero()
        {
            return new IntCategory<TNextReader>()
            {
                type = Type.NonZero,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> zero,
            Func<LeadingDigitReader<DigitsReader<TNextReader>>?, TResult> nonZero)
        {
            switch (this.type)
            {
                case Type.Zero:
                    return zero(default);
                case Type.NonZero:
                    return nonZero(default);
                default:
                    throw new Exception("TODO bug");
            }
        }
    }

    public sealed class LeadingDigitReader<TNextReader> : IValueReader<LeadingDigitReader<TNextReader>, TNextReader, LeadingDigitToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out LeadingDigitToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!LeadingDigitToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            return true;
        }
    }

    public readonly ref struct LeadingDigitToken
    {
        public static bool TryCreate(byte digit, out LeadingDigitToken leadingDigitToken)
        {
            if (digit < '1' || digit > '9')
            {
                leadingDigitToken = default;
                return false;
            }

            leadingDigitToken = new LeadingDigitToken(digit);
            return true;
        }

        private LeadingDigitToken(byte digit)
        {
            Digit = digit;
        }
        public byte Digit { get; }
    }

    public sealed class DigitsReader<TNextReader>
    {
    }

    public readonly ref struct DigitsCategory<TNextReader>
    {
    }

    public sealed class DigitReader<TNextReader>
    {
    }

    public readonly struct Digit //// TODO should all of your non-continuable tokens be ref struct?
    {
    }
}
