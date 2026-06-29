namespace Fx.Parsing.Reader
{
    public static class MoveReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TNextReader>(this IMoveReader<TCurrentReader, TNextReader>? currentReader, Context context, out TNextReader? nextReader)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            return TCurrentReader.TryMove(context, out nextReader);
        }
    }
}
