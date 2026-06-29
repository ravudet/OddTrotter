namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
    {
        public static bool TryMove(Context context, out StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
