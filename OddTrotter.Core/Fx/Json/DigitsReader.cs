namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class DigitsReader<TNextReader> : ICategoryReader<DigitsReader<TNextReader>, DigitsCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out DigitsCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (DigitToken.TryCreate(context.Buffer[context.CurrentByteIndex], out _))
            {
                category = DigitsCategory<TNextReader>.Some();
            }
            else
            {
                category = DigitsCategory<TNextReader>.None();
            }

            return true;
        }
    }
}
