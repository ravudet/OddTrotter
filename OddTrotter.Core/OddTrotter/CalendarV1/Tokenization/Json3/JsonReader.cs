namespace OddTrotter.CalendarV1.Tokenization.Json3
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net.Mime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Sources;

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

    public readonly ref struct TypeHolder<TSelf, T1, T2, T3>
        where TSelf : allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct
    {
        public TypeHolder(TSelf self)
        {
            Self = self;
        }

        public TSelf Self { get; }
    }

    public sealed class ReaderContext
    {
        private ReaderContext(
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
        public int CurrentByteIndex { get; set; }
        public int ValidBytes { get; set; }

        public static async ValueTask<ReaderContext> FromStream(Stream stream, byte[] buffer)
        {
            var readerContext = new ReaderContext(stream, buffer, 0, 0);
            await readerContext.Read().ConfigureAwait(false);
            return readerContext;
        }
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

    public interface IMoveReader<TSelf, TNextReader, TContext>
        where TSelf : IMoveReader<TSelf, TNextReader, TContext>, allows ref struct
        where TNextReader : new(), allows ref struct
        // TContext can't be a ref struct, because it's what we use when we need a task to read more from `readercontext`
    {
        TypeHolder<TSelf, TNextReader, TContext> AsMoveReader { get; }

        bool TryMove(
            ref ReaderContext readerContext,
            // [NotNullWhen(true)][MaybeNullWhen(false)] out TNextReader nextReader, //// TODO we don't need this because we have `tnextreader : new()`
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context); //// TODO is it faster to have a context-free move reader interface? currently, you return a `nothing` that is stored in a task sometimes and then passed to the `create(tcontext)` method, that's not "the best" probably

        static abstract TSelf Create(TContext context);
    }

    public interface IValueReader<TSelf, TNextReader, TContext, TValue> : IMoveReader<TSelf, TNextReader, TContext>
        where TSelf : IMoveReader<TSelf, TNextReader, TContext>, allows ref struct
        where TNextReader : new(), allows ref struct
        where TValue : allows ref struct
    {
        TypeHolder<TSelf, TNextReader, TContext, TValue> AsValueReader { get; }

        bool TryGetValue(
            ref ReaderContext readerContext,
            [NotNullWhen(true)][MaybeNullWhen(false)] out TValue value,
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context);
    }

    public interface ITokenReader<TSelf, TToken, TContext>
        where TSelf : ITokenReader<TSelf, TToken, TContext>, allows ref struct
        where TToken : allows ref struct
    {
        TypeHolder<TSelf, TToken, TContext> AsTokenReader { get; } //// TODO you could have an interface implementation for this if you add a `TSelf Self { get; }` property, but it doesn't really matter because `ref struct`s can't take advantage of interface implementations anyway

        bool TryGetToken(
            ref ReaderContext readerContext, //// TODO use `in` instead of `ref`?
            [NotNullWhen(true)][MaybeNullWhen(false)] out Func<TToken> token, //// TODO these are tokens, call them something else; if they were tokens, you could use `ivaluereader`; they *might* actually be "readers" in a way, and so they would be `imovereader`s, but because there are multiple options for the next reader, you can't give back a concrete instance using `new()` //// TODO actually, maybe you can if the tokens themselves do the "reading"; so, for example, `valuereader` be a `imovereader` and it would return a `valuetoken`, which, when `apply` is called (which now needs to receive `readercontext`), would read the data from the payload to determine which of the delegates to call (which is done right now in `valuereader` and the `valuetoken` is initialized with the conclusion); something i don't like about this approach is that it can't be strongly typed in an interface (in a general way) because different readers will have different numbers of "possible" next readers, so you would need (like tuple) multiple interfaces to handle different numbers of type parameters; you *could* do something like `itoken<tnextreader, ttherest> where ttherest : itoken tresult apply<tresult>(func<tnextreader, tresult>, func<ttherest, tresult>)`, but i've only gotten that to work with abstract classes, not interfaces (not saying it is impossible with interfaces), and so that would prevent you from using ref struct; also, the performance on that is probably terrible
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context);

        static abstract TSelf Create(TContext context);
    }

    public ref struct JsonReader : IMoveReader<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>, Nothing>
    {
        public TypeHolder<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>, Nothing>(this);
            }
        }

        public static JsonReader Create(Nothing context)
        {
            return new JsonReader();
        }

        public bool TryMove(
            ref ReaderContext readerContext,
            ////[MaybeNullWhen(false), NotNullWhen(true)] out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> nextReader,
            [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            context = default;
            ////nextReader = new WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>();
            return true;
        }
    }

    public ref struct WhitespaceReader<TNextReader> : IValueReader<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>>
        where TNextReader : new(), allows ref struct //// TODO the `new()` thing is really just an optimization; other libraries following the same reader pattern don't have to have this constaint, and can just pass "next reader factories" around
    {
        private List<WhitespaceToken> tokens;

        public WhitespaceReader()
            : this(new List<WhitespaceToken>())
        {
        }

        private WhitespaceReader(List<WhitespaceToken> tokens)
        {
            this.tokens = tokens;
        }

        public TypeHolder<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>> AsValueReader
        {
            get
            {
                return new TypeHolder<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>>(this);
            }
        }

        public TypeHolder<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>> AsMoveReader
        {
            get
            {
                return new TypeHolder<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>>(this);
            }
        }

        public bool TryGetValue(
            ref ReaderContext readerContext, 
            [MaybeNullWhen(false), NotNullWhen(true)] out List<WhitespaceToken> value, 
            [MaybeNullWhen(true), NotNullWhen(false)] out List<WhitespaceToken> context)
        {
            if (this.tokens == null)
            {
                this.tokens = new List<WhitespaceToken>();
            }

            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    value = default;
                    context = this.tokens;
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

            value = this.tokens;
            context = default;
            return true;
        }

        public bool TryMove(
            ref ReaderContext readerContext, 
            ////[MaybeNullWhen(false), NotNullWhen(true)] out TNextReader nextReader,
            [MaybeNullWhen(true), NotNullWhen(false)] out List<WhitespaceToken> context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }

        public static WhitespaceReader<TNextReader> Create(List<WhitespaceToken> context)
        {
            return new WhitespaceReader<TNextReader>(context);
        }
    }

    public struct WhitespaceToken
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

    public ref struct ValueReader<TNextReader> : ITokenReader<ValueReader<TNextReader>, ValueToken<TNextReader>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<ValueReader<TNextReader>, ValueToken<TNextReader>, Nothing> AsTokenReader
        {
            get
            {
                return new TypeHolder<ValueReader<TNextReader>, ValueToken<TNextReader>, Nothing>(this);
            }
        }

        public static ValueReader<TNextReader> Create(Nothing context)
        {
            return new ValueReader<TNextReader>();
        }

        public bool TryGetToken(
            ref ReaderContext readerContext,
            [MaybeNullWhen(false), NotNullWhen(true)] out Func<ValueToken<TNextReader>> token,
            [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                context = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            switch ((char)readerContext.Buffer[readerContext.CurrentByteIndex])
            {
                case 'f':
                    context = default;
                    token = ValueToken<TNextReader>.False;
                    return true;
                case 'n':
                    context = default;
                    token = ValueToken<TNextReader>.Null;
                    return true;
                case 't':
                    context = default;
                    token = ValueToken<TNextReader>.True;
                    return true;
                case '{':
                    context = default;
                    token = ValueToken<TNextReader>.Object;
                    return true;
                case '[':
                    context = default;
                    token = ValueToken<TNextReader>.Array;
                    return true;
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
                    context = default;
                    token = ValueToken<TNextReader>.Number;
                    return true;
                case '"':
                    context = default;
                    token = ValueToken<TNextReader>.String;
                    return true;
                default:
                    throw new Exception("tODO invalid JSON");
            }
        }
    }

    public ref struct ValueToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
        private int type { get; init; }

        public static ValueToken<TNextReader> False()
        {
            return new ValueToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static ValueToken<TNextReader> Null()
        {
            return new ValueToken<TNextReader>()
            {
                type = 2,
            };
        }

        public static ValueToken<TNextReader> True()
        {
            return new ValueToken<TNextReader>()
            {
                type = 3,
            };
        }

        public static ValueToken<TNextReader> Object()
        {
            return new ValueToken<TNextReader>()
            {
                type = 4,
            };
        }

        public static ValueToken<TNextReader> Array()
        {
            return new ValueToken<TNextReader>()
            {
                type = 5,
            };
        }

        public static ValueToken<TNextReader> Number()
        {
            return new ValueToken<TNextReader>()
            {
                type = 6,
            };
        }

        public static ValueToken<TNextReader> String()
        {
            return new ValueToken<TNextReader>()
            {
                type = 7,
            };
        }

        public bool TryFalse(out FalseReader<TNextReader> falseReader)
        {
            falseReader = default;
            return this.type == 1;
        }

        public bool TryNull(out NullReader<TNextReader> nullReader)
        {
            nullReader = default;
            return this.type == 2;
        }

        public bool TryTrue(out TrueReader<TNextReader> trueReader)
        {
            trueReader = default;
            return this.type == 3;
        }

        public bool TryObject(out ObjectReader<TNextReader> objectReader)
        {
            objectReader = default;
            return this.type == 4;
        }

        public bool TryArray(out ArrayReader<TNextReader> arrayReader)
        {
            arrayReader = default;
            return this.type == 5;
        }

        public bool TryNumber(out NumberReader<TNextReader> numberReader)
        {
            numberReader = default;
            return this.type == 6;
        }

        public bool TryString(out StringReader<TNextReader> stringReader)
        {
            stringReader = default;
            return this.type == 7;
        }
    }

    public ref struct FalseReader<TNextReader> : IValueReader<FalseReader<TNextReader>, TNextReader, int, FalseToken>
        where TNextReader : new(), allows ref struct
    {
        private static readonly string literal = "false"; //// TODO const?
        private int currentCharacter;

        public FalseReader()
            : this(0)
        {
        }

        private FalseReader(int currentCharacter)
        {
            this.currentCharacter = currentCharacter;
        }

        public TypeHolder<FalseReader<TNextReader>, TNextReader, int, FalseToken> AsValueReader
        {
            get
            {
                return new TypeHolder<FalseReader<TNextReader>, TNextReader, int, FalseToken>(this);
            }
        }

        public TypeHolder<FalseReader<TNextReader>, TNextReader, int> AsMoveReader
        {
            get
            {
                return new TypeHolder<FalseReader<TNextReader>, TNextReader, int>(this);
            }
        }

        public static FalseReader<TNextReader> Create(int context)
        {
            return new FalseReader<TNextReader>(context);
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out FalseToken value, [MaybeNullWhen(true), NotNullWhen(false)] out int context)
        {
            for (; this.currentCharacter < literal.Length; ++this.currentCharacter)
            {
                if (!Helpers.TryReadChar(ref readerContext, literal[this.currentCharacter]))
                {
                    value = default;
                    context = this.currentCharacter;
                    return false;
                }
            }

            context = default;
            value = new FalseToken();
            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out int context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct FalseToken
    {
    }

    public ref struct NullReader<TNextReader>
        where TNextReader : new(), allows ref struct
    {
    }

    public ref struct TrueReader<TNextReader> : IValueReader<TrueReader<TNextReader>, TNextReader, int, TrueToken>
        where TNextReader : new(), allows ref struct
    {
        private static readonly string literal = "true"; //// TODO const?
        private int currentCharacter;

        public TrueReader()
            : this(0)
        {
        }

        private TrueReader(int currentCharacter)
        {
            this.currentCharacter = currentCharacter;
        }

        public TypeHolder<TrueReader<TNextReader>, TNextReader, int, TrueToken> AsValueReader
        {
            get
            {
                return new TypeHolder<TrueReader<TNextReader>, TNextReader, int, TrueToken>(this);
            }
        }

        public TypeHolder<TrueReader<TNextReader>, TNextReader, int> AsMoveReader
        {
            get
            {
                return new TypeHolder<TrueReader<TNextReader>, TNextReader, int>(this);
            }
        }

        public static TrueReader<TNextReader> Create(int context)
        {
            return new TrueReader<TNextReader>(context);
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out TrueToken value, [MaybeNullWhen(true), NotNullWhen(false)] out int context)
        {
            for (; this.currentCharacter < literal.Length; ++this.currentCharacter)
            {
                if (!Helpers.TryReadChar(ref readerContext, literal[this.currentCharacter]))
                {
                    value = default;
                    context = this.currentCharacter;
                    return false;
                }
            }

            context = default;
            value = new TrueToken();
            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out int context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct TrueToken
    {
    }

    public ref struct ObjectReader<TNextReader> : IMoveReader<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>, Nothing>(this);
            }
        }

        public static ObjectReader<TNextReader> Create(Nothing context)
        {
            return new ObjectReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct ArrayReader<TNextReader>
        where TNextReader : new(), allows ref struct
    {
    }
























    public ref struct NumberReader<TNextReader> : IMoveReader<NumberReader<TNextReader>, SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<NumberReader<TNextReader>, SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<NumberReader<TNextReader>, SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>, Nothing>(this);
            }
        }

        public static NumberReader<TNextReader> Create(Nothing context)
        {
            return new NumberReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct SignReader<TNextReader> : IValueReader<SignReader<TNextReader>, TNextReader, Nothing, SignToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<SignReader<TNextReader>, TNextReader, Nothing, SignToken> AsValueReader
        {
            get
            {
                return new TypeHolder<SignReader<TNextReader>, TNextReader, Nothing, SignToken>(this);
            }
        }

        public TypeHolder<SignReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<SignReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static SignReader<TNextReader> Create(Nothing context)
        {
            return new SignReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out SignToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                value = default;
                return false;
            }

            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != '-')
            {
                value = SignToken.Absent();
            }
            else
            {
                ++readerContext.CurrentByteIndex;
                value = SignToken.Negative();
            }

            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct SignToken
    {
        private int type { get; init; }

        public static SignToken Absent()
        {
            return new SignToken()
            {
                type = 1,
            };
        }

        public static SignToken Negative()
        {
            return new SignToken()
            {
                type = 2,
            };
        }

        public bool TryAbsent()
        {
            return this.type == 1;
        }
    }

    public ref struct IntReader<TNextReader> : IValueReader<IntReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>>
        where TNextReader : new(), allows ref struct
    {
        private List<DigitToken> digitTokens;

        public IntReader()
            : this(new List<DigitToken>())
        {
        }

        private IntReader(List<DigitToken> digitTokens)
        {
            this.digitTokens = digitTokens;
        }

        public TypeHolder<IntReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>> AsValueReader
        {
            get
            {
                return new TypeHolder<IntReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>>(this);
            }
        }

        public TypeHolder<IntReader<TNextReader>, TNextReader, List<DigitToken>> AsMoveReader
        {
            get
            {
                return new TypeHolder<IntReader<TNextReader>, TNextReader, List<DigitToken>>(this);
            }
        }

        public static IntReader<TNextReader> Create(List<DigitToken> context)
        {
            return new IntReader<TNextReader>(context);
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out IEnumerable<DigitToken> value, [MaybeNullWhen(true), NotNullWhen(false)] out List<DigitToken> context)
        {
            if (this.digitTokens == null)
            {
                this.digitTokens = new List<DigitToken>();
            }

            if (this.digitTokens.Count == 0)
            {
                var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
                DigitToken digit;
                try
                {
                    digit = new DigitToken(currentByte);
                }
                catch (Exception)
                {
                    throw new Exception("TODO invalid JSON");
                }

                this.digitTokens.Add(digit);
                ++readerContext.CurrentByteIndex;
                if (currentByte == '0')
                {
                    value = this.digitTokens;
                    context = default;
                    return true;
                }
            }

            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    value = default;
                    context = this.digitTokens;
                    return false;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(readerContext.Buffer[readerContext.CurrentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                this.digitTokens.Add(digit);
            }

            value = this.digitTokens;
            context = default;
            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out List<DigitToken> context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public struct DigitToken
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

    public ref struct FracReader<TNextReader> : ITokenReader<FracReader<TNextReader>, FracToken<TNextReader>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<FracReader<TNextReader>, FracToken<TNextReader>, Nothing> AsTokenReader
        {
            get
            {
                return new TypeHolder<FracReader<TNextReader>, FracToken<TNextReader>, Nothing>(this);
            }
        }

        public static FracReader<TNextReader> Create(Nothing context)
        {
            return new FracReader<TNextReader>();
        }

        public bool TryGetToken(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out Func<FracToken<TNextReader>> token, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != '.')
            {
                token = FracToken<TNextReader>.Absent;
                return true;
            }
            else
            {
                token = FracToken<TNextReader>.Present;
                return true;
            }
        }
    }

    public ref struct FracToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
        private int type { get; init; }

        public static FracToken<TNextReader> Absent()
        {
            return new FracToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static FracToken<TNextReader> Present()
        {
            return new FracToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryPresent(out DigitsReader<TNextReader> digitsReader)
        {
            digitsReader = default;
            return this.type == 2;
        }
    }

    public ref struct DigitsReader<TNextReader> : IValueReader<DigitsReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>>
        where TNextReader : new(), allows ref struct
    {
        private List<DigitToken> digitTokens;

        public DigitsReader()
            : this(new List<DigitToken>())
        {
        }

        private DigitsReader(List<DigitToken> digitTokens)
        {
            this.digitTokens = digitTokens;
        }

        public TypeHolder<DigitsReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>> AsValueReader
        {
            get
            {
                return new TypeHolder<DigitsReader<TNextReader>, TNextReader, List<DigitToken>, IEnumerable<DigitToken>>(this);
            }
        }

        public TypeHolder<DigitsReader<TNextReader>, TNextReader, List<DigitToken>> AsMoveReader
        {
            get
            {
                return new TypeHolder<DigitsReader<TNextReader>, TNextReader, List<DigitToken>>(this);
            }
        }

        public static DigitsReader<TNextReader> Create(List<DigitToken> context)
        {
            return new DigitsReader<TNextReader>(context);
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out IEnumerable<DigitToken> value, [MaybeNullWhen(true), NotNullWhen(false)] out List<DigitToken> context)
        {
            if (this.digitTokens == null)
            {
                this.digitTokens = new List<DigitToken>();
            }

            //// TODO there might be a bug that allows an "empty" fraction portion...
            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    value = default;
                    context = this.digitTokens;
                    return false;
                }

                DigitToken digit;
                try
                {
                    digit = new DigitToken(readerContext.Buffer[readerContext.CurrentByteIndex]);
                }
                catch (Exception)
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                this.digitTokens.Add(digit);
            }

            value = this.digitTokens;
            context = default;
            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out List<DigitToken> context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct ExpReader<TNextReader> : ITokenReader<ExpReader<TNextReader>, ExpToken<TNextReader>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<ExpReader<TNextReader>, ExpToken<TNextReader>, Nothing> AsTokenReader
        {
            get
            {
                return new TypeHolder<ExpReader<TNextReader>, ExpToken<TNextReader>, Nothing>(this);
            }
        }

        public static ExpReader<TNextReader> Create(Nothing context)
        {
            return new ExpReader<TNextReader>();
        }

        public bool TryGetToken(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out Func<ExpToken<TNextReader>> token, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != '-')
            {
                token = ExpToken<TNextReader>.Absent;
            }
            else
            {
                ++readerContext.CurrentByteIndex;
                token = ExpToken<TNextReader>.Present;
            }

            return true;
        }
    }

    public ref struct ExpToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
        private int type { get; init; }

        public static ExpToken<TNextReader> Absent()
        {
            return new ExpToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static ExpToken<TNextReader> Present()
        {
            return new ExpToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryAbsent([MaybeNullWhen(false)] out TNextReader nextReader)
        {
            nextReader = default;
            return this.type == 1;
        }

        public bool TryPresent(out EReader<ExpSignReader<DigitsReader<TNextReader>>> eReader)
        {
            eReader = default;
            return this.type == 2;
        }
    }

    public ref struct EReader<TNextReader> : IValueReader<EReader<TNextReader>, TNextReader, Nothing, EToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<EReader<TNextReader>, TNextReader, Nothing, EToken> AsValueReader
        {
            get
            {
                return new TypeHolder<EReader<TNextReader>, TNextReader, Nothing, EToken>(this);
            }
        }

        public TypeHolder<EReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<EReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static EReader<TNextReader> Create(Nothing context)
        {
            return new EReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out EToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            value = new EToken((byte)'e');
            return Helpers.TryReadChar(ref readerContext, 'e'); //// TODO should also allow 'E'
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct EToken
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

    public ref struct ExpSignReader<TNextReader> : IValueReader<ExpSignReader<TNextReader>, TNextReader, >
        where TNextReader : new(), allows ref struct
    {
    }

    public ref struct ExpSignToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
    }

























    public ref struct StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>, Nothing>(this);
            }
        }

        public static StringReader<TNextReader> Create(Nothing context)
        {
            return new StringReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct StringDelimiterReader<TNextReader> : IValueReader<StringDelimiterReader<TNextReader>, TNextReader, Nothing, StringDelimiterToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<StringDelimiterReader<TNextReader>, TNextReader, Nothing, StringDelimiterToken> AsValueReader
        {
            get
            {
                return new TypeHolder<StringDelimiterReader<TNextReader>, TNextReader, Nothing, StringDelimiterToken>(this);
            }
        }

        public TypeHolder<StringDelimiterReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<StringDelimiterReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static StringDelimiterReader<TNextReader> Create(Nothing context)
        {
            return new StringDelimiterReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out StringDelimiterToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return Helpers.TryReadChar(ref readerContext, '"');
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct StringDelimiterToken
    {
    }

    /// <summary>
    /// TODO i'm concerned that when trymove returns true, you can't call anything anymore because we've already "used up" the state or something; you should look into this; it's not necessarily "bad" if this is the case, but you should document it at the very least
    /// </summary>
    /// <typeparam name="TNextReader"></typeparam>
    public ref struct CharsReader<TNextReader> : IValueReader<CharsReader<TNextReader>, TNextReader, (List<CharToken>, bool), IEnumerable<CharToken>>
        where TNextReader : new(), allows ref struct
    {
        private List<CharToken> charTokens;

        private bool isEscaping;

        public CharsReader()
            : this(new List<CharToken>(), false)
        {
        }

        private CharsReader(List<CharToken> charTokens, bool isEscaping)
        {
            this.charTokens = charTokens;
            this.isEscaping = isEscaping;
        }

        public TypeHolder<CharsReader<TNextReader>, TNextReader, (List<CharToken>, bool), IEnumerable<CharToken>> AsValueReader
        {
            get
            {
                return new TypeHolder<CharsReader<TNextReader>, TNextReader, (List<CharToken>, bool), IEnumerable<CharToken>>(this);
            }
        }

        public TypeHolder<CharsReader<TNextReader>, TNextReader, (List<CharToken>, bool)> AsMoveReader
        {
            get
            {
                return new TypeHolder<CharsReader<TNextReader>, TNextReader, (List<CharToken>, bool)>(this);
            }
        }

        public static CharsReader<TNextReader> Create((List<CharToken>, bool) context)
        {
            return new CharsReader<TNextReader>(context.Item1, context.Item2);
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out IEnumerable<CharToken> value, [MaybeNullWhen(true), NotNullWhen(false)] out (List<CharToken>, bool) context)
        {
            if (this.charTokens == null)
            {
                this.charTokens = new List<CharToken>();
            }

            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    // read more from the stream
                    value = default;
                    context = (this.charTokens, false);
                    return false;
                }

                var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++readerContext.CurrentByteIndex;
                    if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                    {
                        value = default;
                        context = (this.charTokens, true); //// TODO you need to leverage the context that we are in the middle of an escape
                        return false;
                    }

                    if (readerContext.ValidBytes == 0)
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    throw new Exception("TODO escaped characters are not yet supported");
                }

                CharToken @char;
                try
                {
                    @char = CharToken.Unescaped(currentByte);
                }
                catch (Exception)
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                this.charTokens.Add(@char);
            }

            value = this.charTokens;
            context = default;
            return true;
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out (List<CharToken>, bool) context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public struct CharToken //// TODO making this readonly preliminarily had good perf results...
    {
        private int type { get; init; }

        public byte Char { get; private init; }

        public static CharToken Unescaped(byte @char)
        {
            if (!IsValid(@char))
            {
                throw new Exception("TODO invalid JSON");
            }

            return new CharToken()
            {
                type = 1,
                Char = @char,
            };
        }

        private static bool IsValid(byte @char)
        {
            return
                (@char >= 0x20 && @char <= 0x21) ||
                (@char >= 0x23 && @char <= 0x5B) ||
                (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
        }
    }


























    public ref struct ObjectStartReader<TNextReader> : IValueReader<ObjectStartReader<TNextReader>, TNextReader, Nothing, ObjectStartToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<ObjectStartReader<TNextReader>, TNextReader, Nothing, ObjectStartToken> AsValueReader
        {
            get
            {
                return new TypeHolder<ObjectStartReader<TNextReader>, TNextReader, Nothing, ObjectStartToken>(this);
            }
        }

        public TypeHolder<ObjectStartReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<ObjectStartReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static ObjectStartReader<TNextReader> Create(Nothing context)
        {
            return new ObjectStartReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out ObjectStartToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return Helpers.TryReadChar(ref readerContext, '{');
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct ObjectStartToken
    {
    }

    public ref struct MembersReader<TNextReader> : ITokenReader<MembersReader<TNextReader>, MembersToken<TNextReader>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<MembersReader<TNextReader>, MembersToken<TNextReader>, Nothing> AsTokenReader
        {
            get
            {
                return new TypeHolder<MembersReader<TNextReader>, MembersToken<TNextReader>, Nothing>(this);
            }
        }

        public static MembersReader<TNextReader> Create(Nothing context)
        {
            return new MembersReader<TNextReader>();
        }

        public bool TryGetToken(
            ref ReaderContext readerContext,
            [MaybeNullWhen(false), NotNullWhen(true)] out Func<MembersToken<TNextReader>> token, 
            [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                context = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            if (readerContext.Buffer[readerContext.CurrentByteIndex] == '"')
            {
                token = MembersToken<TNextReader>.Some;
            }
            else
            {
                token = MembersToken<TNextReader>.None;
            }

            return true;
        }
    }

    public ref struct MembersToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
        private int type { get; init; }

        public static MembersToken<TNextReader> None()
        {
            return new MembersToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static MembersToken<TNextReader> Some()
        {
            return new MembersToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryNone([MaybeNullWhen(false)] out TNextReader nextReader)
        {
            nextReader = default;
            return this.type == 1;
        }

        public bool TrySome(out FirstMemberReader<TNextReader> firstMemberReader)
        {
            firstMemberReader = default;
            return this.type == 2;
        }
    }

    public ref struct FirstMemberReader<TNextReader> : IMoveReader<FirstMemberReader<TNextReader>, MemberReader<SubsequentMembersReader<TNextReader>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<FirstMemberReader<TNextReader>, MemberReader<SubsequentMembersReader<TNextReader>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<FirstMemberReader<TNextReader>, MemberReader<SubsequentMembersReader<TNextReader>>, Nothing>(this);
            }
        }

        public static FirstMemberReader<TNextReader> Create(Nothing context)
        {
            return new FirstMemberReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct SubsequentMembersReader<TNextReader> : ITokenReader<SubsequentMembersReader<TNextReader>, SubsequentMembersToken<TNextReader>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<SubsequentMembersReader<TNextReader>, SubsequentMembersToken<TNextReader>, Nothing> AsTokenReader
        {
            get
            {
                return new TypeHolder<SubsequentMembersReader<TNextReader>, SubsequentMembersToken<TNextReader>, Nothing>(this);
            }
        }

        public static SubsequentMembersReader<TNextReader> Create(Nothing context)
        {
            return new SubsequentMembersReader<TNextReader>();
        }

        public bool TryGetToken(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out Func<SubsequentMembersToken<TNextReader>> token, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            //// TODO is it actually faster to make a copy of the reader context here since you are dereferencing it a lot? this doesn't mean that the `ireader` contract needs to change, because sometimes you barely use the parameter at all, but sometimes maybe it makes more sense to make a local copy
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO");
            }

            if (readerContext.Buffer[readerContext.CurrentByteIndex] == ',')
            {
                token = SubsequentMembersToken<TNextReader>.More;
            }
            else
            {
                token = SubsequentMembersToken<TNextReader>.None;
            }

            return true;
        }
    }

    public ref struct SubsequentMembersToken<TNextReader>
        where TNextReader : new(), allows ref struct
    {
        private int type { get; init; }

        public static SubsequentMembersToken<TNextReader> None()
        {
            return new SubsequentMembersToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static SubsequentMembersToken<TNextReader> More()
        {
            return new SubsequentMembersToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryNone([MaybeNullWhen(false)] out TNextReader nextReader)
        {
            nextReader = default;
            return this.type == 1;
        }

        public bool TryMore(out SubsequentMemberReader<SubsequentMembersReader<TNextReader>> subsequentMemberReader)
        {
            subsequentMemberReader = default;
            return this.type == 2;
        }
    }

    public ref struct MemberReader<TNextReader> : IMoveReader<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>, Nothing>(this);
            }
        }

        public static MemberReader<TNextReader> Create(Nothing context)
        {
            return new MemberReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct ColonReader<TNextReader> : IValueReader<ColonReader<TNextReader>, TNextReader, Nothing, ColonToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<ColonReader<TNextReader>, TNextReader, Nothing, ColonToken> AsValueReader
        {
            get
            {
                return new TypeHolder<ColonReader<TNextReader>, TNextReader, Nothing, ColonToken>(this);
            }
        }

        public TypeHolder<ColonReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<ColonReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static ColonReader<TNextReader> Create(Nothing context)
        {
            return new ColonReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out ColonToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return Helpers.TryReadChar(ref readerContext, ':');
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct ColonToken
    {
    }

    public ref struct SubsequentMemberReader<TNextReader> : IMoveReader<SubsequentMemberReader<TNextReader>, CommaReader<WhitespaceReader<MemberReader<TNextReader>>>, Nothing>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<SubsequentMemberReader<TNextReader>, CommaReader<WhitespaceReader<MemberReader<TNextReader>>>, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<SubsequentMemberReader<TNextReader>, CommaReader<WhitespaceReader<MemberReader<TNextReader>>>, Nothing>(this);
            }
        }

        public static SubsequentMemberReader<TNextReader> Create(Nothing context)
        {
            return new SubsequentMemberReader<TNextReader>();
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return true;
        }
    }

    public ref struct CommaReader<TNextReader> : IValueReader<CommaReader<TNextReader>, TNextReader, Nothing, CommaToken>
        where TNextReader : new(), allows ref struct
    {
        public TypeHolder<CommaReader<TNextReader>, TNextReader, Nothing, CommaToken> AsValueReader
        {
            get
            {
                return new TypeHolder<CommaReader<TNextReader>, TNextReader, Nothing, CommaToken>(this);
            }
        }

        public TypeHolder<CommaReader<TNextReader>, TNextReader, Nothing> AsMoveReader
        {
            get
            {
                return new TypeHolder<CommaReader<TNextReader>, TNextReader, Nothing>(this);
            }
        }

        public static CommaReader<TNextReader> Create(Nothing context)
        {
            return new CommaReader<TNextReader>();
        }

        public bool TryGetValue(ref ReaderContext readerContext, [MaybeNullWhen(false), NotNullWhen(true)] out CommaToken value, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return Helpers.TryReadChar(ref readerContext, ',');
        }

        public bool TryMove(ref ReaderContext readerContext, [MaybeNullWhen(true), NotNullWhen(false)] out Nothing context)
        {
            return this.TryGetValue(ref readerContext, out _, out context);
        }
    }

    public ref struct CommaToken
    {
    }



















    public ref struct ObjectEndReader<TNextReader>
        where TNextReader : new(), allows ref struct
    {
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

    }
}
