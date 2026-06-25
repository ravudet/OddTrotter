namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class EReader<TNextReader> : IValueReader<EReader<TNextReader>, TNextReader, EToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out EToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (!EToken.TryCreate(context.Buffer[context.CurrentByteIndex], out value))
            {
                throw new InvalidPayloadException("TODO");
            }

            ++context.CurrentByteIndex;
            return true;
        }
    }
}
