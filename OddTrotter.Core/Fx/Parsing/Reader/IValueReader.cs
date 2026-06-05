using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface IValueReader<out TSelf, TNextReader, TValue>
        where TSelf : IValueReader<TSelf, TNextReader, TValue>
    {
        static abstract bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value); //// TODO you really have to jump through some hoops to make this covariant; do you want to?
    }
}
