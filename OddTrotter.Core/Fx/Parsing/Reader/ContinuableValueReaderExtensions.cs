using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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

        public static async ValueTask<(TNextReader?, TValue)> Move<TCurrentReader, TNextReader, TValue, TContinuationToken>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>? continuableValueReader,
            Context context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContinuationToken>
        {
            if (continuableValueReader.TryMove(context, out var nextReader, out var value, out var continuationToken))
            {
                return (nextReader, value);
            }

            await context.Read().ConfigureAwait(false);
            while (!continuableValueReader.TryContinue(context, continuationToken, out nextReader, out value, out continuationToken)) //// TODO does it break anything to use the same argument for an in *and* out parameter?
            {
                await context.Read().ConfigureAwait(false);
            }

            return (nextReader, value);
        }
    }
}
