namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ExpReader<TNextReader> : ICategoryReader<ExpReader<TNextReader>, ExpCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out ExpCategory<TNextReader> category)
        {
            if (context.ValidBytes == 0)
            {
                category = ExpCategory<TNextReader>.Absent();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }
        }
    }

    public readonly ref struct ExpCategory<TNextReader>
    {
        private enum Type
        {
            Absent = 1,
            Present,
        }

        private Type type { get; init; }

        public static ExpCategory<TNextReader> Absent()
        {
            return new ExpCategory<TNextReader>()
            {
                type = Type.Absent,
            };
        }

        public static ExpCategory<TNextReader> Present()
        {
            return new ExpCategory<TNextReader>()
            {
                type = Type.Present,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> absent,
            Func<EReader<ExpSignReader<DigitReader<DigitsReader<TNextReader>>>>?, TResult> present)
        {
            switch (this.type)
            {
                case Type.Absent:
                    return absent(default);
                case Type.Present:
                    return present(default);
                default:
                    throw new Exception("TODO bug");
            }
        }
    }

    public sealed class EReader<TNextReader>
    {
    }

    public readonly ref struct EToken
    {
        public static bool TryCreate(byte digit, out EToken eToken)
        {
            if (digit < '1' || digit > '9')
            {
                eToken = default;
                return false;
            }

            eToken = new EToken(digit);
            return true;
        }

        private EToken(byte e)
        {
            E = e;
        }
        public byte E { get; }
    }

    public sealed class ExpSignReader<TNextReader>
    {
    }
}
