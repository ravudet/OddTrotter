namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class CharReader<TNextReader> : ICategoryReader<CharReader<TNextReader>, CharCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out CharCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            var currentByte = context.Buffer[context.CurrentByteIndex];
            if (EscapeCharacterToken.TryCreate(currentByte, out _))
            {
                category = CharCategory<TNextReader>.Escaped();
            }
            else if (UnescapedCharToken.TryCreate(currentByte, out _))
            {
                category = CharCategory<TNextReader>.Unescaped();
            }
            else
            {
                // we are here because someone has a `charreader` at a byte that is an unescaped `"` character, which should never happen if the caller started with a `fx.json.reader`, but if they are trying to just reader the current character as though it is a "char", then it's an invalid payload
                throw new InvalidPayloadException("TODO");
            }

            return true;
        }
    }
}
