namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ArrayElementsReader<TNextReader> : ICategoryReader<ArrayElementsReader<TNextReader>, ArrayElementsCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out ArrayElementsCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (context.Buffer[context.CurrentByteIndex] == ']')
            {
                category = ArrayElementsCategory<TNextReader>.None();
            }
            else
            {
                category = ArrayElementsCategory<TNextReader>.Some();
            }

            return true;
        }
    }
}
