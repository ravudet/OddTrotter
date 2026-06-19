namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class FracReader<TNextReader> : ICategoryReader<FracReader<TNextReader>, FracCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out FracCategory<TNextReader> category)
        {
            if (context.ValidBytes == 0)
            {
                //// TODO there are other places where ensurevalidbytes isn't called that need to follow this pattern
                category = FracCategory<TNextReader>.Absent();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (context.Buffer[context.CurrentByteIndex] == '.')
            {
                category = FracCategory<TNextReader>.Present();
                ++context.CurrentByteIndex;
            }
            else
            {
                category = FracCategory<TNextReader>.Absent();
            }

            return true;
        }
    }
}
