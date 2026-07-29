namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class NullReader<TNextReader> : IContinuableValueReader<NullReader<TNextReader>, TNextReader, NullToken, int>
    {
        private const string literal = "null";

        public static bool TryContinue(Context context, int continuationToken, out TNextReader? nextReader, [MaybeNullWhen(false)] out NullToken value, [MaybeNullWhen(true)] out int nextContinuationToken)
        {
            //// TODO call "valuereader" a "tokenreader"? since you're using the name "token" in the types returned...

            //// TODO you need to have a test for the case where they read past the buffer, but there were no bytes returned
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, continuationToken, out nextReader, out value, out nextContinuationToken))
            {
                return false;
            }

            for (nextContinuationToken = continuationToken; nextContinuationToken < literal.Length; ++nextContinuationToken)
            {
                if (!Helpers.TryReadChar(context, literal[nextContinuationToken]))
                {
                    value = default;
                    return false;
                }
            }

            value = new NullToken();
            return true;
        }

        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out NullToken value, [MaybeNullWhen(true)] out int continuationToken)
        {
            return TryContinue(context, 0, out nextReader, out value, out continuationToken);
        }
    }
}
