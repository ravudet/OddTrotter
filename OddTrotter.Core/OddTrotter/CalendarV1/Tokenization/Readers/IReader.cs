namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    using System.Threading.Tasks;

    public interface IReader<out TNextReader>
        where TNextReader : allows ref struct
    {
        ValueTask Read();

        TNextReader TryMoveNext(out bool moved);
    }

    public interface IReader<out TNextReader, out TValue> : IReader<TNextReader>
        where TNextReader : allows ref struct
        where TValue : allows ref struct
    {
        TValue TryGetValue(out bool moved);
    }
}
