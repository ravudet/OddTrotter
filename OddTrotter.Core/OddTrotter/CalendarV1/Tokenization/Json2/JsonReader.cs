namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class AsyncEnumerableExtensions
    {
        public static async ITask<IEnumerable<T>> ToTask<T>(this IAsyncEnumerable<T> source)
        {
            var enumerable = Enumerable.Empty<T>();
            IAsyncEnumerator<T>? enumerator = null;
            try
            {
                enumerator = source.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    enumerable = enumerable.Append(enumerator.Current);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            return enumerable;
        }
    }

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

        public async ITask<WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>> Move()
        {
            return await Task.FromResult(this.MoveImpl()).ConfigureAwait(false);
        }

        private WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> MoveImpl()
        {
            var validBytes = 1;
            return new WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>(
                this.stream,
                new byte[validBytes],
                validBytes,
                (stream, buffer, validBytes) => new ValueReader<WhitespaceReader<Nothing>>(
                    stream,
                    buffer,
                    validBytes,
                    (nestedStream, nestedBuffer, nestedValidBytes) => new WhitespaceReader<Nothing>(
                        nestedStream, 
                        nestedBuffer, 
                        nestedValidBytes,
                        (_, _, _) => new Nothing())));
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IReader<IEnumerable<WhitespaceToken>, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<Stream, byte[], int, TNextReader> nextReaderFactory;

        public WhitespaceReader(
            Stream stream,
            byte[] buffer,
            int validBytes,
            Func<Stream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<IEnumerable<WhitespaceToken>> GetValue()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<WhitespaceToken> GetValueImpl()
        {
            while (true)
            {
                var read = await stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
                if (read == 0)
                {
                    yield break;
                }

                try
                {
                    var whitespace = new WhitespaceToken(this.buffer[0]);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class WhitespaceToken
    {
        public WhitespaceToken(byte @char)
        {
            switch (@char)
            {
                case 0x20:
                case 0x09:
                case 0x0A:
                case 0x0D:
                    this.Char = @char;
                    break;
                default:
                    throw new Exception("TODO invalid JSON");
            }

        }

        public byte Char { get; }
    }

    public sealed class ValueReader<TNextReader> : IReader<ValueToken<TNextReader>>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<Stream, byte[], int, TNextReader> nextReaderFactory;

        public ValueReader(
            Stream stream,
            byte[] buffer,
            int validBytes,
            Func<Stream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ValueToken<TNextReader>> Move()
        {
            return await Task.FromResult(this.MoveImpl()).ConfigureAwait(false);
        }

        private ValueToken<TNextReader> MoveImpl()
        {
            switch ((char)buffer[0])
            {
                case 'f':
                    return new ValueToken<TNextReader>.False(
                        new FalseReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.validBytes,
                            this.nextReaderFactory));
                case 'n':
                    return new ValueToken<TNextReader>.Null(
                        new NullReader<TNextReader>());
                case 't':
                    return new ValueToken<TNextReader>.True(
                        new TrueReader<TNextReader>());
                case '{':
                    return new ValueToken<TNextReader>.Object(
                        new ObjectReader<TNextReader>());
                case '[':
                    return new ValueToken<TNextReader>.Array(
                        new ArrayReader<TNextReader>());
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return new ValueToken<TNextReader>.Number(
                        new NumberReader<TNextReader>());
                case '"':
                    return new ValueToken<TNextReader>.String(
                        new StringReader<TNextReader>());
                default:
                    throw new Exception("tODO invalid JSON");
            }
        }
    }

    public abstract class ValueToken<TNextReader>
    {
        private ValueToken()
        {
        }

        public sealed class False : ValueToken<TNextReader>
        {
            public False(FalseReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public FalseReader<TNextReader> Reader { get; }
        }

        public sealed class Null : ValueToken<TNextReader>
        {
            public Null(NullReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public NullReader<TNextReader> Reader { get; }
        }

        public sealed class True : ValueToken<TNextReader>
        {
            public True(TrueReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public TrueReader<TNextReader> Reader { get; }
        }

        public sealed class Object : ValueToken<TNextReader>
        {
            public Object(ObjectReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public ObjectReader<TNextReader> Reader { get; }
        }

        public sealed class Array : ValueToken<TNextReader>
        {
            public Array(ArrayReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public ArrayReader<TNextReader> Reader { get; }
        }

        public sealed class Number : ValueToken<TNextReader>
        {
            public Number(NumberReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public NumberReader<TNextReader> Reader { get; }
        }

        public sealed class String : ValueToken<TNextReader>
        {
            public String(StringReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public StringReader<TNextReader> Reader { get; }
        }
    }

    public sealed class FalseReader<TNextReader> : IReader<FalseToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<Stream, byte[], int, TNextReader> nextReaderFactory;

        public FalseReader(
            Stream stream,
            byte[] buffer,
            int validBytes,
            Func<Stream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<FalseToken> GetValue()
        {
            if (this.buffer[0] != 'f')
            {
                throw new Exception("TODO invalid JSON");
            }

            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'a').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'l').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 's').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'e').ConfigureAwait(false);
            return FalseToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class FalseToken
    {
        private FalseToken()
        {
        }

        public static FalseToken Instance { get; } = new FalseToken();
    }

    public sealed class NullReader<TNextReader> : IReader<NullToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<Stream, byte[], int, TNextReader> nextReaderFactory;

        public NullReader(
            Stream stream,
            byte[] buffer,
            int validBytes,
            Func<Stream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<NullToken> GetValue()
        {
            if (this.buffer[0] != 'n')
            {
                throw new Exception("TODO invalid JSON");
            }

            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'u').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'l').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'l').ConfigureAwait(false);
            return NullToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class NullToken
    {
        private NullToken()
        {
        }

        public static NullToken Instance { get; } = new NullToken();
    }

    public sealed class TrueReader<TNextReader> : IReader<TrueToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<Stream, byte[], int, TNextReader> nextReaderFactory;

        public TrueReader(
            Stream stream,
            byte[] buffer,
            int validBytes,
            Func<Stream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<TrueToken> GetValue()
        {
            if (this.buffer[0] != 't')
            {
                throw new Exception("TODO invalid JSON");
            }

            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'r').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'u').ConfigureAwait(false);
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, 'e').ConfigureAwait(false);
            return TrueToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class TrueToken
    {
        private TrueToken()
        {
        }

        public static TrueToken Instance { get; } = new TrueToken();
    }

    public sealed class ObjectReader<TNextReader>
    {
    }

    public sealed class ArrayReader<TNextReader>
    {
    }

    public sealed class NumberReader<TNextReader>
    {
    }

    public sealed class StringReader<TNextReader>
    {
    }

    public static class Helpers
    {
        public static async Task ReadChar(Stream stream, byte[] buffer, int validBytes, char character)
        {
            var read = await stream.ReadAsync(buffer, 0, validBytes).ConfigureAwait(false);
            if (read == 0 || buffer[0] != character)
            {
                throw new Exception("TODO invalid JSON");
            }
        }
    }
}
