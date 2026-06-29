namespace Fx.Parsing.Reader
{
    using System.Diagnostics.CodeAnalysis;

    public static class CategoryReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context, [MaybeNullWhen(false)] out TCategory category)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
            where TCategory : allows ref struct
        {
            return TCurrentReader.TryMove(context, out category);
        }
    }
}
