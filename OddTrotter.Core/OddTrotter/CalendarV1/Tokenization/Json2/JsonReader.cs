namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface IReader<out TNextReader>
    {
        ITask<TNextReader> Move();
    }

    public interface IReader<out TValue, out TNextReader> : IReader<TNextReader>
    {
        ITask<TValue> GetValue();
    }

    public sealed class JsonReader : IReader<WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        private readonly Stream stream;

        public JsonReader(Stream stream)
        {
            this.stream = stream;
        }

        public ITask<WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>> Move()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IReader<IEnumerable<WhitespaceToken>, TNextReader>
    {
        private readonly Stream stream;
        private readonly Func<Stream, TNextReader> nextReaderFactory;

        public WhitespaceReader(Stream stream, Func<Stream, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ITask<IEnumerable<WhitespaceToken>> GetValue()
        {
            throw new System.NotImplementedException();
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue();
            return this.nextReaderFactory(this.stream);
        }
    }

    public sealed class WhitespaceToken
    {
    }

    public sealed class ValueReader<TNextReader>
    {
    }
}
