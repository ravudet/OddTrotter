namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class WhitespaceReader<TNextReader> : ICategoryReader<WhitespaceReader<TNextReader>, WhitespaceCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out WhitespaceCategory<TNextReader> category)
        {
            if (context.ValidBytes == 0)
            {
                category = WhitespaceCategory<TNextReader>.None();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (WhitespaceCharacterToken.TryCreate(context.Buffer[context.CurrentByteIndex], out _))
            {
                category = WhitespaceCategory<TNextReader>.Some();
            }
            else
            {
                category = WhitespaceCategory<TNextReader>.None();
            }

            return true;
        }
    }
}
