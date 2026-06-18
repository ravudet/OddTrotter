namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class FalseReader<TNextReader> : IContinuableValueReader<FalseReader<TNextReader>, TNextReader, FalseToken, int>
    {
        private const string literal = "false";

        public static bool TryContinue(Context context, int continuationToken, out TNextReader? nextReader, [MaybeNullWhen(false)] out FalseToken value, [MaybeNullWhen(true)] out int nextContinuationToken)
        {
            //// TODO call "valuereader" a "tokenreader"? since you're using the name "token" in the types returned...

            //// TODO you need to have a test for the case where they read past the buffer, but there were no bytes returned
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, continuationToken, out nextReader, out value, out nextContinuationToken))
            {
                return false;
            }

            for (; continuationToken < literal.Length; ++continuationToken)
            {

            }
        }

        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out FalseToken value, [MaybeNullWhen(true)] out int continuationToken)
        {
            return TryContinue(context, 0, out nextReader, out value, out continuationToken);
        }
    }

    public readonly struct FalseToken
    {
    }
}
