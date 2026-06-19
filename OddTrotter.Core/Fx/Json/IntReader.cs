namespace Fx.Json
{
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
}
