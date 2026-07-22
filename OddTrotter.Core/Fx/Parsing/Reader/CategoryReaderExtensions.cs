namespace Fx.Parsing.Reader
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class CategoryReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context, [MaybeNullWhen(false)] out TCategory category)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
            where TCategory : allows ref struct
        {
            return TCurrentReader.TryMove(context, out category);
        }

        public static async ValueTask<TCategory> Move<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
        {
            TCategory? category;
            while (!categoryReader.TryMove(context, out category))
            {
                await context.Read().ConfigureAwait(false);
            }

            return category;
        }
    }
}
