using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public static class ContinuableValueReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TNextReader, TValue, TContinuationToken>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>? continuableValueReader,
            Context context,
            out TNextReader? nextReader,
            [MaybeNullWhen(false)] out TValue value,
            [MaybeNullWhen(true)] out TContinuationToken continuationToken)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>
            where TValue : allows ref struct
        {
            return TCurrentReader.TryMove(context, out nextReader, out value, out continuationToken);
        }

        public static bool TryContinue<TCurrentReader, TNextReader, TValue, TContinuationToken>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>? continuableValueReader,
            Context context,
            TContinuationToken continuationToken,
            out TNextReader? nextReader,
            [MaybeNullWhen(false)] out TValue value,
            [MaybeNullWhen(true)] out TContinuationToken nextContinuationToken)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>
            where TValue : allows ref struct
        {
            return TCurrentReader.TryContinue(context, continuationToken, out nextReader, out value, out nextContinuationToken);
        }
    }
}
