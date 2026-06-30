namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class WhitespaceCharacterReader<TNextReader> : IValueReader<WhitespaceCharacterReader<TNextReader>, TNextReader, WhitespaceCharacterToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out WhitespaceCharacterToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!WhitespaceCharacterToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            nextReader = default;
            return true;
        }
    }
}
