namespace OddTrotter.CalendarV1.Tokenization.Json5
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json2;
    using OddTrotter.CalendarV1.Tokenization.Json6;

    using static OddTrotter.Calendar.OdataCollectionResponse;

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



    public interface IMoveReader<TCurrentReader, TNextReader>
        where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader);
    }

    public interface IValueReader<TCurrentReader, TNextReader, TValue>
        where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out TValue value);
    }

    public interface IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out TValue value, out TContext context);

        static abstract bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out TValue value, ref TContext context);
    }

    public interface ITokenReader<TCurrentReader, TToken>
        where TCurrentReader : ITokenReader<TCurrentReader, TToken>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TToken token);
    }


    public sealed class JsonReader : IMoveReader<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    /*public sealed class WhitespaceReader<TNextReader> : ITokenReader<WhitespaceReader<TNextReader>, WhitespaceToken2<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out WhitespaceToken2<TNextReader> token)
        {
            if (readerContext.ValidBytes == 0)
            {
                token = default!;
                return false;
            }

            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default!;
                return false;
            }

            try
            {
                new WhitespaceCharToken(readerContext.Buffer[readerContext.CurrentByteIndex]);
                token = WhitespaceToken2<TNextReader>.More();
            }
            catch //// TODO use control flow logic
            {
                token = WhitespaceToken2<TNextReader>.None();
            }

            return true;
        }
    }

    public struct WhitespaceToken2<TNextReader>
    {
        private int type { get; init; }

        public static WhitespaceToken2<TNextReader> More()
        {
            return new WhitespaceToken2<TNextReader>()
            {
                type = 1,
            };
        }

        public static WhitespaceToken2<TNextReader> None()
        {
            return new WhitespaceToken2<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryMore(out WhitespaceCharReader<WhitespaceReader<TNextReader>> more)
        {
            more = default!; //// TODO !
            return this.type == 1;
        }

        public bool TryNone(out TNextReader none)
        {
            none = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class WhitespaceCharReader<TNextReader> : IValueReader<WhitespaceCharReader<TNextReader>, TNextReader, WhitespaceCharToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out WhitespaceCharToken value)
        {
            if (readerContext.ValidBytes == 0)
            {
                nextReader = default!;
                value = default!;
                return false;
            }

            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                nextReader = default!;
                value = default!;
                return false;
            }

            nextReader = default!;
            value = new WhitespaceCharToken(readerContext.Buffer[readerContext.CurrentByteIndex]); //// TODO handle exception
            ++readerContext.CurrentByteIndex;
            return true;
        }
    }

    public struct WhitespaceCharToken
    {
        public WhitespaceCharToken(byte @char)
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
    }*/








    public sealed class WhitespaceReader<TNextReader> : IContinuableValueReader<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, ref List<WhitespaceToken> context)
        {
            while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    nextReader = default!; //// TODO !
                    value = default!; //// TODO !
                    return false;
                }

                //// TODO use control flow logic for other values, like chars
                if (!WhitespaceToken.TryCreate(readerContext.Buffer[readerContext.CurrentByteIndex], out var whitespace))
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                context.Add(whitespace);
            }

            nextReader = default!; //// TODO !
            value = context;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, out List<WhitespaceToken> context)
        {
            context = new List<WhitespaceToken>();
            return WhitespaceReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
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

    public sealed class ValueReader<TNextReader> : ITokenReader<ValueReader<TNextReader>, ValueToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out ValueToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default!; //// TODO !
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            switch ((char)readerContext.Buffer[readerContext.CurrentByteIndex])
            {
                case 'f':
                    token = ValueToken<TNextReader>.False();
                    return true;
                case 'n':
                    token = ValueToken<TNextReader>.Null();
                    return true;
                case 't':
                    token = ValueToken<TNextReader>.True();
                    return true;
                case '{':
                    token = ValueToken<TNextReader>.Object();
                    return true;
                case '[':
                    token = ValueToken<TNextReader>.Array();
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
                    token = ValueToken<TNextReader>.Number();
                    return true;
                case '"':
                    token = ValueToken<TNextReader>.String();
                    return true;
                default:
                    throw new Exception("tODO invalid JSON");
            }
        }
    }

    public struct ValueToken<TNextReader>
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
            falseReader = default!; //// TODO !
            return this.type == 1;
        }

        public bool TryNull(out NullReader<TNextReader> nullReader)
        {
            nullReader = default!; //// TODO !
            return this.type == 2;
        }

        public bool TryTrue(out TrueReader<TNextReader> trueReader)
        {
            trueReader = default!; //// TODO !
            return this.type == 3;
        }

        public bool TryObject(out ObjectReader<TNextReader> objectReader)
        {
            objectReader = default!; //// TODO !
            return this.type == 4;
        }

        public bool TryArray(out ArrayReader<TNextReader> arrayReader)
        {
            arrayReader = default!; //// TODO !
            return this.type == 5;
        }

        public bool TryNumber(out NumberReader<TNextReader> numberReader)
        {
            numberReader = default!; //// TODO !
            return this.type == 6;
        }

        public bool TryString(out StringReader<TNextReader> stringReader)
        {
            stringReader = default!; //// TODO !
            return this.type == 7;
        }
    }

    public sealed class FalseReader<TNextReader> : IContinuableValueReader<FalseReader<TNextReader>, TNextReader, FalseToken, int>
    {
        private static readonly string literal = "false"; //// TODO const?

        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out FalseToken value, ref int context)
        {
            for (; context < literal.Length; ++context)
            {
                if (!Json6.Helpers.TryReadChar(readerContext, literal[context]))
                {
                    nextReader = default!; //// TODO !
                    value = default;
                    return false;
                }
            }

            nextReader = default!; //// TODO !
            value = new FalseToken();
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out FalseToken value, out int context)
        {
            context = 0;
            return FalseReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
        }
    }

    public struct FalseToken
    {
    }

    public sealed class NullReader<TNextReader> : IContinuableValueReader<NullReader<TNextReader>, TNextReader, NullToken, int>
    {
        private static readonly string literal = "null"; //// TODO const?

        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out NullToken value, ref int context)
        {
            for (; context < literal.Length; ++context)
            {
                if (!Json6.Helpers.TryReadChar(readerContext, literal[context]))
                {
                    nextReader = default!; //// TODO !
                    value = default;
                    return false;
                }
            }

            nextReader = default!; //// TODO !
            value = new NullToken();
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out NullToken value, out int context)
        {
            context = 0;
            return NullReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
        }
    }

    public struct NullToken
    {
    }

    public sealed class TrueReader<TNextReader> : IContinuableValueReader<TrueReader<TNextReader>, TNextReader, TrueToken, int>
    {
        private static readonly string literal = "true"; //// TODO const?

        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out TrueToken value, ref int context)
        {
            for (; context < literal.Length; ++context)
            {
                if (!Json6.Helpers.TryReadChar(readerContext, literal[context]))
                {
                    nextReader = default!; //// TODO !
                    value = default;
                    return false;
                }
            }

            nextReader = default!; //// TODO !
            value = new TrueToken();
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out TrueToken value, out int context)
        {
            context = 0;
            return TrueReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
        }
    }

    public struct TrueToken
    {
    }

    public sealed class ObjectReader<TNextReader> : IMoveReader<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ArrayReader<TNextReader> : IMoveReader<ArrayReader<TNextReader>, ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out ArrayStartReader<WhitespaceReader<ArrayElementsReader<WhitespaceReader<ArrayEndReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!;
            return true;
        }
    }

    public sealed class ArrayStartReader<TNextReader> : IValueReader<ArrayStartReader<TNextReader>, TNextReader, ArrayStartToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ArrayStartToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, '[');
        }
    }

    public struct ArrayStartToken
    {
    }

    public sealed class ArrayElementsReader<TNextReader> : ITokenReader<ArrayElementsReader<TNextReader>, ArrayElementsToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out ArrayElementsToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                token = default;
                return false;
            }

            var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
            if (currentByte == ']')
            {
                token = ArrayElementsToken<TNextReader>.None();
            }
            else
            {
                token = ArrayElementsToken<TNextReader>.Some();
            }

            return true;
        }
    }

    public struct ArrayElementsToken<TNextReader>
    {
        private int type { get; init; }

        public static ArrayElementsToken<TNextReader> None()
        {
            return new ArrayElementsToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static ArrayElementsToken<TNextReader> Some()
        {
            return new ArrayElementsToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryNone(out TNextReader nextReader)
        {
            nextReader = default!;
            return this.type == 1;
        }

        public bool TrySome(out ArrayElementReader<SubsequentArrayElementsReader<TNextReader>> arrayElementReader)
        {
            arrayElementReader = default!;
            return this.type == 2;
        }
    }

    public sealed class ArrayElementReader<TNextReader> : IMoveReader<ArrayElementReader<TNextReader>, ValueReader<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out ValueReader<TNextReader> nextReader)
        {
            nextReader = default!;
            return true;
        }
    }

    public sealed class SubsequentArrayElementsReader<TNextReader> : ITokenReader<SubsequentArrayElementsReader<TNextReader>, SubsequentArrayElementsToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out SubsequentArrayElementsToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                token = default;
                return false;
            }

            var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
            if (currentByte == ',')
            {
                token = SubsequentArrayElementsToken<TNextReader>.More();
            }
            else
            {
                token = SubsequentArrayElementsToken<TNextReader>.None();
            }

            return true;
        }
    }

    public struct SubsequentArrayElementsToken<TNextReader>
    {
        private int type { get; init; }

        public static SubsequentArrayElementsToken<TNextReader> None()
        {
            return new SubsequentArrayElementsToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static SubsequentArrayElementsToken<TNextReader> More()
        {
            return new SubsequentArrayElementsToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryNone(out TNextReader nextReader)
        {
            nextReader = default!;
            return this.type == 1;
        }

        public bool TryMore(out SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>> more)
        {
            more = default!;
            return this.type == 2;
        }
    }

    public sealed class ArrayEndReader<TNextReader> : IValueReader<ArrayEndReader<TNextReader>, TNextReader, ArrayEndToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ArrayEndToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, ']');
        }
    }

    public struct ArrayEndToken
    {
    }

    public sealed class NumberReader<TNextReader> : IMoveReader<NumberReader<TNextReader>, SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out SignReader<IntReader<FracReader<ExpReader<TNextReader>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class SignReader<TNextReader> : IValueReader<SignReader<TNextReader>, TNextReader, SignToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out SignToken value)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                nextReader = default!; //// TODO !
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

            nextReader = default!; //// TODO !
            return true;
        }
    }

    public struct SignToken
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
    }

    public sealed class IntReader<TNextReader> : IContinuableValueReader<IntReader<TNextReader>, TNextReader, List<DigitToken>, List<DigitToken>>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<DigitToken> value, ref List<DigitToken> context)
        {
            if (context.Count == 0)
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

                context.Add(digit);
                ++readerContext.CurrentByteIndex;
                if (currentByte == '0')
                {
                    nextReader = default!; //// TODO !
                    value = context;
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
                    nextReader = default!; //// TODO !
                    value = default!; //// TODO !
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
                context.Add(digit);
            }

            nextReader = default!; //// TODO !
            value = context;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<DigitToken> value, out List<DigitToken> context)
        {
            context = new List<DigitToken>();
            return IntReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
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

    public sealed class FracReader<TNextReader> : ITokenReader<FracReader<TNextReader>, FracToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out FracToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != '.')
            {
                token = FracToken<TNextReader>.Absent();
                return true;
            }
            else
            {
                token = FracToken<TNextReader>.Present();
                return true;
            }
        }
    }

    public struct FracToken<TNextReader>
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

        public bool TryAbsent(out TNextReader nextReader)
        {
            nextReader = default!; //// TODO !
            return this.type == 1;
        }

        public bool TryPresent(out DigitsReader<TNextReader> digitsReader)
        {
            digitsReader = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class DigitsReader<TNextReader> : IContinuableValueReader<DigitsReader<TNextReader>, TNextReader, List<DigitToken>, List<DigitToken>>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<DigitToken> value, ref List<DigitToken> context)
        {
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
                    nextReader = default!; //// TODO !
                    value = default!; //// TODO !
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
                context.Add(digit);
            }

            nextReader = default!; //// TODO !
            value = context;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<DigitToken> value, out List<DigitToken> context)
        {
            context = new List<DigitToken>();
            return DigitsReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
        }
    }

    public sealed class ExpReader<TNextReader> : ITokenReader<ExpReader<TNextReader>, ExpToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out ExpToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != '-')
            {
                token = ExpToken<TNextReader>.Absent();
            }
            else
            {
                ++readerContext.CurrentByteIndex;
                token = ExpToken<TNextReader>.Present();
            }

            return true;
        }
    }

    public struct ExpToken<TNextReader>
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
            eReader = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class EReader<TNextReader> : IValueReader<EReader<TNextReader>, TNextReader, EToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out EToken value)
        {
            nextReader = default!; //// TODO !
            value = new EToken((byte)'e');
            return Json6.Helpers.TryReadChar(readerContext, 'e'); //// TODO should also allow 'E'
        }
    }

    public struct EToken
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

    public sealed class ExpSignReader<TNextReader> : IValueReader<ExpSignReader<TNextReader>, TNextReader, ExpSignToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ExpSignToken value)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                nextReader = default!; //// TODO !
                value = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                nextReader = default!; //// TODO !
                value = ExpSignToken.Absent();
                return true;
            }

            var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
            if (currentByte == '+')
            {
                ++readerContext.CurrentByteIndex;
                value = ExpSignToken.Positive();
            }
            else if (currentByte == '-')
            {
                ++readerContext.CurrentByteIndex;
                value = ExpSignToken.Negative();
            }
            else
            {
                value = ExpSignToken.Absent();
            }

            nextReader = default!; //// TODO !
            return true;
        }
    }

    public struct ExpSignToken
    {
        private int type { get; init; }

        public static ExpSignToken Absent()
        {
            return new ExpSignToken()
            {
                type = 1,
            };
        }

        public static ExpSignToken Positive()
        {
            return new ExpSignToken()
            {
                type = 2,
            };
        }

        public static ExpSignToken Negative()
        {
            return new ExpSignToken()
            {
                type = 3,
            };
        }
    }

    public sealed class StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ObjectStartReader<TNextReader> : IValueReader<ObjectStartReader<TNextReader>, TNextReader, ObjectStartToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ObjectStartToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, '{');
        }
    }

    public struct ObjectStartToken
    {
    }

    public sealed class MembersReader<TNextReader> : ITokenReader<MembersReader<TNextReader>, MembersToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out MembersToken<TNextReader> token)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            if (readerContext.Buffer[readerContext.CurrentByteIndex] == '"')
            {
                token = MembersToken<TNextReader>.Some();
            }
            else
            {
                token = MembersToken<TNextReader>.None();
            }

            return true;
        }
    }

    public struct MembersToken<TNextReader>
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
            firstMemberReader = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class FirstMemberReader<TNextReader> : IMoveReader<FirstMemberReader<TNextReader>, MemberReader<SubsequentMembersReader<TNextReader>>>
    {
        public static bool TryMove(ReaderContext readerContext, out MemberReader<SubsequentMembersReader<TNextReader>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class MemberReader<TNextReader> : IMoveReader<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ColonReader<TNextReader> : IValueReader<ColonReader<TNextReader>, TNextReader, ColonToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ColonToken value)
        {
            nextReader = default!; //// TODO !
            return Json6.Helpers.TryReadChar(readerContext, ':');
        }
    }

    public struct ColonToken
    {
    }

    public sealed class SubsequentMembersReader<TNextReader> : ITokenReader<SubsequentMembersReader<TNextReader>, SubsequentMembersToken<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out SubsequentMembersToken<TNextReader> token)
        {
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
                token = SubsequentMembersToken<TNextReader>.More();
            }
            else
            {
                token = SubsequentMembersToken<TNextReader>.None();
            }

            return true;
        }
    }

    public struct SubsequentMembersToken<TNextReader>
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
            subsequentMemberReader = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class SubsequentMemberReader<TNextReader> : IMoveReader<SubsequentMemberReader<TNextReader>, CommaReader<WhitespaceReader<MemberReader<TNextReader>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out CommaReader<WhitespaceReader<MemberReader<TNextReader>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class CommaReader<TNextReader> : IValueReader<CommaReader<TNextReader>, TNextReader, CommaToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out CommaToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, ',');
        }
    }

    public struct CommaToken
    {
    }

    public sealed class StringDelimiterReader<TNextReader> : IValueReader<StringDelimiterReader<TNextReader>, TNextReader, StringDelimiterToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out StringDelimiterToken value)
        {
            nextReader = default!; //// TODO !
            return Json6.Helpers.TryReadChar(readerContext, '"');
        }
    }

    public struct StringDelimiterToken
    {
    }

    public sealed class CharsReader<TNextReader> : IContinuableValueReader<CharsReader<TNextReader>, TNextReader, List<CharToken>, (List<CharToken>, bool)>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<CharToken> value, ref (List<CharToken>, bool) context)
        {
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
                    nextReader = default!; //// TODO !
                    context = (context.Item1, false);
                    value = context.Item1;
                    return false;
                }

                var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++readerContext.CurrentByteIndex;
                    if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                    {
                        nextReader = default!; //// TODO !
                        context = (context.Item1, true); //// TODO you need to leverage the context that we are in the middle of an escape
                        value = context.Item1;
                        return false;
                    }

                    if (readerContext.ValidBytes == 0)
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    throw new Exception("TODO escaped characters are not yet supported");
                }

                if (!CharToken.TryUnescaped(currentByte, out var @char))
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                context.Item1.Add(@char);
            }

            nextReader = default!; //// TODO !
            value = context.Item1;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<CharToken> value, out (List<CharToken>, bool) context)
        {
            context = (new List<CharToken>(), false);
            return CharsReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);
        }
    }

    public struct CharToken //// TODO making this readonly preliminarily had good perf results...
    {
        private int type { get; init; }

        public byte Char { get; private init; }

        public static bool TryUnescaped(byte @char, out CharToken charToken)
        {
            if (!IsValid(@char))
            {
                charToken = default;
                return false;
            }

            charToken = new CharToken()
            {
                type = 1,
                Char = @char,
            };
            return true;
        }

        private static bool IsValid(byte @char)
        {
            return
                (@char >= 0x20 && @char <= 0x21) ||
                (@char >= 0x23 && @char <= 0x5B) ||
                (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
        }
    }

    public sealed class ObjectEndReader<TNextReader> : IValueReader<ObjectEndReader<TNextReader>, TNextReader, ObjectEndToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ObjectEndToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, '}');
        }
    }

    public struct ObjectEndToken
    {
    }

}

