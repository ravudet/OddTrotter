namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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

    public ref struct Reader : IReader<ReaderToken>
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ReaderToken TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public ref struct ReaderToken
    {
        public TResult Apply<TResult>(
            Func<FalseReader<Nothing>, TResult> @false,
            Func<NullReader<Nothing>, TResult> @null,
            Func<TrueReader<Nothing>, TResult> @true,
            Func<ObjectReader<Nothing>, TResult> @object,
            Func<ArrayReader<Nothing>, TResult> array,
            Func<NumberReader<Nothing>, TResult> number,
            Func<StringReader<Nothing>, TResult> @string)
            where TResult : allows ref struct
        {
            throw new NotImplementedException();
        }
    }

    public ref struct FalseReader<TNextReader> : IReader<TNextReader, FalseToken>
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

    public ref struct ObjectReader<TNextReader> : IReader<TNextReader, ObjectToken>
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ObjectToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct ObjectToken
    {
    }

    public ref struct ArrayReader<TNextReader> : IReader<TNextReader, ArrayToken>
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ArrayToken TryGetValue(out bool moved)
        {
            throw new NotImplementedException();
        }

        public TNextReader TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public readonly ref struct ArrayToken
    {
    }

    public ref struct NumberReader<TNextReader> : IReader<TNextReader, NumberToken>
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
    }

    public ref struct StringReader<TNextReader> : IReader<TNextReader, StringToken>
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
    }








}
