namespace OddTrotter.CalendarV1.Tokenization.Json5
{
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json6;

    public static partial class Extensions
    {
        public static ValueTask<TNextReader> Move<TNextReader>(
            this WhitespaceReader<TNextReader> whitespaceReader, 
            ref ReaderContext readerContext)
        {
            if (whitespaceReader.TryMove(ref readerContext, out var nextReader, out _))
            {
                return ValueTask.FromResult(nextReader);
            }

            // can't use `valuetask`, have to roll your own; which makes abstraction with a static interface very attractive
        }
    }
}
