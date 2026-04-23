namespace OddTrotter.CalendarV1.Tokenization.Json3
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.ConventionContext;

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

        public static ReaderContext FromStream(Stream stream, byte[] buffer)
        {
            return new ReaderContext(stream, buffer, 0, 0);
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
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context);

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
        private readonly List<WhitespaceToken> tokens;

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
            if (!this.TryGetValue(ref readerContext, out _, out context))
            {
                ////nextReader = default;
                return false;
            }

            ////nextReader = new();
            return true;
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

    public ref struct ValueReader<TNextReader>
        where TNextReader : new(), allows ref struct
    {
    }
}
