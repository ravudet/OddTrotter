namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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
        public static ValueReader<Nothing> StartReading()
        {
            throw new Exception("TODO");
        }

        public interface ITask<out T>
            where T : allows ref struct
        {
            /// <inheritdoc cref="Task{TResult}.GetAwaiter"/>
            ITaskAwaiter<T> GetAwaiter();
        }

        public interface ITaskAwaiter<out T> : ICriticalNotifyCompletion
            where T : allows ref struct
        {
            /// <inheritdoc cref="TaskAwaiter{TResult}.IsCompleted"/>
            bool IsCompleted { get; }

            /// <inheritdoc cref="TaskAwaiter{TResult}.GetResult"/>
            T GetResult();
        }

        public static async Task Caller<TNextReader>()
            where TNextReader : allows ref struct
        {
            var reader = new ValueReader<TNextReader>();

            ValueReaderToken<TNextReader> valueReaderToken;
            while (!reader.TryMoveNext(out valueReaderToken))
            {
                reader = await reader.ReadExt();
            }
        }

        public static bool TryMoveNext<TCurrentReader, TNextReader>(this TCurrentReader reader, [MaybeNullWhen(false)] out TNextReader next)
            where TCurrentReader : IReader<TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            next = reader.TryMoveNext(out var moved);
            return moved;
        }

        public static ITask<ValueReader<TNextReader>> ReadExt<TNextReader>(this ValueReader<TNextReader> reader)
            where TNextReader : allows ref struct
        {
            ////
            //// TODO implement everything without async
            //// TODO then implement methods like this, making it async using a new `reftask` type that is able to async return the new reader; the input `reader` will need to have the necessary properties to actually implement the stream read and the creation of the new reader
            //// TODO i think you'll need a `stream` property and a `buffer` property on the readers
            throw new Exception("TODO");
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

    public readonly ref struct ValueReader<TNextReader> : IReader<ValueReaderToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        private readonly Stream stream;
        private readonly IArrayResizer arrayResizer;

        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly int validBytes;

        public ValueReader(Stream stream, IArrayResizer arrayResizer)
            : this(stream, arrayResizer, Array.Empty<byte>(), 0, 0)
        {
        }

        private ValueReader(Stream stream, IArrayResizer arrayResizer, byte[] buffer, int currentIndex, int validBytes)
        {
            this.stream = stream;
            this.arrayResizer = arrayResizer;
            this.buffer = buffer;
            this.currentIndex = 0;
            this.validBytes = 0;
        }

        public RefTask Read2()
        {
            return new RefTask(this.stream, this.arrayResizer, this.buffer, this.currentIndex, this.validBytes);
        }

        public readonly ref struct RefTask
        {
            private readonly Stream stream;
            private readonly IArrayResizer arrayResizer;
            private readonly byte[] buffer;
            private readonly int currentIndex;
            private readonly int validBytes;

            public RefTask(Stream stream, IArrayResizer arrayResizer, byte[] buffer, int currentIndex, int validBytes)
            {
                this.stream = stream;
                this.arrayResizer = arrayResizer;
                this.buffer = buffer;
                this.currentIndex = currentIndex;
                this.validBytes = validBytes;
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this.stream, this.arrayResizer, this.buffer, this.currentIndex, this.validBytes);
            }

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly Stream stream;
                private readonly IArrayResizer arrayResizer;
                private readonly byte[] buffer;
                private readonly int currentIndex;
                private readonly int validBytes;
                private readonly int copiedBuffer;


                private readonly ConfiguredTaskAwaitable<int>.ConfiguredTaskAwaiter awaiter;

                public Awaiter(Stream stream, IArrayResizer arrayResizer, byte[] buffer, int currentIndex, int validBytes)
                {
                    this.stream = stream;
                    this.arrayResizer = arrayResizer;
                    this.buffer = buffer;
                    this.currentIndex = currentIndex;
                    this.validBytes = validBytes;

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

                public bool IsCompleted
                {
                    get
                    {
                        return this.awaiter.IsCompleted;
                    }
                }

                public void OnCompleted(Action continuation)
                {
                    this.awaiter.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.awaiter.UnsafeOnCompleted(continuation);
                }

                public ValueReader<TNextReader> GetResult()
                {
                    return new ValueReader<TNextReader>(
                        this.stream,
                        this.arrayResizer,
                        this.buffer,
                        0,
                        this.copiedBuffer + this.awaiter.GetResult());
                }
            }
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
                    return new ValueReaderToken<TNextReader>(ValueReaderToken<TNextReader>.TokenType.Object);
                case '[':
                    moved = true;
                    return new ValueReaderToken<TNextReader>(ValueReaderToken<TNextReader>.TokenType.Array);
            }

            if (this.currentIndex + 1 >= this.validBytes)
            {
                moved = false;
                return default;
            }

            Span<byte> tokenText;
            switch ((char)this.buffer[this.currentIndex])
            {
                case 'f':
                    tokenText = new Span<byte>([(byte)'a', (byte)'l', (byte)'s', (byte)'e']);
                    if (tokenText == this.buffer.AsSpan(this.currentIndex + 1, ))
                    break;
            }

            moved = true;
            return default;
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

        public ValueReaderToken(TokenType tokenType)
        {
            this.tokenType = tokenType;
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
            throw new NotImplementedException();
        }
    }

    public ref struct FalseReader<TNextReader> : IReader<TNextReader, FalseToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public FalseToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct FalseToken
    {
    }

    public ref struct NullReader<TNextReader> : IReader<TNextReader, NullToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public NullToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct NullToken
    {
    }

    public ref struct TrueReader<TNextReader> : IReader<TNextReader, TrueToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public TrueToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct TrueToken
    {
    }

    public ref struct ObjectReader<TNextReader> : IReader<TNextReader, ObjectToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ObjectToken<TNextReader> TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct ObjectToken<TNextReader>
        where TNextReader : allows ref struct
    {
        public TResult Apply<TResult>(
            Func<MemberReader<ObjectReader<TNextReader>>, TResult> member,
            Func<TNextReader, TResult> endObject)
            where TResult : allows ref struct
        {
            throw new NotImplementedException();
        }
    }

    public ref struct MemberReader<TNextReader> : IReader<MemberNameReader<TNextReader>>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public MemberNameReader<TNextReader> TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public ref struct MemberNameReader<TNextReader> : IReader<ValueReader<TNextReader>, MemberNameToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public MemberNameToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public ValueReader<TNextReader> TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct MemberNameToken
    {
        public string Value { get; }
    }

    public ref struct ArrayReader<TNextReader> : IReader<TNextReader, ArrayToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ArrayToken<TNextReader> TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct ArrayToken<TNextReader>
        where TNextReader : allows ref struct
    {
        public TResult Apply<TResult>(
            Func<ValueReader<ArrayReader<TNextReader>>, TResult> value,
            Func<TNextReader, TResult> endArray)
            where TResult : allows ref struct
        {
            throw new NotImplementedException();
        }
    }

    public ref struct NumberReader<TNextReader> : IReader<TNextReader, NumberToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public NumberToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct NumberToken
    {
        public NumberToken(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }
    }

    public ref struct StringReader<TNextReader> : IReader<TNextReader, StringToken>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public StringToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct StringToken
    {
        public StringToken(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }








}
