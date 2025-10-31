namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Readers;


    /*
json = ws value ws

value = false / null / true / object / array / number / string

false = %x66.61.6C.73.65  ; "false"
null  = %x6E.75.6C.6C     ; "null"
true  = %x74.72.75.65     ; "true"

object = %x7B ws [ member *( %x2C ws member ) ] ws %x7D
member = string ws %x3A ws value

array = %x5B ws [ value *( %x2C ws value ) ] ws %x5D

number = [ "-" ] int [ frac ] [ exp ]
int    = "0" / ( digit1-9 *digit )
frac   = "." 1*digit
exp    = ( "e" / "E" ) [ "+" / "-" ] 1*digit

string = %x22 *char %x22
char   = unescaped / escape ( %x22 / %x5C / %x2F / %x62 / %x66 / %x6E / %x72 / %x74 / unicode )
escape = %x5C
unicode = %x75 4HEXDIG
unescaped = %x20-21 / %x23-5B / %x5D-10FFFF

ws = *(%x20 / %x09 / %x0A / %x0D)

digit = %x30-39
digit1-9 = %x31-39
HEXDIG = digit / %x41-46 / %x61-66  ; 0-9, A-F, a-f
    */


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
