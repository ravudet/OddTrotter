namespace Fx.Parsing.Reader
{
    using System.Threading.Tasks;

    public static class MoveReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TNextReader>(this IMoveReader<TCurrentReader, TNextReader>? currentReader, Context context, out TNextReader? nextReader)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            return TCurrentReader.TryMove(context, out nextReader);
        }

        public static ValueTask<TNextReader> Move<TCurrentReader, TNextReader>(this IMoveReader<TCurrentReader, TNextReader> currentReader, Context context)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            TNextReader? nextReader;
            while (!currentReader.TryMove(context, out nextReader))
            {

            }
        }
    }
}
