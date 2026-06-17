namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class WhitespaceCharacterReader<TNextReader> : IValueReader<WhitespaceCharacterReader<TNextReader>, TNextReader, WhitespaceCharacterToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out WhitespaceCharacterToken value)
        {
            if (context.CurrentByteIndex >= context.ValidBytes)
            {
                nextReader = default;
                value = default;
                return false;
            }

            if (context.ValidBytes == 0)
            {
                throw new InvalidPayloadException("TODO invalid JSON");
            }

            if (!WhitespaceCharacterToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            nextReader = default;
            return true;
        }
    }
}
