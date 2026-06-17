namespace Fx.Parsing.Reader
{
    using System.Diagnostics.CodeAnalysis;

    public interface IContinuableValueReader<out TSelf, TNextReader, TValue, TContinuationToken>
        where TSelf : IContinuableValueReader<TSelf, TNextReader, TValue, TContinuationToken>
        where TValue : allows ref struct
    {
        /// <exception cref="InvalidPayloadException">Thrown if the payload in <paramref name="context"/> is not the format that the reader implementation understands</exception>
        static abstract bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TContinuationToken continuationToken);

        /// <exception cref="InvalidPayloadException">Thrown if the payload in <paramref name="context"/> is not the format that the reader implementation understands</exception>
        static abstract bool TryContinue(Context context, TContinuationToken continuationToken, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TContinuationToken nextContinuationToken);
    }
}
