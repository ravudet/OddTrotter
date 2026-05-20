namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Fx;

    using OddTrotter.CalendarV1.Tokenization.Readers;

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
        ValueTask Read();

        TNextReader TryMove(out bool read);
    }

    public interface IReader2<TSelf, TNextReader>
        where TSelf : IReader2<TSelf, TNextReader>, allows ref struct
        where TNextReader : allows ref struct
    {
        ValueTask Read(ReaderContext readerContext);

        TypeHolder<TSelf, TNextReader> AsReader { get; }

        ////ReaderContext Context { get; }

        ////Func<ReaderContext, TSelf> Factory { get; } //// TODO you should remove this once all of the readers are converted to `ref struct`; it should never need to be called, the factory that was originally used to instantiate the `ireader2` should be re-used instead (the one that the caller got from `trymove3`)

        bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory);
    }

    public interface IReader2<TSelf, TValue, TNextReader> : IReader2<TSelf, TNextReader>
        where TSelf : IReader2<TSelf, TValue, TNextReader>, allows ref struct
        where TNextReader : allows ref struct
        where TValue : allows ref struct
    {
        new TypeHolder<TSelf, TValue, TNextReader> AsReader { get; }

        bool TryGetValue3(ref ReaderContext readerContext, out TValue value);
    }

    public interface IReader2<TSelf, TContext, TValue, TNextReader> : IReader2<TSelf, TNextReader>
        where TSelf : IReader2<TSelf, TContext, TValue, TNextReader>, allows ref struct
        where TNextReader : allows ref struct
        where TValue : allows ref struct
    {
        new TypeHolder<TSelf, TValue, TNextReader> AsReader { get; }

        bool TryGetValue3(ref ReaderContext readerContext, out TValue value, out TContext context, out Func<TContext, TSelf> currentReaderFactory);
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
        public static async ValueTask Read(this ReaderContext readerContext)
        {
            readerContext.ValidBytes = await readerContext.Stream.ReadAsync(readerContext.Buffer.AsMemory()).ConfigureAwait(false);
            readerContext.CurrentByteIndex = 0;
        }

        public static Task Read2(this ReaderContext readerContext)
        {
            return readerContext.Stream.ReadAsync(readerContext.Buffer, 0, readerContext.Buffer.Length).ContinueWith(
                _ =>
                {
                    //// TODO avoid this closure
                    readerContext.ValidBytes = _.Result;
                    readerContext.CurrentByteIndex = 0;
                });
        }
    }

    public ref struct JsonReader : IReader2<JsonReader, WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>>
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

        public Func<JsonReader> Factory { get; } = static () => new JsonReader();

        public ReaderContext Context { get; }

        public TypeHolder<JsonReader, WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>> AsReader
        {
            get
            {
                return new TypeHolder<JsonReader, WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();

            ////this.read = true;
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>> nextFactory)
        {
            nextFactory = WhitespaceReaderFactory;
            return true;
        }

        public static WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>> WhitespaceReaderFactory()
        {
            return new WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>(ValueReaderFactory);
        }

        public static ValueReader2<WhitespaceReader2<Nothing>> ValueReaderFactory()
        {
            return new ValueReader2<WhitespaceReader2<Nothing>>(
                WhitespaceReaderFactory2);
        }

        public static WhitespaceReader2<Nothing> WhitespaceReaderFactory2()
        {
            /*return new WhitespaceReader<Nothing>(
                nestedStream,
                nestedBuffer,
                currentByteIndex,
                nestedValidBytes,
                NothingFactory);*/
            return new WhitespaceReader2<Nothing>(
                NothingFactory);
        }

        public static Nothing NothingFactory()
        {
            return new Nothing();
        }
    }

    public ref struct WhitespaceReader2<TNextReader> : IReader2<WhitespaceReader2<TNextReader>, IEnumerable<WhitespaceToken>, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

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
            Func<TNextReader> nextReaderFactory)
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

                if (!WhitespaceToken.TryCreate(readerContext.Buffer[readerContext.CurrentByteIndex], out var whitespace))
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                this.tokens.Add(whitespace);
            }

            return true;
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            //// TODO remove this method? callers already have to know about the context, let them call it directly
            return readerContext.Read();
            /*this.Context.ValidBytes = await this.Context.Stream.ReadAsync(this.Context.Buffer, 0, this.Context.Buffer.Length).ConfigureAwait(false);
            this.Context.CurrentByteIndex = 0;*/
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out IEnumerable<WhitespaceToken> value)
        {
            value = this.TryGetValue(readerContext, out var read);
            return read;
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory) //// TODO i think the nextfactory doesn't need an input parameter since move and getvalue both take in the context themselves
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

                if (!WhitespaceToken.TryCreate(this.buffer[this.currentByteIndex], out var whitespace))
                {
                    break;
                }

                ++this.currentByteIndex;
                this.tokens.Add(whitespace);
            }

            return true;
        }

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

    public struct WhitespaceToken
    {
        public static bool TryCreate(byte @char, out WhitespaceToken whitespaceToken)
        {
            switch (@char)
            {
                case 0x20:
                case 0x09:
                case 0x0A:
                case 0x0D:
                    whitespaceToken = new WhitespaceToken(@char);
                    return true;
                default:
                    whitespaceToken = default;
                    return false;
            }
        }

        private WhitespaceToken(byte @char)
        {
            this.Char = @char;
        }

        public byte Char { get; }
    }

    public ref struct ValueReader2<TNextReader> : IReader2<ValueReader2<TNextReader>, ValueToken2<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public TypeHolder<ValueReader2<TNextReader>, ValueToken2<TNextReader>> AsReader
        {
            get
            {
                return new TypeHolder<ValueReader2<TNextReader>, ValueToken2<TNextReader>>(this);
            }
        }

        public ValueReader2(
            Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<ValueToken2<TNextReader>> nextFactory)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                nextFactory = default!; //// TODO !
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            var nextReaderFactory = this.nextReaderFactory;
            switch ((char)readerContext.Buffer[readerContext.CurrentByteIndex])
            {
                case '{':
                    nextFactory = () => new ValueToken2<TNextReader>(
                        () => new ObjectReader2<TNextReader>(
                            nextReaderFactory));
                    return true;
            }

            var localReaderContext = readerContext;
            nextFactory = () => Factory(localReaderContext, nextReaderFactory); //// TODO once the subsequent readers follow the new pattern, you shouldn't need to close the readercontext anymore (and by the way, the reader context being closed is a bug...)
            return true;
        }

        private static ValueToken2<TNextReader> Factory(ReaderContext readerContext, Func<TNextReader> nextReaderFactory)
        {
            switch ((char)readerContext.Buffer[readerContext.CurrentByteIndex])
            {
                case 'f':
                    return new ValueToken2<TNextReader>(
                        new FalseReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
                case 'n':
                    return new ValueToken2<TNextReader>(
                        new NullReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
                case 't':
                    return new ValueToken2<TNextReader>(
                        new TrueReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
                case '[':
                    return new ValueToken2<TNextReader>(
                        new ArrayReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
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
                    return new ValueToken2<TNextReader>(
                        new NumberReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
                case '"':
                    return new ValueToken2<TNextReader>(
                        new StringReader<TNextReader>(
                            readerContext.Stream,
                            readerContext.Buffer,
                            readerContext.CurrentByteIndex,
                            readerContext.ValidBytes,
                            (_, _, _, _) => nextReaderFactory()));
                default:
                    throw new Exception("tODO invalid JSON");
            }
        }
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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
                    return new ValueToken<TNextReader>(
                        new FalseReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case 'n':
                    return new ValueToken<TNextReader>(
                        new NullReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case 't':
                    return new ValueToken<TNextReader>(
                        new TrueReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '{':
                    return new ValueToken<TNextReader>(
                        new ObjectReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '[':
                    return new ValueToken<TNextReader>(
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
                    return new ValueToken<TNextReader>(
                        new NumberReader<TNextReader>(
                            this.stream,
                            this.buffer,
                            this.currentByteIndex,
                            this.validBytes,
                            this.nextReaderFactory));
                case '"':
                    return new ValueToken<TNextReader>(
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

    public ref struct ValueToken<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly int type;
        private readonly FalseReader<TNextReader>? falseReader;
        private readonly NullReader<TNextReader>? nullReader;
        private readonly TrueReader<TNextReader>? trueReader;
        private readonly ObjectReader<TNextReader>? objectReader;
        private readonly ArrayReader<TNextReader>? arrayReader;
        private readonly NumberReader<TNextReader>? numberReader;
        private readonly StringReader<TNextReader>? stringReader;

        public ValueToken(FalseReader<TNextReader> falseReader)
        {
            this.type = 1;
            this.falseReader = falseReader;
        }

        public ValueToken(NullReader<TNextReader> nullReader)
        {
            this.type = 2;
            this.nullReader = nullReader;
        }

        public ValueToken(TrueReader<TNextReader> trueReader)
        {
            this.type = 3;
            this.trueReader = trueReader;
        }

        public ValueToken(ObjectReader<TNextReader> objectReader)
        {
            this.type = 4;
            this.objectReader = objectReader;
        }

        public ValueToken(ArrayReader<TNextReader> arrayReader)
        {
            this.type = 5;
            this.arrayReader = arrayReader;
        }

        public ValueToken(NumberReader<TNextReader> numberReader)
        {
            this.type = 6;
            this.numberReader = numberReader;
        }

        public ValueToken(StringReader<TNextReader> stringReader)
        {
            this.type = 7;
            this.stringReader = stringReader;
        }

        public TResult Apply<TResult>(
            Func<FalseReader<TNextReader>, TResult> @false,
            Func<NullReader<TNextReader>, TResult> @null,
            Func<TrueReader<TNextReader>, TResult> @true,
            Func<ObjectReader<TNextReader>, TResult> @object,
            Func<ArrayReader<TNextReader>, TResult> array,
            Func<NumberReader<TNextReader>, TResult> number,
            Func<StringReader<TNextReader>, TResult> @string)
            where TResult : allows ref struct
        {
            switch (this.type)
            {
                case 1:
                    return @false(this.falseReader!);
                case 2:
                    return @null(this.nullReader!);
                case 3:
                    return @true(this.trueReader!);
                case 4:
                    return @object(this.objectReader!);
                case 5:
                    return @array(this.arrayReader!);
                case 6:
                    return @number(this.numberReader!);
                case 7:
                    return @string(this.stringReader!);
                default:
                    throw new Exception("TODO visitor");
            }
        }

        public RefNullable<FalseReader<TNextReader>> TryFalse()
        {
            return this.Apply(
                RefNullable.Value,
                FalseNull,
                FalseTrue,
                FalseObject,
                FalseArray,
                FalseNumber,
                FalseString);
        }

        private static RefNullable<FalseReader<TNextReader>> FalseNull(NullReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseString(StringReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        public RefNullable<NullReader<TNextReader>> TryNull()
        {
            return this.Apply(
                NullFalse,
                RefNullable.Value,
                NullTrue,
                NullObject,
                NullArray,
                NullNumber,
                NullString);
        }

        private static RefNullable<NullReader<TNextReader>> NullFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullString(StringReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        public RefNullable<TrueReader<TNextReader>> TryTrue()
        {
            return this.Apply(
                TrueFalse,
                TrueNull,
                RefNullable.Value,
                TrueObject,
                TrueArray,
                TrueNumber,
                TrueString);
        }

        private static RefNullable<TrueReader<TNextReader>> TrueFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueNull(NullReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueString(StringReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        public RefNullable<ObjectReader<TNextReader>> TryObject()
        {
            return this.Apply(
                ObjectFalse,
                ObjectNull,
                ObjectTrue,
                RefNullable.Value,
                ObjectArray,
                ObjectNumber,
                ObjectString);
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectNull(NullReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        private static RefNullable<ObjectReader<TNextReader>> ObjectString(StringReader<TNextReader> _)
        {
            return new RefNullable<ObjectReader<TNextReader>>();
        }

        public RefNullable<ArrayReader<TNextReader>> TryArray()
        {
            return this.Apply(
                ArrayFalse,
                ArrayNull,
                ArrayTrue,
                ArrayObject,
                RefNullable.Value,
                ArrayNumber,
                ArrayString);
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayNull(NullReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayString(StringReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        public RefNullable<NumberReader<TNextReader>> TryNumber()
        {
            return this.Apply(
                NumberFalse,
                NumberNull,
                NumberTrue,
                NumberObject,
                NumberArray,
                RefNullable.Value,
                NumberString);
        }

        private static RefNullable<NumberReader<TNextReader>> NumberFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberNull(NullReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberString(StringReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        public RefNullable<StringReader<TNextReader>> TryString()
        {
            return this.Apply(
                StringFalse,
                StringNull,
                StringTrue,
                StringObject,
                StringArray,
                StringNumber,
                RefNullable.Value);
        }

        private static RefNullable<StringReader<TNextReader>> StringFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringNull(NullReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringObject(ObjectReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }
    }

    public ref struct ValueToken2<TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly int type;
        private readonly FalseReader<TNextReader>? falseReader;
        private readonly NullReader<TNextReader>? nullReader;
        private readonly TrueReader<TNextReader>? trueReader;
        private readonly Func<ObjectReader2<TNextReader>>? objectReader;
        private readonly ArrayReader<TNextReader>? arrayReader;
        private readonly NumberReader<TNextReader>? numberReader;
        private readonly StringReader<TNextReader>? stringReader;

        public ValueToken2(FalseReader<TNextReader> falseReader)
        {
            this.type = 1;
            this.falseReader = falseReader;
        }

        public ValueToken2(NullReader<TNextReader> nullReader)
        {
            this.type = 2;
            this.nullReader = nullReader;
        }

        public ValueToken2(TrueReader<TNextReader> trueReader)
        {
            this.type = 3;
            this.trueReader = trueReader;
        }

        public ValueToken2(Func<ObjectReader2<TNextReader>> objectReader)
        {
            this.type = 4;
            this.objectReader = objectReader;
        }

        public ValueToken2(ArrayReader<TNextReader> arrayReader)
        {
            this.type = 5;
            this.arrayReader = arrayReader;
        }

        public ValueToken2(NumberReader<TNextReader> numberReader)
        {
            this.type = 6;
            this.numberReader = numberReader;
        }

        public ValueToken2(StringReader<TNextReader> stringReader)
        {
            this.type = 7;
            this.stringReader = stringReader;
        }

        public TResult Apply<TResult>(
            Func<FalseReader<TNextReader>, TResult> @false,
            Func<NullReader<TNextReader>, TResult> @null,
            Func<TrueReader<TNextReader>, TResult> @true,
            Func<Func<ObjectReader2<TNextReader>>, TResult> @object,
            Func<ArrayReader<TNextReader>, TResult> array,
            Func<NumberReader<TNextReader>, TResult> number,
            Func<StringReader<TNextReader>, TResult> @string)
            where TResult : allows ref struct
        {
            switch (this.type)
            {
                case 1:
                    return @false(this.falseReader!);
                case 2:
                    return @null(this.nullReader!);
                case 3:
                    return @true(this.trueReader!);
                case 4:
                    this.objectReader.TryGetValue(out var reader);
                    return @object(reader!);
                case 5:
                    return @array(this.arrayReader!);
                case 6:
                    return @number(this.numberReader!);
                case 7:
                    return @string(this.stringReader!);
                default:
                    throw new Exception("TODO visitor");
            }
        }

        public RefNullable<FalseReader<TNextReader>> TryFalse()
        {
            return this.Apply(
                RefNullable.Value,
                FalseNull,
                FalseTrue,
                FalseObject,
                FalseArray,
                FalseNumber,
                FalseString);
        }

        private static RefNullable<FalseReader<TNextReader>> FalseNull(NullReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        private static RefNullable<FalseReader<TNextReader>> FalseString(StringReader<TNextReader> _)
        {
            return new RefNullable<FalseReader<TNextReader>>();
        }

        public RefNullable<NullReader<TNextReader>> TryNull()
        {
            return this.Apply(
                NullFalse,
                RefNullable.Value,
                NullTrue,
                NullObject,
                NullArray,
                NullNumber,
                NullString);
        }

        private static RefNullable<NullReader<TNextReader>> NullFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        private static RefNullable<NullReader<TNextReader>> NullString(StringReader<TNextReader> _)
        {
            return new RefNullable<NullReader<TNextReader>>();
        }

        public RefNullable<TrueReader<TNextReader>> TryTrue()
        {
            return this.Apply(
                TrueFalse,
                TrueNull,
                RefNullable.Value,
                TrueObject,
                TrueArray,
                TrueNumber,
                TrueString);
        }

        private static RefNullable<TrueReader<TNextReader>> TrueFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueNull(NullReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        private static RefNullable<TrueReader<TNextReader>> TrueString(StringReader<TNextReader> _)
        {
            return new RefNullable<TrueReader<TNextReader>>();
        }

        public bool TryObject(out Func<ObjectReader2<TNextReader>> objectReader)
        {
            objectReader = this.Apply(
                ObjectFalse,
                ObjectNull,
                ObjectTrue,
                _ => _,
                ObjectArray,
                ObjectNumber,
                ObjectString)!; //// TODO !
            return objectReader != null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectFalse(FalseReader<TNextReader> _)
        {
            return null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectNull(NullReader<TNextReader> _)
        {
            return null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectTrue(TrueReader<TNextReader> _)
        {
            return null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectArray(ArrayReader<TNextReader> _)
        {
            return null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectNumber(NumberReader<TNextReader> _)
        {
            return null;
        }

        private static Func<ObjectReader2<TNextReader>>? ObjectString(StringReader<TNextReader> _)
        {
            return null;
        }

        public RefNullable<ArrayReader<TNextReader>> TryArray()
        {
            return this.Apply(
                ArrayFalse,
                ArrayNull,
                ArrayTrue,
                ArrayObject,
                RefNullable.Value,
                ArrayNumber,
                ArrayString);
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayNull(NullReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        private static RefNullable<ArrayReader<TNextReader>> ArrayString(StringReader<TNextReader> _)
        {
            return new RefNullable<ArrayReader<TNextReader>>();
        }

        public RefNullable<NumberReader<TNextReader>> TryNumber()
        {
            return this.Apply(
                NumberFalse,
                NumberNull,
                NumberTrue,
                NumberObject,
                NumberArray,
                RefNullable.Value,
                NumberString);
        }

        private static RefNullable<NumberReader<TNextReader>> NumberFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberNull(NullReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        private static RefNullable<NumberReader<TNextReader>> NumberString(StringReader<TNextReader> _)
        {
            return new RefNullable<NumberReader<TNextReader>>();
        }

        public RefNullable<StringReader<TNextReader>> TryString()
        {
            return this.Apply(
                StringFalse,
                StringNull,
                StringTrue,
                StringObject,
                StringArray,
                StringNumber,
                RefNullable.Value);
        }

        private static RefNullable<StringReader<TNextReader>> StringFalse(FalseReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringNull(NullReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringTrue(TrueReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringObject(Func<ObjectReader2<TNextReader>> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringArray(ArrayReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }

        private static RefNullable<StringReader<TNextReader>> StringNumber(NumberReader<TNextReader> _)
        {
            return new RefNullable<StringReader<TNextReader>>();
        }
    }

    public ref struct FalseReader2<TNextReader> : IReader2<FalseReader2<TNextReader>, fReader<aReader<lReader<sReader<eReader<TNextReader>>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public FalseReader2(Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<FalseReader2<TNextReader>, fReader<aReader<lReader<sReader<eReader<TNextReader>>>>>> AsReader => throw new NotImplementedException();

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<fReader<aReader<lReader<sReader<eReader<TNextReader>>>>>> nextFactory)
        {
            var nextReaderFactory = this.nextReaderFactory;
            nextFactory = () =>
                new fReader<aReader<lReader<sReader<eReader<TNextReader>>>>>(
                    () => new aReader<lReader<sReader<eReader<TNextReader>>>>(
                        () => new lReader<sReader<eReader<TNextReader>>>(
                            () => new sReader<eReader<TNextReader>>(
                                () => new eReader<TNextReader>(
                                    nextReaderFactory)))));
            return true;
        }
    }

    public ref struct fReader<TNextReader> : IReader2<fReader<TNextReader>, fToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public fReader(Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<fReader<TNextReader>, fToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<fReader<TNextReader>, fToken, TNextReader>(this);
            }
        }

        TypeHolder<fReader<TNextReader>, TNextReader> IReader2<fReader<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<fReader<TNextReader>, TNextReader>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out fToken value)
        {
            value = fToken.Instance;
            return Helpers.TryReadChar(ref readerContext, 'f');
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            nextFactory = this.nextReaderFactory;
            return true;
        }
    }

    public sealed class fToken
    {
        private fToken()
        {
        }

        public static fToken Instance { get; } = new fToken();
    }

    public ref struct aReader<TNextReader> : IReader2<aReader<TNextReader>, aToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public aReader(Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<aReader<TNextReader>, aToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<aReader<TNextReader>, aToken, TNextReader>(this);
            }
        }

        TypeHolder<aReader<TNextReader>, TNextReader> IReader2<aReader<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<aReader<TNextReader>, TNextReader>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out aToken value)
        {
            value = aToken.Instance;
            return Helpers.TryReadChar(ref readerContext, 'a');
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            nextFactory = this.nextReaderFactory;
            return true;
        }
    }

    public sealed class aToken
    {
        private aToken()
        {
        }

        public static aToken Instance { get; } = new aToken();
    }

    public ref struct lReader<TNextReader> : IReader2<lReader<TNextReader>, lToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public lReader(Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<lReader<TNextReader>, lToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<lReader<TNextReader>, lToken, TNextReader>(this);
            }
        }

        TypeHolder<lReader<TNextReader>, TNextReader> IReader2<lReader<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<lReader<TNextReader>, TNextReader>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out lToken value)
        {
            value = lToken.Instance;
            return Helpers.TryReadChar(ref readerContext, 'l');
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            nextFactory = this.nextReaderFactory;
            return true;
        }
    }

    public sealed class lToken
    {
        private lToken()
        {
        }

        public static lToken Instance { get; } = new lToken();
    }

    public ref struct sReader<TNextReader> : IReader2<sReader<TNextReader>, sToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public sReader(Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<sReader<TNextReader>, sToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<sReader<TNextReader>, sToken, TNextReader>(this);
            }
        }

        TypeHolder<sReader<TNextReader>, TNextReader> IReader2<sReader<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<sReader<TNextReader>, TNextReader>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out sToken value)
        {
            value = sToken.Instance;
            return Helpers.TryReadChar(ref readerContext, 's');
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            nextFactory = this.nextReaderFactory;
            return true;
        }
    }

    public sealed class sToken
    {
        private sToken()
        {
        }

        public static sToken Instance { get; } = new sToken();
    }

    public ref struct eReader<TNextReader> : IReader2<eReader<TNextReader>, eToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public eReader(Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<eReader<TNextReader>, eToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<eReader<TNextReader>, eToken, TNextReader>(this);
            }
        }

        TypeHolder<eReader<TNextReader>, TNextReader> IReader2<eReader<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<eReader<TNextReader>, TNextReader>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out eToken value)
        {
            value = eToken.Instance;
            return Helpers.TryReadChar(ref readerContext, 'e');
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            var stream = readerContext.Stream;
            var buffer = readerContext.Buffer;
            var currentByteIndex = readerContext.CurrentByteIndex;
            var validBytes = readerContext.ValidBytes;
            var nextReaderFactory = this.nextReaderFactory;

            nextFactory = () => nextReaderFactory(stream, buffer, currentByteIndex, validBytes);
            return true;
        }
    }

    public sealed class eToken
    {
        private eToken()
        {
        }

        public static eToken Instance { get; } = new eToken();
    }

    public ref struct FalseReader3<TNextReader> : IReader2<FalseReader3<TNextReader>, int, FalseToken, TNextReader>
        where TNextReader : allows ref struct
    {
        private readonly string literal = "false";
        private int currentCharacter;
        private readonly Func<Stream, byte[], int, int, TNextReader> nextReaderFactory;

        public TypeHolder<FalseReader3<TNextReader>, FalseToken, TNextReader> AsReader
        {
            get
            {
                return new TypeHolder<FalseReader3<TNextReader>, FalseToken, TNextReader>(this);
            }
        }

        TypeHolder<FalseReader3<TNextReader>, TNextReader> IReader2<FalseReader3<TNextReader>, TNextReader>.AsReader
        {
            get
            {
                return new TypeHolder<FalseReader3<TNextReader>, TNextReader>(this);
            }
        }

        public FalseReader3(
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        private FalseReader3(
            int currentCharacter,
            Func<Stream, byte[], int, int, TNextReader> nextReaderFactory)
        {
            this.currentCharacter = currentCharacter;
            this.nextReaderFactory = nextReaderFactory;
        }

        public bool TryGetValue3(ref ReaderContext readerContext, out FalseToken value, out int context, out Func<int, FalseReader3<TNextReader>> currentReaderFactory)
        {
            for (; this.currentCharacter < this.literal.Length; ++this.currentCharacter)
            {
                if (!Helpers.TryReadChar(ref readerContext, this.literal[this.currentCharacter]))
                {
                    value = default!; //// TODO !
                    context = this.currentCharacter;
                    var nextReaderFactory = this.nextReaderFactory;
                    currentReaderFactory = currentCharacter => new FalseReader3<TNextReader>(
                        currentCharacter,
                        nextReaderFactory);
                    return false;
                }
            }

            value = FalseToken.Instance;
            context = default;
            currentReaderFactory = default!; //// TODO !
            return true;
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<TNextReader> nextFactory)
        {
            var stream = readerContext.Stream;
            var buffer = readerContext.Buffer;
            var currentByteIndex = readerContext.CurrentByteIndex;
            var validBytes = readerContext.ValidBytes;
            var nextReaderFactory = this.nextReaderFactory;
            nextFactory = () => nextReaderFactory(stream, buffer, currentByteIndex, validBytes);

            return true;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

    public ref struct ObjectReader2<TNextReader> : IReader2<ObjectReader2<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
        where TNextReader : allows ref struct
    {
        private readonly Func<TNextReader> nextReaderFactory;

        public ObjectReader2(Func<TNextReader> nextReaderFactory)
        {
            this.nextReaderFactory = nextReaderFactory;
        }

        public TypeHolder<ObjectReader2<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>> AsReader
        {
            get
            {
                return new TypeHolder<ObjectReader2<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>(this);
            }
        }

        public ValueTask Read(ReaderContext readerContext)
        {
            return readerContext.Read();
        }

        public bool TryMove3(ref ReaderContext readerContext, out Func<ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>> nextFactory)
        {
            var stream = readerContext.Stream;
            var buffer = readerContext.Buffer;
            var currentByteIndex = readerContext.CurrentByteIndex;
            var validBytes = readerContext.ValidBytes;
            var nextReaderFactory = this.nextReaderFactory;
            nextFactory = () => new ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>(
                stream,
                buffer,
                currentByteIndex,
                validBytes,
                (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>(
                    stream,
                    buffer,
                    currentByteIndex,
                    validBytes,
                    (stream, buffer, currentByteIndex, validBytes) => new MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>(
                        stream,
                        buffer,
                        currentByteIndex,
                        validBytes,
                        (stream, buffer, currentByteIndex, validBytes) => new WhitespaceReader<ObjectEndReader<TNextReader>>(
                            stream,
                            buffer,
                            currentByteIndex,
                            validBytes,
                            (stream, buffer, currentByteIndex, validBytes) => new ObjectEndReader<TNextReader>(
                                stream,
                                buffer,
                                currentByteIndex,
                                validBytes,
                                (stream, buffer, currentByteIndex, validBytes) => nextReaderFactory())))));
            return true;
        }
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

    public static class RefNullable
    {
        public static RefNullable<T> Value<T>(T value)
            where T : allows ref struct
        {
            return new RefNullable<T>(value);
        }

        public static RefNullable<T> Null<T>()
            where T : allows ref struct
        {
            return new RefNullable<T>();
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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
        
        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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


        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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

        public async ValueTask Read()
        {
            this.validBytes = await this.stream.ReadAsync(this.buffer.AsMemory()).ConfigureAwait(false);
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
        public static bool TryReadChar(ref ReaderContext readerContext, char character)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                return false;
            }

            ReadChar(ref readerContext, character);
            ++readerContext.CurrentByteIndex;
            return true;
        }

        private static void ReadChar(ref ReaderContext readerContext, char character)
        {
            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != character)
            {
                throw new Exception("TODO invalid JSON");
            }
        }

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
