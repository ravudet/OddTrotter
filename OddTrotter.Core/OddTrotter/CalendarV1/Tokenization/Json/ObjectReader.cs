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

    public static class Helpers
    {
        public static ValueReader<Nothing> StartReading()
        {
            throw new Exception("TODO");
        }
    }

    public ref struct ValueReader<TNextReader> : IReader<ValueReaderToken<TNextReader>>
        where TNextReader : allows ref struct
    {
        public ValueTask Read()
        {
            throw new NotImplementedException();
        }

        public ValueReaderToken<TNextReader> TryMoveNext(out bool moved)
        {
            throw new NotImplementedException();
        }
    }

    public ref struct ValueReaderToken<TNextReader>
        where TNextReader : allows ref struct
    {
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
    }








}
