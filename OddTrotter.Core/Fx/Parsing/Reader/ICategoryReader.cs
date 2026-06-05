using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface ICategoryReader<out TSelf, TCategory>
        where TSelf : ICategoryReader<TSelf, TCategory>
        where TCategory : allows ref struct
    {
        static abstract bool TryMove(Context context, [MaybeNullWhen(false)] out TCategory category);
    }
}
