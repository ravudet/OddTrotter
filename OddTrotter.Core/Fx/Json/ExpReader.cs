namespace Fx.Json
{
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

            if (context.Buffer[context.CurrentByteIndex] == '-')
            {
                category = ExpCategory<TNextReader>.Present();
                ++context.CurrentByteIndex;
            }
            else
            {
                category = ExpCategory<TNextReader>.Absent();
            }

            return true;
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
