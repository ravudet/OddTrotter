namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class HexDigReader<TNextReader> : IValueReader<HexDigReader<TNextReader>, TNextReader, HexDigToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out HexDigToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!HexDigToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            return true;
        }
    }
}
