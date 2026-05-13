namespace OddTrotter.CalendarV1.Tokenization.Json5
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json3;

    public struct ReaderContext
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
            readerContext = await readerContext.Read().ConfigureAwait(false);
            return readerContext;
        }
    }

    public static class ReaderContextExtensions
    {
        public static async ValueTask<ReaderContext> Read(this ReaderContext readerContext)
        {
            readerContext.ValidBytes = await readerContext.Stream.ReadAsync(readerContext.Buffer.AsMemory()).ConfigureAwait(false);
            readerContext.CurrentByteIndex = 0;
            return readerContext;
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
        static abstract bool TryMove(ref ReaderContext readerContext, out TNextReader nextReader);
    }

    public interface IValueReader<TCurrentReader, TNextReader, TValue>
        where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
    {
        static abstract bool TryMove(ref ReaderContext readerContext, out TNextReader nextReader, out TValue value);
    }

    public interface IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
    {
        static abstract bool TryMove(ref ReaderContext readerContext, out TNextReader nextReader, out TValue value, out TContext context);

        static abstract bool TryContinue(ref ReaderContext readerContext, out TNextReader nextReader, out TValue value, ref TContext context);
    }

    public interface ITokenReader<TCurrentReader, TToken>
        where TCurrentReader : ITokenReader<TCurrentReader, TToken>
    {
        static abstract bool TryMove(ref ReaderContext readerContext, out TToken token);
    }


    public sealed class JsonReader : IMoveReader<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IContinuableValueReader<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>>
    {
        public static bool TryContinue(ref ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, ref List<WhitespaceToken> context)
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
                context.Add(whitespace);
            }

            nextReader = default!; //// TODO !
            value = context;
            return true;
        }

        public static bool TryMove(ref ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, out List<WhitespaceToken> context)
        {
            context = new List<WhitespaceToken>();
            return WhitespaceReader<TNextReader>.TryContinue(ref readerContext, out nextReader, out value, ref context);
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

    public sealed class ValueReader<TNextReader> : ITokenReader<ValueReader<TNextReader>, ValueToken<TNextReader>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out ValueToken<TNextReader> token)
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

    public sealed class FalseReader<TNextReader>
    {
    }

    public sealed class NullReader<TNextReader>
    {
    }

    public sealed class TrueReader<TNextReader>
    {
    }

    public sealed class ObjectReader<TNextReader> : IMoveReader<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ArrayReader<TNextReader>
    {
    }

    public sealed class NumberReader<TNextReader>
    {
    }

    public sealed class StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out StringDelimiterReader<CharsReader<StringDelimiterReader<TNextReader>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ObjectStartReader<TNextReader> : IValueReader<ObjectStartReader<TNextReader>, TNextReader, ObjectStartToken>
    {
        public static bool TryMove(ref ReaderContext readerContext, out TNextReader nextReader, out ObjectStartToken value)
        {
            nextReader = default!;
            return Json6.Helpers.TryReadChar(ref readerContext, '{');
        }
    }

    public struct ObjectStartToken
    {
    }

    public sealed class MembersReader<TNextReader> : ITokenReader<MembersReader<TNextReader>, MembersToken<TNextReader>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out MembersToken<TNextReader> token)
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

    public sealed class FirstMemberReader<TNextReader> : IMoveReader<FirstMemberReader<TNextReader>, MemberReader<SubsequentMemberReader<TNextReader>>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out MemberReader<SubsequentMemberReader<TNextReader>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class MemberReader<TNextReader> : IMoveReader<MemberReader<TNextReader>, StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>>>
    {
        public static bool TryMove(ref ReaderContext readerContext, out StringReader<WhitespaceReader<ColonReader<WhitespaceReader<ValueReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ColonReader<TNextReader>
    {
    }

    public sealed class SubsequentMemberReader<TNextReader>
    {
    }

    public sealed class StringDelimiterReader<TNextReader>
    {
    }

    public ref struct StringDelimiterToken
    {
    }

    public sealed class CharsReader<TNextReader>
    {
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

    public sealed class ObjectEndReader<TNextReader>
    {
    }
}

namespace OddTrotter.CalendarV1.Tokenization.Json6 //// TODO should be json5
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;

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
            ref ReaderContext readerContext,
            out TNextReader nextReader)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            return TCurrentReader.TryMove(ref readerContext, out nextReader);
        }

        public static bool TryMove2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            out TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryMove(ref readerContext, out nextReader, out value, out context);
        }

        public static bool TryContinue2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            ref TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryContinue(ref  readerContext, out nextReader, out value, ref context);
        }

        public static bool TryMove3<TCurrentReader, TToken>(this ITokenReader<TCurrentReader, TToken> tokenReader, ref ReaderContext readerContext, out TToken token)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            return TCurrentReader.TryMove(ref readerContext, out token);
        }

        public static bool TryMove4<TCurrentReader, TNextReader, TValue>(this IValueReader<TCurrentReader, TNextReader, TValue> valueReader, ref ReaderContext readerContext, out TNextReader nextReader, out TValue value)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            return TCurrentReader.TryMove(ref readerContext, out nextReader, out value);
        }









        

        
        public static bool TryMove<TNextReader>(
            this StringDelimiterReader<TNextReader> stringDelimiterReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out StringDelimiterToken value)
        {
            nextReader = default!; //// TODO !
            return Helpers.TryReadChar(ref readerContext, '"');
        }

        public static bool TryMove<TNextReader>(
            this CharsReader<TNextReader> charsReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out (List<CharToken>, bool) value)
        {
            value = (new List<CharToken>(), false);
            return charsReader.TryContinue(ref readerContext, out nextReader, ref value);
        }

        public static bool TryContinue<TNextReader>(
            this CharsReader<TNextReader> charsReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            ref (List<CharToken>, bool) value)
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
                    value = (value.Item1, false);
                    return false;
                }

                var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++readerContext.CurrentByteIndex;
                    if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                    {
                        nextReader = default!; //// TODO !
                        value = (value.Item1, true); //// TODO you need to leverage the context that we are in the middle of an escape
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
                value.Item1.Add(@char);
            }

            nextReader = default!; //// TODO !
            return true;
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

    }
}
