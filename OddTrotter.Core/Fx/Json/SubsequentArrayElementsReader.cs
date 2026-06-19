namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class SubsequentArrayElementsReader<TNextReader> : ICategoryReader<SubsequentArrayElementsReader<TNextReader>, SubsequentArrayElementsCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out SubsequentArrayElementsCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (context.Buffer[context.CurrentByteIndex] == ',')
            {
                category = SubsequentArrayElementsCategory<TNextReader>.None();
            }
            else
            {
                category = SubsequentArrayElementsCategory<TNextReader>.Some();
            }

            return true;
        }
    }
}
