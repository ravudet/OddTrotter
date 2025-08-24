namespace OddTrotter.CalendarV1.Readers
{
    using System.Threading.Tasks;

    public interface IReader<out TNextReader>
    {
        ValueTask Read();

        TNextReader TryMoveNext(out bool moved);
    }

    public interface IReader<out TNextReader, out TValue> : IReader<TNextReader>
    {
        TValue TryGetValue(out bool moved);
    }
}
