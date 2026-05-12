namespace OddTrotter.CalendarV1.Tokenization.Json5
{
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json6;

    public static partial class Extensions
    {
        public static Move1Task<TNextReader> Move1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> currentReader, 
            ref ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            if (currentReader.TryMove(ref readerContext, out var nextReader))
            {
                return ValueTask.FromResult(nextReader);
            }

            // can't use `valuetask`, have to roll your own; which makes abstraction with a static interface very attractive
        }

        public struct Move1Task<TNextReader>
        {
        }
    }
}
