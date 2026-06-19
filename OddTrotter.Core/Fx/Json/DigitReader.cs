namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class DigitReader<TNextReader> : IValueReader<DigitReader<TNextReader>, TNextReader, DigitToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out DigitToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (DigitToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                ++context.CurrentByteIndex;
                return true;
            }
            else
            {
                throw new InvalidPayloadException("TODO");
            }
        }
    }
}
