using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface IContinuableValueReader<out TSelf, TNextReader, TValue, TContinuationToken>
        where TSelf : IContinuableValueReader<TSelf, TNextReader, TValue, TContinuationToken>
    {
        static abstract bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TContinuationToken continuationToken);

        static abstract bool TryContinue(Context context, TContinuationToken continuationToken, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TContinuationToken nextContinuationToken);
    }
}
