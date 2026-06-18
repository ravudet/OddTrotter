namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class ArrayReader<TNextReader> : IMoveReader<ArrayReader<TNextReader>, ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(Context context, out ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
