namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    using Fx.Parsing.Reader;

    public sealed class FalseReader<TNextReader> : IContinuableValueReader<FalseReader<TNextReader>, TNextReader, FalseToken, int>
    {
        private const string literal = "false";

        public static bool TryContinue(Context context, int continuationToken, out TNextReader? nextReader, [MaybeNullWhen(false)] out FalseToken value, [MaybeNullWhen(true)] out int nextContinuationToken)
        {
            //// TODO you need to fix the case where they read past the buffer, but there were no bytes returned
            //// TODO have a test for this case
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value, out nextContinuationToken))
            {
                return false;
            }

            Helpers.EnsureValidBytes(context);

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

    public static class Helpers
    {
        public static bool NeedsMoreBytes<TNextReader, TValue, TContinuationToken>(
            Context context, 
            out TNextReader? nextReader,
            [MaybeNullWhen(false)] out TValue value,
            [MaybeNullWhen(true)] out TContinuationToken nextContinuationToken)
        {
            nextReader = default;
            value = default;
            nextContinuationToken = default;
            return context.CurrentByteIndex >= context.ValidBytes;
        }

        public static void EnsureValidBytes(Context context)
        {
            if (context.ValidBytes == 0)
            {
                throw new InvalidPayloadException("TODO invalid JSON");
            }
        }
    }
}
