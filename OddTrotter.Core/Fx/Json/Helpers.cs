namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

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
