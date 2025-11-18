namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.CalendarV1.Tokenization.Readers;

    [TestClass]
    public sealed class ReaderUnitTests
    {
        [TestMethod]
        public async Task Broad()
        {
            var data =
"""
{
    "true": true,
    "false": false,
    "number": 1234,
    "string": "asdf",
    "null": null,
    "object": {
        "true": true,
        "false": false,
        "number": 1234,
        "string": "asdf",
        "null": null
    },
    "array": [
        {
            "true": true,
            "false": false,
            "number": 1234,
            "string": "asdf",
            "null": null
        }
    ]
}
""";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                await Helpers.ReadToEnd(Helpers.StartReading(stream, new ArrayResizer(stream.Length))).ConfigureAwait(false);
            }
        }

        private sealed class ArrayResizer : IArrayResizer
        {
            private readonly long defaultSize;

            public ArrayResizer(long defaultSize)
            {
                this.defaultSize = defaultSize;
            }

            public byte[] Resize(byte[] previous)
            {
                if (previous.Length == 0)
                {
                    return new byte[this.defaultSize];
                }

                var next = new byte[previous.Length * 2];
                Array.Copy(previous, 0, next, 0, previous.Length);
                return next;
            }
        }
    }

    public static class ReaderExtensions
    {
        public static async ValueTask DoWork()
        {
            var someReader = new SomeReader();
            await someReader.ReadToEnd();
        }

        public static ReadToEndTask ReadToEnd(this SomeReader someReader)
        {
            var someReaderTask = someReader.MoveNext();
            if (someReaderTask.TryGetValue(out var nextReader))
            {
                nextReader.MoveNext();
            }
            else
            {

            }
        }

        public ref struct ReadToEndTask
        {
            public struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public void GetResult()
                {
                    throw new NotImplementedException();
                }

                public void OnCompleted(Action continuation)
                {
                    throw new NotImplementedException();
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    throw new NotImplementedException();
                }
            }

            public Awaiter GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        public static ReadToEndTask<SomeReader, NextReader> MoveNext(this SomeReader reader)
        {
            if (reader.TryMoveNext(out var next))
            {
                return new ReadToEndTask<SomeReader, NextReader>(next);
            }
            else
            {
                return new ReadToEndTask<SomeReader, NextReader>(reader.Read(), old => old.TryMoveNext(out var next) ? next : throw new Exception("TODO in this iteration, this is a bug, because read should always give us enough information to do a proper move in the subsequent call"));
            }
        }

        public ref struct ReadToEndTask<TOld, TNew>
            where TOld : allows ref struct
            where TNew : allows ref struct
        {
            private readonly TNew value;

            private readonly ReaderTask<TOld> task;

            private readonly Func<TOld, TNew>? adapter;

            public ReadToEndTask(TNew value)
            {
                this.value = value;

                this.adapter = null;
            }

            public ReadToEndTask(ReaderTask<TOld> task, Func<TOld, TNew> adapter)
            {
                this.value = default!;

                this.task = task;

                this.adapter = adapter;
            }

            public bool TryGetValue([MaybeNullWhen(false)] out TNew value)
            {
                value = this.value;
                return this.adapter == null;
            }

            public Awaiter GetAwaiter()
            {
                if (this.adapter == null)
                {
                    throw new Exception("TODO");
                }

                return new Awaiter(this.task, this.adapter);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private ReaderTask<TOld>.Awaiter readerTask;

                private Func<TOld, TNew> adapter;

                public Awaiter(ReaderTask<TOld> readerTask, Func<TOld, TNew> adapter)
                {
                    this.readerTask = readerTask.GetAwaiter();
                    this.adapter = adapter;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.readerTask.IsCompleted;
                    }
                }

                public TNew GetResult()
                {
                    return this.adapter(this.readerTask.GetResult());
                }

                public void OnCompleted(Action continuation)
                {
                    this.readerTask.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.readerTask.UnsafeOnCompleted(continuation);
                }
            }

        }
    }

    public ref struct ReaderTask<T>
        where T : allows ref struct
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int currentIndex;
        private readonly Func<Stream, byte[], int, T> factory;
        private readonly bool doNothing;

        public ReaderTask(
            Stream stream,
            byte[] buffer,
            int currentIndex,
            Func<Stream, byte[], int, T> factory,
            bool doNothing)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
            this.factory = factory;
            this.doNothing = doNothing;
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly Stream stream;
            private readonly byte[] buffer;
            private readonly int currentIndex;
            private readonly Func<Stream, byte[], int, T> factory;

            private TaskAwaiter<int> taskAwaiter;

            public Awaiter(
                Stream stream,
                byte[] buffer,
                int currentIndex,
                Func<Stream, byte[], int, T> factory)
            {
                this.stream = stream;
                this.buffer = buffer;
                this.currentIndex = currentIndex;
                this.factory = factory;

                this.taskAwaiter = this.stream.ReadAsync(this.buffer, 0, this.buffer.Length).GetAwaiter();
            }

            public bool IsCompleted
            {
                get
                {
                    return this.taskAwaiter.IsCompleted;
                }
            }

            public T GetResult()
            {
                return this.factory(this.stream, this.buffer, 0);
            }

            public void OnCompleted(Action continuation)
            {
                this.taskAwaiter.OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                this.taskAwaiter.UnsafeOnCompleted(continuation);
            }
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter(
                this.stream,
                this.buffer,
                this.currentIndex,
                this.factory);
        }
    }

    public ref struct SomeReader
    {
        private static readonly byte[] someBytes = [(byte)'s', (byte)'o', (byte)'m', (byte)'e'];

        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int currentIndex;

        public SomeReader(
            Stream stream, 
            byte[] buffer, 
            int currentIndex)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
        }

        public ReaderTask<SomeReader> Read()
        {
            return new ReaderTask<SomeReader>(
                this.stream,
                this.buffer,
                this.currentIndex,
                (stream, buffer, currentIndex) => new SomeReader(stream, buffer, currentIndex),
                false);
        }

        public bool TryMoveNext(out NextReader nextReader)
        {
            if (this.currentIndex + someBytes.Length > this.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.buffer.AsSpan(this.currentIndex, someBytes.Length).SequenceEqual(someBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new NextReader(this.stream, this.buffer, this.currentIndex + someBytes.Length);
            return true;
        }
    }

    public ref struct NextReader
    {
        private static readonly byte[] nextBytes = [(byte)'n', (byte)'e', (byte)'x', (byte)'t'];

        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int currentIndex;

        public NextReader(
            Stream stream,
            byte[] buffer,
            int currentIndex)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
        }

        public ReaderTask<NextReader> Read()
        {
            return new ReaderTask<NextReader>(
                this.stream,
                this.buffer,
                this.currentIndex,
                (stream, buffer, currentIndex) => new NextReader(stream, buffer, currentIndex),
                false);
        }

        public bool TryMoveNext(out ThatReader nextReader)
        {
            if (this.currentIndex + nextBytes.Length > this.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.buffer.AsSpan(this.currentIndex, nextBytes.Length).SequenceEqual(nextBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new ThatReader(this.stream, this.buffer, this.currentIndex + nextBytes.Length);
            return true;
        }
    }

    public ref struct ThatReader
    {
        private static readonly byte[] thatBytes = [(byte)'t', (byte)'h', (byte)'a', (byte)'t'];

        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int currentIndex;

        public ThatReader(
            Stream stream,
            byte[] buffer,
            int currentIndex)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
        }

        public ReaderTask<ThatReader> Read()
        {
            return new ReaderTask<ThatReader>(
                this.stream,
                this.buffer,
                this.currentIndex,
                (stream, buffer, currentIndex) => new ThatReader(stream, buffer, currentIndex),
                false);
        }

        public bool TryMoveNext(out Nothing nextReader)
        {
            if (this.currentIndex + thatBytes.Length > this.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.buffer.AsSpan(this.currentIndex, thatBytes.Length).SequenceEqual(thatBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new Nothing(); ////TODO do a "nothing reader" new ThatReader(this.stream, this.buffer, this.currentIndex + thatBytes.Length);
            return true;
        }
    }
}
