namespace OddTrotter.CalendarV1.Tokenization.Json5
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

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




    public sealed class JsonReader
    {
    }

    public sealed class WhitespaceReader<TNextReader>
    {
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

    public sealed class ValueReader<TNextReader>
    {
    }

    public ref struct ValueToken<TNextReader>
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

    public sealed class ObjectReader<TNextReader>
    {
    }

    public sealed class ArrayReader<TNextReader>
    {
    }

    public sealed class NumberReader<TNextReader>
    {
    }

    public sealed class StringReader<TNextReader>
    {
    }

    public sealed class ObjectStartReader<TNextReader>
    {
    }

    public ref struct ObjectStartToken
    {
    }

    public sealed class MembersReader<TNextReader>
    {
    }

    public ref struct MembersToken<TNextReader>
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

    public sealed class FirstMemberReader<TNextReader>
    {
    }

    public sealed class ObjectEndReader<TNextReader>
    {
    }
}

namespace OddTrotter.CalendarV1.Tokenization.Json6 //// TODO should be json5
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.CalendarV1.Tokenization.Json5;

    public static class Readers
    {
        public static JsonReader Create()
        {
            return null!; //// TODO !
        }

        public static bool TryMove(
            this JsonReader jsonReader,
            ref ReaderContext readerContext,
            out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> nextReader)
        {
            nextReader = null!; //// TODO !
            return true;
        }

        public static bool TryMove<TNextReader>(
            this WhitespaceReader<TNextReader> whitespaceReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out List<WhitespaceToken> value)
        {
            value = new List<WhitespaceToken>();
            return whitespaceReader.TryContinue(ref readerContext, value, out nextReader);
        }

        public static bool TryContinue<TNextReader>(
            this WhitespaceReader<TNextReader> whitespaceReader,
            ref ReaderContext readerContext,
            List<WhitespaceToken> value,
            out TNextReader nextReader)
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
                value.Add(whitespace);
            }

            nextReader = default!; //// TODO !
            return true;
        }

        public static bool TryMove<TNextReader>(
            this ValueReader<TNextReader> valueReader,
            ref ReaderContext readerContext,
            out ValueToken<TNextReader> token)
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

        public static bool TryMove<TNextReader>(
            this ObjectReader<TNextReader> objectReader,
            ref ReaderContext readerContext,
            out ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>> nextReader)
        {
            nextReader = null!; //// TODO !
            return true;
        }

        public static bool TryMove<TNextReader>(
            this ObjectStartReader<TNextReader> objectStartReader,
            ref ReaderContext readerContext,
            out TNextReader nextReader,
            out ObjectStartToken value)
        {
            nextReader = default!;
            return Helpers.TryReadChar(ref readerContext, '{');
        }

        public static bool TryMove<TNextReader>(
            this MembersReader<TNextReader> membersReader,
            ref ReaderContext readerContext,
            out MembersToken<TNextReader> token)
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
