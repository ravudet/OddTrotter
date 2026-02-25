namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
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

    public interface IAsyncReader<out TNextReader>
    {
        Task Read();

        TNextReader TryMove(out bool read);
    }

    public interface IAsyncReader<out TValue, out TNextReader> : IAsyncReader<TNextReader>
    {
        TValue TryGetValue(out bool read);
    }

    public interface IReader<out TNextReader>
    {
        ITask<TNextReader> Move();
    }

    public interface IReader<out TValue, out TNextReader> : IReader<TNextReader>
    {
        ITask<TValue> GetValue();
    }

    public sealed class JsonReader : IAsyncReader<WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private bool read;

        public JsonReader(Stream stream)
        {
            this.stream = stream;
            this.buffer = new byte[20]; //// TODO parameterize
        }

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
            this.read = true;
        }

        public WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> TryMove(out bool read)
        {
            read = this.read;
            return new WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>(
                this.stream,
                this.buffer,
                this.currentByteIndex,
                this.validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new ValueReader<WhitespaceReader<Nothing>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (nestedStream, nestedBuffer, currentByteIndex, nestedValidBytes) => new WhitespaceReader<Nothing>(
                        nestedStream,
                        nestedBuffer,
                        currentByteIndex,
                        nestedValidBytes,
                        (_, _, _, _) => new Nothing())));
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IAsyncReader<IEnumerable<WhitespaceToken>, TNextReader>
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

    public sealed class ValueReader<TNextReader> : IAsyncReader<ValueToken<TNextReader>>
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

    public sealed class FalseReader<TNextReader> : IAsyncReader<FalseToken, TNextReader>
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

    public sealed class NullReader<TNextReader> : IAsyncReader<NullToken, TNextReader>
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

    public sealed class TrueReader<TNextReader> : IAsyncReader<TrueToken, TNextReader>
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

    public sealed class ObjectReader<TNextReader> : IAsyncReader<ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
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

    public sealed class ObjectStartReader<TNextReader> : IAsyncReader<ObjectStartToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

            return ObjectStartToken.Instance;
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

    public sealed class ObjectStartToken
    {
        private ObjectStartToken()
        {
        }

        public static ObjectStartToken Instance { get; } = new ObjectStartToken();
    }

    public sealed class MembersReader<TNextReader> : IAsyncReader<MembersToken<TNextReader>>
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
                return new MembersToken<TNextReader>.None(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new MembersToken<TNextReader>.Some(
                new FirstMemberReader<TNextReader>(
                    this.stream,
                    this.buffer,
                    this.currentByteIndex,
                    this.validBytes,
                    this.nextReaderFactory));
        }
    }

    public abstract class MembersToken<TNextReader>
    {
        private MembersToken()
        {
        }

        public sealed class None : MembersToken<TNextReader>
        {
            public None(TNextReader reader)
            {
                Reader = reader;
            }

            public TNextReader Reader { get; }
        }

        public sealed class Some : MembersToken<TNextReader>
        {
            public Some(FirstMemberReader<TNextReader> reader)
            {
                Reader = reader;
            }

            public FirstMemberReader<TNextReader> Reader { get; }
        }
    }

    public sealed class FirstMemberReader<TNextReader> : IAsyncReader<MemberReader<SubsequentMembersReader<TNextReader>>>
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

    public sealed class SubsequentMembersReader<TNextReader> : IAsyncReader<SubsequentMembersToken<TNextReader>>
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
                return new SubsequentMembersToken<TNextReader>.None(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new SubsequentMembersToken<TNextReader>.More(
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

    public abstract class SubsequentMembersToken<TNextReader>
    {
        private SubsequentMembersToken()
        {
        }

        public sealed class None : SubsequentMembersToken<TNextReader>
        {
            public None(TNextReader reader)
            {
                Reader = reader;
            }

            public TNextReader Reader { get; }
        }

        public sealed class More : SubsequentMembersToken<TNextReader>
        {
            public More(SubsequentMemberReader<SubsequentMembersReader<TNextReader>> reader)
            {
                Reader = reader;
            }

            public SubsequentMemberReader<SubsequentMembersReader<TNextReader>> Reader { get; }
        }
    }

    public sealed class MemberReader<TNextReader> : IAsyncReader<StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
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

    public sealed class ColonReader<TNextReader> : IAsyncReader<ColonToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

            return ColonToken.Instance;
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

    public sealed class ColonToken
    {
        private ColonToken()
        {
        }

        public static ColonToken Instance { get; } = new ColonToken();
    }

    public sealed class SubsequentMemberReader<TNextReader> : IAsyncReader<CommaReader<WhitespaceReader<MemberReader<TNextReader>>>>
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

    public sealed class CommaReader<TNextReader> : IAsyncReader<CommaToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

            return CommaToken.Instance;
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

    public sealed class CommaToken
    {
        private CommaToken()
        {
        }

        public static CommaToken Instance { get; } = new CommaToken();
    }

    public sealed class ObjectEndReader<TNextReader> : IAsyncReader<ObjectEndToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

            return ObjectEndToken.Instance;
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

    public sealed class ObjectEndToken
    {
        private ObjectEndToken()
        {
        }

        public static ObjectEndToken Instance { get; } = new ObjectEndToken();
    }

    public sealed class ArrayReader<TNextReader> : IAsyncReader<ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>
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

    public sealed class ArrayStartReader<TNextReader> : IAsyncReader<ArrayStartToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

            return ArrayStartToken.Instance;
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

    public sealed class ArrayStartToken
    {
        private ArrayStartToken()
        {
        }

        public static ArrayStartToken Instance { get; } = new ArrayStartToken();
    }

    public sealed class ArrayElementsReader<TNextReader> : IAsyncReader<ArrayElementsToken<TNextReader>>
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
                return new ArrayElementsToken<TNextReader>.None(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new ArrayElementsToken<TNextReader>.Some(
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

    public abstract class ArrayElementsToken<TNextReader>
    {
        private ArrayElementsToken()
        {
        }

        public sealed class None : ArrayElementsToken<TNextReader>
        {
            public None(TNextReader reader)
            {
                Reader = reader;
            }

            public TNextReader Reader { get; }
        }

        public sealed class Some : ArrayElementsToken<TNextReader>
        {
            public Some(ArrayElementReader<SubsequentArrayElementsReader<TNextReader>> reader)
            {
                Reader = reader;
            }

            public ArrayElementReader<SubsequentArrayElementsReader<TNextReader>> Reader { get; }
        }
    }

    public sealed class ArrayElementReader<TNextReader> : IAsyncReader<ValueReader<TNextReader>>
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

    public sealed class SubsequentArrayElementsReader<TNextReader> : IAsyncReader<SubsequentArrayElementsToken<TNextReader>>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

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

        public async ITask<SubsequentArrayElementsToken<TNextReader>> Move()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            var currentByte = this.buffer[this.currentByteIndex];
            if (currentByte != ',')
            {
                return new SubsequentArrayElementsToken<TNextReader>.None(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new SubsequentArrayElementsToken<TNextReader>.More(
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

        public async Task Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
            this.currentByteIndex = 0;
        }

        public SubsequentArrayElementsToken<TNextReader> TryMove(out bool read)
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
            if (currentByte != ',')
            {
                return new SubsequentArrayElementsToken<TNextReader>.None(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new SubsequentArrayElementsToken<TNextReader>.More(
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

    public abstract class SubsequentArrayElementsToken<TNextReader>
    {
        private SubsequentArrayElementsToken()
        {
        }

        public sealed class None : SubsequentArrayElementsToken<TNextReader>
        {
            public None(TNextReader reader)
            {
                Reader = reader;
            }

            public TNextReader Reader { get; }
        }

        public sealed class More : SubsequentArrayElementsToken<TNextReader>
        {
            public More(SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>> reader)
            {
                Reader = reader;
            }

            public SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>> Reader { get; }
        }
    }

    public sealed class SubsequentArrayElementReader<TNextReader> : IAsyncReader<CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>>
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

    public sealed class ArrayEndReader<TNextReader> : IAsyncReader<ArrayEndToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<ArrayEndToken>> task;

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

            this.task = new Task<ITask<ArrayEndToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<ArrayEndToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<ArrayEndToken> GetValue2()
        {
            (this.currentByteIndex, this.validBytes) = await Helpers.ReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, ']').ConfigureAwait(false);
            return ArrayEndToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }

        public ArrayEndToken TryGetValue(out bool read)
        {
            (read, this.currentByteIndex, this.validBytes) = Helpers.TryReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, ']');
            if (!read)
            {
                return default!; //// TODO !
            }

            return ArrayEndToken.Instance;
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

    public sealed class ArrayEndToken
    {
        private ArrayEndToken()
        {
        }

        public static ArrayEndToken Instance { get; } = new ArrayEndToken();
    }

    public sealed class NumberReader<TNextReader> : IReader<SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>>
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

        public async ITask<SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>> Move()
        {
            return await Task.FromResult(
                new SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>(
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
                                this.nextReaderFactory)))))
                .ConfigureAwait(false);
        }
    }

    public sealed class SignReader<TNextReader> : IReader<SignToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<SignToken>> task;

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

            this.task = new Task<ITask<SignToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<SignToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<SignToken> GetValue2()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0 || this.buffer[this.currentByteIndex] != '-')
            {
                return SignToken.Absent.Instance;
            }

            ++this.currentByteIndex;
            return SignToken.Negative.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<IEnumerable<DigitToken>>> task;

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

            this.task = new Task<ITask<IEnumerable<DigitToken>>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<IEnumerable<DigitToken>> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<IEnumerable<DigitToken>> GetValue2()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<DigitToken> GetValueImpl()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            var currentByte = this.buffer[this.currentByteIndex];
            var digit = new DigitToken(currentByte);
            yield return digit;
            if (currentByte == '0')
            {
                yield break;
            }

            while (true)
            {
                if (this.currentByteIndex >= this.validBytes)
                {
                    this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                    this.currentByteIndex = 0;
                }

                if (this.validBytes == 0)
                {
                    yield break;
                }

                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception) //// TODO use correct exception type
                {
                    yield break;
                }

                ++this.currentByteIndex;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<FracToken>> task;

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

            this.task = new Task<ITask<FracToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<FracToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<FracToken> GetValue2()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0 || this.buffer[this.currentByteIndex] != '.')
            {
                return FracToken.Absent.Instance;
            }

            ++this.currentByteIndex;
            return new FracToken.Frac(await this.GetValueImpl().ToTask().ConfigureAwait(false));
        }

        private async IAsyncEnumerable<DigitToken> GetValueImpl()
        {
            while (true)
            {
                if (this.currentByteIndex >= this.validBytes)
                {
                    this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                    this.currentByteIndex = 0;
                }

                if (this.validBytes == 0)
                {
                    yield break;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception) //// TODO use correct exception type
                {
                    yield break;
                }

                ++this.currentByteIndex;
                yield return digit;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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

        public async ITask<ExpToken<TNextReader>> Move()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0 || (this.buffer[this.currentByteIndex] != 'e' && this.buffer[this.currentByteIndex] != 'E'))
            {
                return new ExpToken<TNextReader>.Absent(
                    this.nextReaderFactory(
                        this.stream,
                        this.buffer,
                        this.currentByteIndex,
                        this.validBytes));
            }

            return new ExpToken<TNextReader>.Present(
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

    public abstract class ExpToken<TNextReader>
    {
        private ExpToken()
        {
        }

        public sealed class Absent : ExpToken<TNextReader>
        {
            public Absent(TNextReader reader)
            {
                Reader = reader;
            }

            public TNextReader Reader { get; }
        }

        public sealed class Present : ExpToken<TNextReader>
        {
            public Present(EReader<ExpSignReader<DigitsReader<TNextReader>>> reader)
            {
                Reader = reader;
            }

            public EReader<ExpSignReader<DigitsReader<TNextReader>>> Reader { get; }
        }
    }

    public sealed class EReader<TNextReader> : IReader<EToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<EToken>> task;

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

            this.task = new Task<ITask<EToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<EToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<EToken> GetValue2()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0 || (this.buffer[this.currentByteIndex] != 'e' && this.buffer[this.currentByteIndex] != 'E'))
            {
                throw new Exception("TODO invalid JSON");
            }

            ++this.currentByteIndex;
            return new EToken(this.buffer[this.currentByteIndex]);
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<ExpSignToken>> task;

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

            this.task = new Task<ITask<ExpSignToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<ExpSignToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<ExpSignToken> GetValue2()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

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

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
        //// TODO reuse digits reader
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<IEnumerable<DigitToken>>> task;

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

            this.task = new Task<ITask<IEnumerable<DigitToken>>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<IEnumerable<DigitToken>> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<IEnumerable<DigitToken>> GetValue2()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<DigitToken> GetValueImpl()
        {
            if (this.currentByteIndex >= this.validBytes)
            {
                this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                this.currentByteIndex = 0;
            }

            if (this.validBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            while (true)
            {
                if (this.currentByteIndex >= this.validBytes)
                {
                    this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                    this.currentByteIndex = 0;
                }

                if (this.validBytes == 0)
                {
                    yield break;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(this.buffer[this.currentByteIndex]);
                }
                catch (Exception) //// TODO use correct exception type
                {
                    yield break;
                }

                ++this.currentByteIndex;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.currentByteIndex, this.validBytes);
        }
    }

    public sealed class StringReader<TNextReader> : IReader<StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
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

        public async ITask<StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>> Move()
        {
            return await Task.FromResult(
                new StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>(
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
                            this.nextReaderFactory)))).ConfigureAwait(false);
        }
    }

    //// TODO is "delimiter" a good name for this?
    public sealed class StringDelimiterReader<TNextReader> : IReader<StringDelimiterToken, TNextReader>
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<StringDelimiterToken>> task;

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

            this.task = new Task<ITask<StringDelimiterToken>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<StringDelimiterToken> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<StringDelimiterToken> GetValue2()
        {
            (this.currentByteIndex, this.validBytes) = await Helpers.ReadChar(this.stream, this.buffer, this.currentByteIndex, this.validBytes, '"').ConfigureAwait(false);
            return StringDelimiterToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int currentByteIndex;
        private int validBytes;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        private readonly Task<ITask<IEnumerable<CharToken>>> task;

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

            this.task = new Task<ITask<IEnumerable<CharToken>>>(async () => await this.GetValue2().ConfigureAwait(false));
        }

        public async ITask<IEnumerable<CharToken>> GetValue()
        {
            try
            {
                this.task.Start();
            }
            catch (InvalidOperationException)
            {
            }

            return await (await this.task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async ITask<IEnumerable<CharToken>> GetValue2()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<CharToken> GetValueImpl()
        {
            while (true)
            {
                if (this.currentByteIndex >= this.validBytes)
                {
                    this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                    this.currentByteIndex = 0;
                }

                if (this.validBytes == 0)
                {
                    yield break;
                }

                var currentByte = this.buffer[this.currentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++this.currentByteIndex;
                    if (this.currentByteIndex >= this.validBytes)
                    {
                        this.validBytes = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).ConfigureAwait(false);
                        this.currentByteIndex = 0;
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
                yield return @char;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
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
