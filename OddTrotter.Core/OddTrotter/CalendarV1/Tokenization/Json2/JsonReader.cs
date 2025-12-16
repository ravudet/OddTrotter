namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
                new PeekableStream(this.stream),
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public WhitespaceReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
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
                var peeked = await stream.PeekAsync().ConfigureAwait(false);
                if (peeked == null)
                {
                    yield break;
                }

                WhitespaceToken whitespace;
                try
                {
                    whitespace = new WhitespaceToken(peeked.Value);
                }
                catch (Exception)
                {
                    break;
                }

                await stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
                yield return whitespace;
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ValueReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ValueToken<TNextReader>> Move()
        {
            var peeked = await stream.PeekAsync().ConfigureAwait(false);
            if (peeked == null)
            {
                throw new Exception("TODO invalid JSON");
            }

            switch ((char)peeked)
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
                        new NullReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.validBytes,
                            this.nextReaderFactory));
                case 't':
                    return new ValueToken<TNextReader>.True(
                        new TrueReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.validBytes,
                            this.nextReaderFactory));
                case '{':
                    return new ValueToken<TNextReader>.Object(
                        new ObjectReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.validBytes,
                            this.nextReaderFactory));
                case '[':
                    return new ValueToken<TNextReader>.Array(
                        new ArrayReader<TNextReader>(
                            this.stream,
                            this.buffer,
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
                        new NumberReader<TNextReader>());
                case '"':
                    return new ValueToken<TNextReader>.String(
                        new StringReader<TNextReader>(
                            this.stream,
                            this.buffer,
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

    public sealed class FalseReader<TNextReader> : IReader<FalseToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public FalseReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<FalseToken> GetValue()
        {
            foreach (var @char in "false")
            {
                await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, @char).ConfigureAwait(false);
            }

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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public NullReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<NullToken> GetValue()
        {
            foreach (var @char in "null")
            {
                await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, @char).ConfigureAwait(false);
            }

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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public TrueReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<TrueToken> GetValue()
        {
            foreach (var @char in "true")
            {
                await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, @char).ConfigureAwait(false);
            }

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

    public sealed class ObjectReader<TNextReader> : IReader<ObjectStartReader<WhitespaceReader<MemberReader<SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ObjectReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ObjectStartReader<WhitespaceReader<MemberReader<SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>> Move()
        {
            return await Task.FromResult(
                new ObjectStartReader<WhitespaceReader<MemberReader<SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) =>
                        new WhitespaceReader<MemberReader<SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>(
                            stream,
                            buffer,
                            validBytes,
                            (stream, buffer, validBytes) => new MemberReader<SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>(
                                stream,
                                buffer,
                                validBytes,
                                (strema, buffer, validBytes) => new SubsequentMemberReader<WhitespaceReader<ObjectEndReader<TNextReader>>>(
                                    stream,
                                    buffer,
                                    validBytes,
                                    (stream, buffer, validBytes) => new WhitespaceReader<ObjectEndReader<TNextReader>>(
                                        stream,
                                        buffer,
                                        validBytes,
                                        (stream, buffer, validBytes) => new ObjectEndReader<TNextReader>(
                                            stream,
                                            buffer,
                                            validBytes,
                                            this.nextReaderFactory))))))).ConfigureAwait(false);
        }
    }

    public sealed class ObjectStartReader<TNextReader> : IReader<ObjectStartToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ObjectStartReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ObjectStartToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, '{').ConfigureAwait(false);
            return ObjectStartToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class ObjectStartToken
    {
        private ObjectStartToken()
        {
        }

        public static ObjectStartToken Instance { get; } = new ObjectStartToken();
    }

    //// TODO you got this wrong, there might not be any members
    public sealed class MemberReader<TNextReader> : IReader<StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public MemberReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>> Move()
        {
            return await Task.FromResult(
                new StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new ColonReader<WhitespaceReader<ValueReader<TNextReader>>>(
                            stream,
                            buffer,
                            validBytes,
                            (stream, buffer, validBytes) => new WhitespaceReader<ValueReader<TNextReader>>(
                                stream,
                                buffer,
                                validBytes,
                                (stream, buffer, validBytes) => new ValueReader<TNextReader>(
                                    stream,
                                    buffer,
                                    validBytes,
                                    this.nextReaderFactory)))))).ConfigureAwait(false);
        }
    }

    public sealed class ColonReader<TNextReader> : IReader<ColonToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ColonReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ColonToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, ':').ConfigureAwait(false);
            return ColonToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public SubsequentMemberReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<CommaReader<WhitespaceReader<MemberReader<TNextReader>>>> Move()
        {
            return await Task.FromResult(
                new CommaReader<WhitespaceReader<MemberReader<TNextReader>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new WhitespaceReader<MemberReader<TNextReader>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new MemberReader<TNextReader>(
                            stream,
                            buffer,
                            validBytes,
                            this.nextReaderFactory)))).ConfigureAwait(false);
        }
    }

    public sealed class CommaReader<TNextReader> : IReader<CommaToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public CommaReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<CommaToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, ',').ConfigureAwait(false);
            return CommaToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ObjectEndReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ObjectEndToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, '}').ConfigureAwait(false);
            return ObjectEndToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class ObjectEndToken
    {
        private ObjectEndToken()
        {
        }

        public static ObjectEndToken Instance { get; } = new ObjectEndToken();
    }

    //// TODO you got this wrong, there might not be any array elements
    public sealed class ArrayReader<TNextReader> : IReader<ArrayStartReader<WhitespaceReader<ArrayElementReader<SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ArrayReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ArrayStartReader<WhitespaceReader<ArrayElementReader<SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>> Move()
        {
            return await Task.FromResult(
                new ArrayStartReader<WhitespaceReader<ArrayElementReader<SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new WhitespaceReader<ArrayElementReader<SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new ArrayElementReader<SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>(
                            stream,
                            buffer,
                            validBytes,
                            (stream, buffer, validBytes) => new SubsequentArrayElementReader<WhitespaceReader<ArrayEndReader<TNextReader>>>(
                                stream,
                                buffer,
                                validBytes,
                                (stream, buffer, validBytes) => new WhitespaceReader<ArrayEndReader<TNextReader>>(
                                    stream,
                                    buffer,
                                    validBytes,
                                    (stream, buffer, validBytes) => new ArrayEndReader<TNextReader>(
                                        stream,
                                        buffer,
                                        validBytes,
                                        this.nextReaderFactory)))))))
                .ConfigureAwait(false);
        }
    }

    public sealed class ArrayStartReader<TNextReader> : IReader<ArrayStartToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ArrayStartReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ArrayStartToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, '[').ConfigureAwait(false);
            return ArrayStartToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
        }
    }

    public sealed class ArrayStartToken
    {
        private ArrayStartToken()
        {
        }

        public static ArrayStartToken Instance { get; } = new ArrayStartToken();
    }

    public sealed class ArrayElementReader<TNextReader> : IReader<ValueReader<TNextReader>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ArrayElementReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ValueReader<TNextReader>> Move()
        {
            return await Task.FromResult(
                new ValueReader<TNextReader>(
                    this.stream, 
                    this.buffer, 
                    this.validBytes, 
                    this.nextReaderFactory)).ConfigureAwait(false);
        }
    }

    public sealed class SubsequentArrayElementReader<TNextReader> : IReader<CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public SubsequentArrayElementReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>> Move()
        {
            return await Task.FromResult(
                new CommaReader<WhitespaceReader<ArrayElementReader<TNextReader>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new WhitespaceReader<ArrayElementReader<TNextReader>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new ArrayElementReader<TNextReader>(
                            stream,
                            buffer,
                            validBytes,
                            this.nextReaderFactory))))
                .ConfigureAwait(false);
        }
    }

    public sealed class ArrayEndReader<TNextReader> : IReader<ArrayEndToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ArrayEndReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<ArrayEndToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, ']').ConfigureAwait(false);
            return ArrayEndToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public NumberReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>> Move()
        {
            return await Task.FromResult(
                new SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new IntReader<FracReader<ExpReader<TNextReader>>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new FracReader<ExpReader<TNextReader>>(
                            stream,
                            buffer,
                            validBytes,
                            (stream, buffer, validBytes) => new ExpReader<TNextReader>()))
                .ConfigureAwait(false);
        }
    }

    public sealed class SignReader<TNextReader> : IReader<SignToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public SignReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<SignToken> GetValue()
        {
            var peeked = await this.stream.PeekAsync().ConfigureAwait(false);
            if (peeked != '-')
            {
                return SignToken.Absent.Instance;
            }

            await this.stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
            return SignToken.Negative.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public IntReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<IEnumerable<DigitToken>> GetValue()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<DigitToken> GetValueImpl()
        {
            var read = await this.stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
            if (read == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            var digit = new DigitToken(this.buffer[0]);
            yield return digit;
            if (this.buffer[0] == '0')
            {
                yield break;
            }

            while (true)
            {
                var peeked = await this.stream.PeekAsync().ConfigureAwait(false);
                if (peeked == null)
                {
                    yield break;
                }

                try
                {
                    digit = new DigitToken(peeked.Value);
                }
                catch (Exception) //// TODO use correct exception type
                {
                    yield break;
                }

                await this.stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public FracReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<FracToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, '.').ConfigureAwait(false);
            return new FracToken.Frac(await this.GetValueImpl().ToTask().ConfigureAwait(false));
        }

        private async IAsyncEnumerable<DigitToken> GetValueImpl()
        {
            while (true)
            {
                var peeked = await this.stream.PeekAsync().ConfigureAwait(false);
                if (peeked == null)
                {
                    yield break;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(peeked.Value);
                }
                catch (Exception) //// TODO use correct exception type
                {
                    yield break;
                }

                await this.stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
                yield return digit;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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

    public sealed class ExpReader<TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public ExpReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

    }

    public sealed class StringReader<TNextReader> : IReader<StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public StringReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>> Move()
        {
            return await Task.FromResult(
                new StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>(
                    this.stream,
                    this.buffer,
                    this.validBytes,
                    (stream, buffer, validBytes) => new CharsReader<StringDelimiterReader<TNextReader>>(
                        stream,
                        buffer,
                        validBytes,
                        (stream, buffer, validBytes) => new StringDelimiterReader<TNextReader>(
                            stream,
                            buffer,
                            validBytes,
                            this.nextReaderFactory)))).ConfigureAwait(false);
        }
    }

    //// TODO is "delimiter" a good name for this?
    public sealed class StringDelimiterReader<TNextReader> : IReader<StringDelimiterToken, TNextReader>
    {
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public StringDelimiterReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<StringDelimiterToken> GetValue()
        {
            await Helpers.ReadChar(this.stream, this.buffer, this.validBytes, '"').ConfigureAwait(false);
            return StringDelimiterToken.Instance;
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
        private readonly PeekableStream stream;
        private readonly byte[] buffer;
        private readonly int validBytes;
        private readonly Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory;

        public CharsReader(
            PeekableStream stream,
            byte[] buffer,
            int validBytes,
            Func<PeekableStream, byte[], int, TNextReader> nextReaderFactory)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.validBytes = validBytes;
            this.nextReaderFactory = nextReaderFactory;
        }

        public async ITask<IEnumerable<CharToken>> GetValue()
        {
            return await this.GetValueImpl().ToTask().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<CharToken> GetValueImpl()
        {
            while (true)
            {
                var peeked = await stream.PeekAsync().ConfigureAwait(false);
                if (peeked == null)
                {
                    yield break;
                }

                if (peeked == 0x5C)
                {
                    await stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
                    peeked = await stream.PeekAsync().ConfigureAwait(false);
                    if (peeked == null)
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    throw new Exception("TODO escaped characters are not yet supported");
                }

                CharToken @char;
                try
                {
                    @char = new CharToken.Unescaped(peeked.Value);
                }
                catch (Exception)
                {
                    break;
                }

                await stream.ReadAsync(this.buffer, 0, this.validBytes).ConfigureAwait(false);
                yield return @char;
            }
        }

        public async ITask<TNextReader> Move()
        {
            await this.GetValue().ConfigureAwait(false);
            return this.nextReaderFactory(this.stream, this.buffer, this.validBytes);
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
