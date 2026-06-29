namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class CharsReader<TNextReader> : ICategoryReader<CharsReader<TNextReader>, CharsCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out CharsCategory<TNextReader> category)
        {
            if (context.ValidBytes == 0)
            {
                category = CharsCategory<TNextReader>.None();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            var currentByte = context.Buffer[context.CurrentByteIndex];
            if (UnescapedCharToken.TryCreate(context.Buffer[context.CurrentByteIndex], out _))
            {
                // unescaped
                category = CharsCategory<TNextReader>.Some();
            }
            else if (EscapeCharacterToken.TryCreate(currentByte, out _))
            {
                // escaped
                category = CharsCategory<TNextReader>.Some();
            }
            else
            {
                category = CharsCategory<TNextReader>.None();
            }

            return true;
        }
    }
}
