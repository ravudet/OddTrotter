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

    public static class Helpers
    {
        public static bool NeedsMoreBytes<TCategory>(
            Context context,
            [MaybeNull] out TCategory category)
            where TCategory : allows ref struct
        {
            category = default;
            return context.CurrentByteIndex >= context.ValidBytes;
        }

        public static bool NeedsMoreBytes<TNextReader, TValue>(Context context, [MaybeNull] out TNextReader nextReader, [MaybeNull] out TValue value)
            where TValue : allows ref struct
        {
            nextReader = default;
            value = default;
            return context.CurrentByteIndex >= context.ValidBytes;
        }

        public static bool NeedsMoreBytes<TNextReader, TValue, TContinuationToken>(
            Context context, 
            TContinuationToken continuationToken,
            out TNextReader? nextReader,
            [MaybeNull] out TValue value,
            out TContinuationToken nextContinuationToken)
            where TValue : allows ref struct
        {
            nextReader = default;
            value = default;
            nextContinuationToken = continuationToken;
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
