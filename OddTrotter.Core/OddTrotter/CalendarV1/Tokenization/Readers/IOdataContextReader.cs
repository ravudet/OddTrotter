namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    using System;

    public interface IOdataContextReader<out TNextReader> : IReader<IOdataContextToken<TNextReader>>
    {
    }

    public interface IOdataContextToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IOdataContextUrlReader<TNextReader>, TResult> odataContextUrlReader,
            Func<TNextReader, TResult> nextReader);
    }

    public interface IOdataContextUrlReader<out TNextReader> : IReader<TNextReader, OdataContextUrl>
    {
    }

    public sealed class OdataContextUrl
    {
        internal OdataContextUrl(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }
}
