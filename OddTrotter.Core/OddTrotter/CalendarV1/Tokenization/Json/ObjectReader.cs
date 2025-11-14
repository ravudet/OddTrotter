namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Readers;

    internal readonly ref struct RefNullable<T>
        where T : allows ref struct
    {
        private readonly T value;

        private readonly bool hasValue;

        public RefNullable(T value)
        {
            this.value = value;

            this.hasValue = true;
        }

        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            if (this.hasValue)
            {
                value = this.value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public static class Helpers
    {
        public static ValueReader<NothingReader> StartReading(Stream stream, IArrayResizer arrayResizer)
        {
            //// TODO if you give it "true1234234lkajsdflkja" it will "parse" that as a true token, and not give any indication that the rest isn't a valid json payload
            return new ValueReader<NothingReader>(
                stream, 
                arrayResizer,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new NothingReader(stream, arrayResizer, buffer, currentIndex, validBytes));
        }

        public readonly ref struct NothingReader : IReader<Nothing>
        {
            private static readonly byte[] bytes = [(byte)'t', (byte)'r', (byte)'e', (byte)'e'];

            private readonly Stream stream;
            private readonly IArrayResizer arrayResizer;
            private readonly byte[] buffer;
            private readonly int currentIndex;
            private readonly int validBytes;

            public NothingReader(
                Stream stream,
                IArrayResizer arrayResizer,
                byte[] buffer,
                int currentIndex,
                int validBytes)
            {
                this.stream = stream;
                this.arrayResizer = arrayResizer;
                this.buffer = buffer;
                this.currentIndex = currentIndex;
                this.validBytes = validBytes;
            }

            public RefTask<NothingReader> Read2()
            {
                return new RefTask<NothingReader>(
                    this.currentIndex < this.validBytes,
                    this.stream,
                    this.arrayResizer,
                    this.buffer,
                    this.currentIndex,
                    this.validBytes,
                    (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                        new NothingReader(stream, arrayResizer, buffer, currentIndex, validBytes));
            }

            public ValueTask Read()
            {
                throw new NotImplementedException();
            }

            public Nothing TryMoveNext(out bool moved)
            {
                if (this.validBytes == 0)
                {
                    moved = true;
                    return default;
                }

                if (this.currentIndex >= this.validBytes)
                {
                    moved = false;
                    return default;
                }

                throw new Exception("TODO invalid JSON; there was more data to be read when no tokens were expected");
            }
        }

        public static bool TryMoveNext<TCurrentReader, TNextReader>(this TCurrentReader reader, [MaybeNullWhen(false)] out TNextReader next)
            where TCurrentReader : IReader<TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            next = reader.TryMoveNext(out var moved);
            return moved;
        }









        public static async Task Caller2<TNextReader>()
            where TNextReader : allows ref struct
        {
            var reader = new ValueReader<TNextReader>();

            ValueReaderToken<TNextReader> valueReaderToken;
            while (!reader.TryMoveNext(out valueReaderToken))
            {
                reader = await reader.Read2();
            }
        }

    }

















    public interface IArrayResizer
    {
        byte[] Resize(byte[] previous); //// TODO you should really do the disposable thing to make sure they get put back in the pool
    }

    public readonly ref struct RefTask<TReader>
        where TReader : allows ref struct
    {
        private readonly bool completed;
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TReader> readerFactory;

        public RefTask(
            bool completed,
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TReader> readerFactory)
        {
            this.completed = completed;
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter(
                this.completed,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                this.readerFactory);
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly bool completed;
            private readonly ValueTaskAwaiter completedAwaiter;

            private readonly Stream stream;
            private readonly IArrayResizer arrayResizer;
            private readonly byte[] buffer;
            private readonly int currentIndex;
            private readonly int validBytes;
            private readonly Func<Stream, IArrayResizer, byte[], int, int, TReader> readerFactory;
            private readonly int copiedBuffer;

            private readonly ConfiguredTaskAwaitable<int>.ConfiguredTaskAwaiter awaiter;

            public Awaiter(
                bool completed,
                Stream stream,
                IArrayResizer arrayResizer,
                byte[] buffer,
                int currentIndex,
                int validBytes,
                Func<Stream, IArrayResizer, byte[], int, int, TReader> readerFactory)
            {
                this.completed = completed;

                this.stream = stream;
                this.arrayResizer = arrayResizer;
                this.buffer = buffer;
                this.currentIndex = currentIndex;
                this.validBytes = validBytes;
                this.readerFactory = readerFactory;

                if (completed)
                {
                    this.completedAwaiter = ValueTask.CompletedTask.GetAwaiter();
                }
                else
                {
                    if (this.currentIndex == 0)
                    {
                        // we've tried just reading more into the buffer (or we are on the 0-length initial buffer), we now need to resize the buffer
                        this.buffer = this.arrayResizer.Resize(this.buffer);
                    }

                    // copy the remaining bytes to the beginning of the buffer
                    this.copiedBuffer = this.validBytes - this.currentIndex;
                    Array.Copy(this.buffer, this.currentIndex, this.buffer, 0, this.copiedBuffer);

                    // read more data into the now-freed buffer space
                    this.awaiter = this.stream.ReadAsync(this.buffer, this.copiedBuffer, this.buffer.Length - this.copiedBuffer).ConfigureAwait(false).GetAwaiter();
                }
            }

            public bool IsCompleted
            {
                get
                {
                    if (this.completed)
                    {
                        return completedAwaiter.IsCompleted;
                    }
                    else
                    {
                        return this.awaiter.IsCompleted;
                    }
                }
            }

            public void OnCompleted(Action continuation)
            {
                if (this.completed)
                {
                    this.completedAwaiter.OnCompleted(continuation);
                }
                else
                {
                    this.awaiter.OnCompleted(continuation);
                }
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                if (this.completed)
                {
                    this.completedAwaiter.UnsafeOnCompleted(continuation);
                }
                else
                {
                    this.awaiter.UnsafeOnCompleted(continuation);
                }
            }

            public TReader GetResult()
            {
                return this.readerFactory(
                    this.stream,
                    this.arrayResizer,
                    this.buffer,
                    0,
                    this.copiedBuffer + this.awaiter.GetResult());
            }
        }
    }

    public readonly ref struct ValueReader<TNextReader> : IReader<ValueReaderToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;

        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public ValueReader(
            Stream stream, 
            IArrayResizer arrayResizer,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
            : this(stream, arrayResizer, Array.Empty<byte>(), 0, 0, readerFactory)
        {
        }

        public ValueReader(
            Stream stream, 
            IArrayResizer arrayResizer, 
            byte[] buffer, 
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.readerFactory = readerFactory;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
        }

        public RefTask<ValueReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<ValueReader<TNextReader>>(
                this.currentIndex < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new ValueReader<TNextReader>(
                        stream, 
                        arrayResizer, 
                        buffer, 
                        currentIndex, 
                        validBytes, 
                        readerFactory //// TODO do you really want to create a closure here? maybe ref task could take a "reader context" or something //// TODO note that you follow this pattern in other readers too
                        ));
        }

        public ValueTask Read()
        {
            //// TODO update `ireader` interfaces to allow using the `reftask` stuff?
            throw new NotImplementedException();
        }

        public ValueReaderToken<TNextReader> TryMoveNext(out bool moved)
        {
            if (this.currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }


            //// TODO actually leverage utf8
            //// TODO you aren't allowing comments...

            switch ((char)this.buffer[this.currentIndex])
            {
                case '{':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.Object,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                case '[':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.Array,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
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
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.Number,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                case '"':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.String,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                case 'f':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.False,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                case 'n':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.Null,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                case 't':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(
                        ValueReaderToken<TNextReader>.TokenType.True,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
            }

            throw new Exception("TODO invalid JSON");
        }
    }

    public ref struct ValueReaderToken<TNextReader>
        where TNextReader : allows ref struct
    {
        public enum TokenType
        {
            False,
            Null,
            True,
            Object,
            Array,
            Number,
            String,
        }

        private readonly TokenType tokenType;

        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;

        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public ValueReaderToken(
            TokenType tokenType,
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.tokenType = tokenType;
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer; 
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
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
            switch (this.tokenType)
            {
                case TokenType.False:
                    var falseReader = new FalseReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer, 
                        this.currentIndex, 
                        this.validBytes,
                        this.readerFactory);
                    return @false(falseReader);
                case TokenType.Null:
                    var nullReader = new NullReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                    return @null(nullReader);
                case TokenType.True:
                    var trueReader = new TrueReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                    return @true(trueReader);
                case TokenType.Object:
                    var objectReader = new ObjectReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory,
                        true);
                    return @object(objectReader);
                case TokenType.Array:
                    var arrayReader = new ArrayReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory,
                        true);
                    return array(arrayReader);
                case TokenType.Number:
                    var numberReader = new NumberReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                    return number(numberReader);
                case TokenType.String:
                    var stringReader = new StringReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        this.readerFactory);
                    return @string(stringReader);
            }

            throw new Exception("TODO bug");
        }
    }

    public readonly ref struct FalseReader<TNextReader> : IReader<TNextReader, FalseToken>
        where TNextReader : allows ref struct
    {
        private static readonly byte[] bytes = [(byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e'];
        
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public FalseReader(
            Stream stream,
            IArrayResizer arrayResizer, 
            byte[] buffer, 
            int currentIndex, 
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<FalseReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<FalseReader<TNextReader>>(
                this.currentIndex + bytes.Length - 1 < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new FalseReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public FalseToken TryGetValue(out bool moved)
        {
            if (this.currentIndex + bytes.Length - 1 >= this.validBytes)
            {
                moved = false;
                return default;
            }

            if (this.buffer.AsSpan(this.currentIndex, bytes.Length) == bytes.AsSpan())
            {
                moved = true;
                return new FalseToken();
            }

            throw new Exception("TODO invalid JSON");
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            this.TryGetValue(out moved);
            if (!moved)
            {
                return default!;
            }

            moved = true;
            return this.readerFactory(
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex + bytes.Length,
                this.validBytes);
        }
    }

    public readonly ref struct FalseToken
    {
    }

    public readonly ref struct NullReader<TNextReader> : IReader<TNextReader, NullToken>
        where TNextReader : allows ref struct
    {
        private static readonly byte[] bytes = [(byte)'n', (byte)'u', (byte)'l', (byte)'l'];

        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public NullReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<NullReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<NullReader<TNextReader>>(
                this.currentIndex + bytes.Length - 1 < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new NullReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public NullToken TryGetValue(out bool moved)
        {
            if (this.currentIndex + bytes.Length - 1 >= this.validBytes)
            {
                moved = false;
                return default;
            }

            if (this.buffer.AsSpan(this.currentIndex, bytes.Length) == bytes.AsSpan())
            {
                moved = true;
                return new NullToken();
            }

            throw new Exception("TODO invalid JSON");
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            this.TryGetValue(out moved);
            if (!moved)
            {
                return default!;
            }

            moved = true;
            return this.readerFactory(
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex + bytes.Length,
                this.validBytes);
        }
    }

    public readonly ref struct NullToken
    {
    }

    public readonly ref struct TrueReader<TNextReader> : IReader<TNextReader, TrueToken>
        where TNextReader : allows ref struct
    {
        private static readonly byte[] bytes = [(byte)'t', (byte)'r', (byte)'e', (byte)'e'];

        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public TrueReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<TrueReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<TrueReader<TNextReader>>(
                this.currentIndex + bytes.Length - 1 < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new TrueReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public TrueToken TryGetValue(out bool moved)
        {
            if (this.currentIndex + bytes.Length - 1 >= this.validBytes)
            {
                moved = false;
                return default;
            }

            if (this.buffer.AsSpan(this.currentIndex, bytes.Length) == bytes.AsSpan())
            {
                moved = true;
                return new TrueToken();
            }

            throw new Exception("TODO invalid JSON");
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            this.TryGetValue(out moved);
            if (!moved)
            {
                return default!;
            }

            moved = true;
            return this.readerFactory(
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex + bytes.Length,
                this.validBytes);
        }
    }

    public readonly ref struct TrueToken
    {
    }

    public readonly ref struct ObjectReader<TNextReader> : IReader<ObjectToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        private readonly bool isFirstMember;

        public ObjectReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory,
            bool isFirstMember)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
            this.isFirstMember = isFirstMember;
        }

        public RefTask<ObjectReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            var isFirstMember = this.isFirstMember;
            return new RefTask<ObjectReader<TNextReader>>(
                this.currentIndex < this.validBytes, //// TODO check all of the `iscompleted` arguments to make sure they make sense
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new ObjectReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory, isFirstMember));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ObjectToken<TNextReader> TryMoveNext(out bool moved)
        {
            if (this.currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }

            var currentIndex = this.currentIndex;
            if (isFirstMember)
            {
                if (this.buffer[currentIndex] != '{')
                {
                    throw new Exception("TODO invalid JSON");
                }

                ++currentIndex;
                if (currentIndex >= this.validBytes)
                {
                    moved = false;
                    return default;
                }

                while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                {
                    ++currentIndex;
                    if (currentIndex >= this.validBytes)
                    {
                        moved = false;
                        return default;
                    }
                }

                if (this.buffer[currentIndex] == '}')
                {
                    moved = true;
                    return new ObjectToken<TNextReader>(
                        ObjectToken<TNextReader>.TokenType.Next,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex + 1,
                        this.validBytes,
                        this.readerFactory);
                }
                else
                {
                    moved = true;
                    return new ObjectToken<TNextReader>(
                        ObjectToken<TNextReader>.TokenType.Member,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex,
                        this.validBytes,
                        this.readerFactory);
                }
            }
            else
            {
                if (this.buffer[currentIndex] == ',')
                {
                    ++currentIndex;
                    if (currentIndex >= this.validBytes)
                    {
                        moved = false;
                        return default;
                    }

                    while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                    {
                        ++currentIndex;
                        if (currentIndex >= this.validBytes)
                        {
                            moved = false;
                            return default;
                        }
                    }

                    moved = true;
                    return new ObjectToken<TNextReader>(
                        ObjectToken<TNextReader>.TokenType.Member,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex,
                        this.validBytes,
                        this.readerFactory);
                }
                else
                {
                    while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                    {
                        ++currentIndex;
                        if (currentIndex >= this.validBytes)
                        {
                            moved = false;
                            return default;
                        }
                    }

                    if (this.buffer[currentIndex] != '}')
                    {
                        throw new Exception("tODO invalid JSON");
                    }

                    moved = true;
                    return new ObjectToken<TNextReader>(
                        ObjectToken<TNextReader>.TokenType.Next,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex + 1,
                        this.validBytes,
                        this.readerFactory);
                }
            }
        }
    }

    public readonly ref struct ObjectToken<TNextReader>
        where TNextReader : allows ref struct
    {
        public enum TokenType
        {
            Member,
            Next,
        }

        private readonly TokenType tokenType;

        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;

        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public ObjectToken(
            TokenType tokenType,
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.tokenType = tokenType;
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public TResult Apply<TResult>(
            Func<MemberReader<ObjectReader<TNextReader>>, TResult> member,
            Func<TNextReader, TResult> endObject)
            where TResult : allows ref struct
        {
            var localReaderFactory = this.readerFactory;
            switch (tokenType)
            {
                case TokenType.Member:
                    var memberReader = new MemberReader<ObjectReader<TNextReader>>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                            new ObjectReader<TNextReader>(
                                stream,
                                arrayResizer,
                                buffer,
                                currentIndex,
                                validBytes,
                                localReaderFactory,
                                false));
                    return member(memberReader);
                case TokenType.Next:
                    var nextReader = this.readerFactory(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes);
                    return endObject(nextReader);
            }

            throw new Exception("tODO bug");
        }
    }

    public readonly ref struct MemberReader<TNextReader> : IReader<MemberNameReader<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public MemberReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<MemberReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<MemberReader<TNextReader>>(
                this.currentIndex < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new MemberReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public MemberNameReader<TNextReader> TryMoveNext(out bool moved)
        {
            if (this.currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }

            moved = true;
            return new MemberNameReader<TNextReader>(
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                this.readerFactory);
        }
    }

    public ref struct MemberNameReader<TNextReader> : IReader<ValueReader<ObjectReader<TNextReader>>, MemberNameToken>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        private int? finalIndex;

        public MemberNameReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<MemberNameReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            return new RefTask<MemberNameReader<TNextReader>>(
                this.currentIndex < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new MemberNameReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        //// TODO make sure all of the readers and tokens are `readonly` where appropriate

        public MemberNameToken TryGetValue(out bool moved)
        {
            var stringReader = new StringReader<Nothing>(
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (_, _, _, _, _) => new Nothing());
            var stringToken = stringReader.TryGetValue(out moved);
            if (!moved)
            {
                return default;
            }

            this.finalIndex = this.currentIndex + stringToken.Value.Length;
            return new MemberNameToken(stringToken.Value);
        }

        public ValueReader<ObjectReader<TNextReader>> TryMoveNext(out bool moved)
        {
            if (this.finalIndex == null)
            {
                this.TryGetValue(out moved);
                if (!moved)
                {
                    return default;
                }
            }

            //// TODO convince the compiler finalidnex isn't null here
            var currentIndex = this.finalIndex!.Value;
            if (currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }

            while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
            {
                ++currentIndex;
                if (currentIndex >= this.validBytes)
                {
                    moved = false;
                    return default;
                }
            }

            if (this.buffer[currentIndex] != '=')
            {
                throw new Exception("TODO invalid JSON");
            }

            ++currentIndex;
            if (currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }

            while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
            {
                ++currentIndex;
                if (currentIndex >= this.validBytes)
                {
                    moved = false;
                    return default;
                }
            }

            moved = true;
            var readerFactory = this.readerFactory;
            return new ValueReader<ObjectReader<TNextReader>>(
                this.stream,
                this.arrayResizer,
                this.buffer,
                currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new ObjectReader<TNextReader>(
                        stream,
                        arrayResizer,
                        buffer,
                        currentIndex,
                        validBytes,
                        readerFactory,
                        false));
        }
    }

    public readonly ref struct MemberNameToken
    {
        public MemberNameToken(Span<byte> value)
        {
            this.Value = value;
        }

        public Span<byte> Value { get; }
    }

    public readonly ref struct ArrayReader<TNextReader> : IReader<ArrayToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        private readonly bool isFirstElement;

        public ArrayReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory,
            bool isFirstElement)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
            this.isFirstElement = isFirstElement;
        }

        public RefTask<ArrayReader<TNextReader>> Read2()
        {
            var readerFactory = this.readerFactory;
            var isFirstElement = this.isFirstElement;
            return new RefTask<ArrayReader<TNextReader>>(
                this.currentIndex < this.validBytes,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new ArrayReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory, isFirstElement));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ArrayToken<TNextReader> TryMoveNext(out bool moved)
        {
            if (this.currentIndex >= this.validBytes)
            {
                moved = false;
                return default;
            }

            if (this.isFirstElement)
            {
                var currentIndex = this.currentIndex;
                if (this.buffer[currentIndex] != '[')
                {
                    throw new Exception("TODO invalid JSON");
                }

                //// TODO is this a do-while? if it is, are there other places like it?
                ++currentIndex;
                if (currentIndex >= this.buffer.Length)
                {
                    moved = false;
                    return default;
                }

                while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }
                }

                if (this.buffer[currentIndex] == ']')
                {
                    moved = true;
                    return new ArrayToken<TNextReader>(
                        ArrayToken<TNextReader>.TokenType.Next,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex + 1,
                        this.validBytes,
                        this.readerFactory);
                }
                else
                {
                    moved = true;
                    return new ArrayToken<TNextReader>(
                        ArrayToken<TNextReader>.TokenType.Value,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex,
                        this.validBytes,
                        this.readerFactory);
                }
            }
            else
            {
                var currentIndex = this.currentIndex;
                if (this.buffer[currentIndex] == ',')
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }

                    while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                    {
                        ++currentIndex;
                        if (currentIndex >= this.buffer.Length)
                        {
                            moved = false;
                            return default;
                        }
                    }

                    moved = true;
                    return new ArrayToken<TNextReader>(
                        ArrayToken<TNextReader>.TokenType.Value,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex,
                        this.validBytes,
                        this.readerFactory);
                }
                else
                {
                    while (char.IsWhiteSpace((char)this.buffer[currentIndex]))
                    {
                        ++currentIndex;
                        if (currentIndex >= this.buffer.Length)
                        {
                            moved = false;
                            return default;
                        }
                    }

                    if (this.buffer[currentIndex] != ']')
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    moved = true;
                    return new ArrayToken<TNextReader>(
                        ArrayToken<TNextReader>.TokenType.Next,
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        currentIndex + 1,
                        this.validBytes,
                        this.readerFactory);
                }
            }
        }
    }

    public readonly ref struct ArrayToken<TNextReader>
        where TNextReader : allows ref struct
    {
        public enum TokenType
        {
            Value,
            Next,
        }

        private readonly TokenType tokenType;

        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;

        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        public ArrayToken(
            TokenType tokenType,
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.tokenType = tokenType;
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public TResult Apply<TResult>(
            Func<ValueReader<ArrayReader<TNextReader>>, TResult> value,
            Func<TNextReader, TResult> endArray)
            where TResult : allows ref struct
        {
            var localReaderFactory = this.readerFactory;
            switch (this.tokenType)
            {
                case TokenType.Value:
                    var valueReader = new ValueReader<ArrayReader<TNextReader>>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes,
                        (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                            new ArrayReader<TNextReader>(
                                stream,
                                arrayResizer,
                                buffer,
                                currentIndex,
                                validBytes,
                                localReaderFactory,
                                false));
                    return value(valueReader);
                case TokenType.Next:
                    var nextReader = this.readerFactory(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        this.currentIndex,
                        this.validBytes);
                    return endArray(nextReader);
            }

            throw new Exception("TODO bug");
        }
    }

    public ref struct NumberReader<TNextReader> : IReader<TNextReader, NumberToken>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        private int? finalIndex;

        public NumberReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<NumberReader<TNextReader>> Read2()
        {
            this.TryGetValue(out var moved); //// TODO how much effort do you want to put into avoiding reading from the stream? because it's "possible" i guess that reading the current "number" value is very expensive, and we would be reading it so that we can know to return a "completed" reftask here; is it always better to read it, or sometimes does it make more sense to read from the stream and potentially resize the buffer?

            var readerFactory = this.readerFactory;
            return new RefTask<NumberReader<TNextReader>>(
                moved,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new NumberReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public NumberToken TryGetValue(out bool moved)
        {
            var currentIndex = this.currentIndex;
            
            var negative = false;
            if (this.buffer[currentIndex] == '-')
            {
                ++currentIndex;
                negative = true;
            }

            if (currentIndex >= this.buffer.Length)
            {
                moved = false;
                return default;
            }

            IntToken intToken;
            if (this.buffer[currentIndex] == '0')
            {
                ++currentIndex;
                if (currentIndex >= this.buffer.Length)
                {
                    moved = false;
                    return default;
                }

                intToken = new IntToken(this.buffer.AsSpan(currentIndex, 1));
            }
            else
            {
                var startIndex = currentIndex;
                if (!char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    throw new Exception("TODO invalid JSON");
                }

                while (char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }
                }

                intToken = new IntToken(this.buffer.AsSpan(startIndex, currentIndex - startIndex + 1));
            }

            FractionToken fractionToken;
            if (this.buffer[currentIndex] == '.')
            {
                ++currentIndex;
                var startIndex = currentIndex;
                if (!char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    throw new Exception("TODO invalid JSON");
                }

                while (char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }
                }

                fractionToken = new FractionToken(this.buffer.AsSpan(startIndex, currentIndex - startIndex + 1));
            }
            else
            {
                fractionToken = new FractionToken(Span<byte>.Empty);
            }

            ExponentToken exponentToken;
            if (this.buffer[currentIndex] == 'e' || this.buffer[currentIndex] == 'E')
            {
                ++currentIndex;
                if (currentIndex >= this.buffer.Length)
                {
                    moved = false;
                    return default;
                }

                var sign = ExponentToken.SignValue.None;
                if (this.buffer[currentIndex] == '+')
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }

                    sign = ExponentToken.SignValue.Positive;
                }
                else if (this.buffer[currentIndex] == '-')
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }

                    sign = ExponentToken.SignValue.Negative;
                }

                var startIndex = currentIndex;
                if (!char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    throw new Exception("TODO invalid JSON");
                }

                while (char.IsAsciiDigit((char)this.buffer[currentIndex]))
                {
                    ++currentIndex;
                    if (currentIndex >= this.buffer.Length)
                    {
                        moved = false;
                        return default;
                    }
                }

                exponentToken = new ExponentToken(sign, this.buffer.AsSpan(startIndex, currentIndex - startIndex + 1));
            }
            else
            {
                exponentToken = new ExponentToken(ExponentToken.SignValue.None, Span<byte>.Empty);
            }

            this.finalIndex = currentIndex;

            moved = true;
            return new NumberToken(negative, intToken, fractionToken, exponentToken);
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            if (this.finalIndex == null)
            {
                this.TryGetValue(out moved);
                if (!moved)
                {
                    return default!;
                }
            }

            //// TODO make the compiler undrstand final index won't be null
            //// TODO your bounds checking needs to be on `validBytes` and not `buffer.length`
            moved = true;
            return this.readerFactory(this.stream, this.arrayResizer, this.buffer, this.finalIndex!.Value, this.validBytes);
        }
    }

    public readonly ref struct NumberToken //// TODO each part of this should probably be its own reader...
    {
        public NumberToken(bool negative, IntToken intToken, FractionToken fractionToken, ExponentToken exponentToken)
        {
            Negative = negative;
            IntToken = intToken;
            FractionToken = fractionToken;
            ExponentToken = exponentToken;
        }

        public bool Negative { get; }
        public IntToken IntToken { get; }
        public FractionToken FractionToken { get; }
        public ExponentToken ExponentToken { get; }
    }

    public readonly ref struct IntToken
    {
        public IntToken(Span<byte> digits)
        {
            Digits = digits;
        }

        public Span<byte> Digits { get; }
    }

    public readonly ref struct FractionToken
    {
        public FractionToken(Span<byte> digits)
        {
            Digits = digits;
        }

        public Span<byte> Digits { get; }
    }

    public readonly ref struct ExponentToken
    {
        public enum SignValue
        {
            None,
            Positive,
            Negative,
        }

        public ExponentToken(SignValue sign, Span<byte> digits)
        {
            Sign = sign;
            Digits = digits;
        }

        public SignValue Sign { get; }
        public Span<byte> Digits { get; }
    }

    public ref struct StringReader<TNextReader> : IReader<TNextReader, StringToken>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;
        private readonly Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory;

        private int? finalIndex;

        public StringReader(
            Stream stream,
            IArrayResizer arrayResizer,
            byte[] buffer,
            int currentIndex,
            int validBytes,
            Func<Stream, IArrayResizer, byte[], int, int, TNextReader> readerFactory)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.validBytes = validBytes;
            this.readerFactory = readerFactory;
        }

        public RefTask<StringReader<TNextReader>> Read2()
        {
            this.TryGetValue(out var moved); //// TODO how much effort do you want to put into avoiding reading from the stream? because it's "possible" i guess that reading the current "number" value is very expensive, and we would be reading it so that we can know to return a "completed" reftask here; is it always better to read it, or sometimes does it make more sense to read from the stream and potentially resize the buffer?

            var readerFactory = this.readerFactory;
            return new RefTask<StringReader<TNextReader>>(
                moved,
                this.stream,
                this.arrayResizer,
                this.buffer,
                this.currentIndex,
                this.validBytes,
                (stream, arrayResizer, buffer, currentIndex, validBytes) =>
                    new StringReader<TNextReader>(stream, arrayResizer, buffer, currentIndex, validBytes, readerFactory));
        }

        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public StringToken TryGetValue(out bool moved)
        {
            var currentIndex = this.currentIndex;
            if (this.buffer[currentIndex] != '"')
            {
                throw new Exception("tODO invalid JSON");
            }

            ++currentIndex;
            if (currentIndex >= this.buffer.Length)
            {
                moved = false;
                return default;
            }

            var startIndex = currentIndex;
            while (this.buffer[currentIndex] != '"')
            {
                ++currentIndex;
                if (currentIndex >= this.buffer.Length)
                {
                    moved = false;
                    return default;
                }
            }

            this.finalIndex = currentIndex + 1;

            moved = true;
            return new StringToken(this.buffer.AsSpan(startIndex, currentIndex - startIndex + 1));
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            if (this.finalIndex == null)
            {
                this.TryGetValue(out moved);
                if (!moved)
                {
                    return default!;
                }
            }

            moved = true;
            //// TODO convince the compiler that finalindex is not null here
            return this.readerFactory(this.stream, this.arrayResizer, this.buffer, this.finalIndex!.Value, this.validBytes);
        }
    }

    public readonly ref struct StringToken
    {
        public StringToken(Span<byte> value)
        {
            Value = value;
        }

        public Span<byte> Value { get; }
    }








}
