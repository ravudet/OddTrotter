namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class StringDelimiterReader<TNextReader> : IValueReader<StringDelimiterReader<TNextReader>, TNextReader, StringDelimiterToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out StringDelimiterToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            return Helpers.TryReadChar(context, '"');
        }
    }
}
