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

    public sealed class ExpSignReader<TNextReader>
    {
    }
}
