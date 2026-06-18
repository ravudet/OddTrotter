namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public static class Helpers
    {
        public static bool NeedsMoreBytes(Context context)
        {
            return context.CurrentByteIndex >= context.ValidBytes;
        }

        public static bool NeedsMoreBytes<TCategory>(
            Context context,
            [MaybeNull] out TCategory category)
            where TCategory : allows ref struct
        {
            category = default;
            return Helpers.NeedsMoreBytes(context);
        }

        public static bool NeedsMoreBytes<TNextReader, TValue>(Context context, [MaybeNull] out TNextReader nextReader, [MaybeNull] out TValue value)
            where TValue : allows ref struct
        {
            nextReader = default;
            value = default;
            return Helpers.NeedsMoreBytes(context);
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
            return Helpers.NeedsMoreBytes(context);
        }

        /// <exception cref="InvalidPayloadException"></exception>
        public static void EnsureValidBytes(Context context)
        {
            if (context.ValidBytes == 0)
            {
                throw new InvalidPayloadException("TODO invalid JSON");
            }
        }

        /// <exception cref="InvalidPayloadException"></exception>
        public static bool TryReadChar(Context context, char character)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context))
            {
                return false;
            }

            Helpers.ReadChar(context, character);
            ++context.CurrentByteIndex;
            return true;
        }

        /// <exception cref="InvalidPayloadException"></exception>
        private static void ReadChar(Context context, char character)
        {
            if (context.Buffer[context.CurrentByteIndex] != character)
            {
                throw new InvalidPayloadException("TODO invalid JSON");
            }
        }
    }
}
