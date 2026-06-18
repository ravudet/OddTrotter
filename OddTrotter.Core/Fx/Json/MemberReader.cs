namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class MemberReader<TNextReader> : IMoveReader<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
    {
        public static bool TryMove(Context context, out StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
