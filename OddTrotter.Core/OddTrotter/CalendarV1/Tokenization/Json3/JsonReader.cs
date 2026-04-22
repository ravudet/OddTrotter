namespace OddTrotter.CalendarV1.Tokenization.Json3
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

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
        public ReaderContext(
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
        where TNextReader : allows ref struct
        // TContext can't be a ref struct, because it's what we use when we need a task to read more from `readercontext`
    {
        TypeHolder<TSelf, TNextReader, TContext> AsMoveReader { get; }

        bool TryMove(
            ref ReaderContext readerContext, 
            [NotNullWhen(true)][MaybeNullWhen(false)] out TNextReader nextReader, 
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context, 
            [NotNullWhen(false)][MaybeNullWhen(true)] out Func<TContext, TSelf> currentReaderFactory);
    }

    public interface IValueReader<TSelf, TNextReader, TContext, TValue> : IMoveReader<TSelf, TNextReader, TContext>
        where TSelf : IMoveReader<TSelf, TNextReader, TContext>, allows ref struct
        where TNextReader : allows ref struct
        where TValue : allows ref struct
    {
        TypeHolder<TSelf, TNextReader, TContext, TValue> AsValueReader { get; }

        bool TryGetValue(
            ref ReaderContext readerContext, 
            [NotNullWhen(true)][MaybeNullWhen(false)] out TValue value,
            [NotNullWhen(false)][MaybeNullWhen(true)] out TContext context, 
            [NotNullWhen(false)][MaybeNullWhen(true)] out Func<TContext, TSelf> currentReaderFactory);
    }


}
