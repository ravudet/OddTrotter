using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface ICategoryReader<TSelf, TCategory>
        where TSelf : ICategoryReader<TSelf, TCategory>
    {
        static abstract bool TryMove(Context context, [MaybeNullWhen(false)] out TCategory category);
    }
}
