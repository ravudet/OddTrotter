namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class EscapableCharReader<TNextReader> : ICategoryReader<EscapableCharReader<TNextReader>, EscapableCharCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out EscapableCharCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (UToken.TryCreate(context.Buffer[context.CurrentByteIndex], out _))
            {
                category = EscapableCharCategory<TNextReader>.Unicode();
            }
            else
            {
                category = EscapableCharCategory<TNextReader>.NonUnicode();
            }

            return true;
        }
    }
}