namespace OddTrotter.CalendarV1.Tokenization.Json6 //// TODO should be json5
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json5;

    public static class Readers
    {
        public static JsonReader Create()
        {
            return null!; //// TODO !
        }

        public static void DoWork<T1, T2>(
            this T1 t1,
            out T2 t2)
        {
            t2 = default!;
        }

        public static bool TryMove1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> moveReader,
            ReaderContext readerContext,
            out TNextReader nextReader)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader);
        }

        public static bool TryMove2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            out TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader, out value, out context);
        }

        public static bool TryContinue2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            ref TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryContinue( readerContext, out nextReader, out value, ref context);
        }

        public static bool TryMove3<TCurrentReader, TToken>(this ITokenReader<TCurrentReader, TToken> tokenReader, ReaderContext readerContext, out TToken token)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            return TCurrentReader.TryMove(readerContext, out token);
        }

        public static bool TryMove4<TCurrentReader, TNextReader, TValue>(this IValueReader<TCurrentReader, TNextReader, TValue> valueReader, ReaderContext readerContext, out TNextReader nextReader, out TValue value)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader, out value);
        }

        
        /*public static async Task<(ReaderContext, TNextReader)> Move2<TNextReader>(this WhitespaceReader<TNextReader> whitespaceReader, ReaderContext readerContext)
        {
            while (true)
            {
                var whitespaceToken = await whitespaceReader.Move31(readerContext).ConfigureAwait(false);
                if (whitespaceToken.TryMore(out var whitespaceCharReader))
                {
                    whitespaceReader = await whitespaceCharReader.Move4(readerContext).ConfigureAwait(false);
                }
                else if (whitespaceToken.TryNone(out var nextReader))
                {
                    return (readerContext, nextReader);
                }
            }
        }*/

    }

    public static class Helpers
    {

        public static bool TryReadChar(ReaderContext readerContext, char character)
        {
            if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                return false;
            }

            ReadChar(readerContext, character);
            ++readerContext.CurrentByteIndex;
            return true;
        }

        private static void ReadChar(ReaderContext readerContext, char character)
        {
            if (readerContext.ValidBytes == 0 || readerContext.Buffer[readerContext.CurrentByteIndex] != character)
            {
                throw new Exception("TODO invalid JSON");
            }
        }

    }
}
