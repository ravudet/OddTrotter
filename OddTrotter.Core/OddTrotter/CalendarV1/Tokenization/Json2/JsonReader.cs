namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Fx;

    using Stash;

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

    public readonly ref struct TypeHolder<TSelf, T1>
        where TSelf : allows ref struct
        where T1 : allows ref struct
    {
        public TypeHolder(TSelf self)
        {
            Self = self;
        }

        public TSelf Self { get; }
    }

    public readonly ref struct TypeHolder<TSelf, T1, T2>
        where TSelf : allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        public TypeHolder(TSelf self)
        {
            Self = self;
        }

        public TSelf Self { get; }
    }

    public interface IReader<out TNextReader>
        where TNextReader : allows ref struct
    {
        Task Read();

        TNextReader TryMove(out bool read);
    }

    public interface IReader2<TSelf, TNextReader>
        where TSelf : IReader2<TSelf, TNextReader>, allows ref struct
        where TNextReader : allows ref struct
    {
        Task Read(ReaderContext readerContext);

        TypeHolder<TSelf, TNextReader> AsReader { get; }

        ////ReaderContext Context { get; }

        ////Func<ReaderContext, TSelf> Factory { get; } //// TODO you should remove this once all of the readers are converted to `ref struct`; it should never need to be called, the factory that was originally used to instantiate the `ireader2` should be re-used instead (the one that the caller got from `trymove3`)

        bool TryMove3(ReaderContext readerContext, out Func<ReaderContext, TNextReader> nextFactory);
    }

    public interface IReader2<TSelf, TValue, TNextReader> : IReader2<TSelf, TNextReader>
        where TSelf : IReader2<TSelf, TValue, TNextReader>, allows ref struct
        where TNextReader : allows ref struct
        where TValue : allows ref struct
    {
        new TypeHolder<TSelf, TValue, TNextReader> AsReader { get; }

        bool TryGetValue3(ReaderContext readerContext, out TValue value);
    }

    public interface IReader<out TValue, out TNextReader> : IReader<TNextReader>
        where TValue : allows ref struct
        where TNextReader : allows ref struct
    {
        TValue TryGetValue(out bool read);
    }

    public sealed class ReaderContext
    {
        public ReaderContext(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes)
        {
            Stream = stream;
            Buffer = buffer;
            CurrentByteIndex = currentByteIndex;
            ValidBytes = validBytes;
        }

        public Stream Stream { get; }
        public byte[] Buffer { get; }
        public int CurrentByteIndex { get; set;  }
        public int ValidBytes { get; set; }
    }

    public static class ReaderContextExtensions
    {
        public static async Task Read(this ReaderContext readerContext)
        {
            readerContext.ValidBytes = await readerContext.Stream.ReadAsync(readerContext.Buffer, 0, readerContext.Buffer.Length).ConfigureAwait(false);
            readerContext.CurrentByteIndex = 0;
        }
    }

    public ref struct JsonReader : IReader2<JsonReader, WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>>
    {
        ////private bool read;

        public JsonReader(Stream stream)
        {
            this.Context = new ReaderContext(
                stream, 
                new byte[20], //// TODO parameterize
                0, 
                0);
        }

        public Func<ReaderContext, JsonReader> Factory { get; } = static (readerContext) => new JsonReader(readerContext.Stream);

        public ReaderContext Context { get; }

        public TypeHolder<JsonReader, WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>> AsReader
        {
            get
            {
                return new TypeHolder<JsonReader, WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>>(this);
            }
        }

        public Task Read(ReaderContext readerContext)
        {
            return readerContext.Read();

            ////this.read = true;
        }

        public WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>> TryMove(out bool read)
        {
            ////read = this.read;
            read = true;
            return new WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>(
                (context) => new ValueReader<WhitespaceReader<Nothing>>(
                    context.Stream,
                    context.Buffer,
                    context.CurrentByteIndex,
                    context.ValidBytes,
                    (nestedStream, nestedBuffer, currentByteIndex, nestedValidBytes) => new WhitespaceReader<Nothing>(
                        nestedStream,
                        nestedBuffer,
                        currentByteIndex,
                        nestedValidBytes,
                        (_, _, _, _) => new Nothing())));
        }

        public bool TryMove3(ReaderContext readerContext, out Func<ReaderContext, WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>> nextFactory)
        {
            nextFactory = WhitespaceReaderFactory;
            return true;
        }

        public static WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>> WhitespaceReaderFactory(ReaderContext context)
        {
            return new WhitespaceReader2<ValueReader<WhitespaceReader<Nothing>>>(ValueReaderFactory);
        }

        public static ValueReader<WhitespaceReader<Nothing>> ValueReaderFactory(ReaderContext context)
        {
            return new ValueReader<WhitespaceReader<Nothing>>(
                context.Stream,
                context.Buffer,
                context.CurrentByteIndex,
                context.ValidBytes,
                (nestedStream, nestedBuffer, currentByteIndex, nestedValidBytes) => new WhitespaceReader<Nothing>(
                    nestedStream,
                    nestedBuffer,
                    currentByteIndex,
                    nestedValidBytes,
                    NothingFactory));
        }

        public static Nothing NothingFactory(Stream stream, byte[] buffer, int currentByteIndex, int validBytes)
        {
            return new Nothing();
        }
    }

    public readonly ref struct WhitespaceReader2<TNextReader> : IReader2<WhitespaceReader2<TNextReader>, IEnumerable<WhitespaceToken>, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<ReaderContext, TNextReader> nextReaderFactory;

        ////private bool finished;

        private readonly List<WhitespaceToken> tokens;

        public TypeHolder<WhitespaceReader2<TNextReader>, IEnumerable<WhitespaceToken>, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<WhitespaceReader2<TNextReader>, IEnumerable<WhitespaceToken>, TNextReader>(this);
            }
        }

        ////public ReaderContext Context { get; }

        /*public Func<ReaderContext, WhitespaceReader2<TNextReader>> Factory
        {
            get
            {
                var factory = this.nextReaderFactory;
                return context => new WhitespaceReader2<TNextReader>(context, factory);
            }
        }*/

        TypeHolder<WhitespaceReader2<TNextReader>, TNextReader> IReader2<WhitespaceReader2<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<WhitespaceReader2<TNextReader>, TNextReader>(this);
            }
        }

        public WhitespaceReader2(
            Func<ReaderContext, TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<WhitespaceToken>();
            ////this.finished = false;
        }

        public IEnumerable<WhitespaceToken> TryGetValue(ReaderContext readerContext, out bool read)
        {
            /*if (this.finished)
            {
                read = true;
            }
            else*/
            {
                read = this.TryGetValue2(readerContext);
            }

            ////this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2(ReaderContext readerContext)
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader
            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    return false;
                }

                WhitespaceToken whitespace;
                try
                {
                    whitespace = new WhitespaceToken(readerContext.Buffer[readerContext.CurrentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                this.tokens.Add(whitespace);
            }

            return true;
        }

        public Task Read(ReaderContext readerContext)
        {
            return readerContext.Read();
            /*this.Context.ValidBytes = await this.Context.Stream.ReadAsync(this.Context.Buffer, 0, this.Context.Buffer.Length).ConfigureAwait(false);
            this.Context.CurrentByteIndex = 0;*/
        }

        public bool TryGetValue3(ReaderContext readerContext, out IEnumerable<WhitespaceToken> value)
        {
            value = this.TryGetValue(readerContext, out var read);
            return read;
        }

        public bool TryMove3(ReaderContext readerContext, out Func<ReaderContext, TNextReader> nextFactory)
        {
            nextFactory = this.nextReaderFactory;
            return true;
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IReader<IEnumerable<WhitespaceToken>, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool finished;

        private readonly List<WhitespaceToken> tokens;

        public WhitespaceReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<WhitespaceToken>();
            this.finished = false;
        }

        public IEnumerable<WhitespaceToken> TryGetValue(out bool read)
        {
            if (this.finished)
            {
                read = true;
            }
            else
            {
                read = this.TryGetValue2();
            }

            this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2()
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader
            while (true)
            {
                if (this.validBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (this.currentByteIndex >= this.validBytes)
                {
                    return false;
                }

                WhitespaceToken whitespace;
                try
                {
                    whitespace = new WhitespaceToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(whitespace);
            }

            return true;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
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
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ValueReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public ValueToken<TNextReader> TryMove(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            read = true;
            switch ((char)this.buffer[this.currentByteIndex])
            {
                case 'f':
                    return new ValueToken<TNextReader>.False(
                        new FalseReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case 'n':
                    return new ValueToken<TNextReader>.Null(
                        new NullReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case 't':
                    return new ValueToken<TNextReader>.True(
                        new TrueReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '{':
                    return new ValueToken<TNextReader>.Object(
                        new ObjectReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '[':
                    return new ValueToken<TNextReader>.Array(
                        new ArrayReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
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
                        new NumberReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '"':
                    return new ValueToken<TNextReader>.String(
                        new StringReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                default:
                    throw new Exception("tODO invalid JSON");
            }
        }
    }

    public abstract class ValueToken<TNextReader>
        where TNextReader : allows ref struct
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
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly string literal = "false";
        private int currentCharacter;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public FalseReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public FalseToken TryGetValue(out bool read)
        {
            for (; this.currentCharacter < this.literal.Length; ++this.currentCharacter)
            {
                (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, this.literal[this.currentCharacter]);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = true;
            return FalseToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
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
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly string literal = "null";
        private int currentCharacter;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;
        
        public NullReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public NullToken TryGetValue(out bool read)
        {
            for (; this.currentCharacter < this.literal.Length; ++this.currentCharacter)
            {
                (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, this.literal[this.currentCharacter]);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = true;
            return NullToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
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
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly string literal = "true";
        private int currentCharacter;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public TrueReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public TrueToken TryGetValue(out bool read)
        {
            for (; this.currentCharacter < this.literal.Length; ++this.currentCharacter)
            {
                (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, this.literal[this.currentCharacter]);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = true;
            return TrueToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class TrueToken
    {
        private TrueToken()
        {
        }

        public static TrueToken Instance { get; } = new TrueToken();
    }

    public sealed class ObjectReader<TNextReader> : IReader<ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ObjectReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>> TryMove(out bool read)
        {
            read = true;
            return new ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) =>
                    new WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            (strema, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ObjectEndReader<TNextReader>>(
                                    stream,
                                    buffer,
                                    currentByteIndex,
                                    validBytes,
                                    (stream, buffer, currentByteIndex, validBytes) => new ObjectEndReader<TNextReader>(
                                        stream,
                                        buffer,
                                        currentByteIndex,
                                        validBytes,
                                            this.nextReaderFactory)))));
        }
    }

    public sealed class ObjectStartReader<TNextReader> : IReader<ObjectStartToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public ObjectStartReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ObjectStartToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, '{');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = read;
            return ObjectStartToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class ObjectStartToken
    {
        private ObjectStartToken()
        {
        }

        public static ObjectStartToken Instance { get; } = new ObjectStartToken();
    }

    public sealed class MembersReader<TNextReader> : IReader<MembersToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public MembersReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public MembersToken<TNextReader> TryMove(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            read = true;
            if (this.buffer[this.currentByteIndex] != '"')
            {
                return new MembersToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new MembersToken<TNextReader>(
                new FirstMemberReader<TNextReader>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    this.nextReaderFactory));
        }
    }

    public readonly ref struct RefNullable<T>
        where T : allows ref struct
    {
        private readonly T value;

        private readonly bool hasValue;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        public RefNullable(T value)
        {
            this.value = value;

            this.hasValue = true;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(out T value)
        {
            value = this.value;
            return this.hasValue;
        }
    }

    public readonly ref struct MembersToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly RefNullable<TNextReader> none;
        private readonly RefNullable<FirstMemberReader<TNextReader>> some;

        public MembersToken(TNextReader reader)
        {
            this.none = new RefNullable<TNextReader>(reader);
        }

        public MembersToken(FirstMemberReader<TNextReader> reader)
        {
            this.some = new RefNullable<FirstMemberReader<TNextReader>>(reader);
        }

        public TResult Apply<TResult>(
            Func<TNextReader, TResult> noneMap,
            Func<FirstMemberReader<TNextReader>, TResult> someMap)
            where TResult : allows ref struct
        {
            if (this.none.TryGetValue(out var none))
            {
                return noneMap(none);
            }
            else if (this.some.TryGetValue(out var some))
            {
                return someMap(some);
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }

    public sealed class FirstMemberReader<TNextReader> : IReader<MemberReader<SubsequentMembersReader<TNextReader>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public FirstMemberReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public MemberReader<SubsequentMembersReader<TNextReader>> TryMove(out bool read)
        {
            read = true;
            return new MemberReader<SubsequentMembersReader<TNextReader>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new SubsequentMembersReader<TNextReader>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    this.nextReaderFactory));
        }
    }

    public sealed class SubsequentMembersReader<TNextReader> : IReader<SubsequentMembersToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public SubsequentMembersReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public SubsequentMembersToken<TNextReader> TryMove(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            read = true;
            if (this.buffer[this.currentByteIndex] != ',')
            {
                return new SubsequentMembersToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new SubsequentMembersToken<TNextReader>(
                new SubsequentMemberReader<SubsequentMembersReader<TNextReader>>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new SubsequentMembersReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    public readonly ref struct SubsequentMembersToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly RefNullable<TNextReader> none;
        private readonly RefNullable<SubsequentMemberReader<SubsequentMembersReader<TNextReader>>> more;

        public SubsequentMembersToken(TNextReader reader)
        {
            this.none = new RefNullable<TNextReader>(reader);
        }

        public SubsequentMembersToken(SubsequentMemberReader<SubsequentMembersReader<TNextReader>> reader)
        {
            this.more = new RefNullable<SubsequentMemberReader<SubsequentMembersReader<TNextReader>>>(reader);
        }

        public TResult Apply<TResult>(
            Func<TNextReader, TResult> noneMap,
            Func<SubsequentMemberReader<SubsequentMembersReader<TNextReader>>, TResult> moreMap)
            where TResult : allows ref struct
        {
            if (this.none.TryGetValue(out var none))
            {
                return noneMap(none);
            }
            else if (this.more.TryGetValue(out var more))
            {
                return moreMap(more);
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }
    public sealed class MemberReader<TNextReader> : IReader<StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public MemberReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>> TryMove(out bool read)
        {
            read = true;
            return new StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new ColonReader<WhitespaceReader<ValueReader<TNextReader>>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ValueReader<TNextReader>>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            (stream, buffer, currentByteIndex, validBytes) => new ValueReader<TNextReader>(
                                stream,
                                buffer,
                                currentByteIndex,
                                validBytes,
                                this.nextReaderFactory)))));
        }
    }

    public sealed class ColonReader<TNextReader> : IReader<ColonToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public ColonReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ColonToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, ':');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return ColonToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class ColonToken
    {
        private ColonToken()
        {
        }

        public static ColonToken Instance { get; } = new ColonToken();
    }

    public sealed class SubsequentMemberReader<TNextReader> : IReader<CommaReader<WhitespaceReader<MemberReader<TNextReader>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public SubsequentMemberReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public CommaReader<WhitespaceReader<MemberReader<TNextReader>>> TryMove(out bool read)
        {
            read = true;
            return new CommaReader<WhitespaceReader<MemberReader<TNextReader>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<MemberReader<TNextReader>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new MemberReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    public sealed class CommaReader<TNextReader> : IReader<CommaToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public CommaReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public CommaToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, ',');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return CommaToken.Instance;
        }
        
        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class CommaToken
    {
        private CommaToken()
        {
        }

        public static CommaToken Instance { get; } = new CommaToken();
    }

    public sealed class ObjectEndReader<TNextReader> : IReader<ObjectEndToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public ObjectEndReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ObjectEndToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, '}');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return ObjectEndToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class ObjectEndToken
    {
        private ObjectEndToken()
        {
        }

        public static ObjectEndToken Instance { get; } = new ObjectEndToken();
    }

    public sealed class ArrayReader<TNextReader> : IReader<ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ArrayReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>> TryMove(out bool read)
        {
            read = true;
            return new ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ArrayEndReader<TNextReader>>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            (stream, buffer, currentByteIndex, validBytes) => new ArrayEndReader<TNextReader>(
                                stream,
                                buffer,
                                currentByteIndex,
                                validBytes,
                                this.nextReaderFactory)))));
        }
    }

    public sealed class ArrayStartReader<TNextReader> : IReader<ArrayStartToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public ArrayStartReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ArrayStartToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, '[');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return ArrayStartToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class ArrayStartToken
    {
        private ArrayStartToken()
        {
        }

        public static ArrayStartToken Instance { get; } = new ArrayStartToken();
    }

    public sealed class ArrayElementsReader<TNextReader> : IReader<ArrayElementsToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ArrayElementsReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public ArrayElementsToken<TNextReader> TryMove(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            read = true;
            var currentByte = this.buffer[this.currentByteIndex];
            if (currentByte == ']')
            {
                return new ArrayElementsToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new ArrayElementsToken<TNextReader>(
                new ArrayElementReader<SubsequentArrayElementsReader<TNextReader>>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    (strema, buffer, currentByteIndex, validBytes) => new SubsequentArrayElementsReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    public readonly ref struct ArrayElementsToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly RefNullable<TNextReader> none;
        private readonly RefNullable<ArrayElementReader<SubsequentArrayElementsReader<TNextReader>>> some;

        public ArrayElementsToken(TNextReader reader)
        {
            this.none = new RefNullable<TNextReader>(reader);
        }

        public ArrayElementsToken(ArrayElementReader<SubsequentArrayElementsReader<TNextReader>> reader)
        {
            this.some = new RefNullable<ArrayElementReader<SubsequentArrayElementsReader<TNextReader>>>(reader);
        }

        public TResult Apply<TResult>(
            Func<TNextReader, TResult> noneMap,
            Func<ArrayElementReader<SubsequentArrayElementsReader<TNextReader>>, TResult> someMap)
            where TResult : allows ref struct
        {
            if (this.none.TryGetValue(out var none))
            {
                return noneMap(none);
            }
            else if (this.some.TryGetValue(out var some))
            {
                return someMap(some);
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }

    public sealed class ArrayElementReader<TNextReader> : IReader<ValueReader<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ArrayElementReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public ValueReader<TNextReader> TryMove(out bool read)
        {
            read = true;
            return new ValueReader<TNextReader>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                this.nextReaderFactory);
        }
    }

    public sealed class SubsequentArrayElementsReader<TNextReader> : IReader<SubsequentArrayElementsToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private int read;
        
        public SubsequentArrayElementsReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public SubsequentArrayElementsToken<TNextReader> TryMove(out bool read)
        {
            if (this.read == 1)
            {
                read = true;
                return new SubsequentArrayElementsToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            if (this.read == 2)
            {
                read = true;
                return new SubsequentArrayElementsToken<TNextReader>(
                new SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new SubsequentArrayElementsReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
            }

            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            read = true;
            var currentByte = this.buffer[this.currentByteIndex];
            if (currentByte != ',')
            {
                this.read = 1;
                return new SubsequentArrayElementsToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            this.read = 2;
            return new SubsequentArrayElementsToken<TNextReader>(
                new SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new SubsequentArrayElementsReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    public readonly ref struct SubsequentArrayElementsToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly RefNullable<TNextReader> none;
        private readonly RefNullable<SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>> more;

        public SubsequentArrayElementsToken(TNextReader reader)
        {
            this.none = new RefNullable<TNextReader>(reader);
        }

        public SubsequentArrayElementsToken(SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>> reader)
        {
            this.more = new RefNullable<SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>>(reader);
        }

        public TResult Apply<TResult>(
            Func<TNextReader, TResult> noneMap,
            Func<SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>, TResult> moreMap)
            where TResult : allows ref struct
        {
            if (this.none.TryGetValue(out var none))
            {
                return noneMap(none);
            }
            else if (this.more.TryGetValue(out var more))
            {
                return moreMap(more);
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }

    public sealed class SubsequentArrayElementReader<TNextReader> : IReader<CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public SubsequentArrayElementReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>> TryMove(out bool read)
        {
            read = true;
            return new CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ArrayElementReader<TNextReader>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new ArrayElementReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    public sealed class ArrayEndReader<TNextReader> : IReader<ArrayEndToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public ArrayEndReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ArrayEndToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, ']');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return ArrayEndToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class ArrayEndToken
    {
        private ArrayEndToken()
        {
        }

        public static ArrayEndToken Instance { get; } = new ArrayEndToken();
    }

    public sealed class NumberReader<TNextReader> : IReader<SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public NumberReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public SignReader<IntReader<FracReader<ExpReader<TNextReader>>>> TryMove(out bool read)
        {
            read = true;
            return new SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new IntReader<FracReader<ExpReader<TNextReader>>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new FracReader<ExpReader<TNextReader>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new ExpReader<TNextReader>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            this.nextReaderFactory))));
        }
    }

    public sealed class SignReader<TNextReader> : IReader<SignToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public SignReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public SignToken TryGetValue(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            read = true;
            if (this.validBytes == 0 || this.buffer[this.currentByteIndex] != '-')
            {
                return SignToken.Absent.Instance;
            }

            ++this.currentByteIndex;
            return SignToken.Negative.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public abstract class SignToken
    {
        private SignToken()
        {
        }

        public sealed class Absent : SignToken
        {
            private Absent()
            {
            }

            public static Absent Instance { get; } = new Absent();
        }

        public sealed class Negative : SignToken
        {
            private Negative()
            {
            }

            public static Negative Instance { get; } = new Negative();
        }
    }
    
    public sealed class IntReader<TNextReader> : IReader<IEnumerable<DigitToken>, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;
        private bool finished;
        private bool firstDigitRead;

        private readonly List<DigitToken> tokens;

        public IntReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<DigitToken>();
        }

        public IEnumerable<DigitToken> TryGetValue(out bool read)
        {
            if (this.finished)
            {
                read = true;
            }
            else
            {
                read = this.TryGetValue2();
            }

            this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2()
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader

            if (!this.firstDigitRead)
            {
                var currentByte = this.buffer[this.currentByteIndex];
                DigitToken digit;
                try
                {
                    digit = new DigitToken(currentByte);
                }
                catch (Exception)
                {
                    throw new Exception("TODO invalid JSON");
                }

                this.tokens.Add(digit);
                this.firstDigitRead = true;
                ++this.currentByteIndex;
                if (currentByte == '0')
                {
                    return true;
                }
            }

            while (true)
            {
                if (this.validBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (this.currentByteIndex >= this.validBytes)
                {
                    return false;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(digit);
            }

            return true;
        }


        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class DigitToken
    {
        public DigitToken(byte digit)
        {
            if (digit < '0' || digit > '9')
            {
                throw new Exception("TODO invalid JSON");
            }

            Digit = digit;
        }

        public byte Digit { get; }
    }

    public sealed class FracReader<TNextReader> : IReader<FracToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly List<DigitToken> tokens;

        private bool finished;

        public FracReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<DigitToken>();
        }

        public FracToken TryGetValue(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            if (this.validBytes == 0 || this.buffer[this.currentByteIndex] != '.')
            {
                read = true;
                return FracToken.Absent.Instance;
            }

            ++this.currentByteIndex;
            var digits = this.TryGetValue3(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return new FracToken.Frac(digits);
        }

        private IEnumerable<DigitToken> TryGetValue3(out bool read)
        {
            if (this.finished)
            {
                read = true;
            }
            else
            {
                read = this.TryGetValue2();
            }

            this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2()
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader

            while (true)
            {
                if (this.validBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (this.currentByteIndex >= this.validBytes)
                {
                    return false;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(digit);
            }

            return true;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public abstract class FracToken
    {
        private FracToken()
        {
        }

        public sealed class Absent : FracToken
        {
            private Absent()
            {
            }

            public static Absent Instance { get; } = new Absent();
        }

        public sealed class Frac : FracToken
        {
            public Frac(IEnumerable<DigitToken> digits)
            {
                Digits = digits;
            }

            public IEnumerable<DigitToken> Digits { get; }
        }
    }

    public sealed class ExpReader<TNextReader> : IReader<ExpToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ExpReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public ExpToken<TNextReader> TryMove(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            read = true;
            if (this.validBytes == 0 || this.buffer[this.currentByteIndex] != '-')
            {
                return new ExpToken<TNextReader>(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            ++this.currentByteIndex;
            return new ExpToken<TNextReader>(
                new EReader<ExpSignReader<DigitsReader<TNextReader>>>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new ExpSignReader<DigitsReader<TNextReader>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new DigitsReader<TNextReader>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            this.nextReaderFactory))));
        }
    }

    public readonly ref struct ExpToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly RefNullable<TNextReader> absent;
        private readonly RefNullable<EReader<ExpSignReader<DigitsReader<TNextReader>>>> present;

        public ExpToken(TNextReader reader)
        {
            this.absent = new RefNullable<TNextReader>(reader);
        }

        public ExpToken(EReader<ExpSignReader<DigitsReader<TNextReader>>> reader)
        {
            this.present = new RefNullable<EReader<ExpSignReader<DigitsReader<TNextReader>>>>(reader);
        }

        public TResult Apply<TResult>(
            Func<TNextReader, TResult> absentMap,
            Func<EReader<ExpSignReader<DigitsReader<TNextReader>>>, TResult> presentMap)
            where TResult : allows ref struct
        {
            if (this.absent.TryGetValue(out var absent))
            {
                return absentMap(absent);
            }
            else if (this.present.TryGetValue(out var present))
            {
                return presentMap(present);
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }

    public sealed class EReader<TNextReader> : IReader<EToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public EReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public EToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, 'e'); //// TODO should also allow 'E'
            if (!read)
            {
                return default!; //// TODO !
            }

            return new EToken((byte)'e');
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class EToken
    {
        public EToken(byte e)
        {
            if (e != 'e' && e != 'E')
            {
                throw new Exception("TODO invalid JSON");
            }

            E = e;
        }

        public byte E { get; }
    }

    public sealed class ExpSignReader<TNextReader> : IReader<ExpSignToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public ExpSignReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public ExpSignToken TryGetValue(out bool read)
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                read = false;
                return default!; //// TODO !
            }

            read = true;
            if (this.validBytes == 0)
            {
                return ExpSignToken.Absent.Instance;
            }

            var currentByte = this.buffer[this.currentByteIndex];
            if (currentByte == '+')
            {
                ++this.currentByteIndex;
                return ExpSignToken.Positive.Instance;
            }
            else if (currentByte == '-')
            {
                ++this.currentByteIndex;
                return ExpSignToken.Negative.Instance;
            }
            else
            {
                return ExpSignToken.Absent.Instance;
            }
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public abstract class ExpSignToken
    {
        private ExpSignToken()
        {
        }

        public sealed class Absent : ExpSignToken
        {
            private Absent()
            {
            }

            public static Absent Instance { get; } = new Absent();
        }

        public sealed class Positive : ExpSignToken
        {
            private Positive()
            {
            }

            public static Positive Instance { get; } = new Positive();
        }

        public sealed class Negative : ExpSignToken
        {
            private Negative()
            {
            }

            public static Negative Instance { get; } = new Negative();
        }
    }

    public sealed class DigitsReader<TNextReader> : IReader<IEnumerable<DigitToken>, TNextReader>
        where TNextReader : allows ref struct
        //// TODO reuse digits reader
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly List<DigitToken> tokens;
        private bool finished;

        public DigitsReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<DigitToken>();
        }

        public IEnumerable<DigitToken> TryGetValue(out bool read)
        {
            if (this.finished)
            {
                read = true;
            }
            else
            {
                read = this.TryGetValue2();
            }

            this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2()
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader
            while (true)
            {
                if (this.validBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (this.currentByteIndex >= this.validBytes)
                {
                    return false;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(digit);
            }

            return true;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class StringReader<TNextReader> : IReader<StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public StringReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public Task Read()
        {
            return Task.CompletedTask;
        }

        public StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>> TryMove(out bool read)
        {
            read = true;
            return new StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new CharsReader<StringDelimiterReader<TNextReader>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new StringDelimiterReader<TNextReader>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        this.nextReaderFactory)));
        }
    }

    //// TODO is "delimiter" a good name for this?
    public sealed class StringDelimiterReader<TNextReader> : IReader<StringDelimiterToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private bool read;

        public StringDelimiterReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public StringDelimiterToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, '"');
            if (!read)
            {
                return default!; //// TODO !
            }

            this.read = true;
            return StringDelimiterToken.Instance;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }
        public TNextReader TryMove(out bool read)
        {
            if (!this.read)
            {
                this.TryGetValue(out read);
                if (!read)
                {
                    return default!; //// TODO !
                }
            }

            read = this.read;
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class StringDelimiterToken
    {
        private StringDelimiterToken()
        {
        }

        public static StringDelimiterToken Instance { get; } = new StringDelimiterToken();
    }

    public sealed class CharsReader<TNextReader> : IReader<IEnumerable<CharToken>, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly List<CharToken> tokens;
        private bool finished;

        public CharsReader(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentByteIndex = currentByteIndex;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;

            this.tokens = new List<CharToken>();
        }

        public IEnumerable<CharToken> TryGetValue(out bool read)
        {
            if (this.finished)
            {
                read = true;
            }
            else
            {
                read = this.TryGetValue2();
            }

            this.finished = read;
            return this.tokens;
        }

        private bool TryGetValue2()
        {
            // NOTE: if you want the tokens "streamed", you can do that by having a reader that is either a "we have a whitespace" or "we are done with whitespace" token, and then "we have a whitespace" variant has the next whitespace reader
            while (true)
            {
                if (this.validBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (this.currentByteIndex >= this.validBytes)
                {
                    return false;
                }

                var currentByte = this.buffer[this.currentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++this.currentByteIndex;
                    if (this.currentByteIndex >= this.validBytes)
                    {
                        //// TODO keep track that you are reading an escaped character
                        return false;
                    }

                    if (this.validBytes == 0)
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    throw new Exception("TODO escaped characters are not yet supported");
                }

                CharToken @char;
                try
                {
                    @char = new CharToken.Unescaped(currentByte);
                }
                catch (Exception)
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(@char);
            }

            return true;
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public TNextReader TryMove(out bool read)
        {
            this.TryGetValue(out read);
            if (!read)
            {
                return default!; //// TODO !
            }

            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public abstract class CharToken
    {
        private CharToken()
        {
        }

        public sealed class Unescaped : CharToken
        {
            public Unescaped(byte @char)
            {
                if (!IsValid(@char))
                {
                    throw new Exception("TODO invalid JSON");
                }

                Char = @char;
            }

            private static bool IsValid(byte @char)
            {
                return
                    (@char >= 0x20 && @char <= 0x21) ||
                    (@char >= 0x23 && @char <= 0x5B) ||
                    (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
            }

            public byte Char { get; }
        }
    }

    public static class Helpers
    {
        public static (bool Read, int CurrentByteIndex, int ValidBytes) TryReadChar(Stream stream, byte[] buffer, int currentByteIndex, int validBytes, char character)
        {
            if (currentByteIndex >= validBytes)
            {
                return (false, currentByteIndex, validBytes);
            }

            ReadChar(buffer, currentByteIndex, validBytes, character);
            return (true, currentByteIndex + 1, validBytes);
        }

        public static async Task<(int CurrentByteIndex, int ValidBytes)> ReadChar(Stream stream, byte[] buffer, int currentByteIndex, int validBytes, char character)
        {
            if (currentByteIndex >= validBytes)
            {
                validBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                currentByteIndex = 0;

            }

            ReadChar(buffer, currentByteIndex, validBytes, character);

            return (currentByteIndex + 1, validBytes);
        }

        private static void ReadChar(byte[] buffer, int currentByteIndex, int validBytes, char character)
        {
            if (validBytes == 0 || buffer[currentByteIndex] != character)
            {
                throw new Exception("TODO invalid JSON");
            }
        }

        public static async Task ReadChar(Stream stream, byte[] buffer, int validBytes, char character)
        {
            var read = await stream.ReadAsync(buffer, 0, validBytes).ConfigureAwait(false);
            if (read == 0 || buffer[0] != character)
            {
                throw new Exception("TODO invalid JSON");
            }
        }
    }

    public sealed class PeekableStream : Stream
    {
        private readonly Stream stream;

        private readonly byte[] peekedByte;
        private bool hasPeeked;

        public PeekableStream(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new Exception("TODO");
            }

            this.stream = stream;

            this.peekedByte = new byte[1];
            this.hasPeeked = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException("TODO");

        public override long Position { get => throw new NotSupportedException("TODO"); set => throw new NotSupportedException("TODO"); }

        public override void Flush()
        {
            throw new NotSupportedException("TODO");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.hasPeeked)
            {
                buffer[offset] = this.peekedByte[0];
                this.hasPeeked = false;
                return 1;
            }

            return stream.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.hasPeeked)
            {
                buffer[offset] = peekedByte[0];
                this.hasPeeked = false;
                return 1;
            }

            this.hasPeeked = false;
            return await stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte?> PeekAsync()
        {
            var read = await this.ReadAsync(this.peekedByte).ConfigureAwait(false);
            if (read == 0)
            {
                return null;
            }

            this.hasPeeked = true;
            return this.peekedByte[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("TODO");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("TODO");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("TODO");
        }
    }
}
