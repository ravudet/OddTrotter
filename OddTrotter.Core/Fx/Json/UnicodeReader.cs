namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class UnicodeReader<TNextReader> : IMoveReader<UnicodeReader<TNextReader>, UReader<HexDigReader<HexDigReader<HexDigReader<HexDigReader<TNextReader>>>>>>
    {
        public static bool TryMove(Context context, out UReader<HexDigReader<HexDigReader<HexDigReader<HexDigReader<TNextReader>>>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
