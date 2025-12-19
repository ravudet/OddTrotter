namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.CalendarV1.Tokenization.Json2;
    using OddTrotter.CalendarV1.Tokenization.Readers;

    [TestClass]
    public sealed class ReaderUnitTests
    {
        [TestMethod]
        public async Task V2Broad()
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
                var reader = new Json2.JsonReader(stream);
                await ReadToEnd(reader);
            }
        }

        private static async Task ReadToEnd(Json2.JsonReader reader)
        {
            var whitespaceReader = await reader.Move().ConfigureAwait(false);
            await ReadToEnd(
                whitespaceReader,
                async valueReader => await ReadToEnd(
                    valueReader,
                    async arrayReader => await ReadToEnd(
                        arrayReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async falseReader => await ReadToEnd(
                        falseReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async nullReader => await ReadToEnd(
                        nullReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async numberReader => await ReadToEnd(
                        numberReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async objectReader => await ReadToEnd(
                        objectReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async stringReader => await ReadToEnd(
                        stringReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask)),
                    async trueReader => await ReadToEnd(
                        trueReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            nothing => Task.CompletedTask))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.TrueReader<TNextReader> trueReader, Func<TNextReader, Task> readToEnd)
        {

        }

        private static async Task ReadToEnd<TNextReader>(Json2.StringReader<TNextReader> stringReader, Func<TNextReader, Task> readToEnd)
        {

        }

        private static async Task ReadToEnd<TNextReader>(Json2.ObjectReader<TNextReader> objectReader, Func<TNextReader, Task> readToEnd)
        {
            var objectStartReader = await objectReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                objectStartReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async memberReader => await ReadToEnd(
                        memberReader,
                        async subsequentMemberReader => await ReadToEnd(
                            subsequentMemberReader,
                            async whitespaceReader => await ReadToEnd(
                                whitespaceReader,
                                async objectEndReader => await ReadToEnd(
                                    objectEndReader,
                                    async nextReader => await readToEnd(
                                        nextReader)))))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ObjectEndReader<TNextReader> objectEndReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await objectStartReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.SubsequentMemberReader<TNextReader> subsequentMemberReader, Func<TNextReader, Task> readToEnd)
        {
            var commaReader = await subsequentMemberReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                commaReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async memberReader => await ReadToEnd(
                        memberReader,
                        async nextReader => await readToEnd(
                            nextReader))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.MemberReader<TNextReader> memberReader, Func<TNextReader, Task> readToEnd)
        {
            var stringReader = await memberReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                stringReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async colonReader => await ReadToEnd(
                        colonReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            async valueReader => await ReadToEnd(
                                valueReader,
                                async arrayReader => await ReadToEnd(
                                    arrayReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async falseReader => await ReadToEnd(
                                    falseReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async nullReader => await ReadToEnd(
                                    nullReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async numberReader => await ReadToEnd(
                                    numberReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async objectReader => await ReadToEnd(
                                    objectReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async stringReader => await ReadToEnd(
                                    stringReader,
                                    async nextReader => await readToEnd(
                                        nextReader)),
                                async trueReader => await ReadToEnd(
                                    trueReader,
                                    async nextReader => await readToEnd(
                                        nextReader)))))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ColonReader<TNextReader> colonReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await colonReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ObjectStartReader<TNextReader> objectStartReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await objectStartReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.NumberReader<TNextReader> numberReader, Func<TNextReader, Task> readToEnd)
        {
            var signReader = await numberReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                signReader,
                async intReader => await ReadToEnd(
                    intReader,
                    async fracReader => await ReadToEnd(
                        fracReader,
                        async expReader => await ReadToEnd(
                            expReader,
                            async nextReader => await readToEnd(
                                nextReader)))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ExpReader<TNextReader> expReader, Func<TNextReader, Task> readToEnd)
        {
            var eReader = await expReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                eReader,
                async expSignReader => await ReadToEnd(
                    expSignReader,
                    async digitsReader => await ReadToEnd(
                        digitsReader,
                        async nextReader => await readToEnd(
                            nextReader))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.DigitsReader<TNextReader> digitsReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await digitsReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ExpSignReader<TNextReader> expSignReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await expSignReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.EReader<TNextReader> eReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await eReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.FracReader<TNextReader> fracReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await fracReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.IntReader<TNextReader> intReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await intReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.SignReader<TNextReader> signReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await signReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.NullReader<TNextReader> nullReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await nullReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.FalseReader<TNextReader> falseReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await falseReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayReader<TNextReader> arrayReader, Func<TNextReader, Task> readToEnd)
        {
            var arrayStartReader = await arrayReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                arrayStartReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async arrayElementReader => await ReadToEnd(
                        arrayElementReader,
                        async subsequenetArrayElementReader => await ReadToEnd(
                            subsequenetArrayElementReader,
                            async whitespaceReader => await ReadToEnd(
                                whitespaceReader,
                                async arrayEndReader => await ReadToEnd(
                                    arrayEndReader,
                                    async nextReader => await readToEnd(
                                        nextReader)))))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayEndReader<TNextReader> arrayEndReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await arrayEndReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.SubsequentArrayElementReader<TNextReader> subsequentArrayElementReader, Func<TNextReader, Task> readToEnd)
        {
            var commaReader = await subsequentArrayElementReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                commaReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async arrayElementReader => await ReadToEnd(
                        arrayElementReader,
                        async nextReader => await readToEnd(nextReader))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.CommaReader<TNextReader> commaReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await commaReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayElementReader<TNextReader> arrayElementReader, Func<TNextReader, Task> readToEnd)
        {
            var valueReader = await arrayElementReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                valueReader,
                async arrayReader => await ReadToEnd(
                    arrayReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async falseReader => await ReadToEnd(
                    falseReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async nullReader => await ReadToEnd(
                    nullReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async numberReader => await ReadToEnd(
                    numberReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async objectReader => await ReadToEnd(
                    objectReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async stringReader => await ReadToEnd(
                    stringReader,
                    async nextReader => await readToEnd(
                        nextReader)),
                async trueReader => await ReadToEnd(
                    trueReader,
                    async nextReader => await readToEnd(
                        nextReader)));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayStartReader<TNextReader> arrayStartReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await arrayStartReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        private static async Task ReadToEnd<TNextReader>(
            Json2.ValueReader<TNextReader> valueReader,
            Func<Json2.ArrayReader<TNextReader>, Task> arrayReadToEnd,
            Func<Json2.FalseReader<TNextReader>, Task> falseReadToEnd,
            Func<Json2.NullReader<TNextReader>, Task> nullReadToEnd,
            Func<Json2.NumberReader<TNextReader>, Task> numberReadToEnd,
            Func<Json2.ObjectReader<TNextReader>, Task> objectReadToEnd,
            Func<Json2.StringReader<TNextReader>, Task> stringReadToEnd,
            Func<Json2.TrueReader<TNextReader>, Task> trueReadToEnd)
        {
            var valueToken = await valueReader.Move();
            if (valueToken is ValueToken<TNextReader>.Array array)
            {
                await arrayReadToEnd(array.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.False @false)
            {
                await falseReadToEnd(@false.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Null @null)
            {
                await nullReadToEnd(@null.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Number number)
            {
                await numberReadToEnd(number.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Object @object)
            {
                await objectReadToEnd(@object.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.String @string)
            {
                await stringReadToEnd(@string.Reader).ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.True @true)
            {
                await trueReadToEnd(@true.Reader).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("TODO you should have an `apply` method or something on `valuetoken<T>`");
            }
        }

        private static async Task ReadToEnd<TNextReader>(Json2.WhitespaceReader<TNextReader> whitespaceReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await whitespaceReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

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
            var members = new Members();
            var someReader = new SomeReader(ref members);
            await someReader.ReadToEnd();
        }

        public static async Task ReadToEnd(this SomeReader someReader)
        {
            var someReaderTask = someReader.MoveNext();
            if (!someReaderTask.TryGetValue(out var nextReader))
            {
                nextReader = await someReaderTask;
            }

            await nextReader.ReadToEnd();
        }

        public static async Task ReadToEnd(this NextReader nextReader)
        {
            var nextReaderTask = nextReader.MoveNext();
            if (nextReaderTask.TryGetValue(out var thatReader))
            {
                thatReader = await nextReaderTask;
            }

            await thatReader.ReadToEnd();
        }

        public static ReadToEndTask1 ReadToEnd(this ThatReader thatReader)
        {
            var thatReaderTask = thatReader.MoveNext();
            if (thatReaderTask.TryGetValue(out var nothing))
            {
                nothing = await Task.FromResult(new Nothing());
            }
        }

        public ref struct ReadToEndTask1
        {
            private readonly ReadToEndTask<ThatReader, Nothing> readerTask;

            public ReadToEndTask1(ReadToEndTask<ThatReader, Nothing> readerTask)
            {
                this.readerTask = readerTask;
            }

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

        public static ReadToEndTask<ThatReader, Nothing> MoveNext(this ThatReader reader)
        {
            if (reader.TryMoveNext(out var nothing))
            {
                return new ReadToEndTask<ThatReader, Nothing>(nothing);
            }
            else
            {
                return new ReadToEndTask<ThatReader, Nothing>(reader.Read(), old => old.TryMoveNext(out var next) ? next : throw new Exception("TODO in this iteration, this is a bug, because read should always give us enough information to do a proper move in the subsequent call"));
            }
        }

        public static ReadToEndTask<NextReader, ThatReader> MoveNext(this NextReader reader)
        {
            if (reader.TryMoveNext(out var that))
            {
                return new ReadToEndTask<NextReader, ThatReader>(that);
            }
            else
            {
                return new ReadToEndTask<NextReader, ThatReader>(reader.Read(), old => old.TryMoveNext(out var next) ? next : throw new Exception("TODO in this iteration, this is a bug, because read should always give us enough information to do a proper move in the subsequent call"));
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

        public interface ITaskAwaiter : ICriticalNotifyCompletion
        {
            bool IsCompleted { get; }

            void GetResult();
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

            public struct Awaiter : ICriticalNotifyCompletion, ITaskAwaiter
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

                void ITaskAwaiter.GetResult()
                {
                    this.GetResult();
                }
            }

        }
    }

    public readonly struct Members
    {
        public Members(
            Stream stream,
            byte[] buffer,
            int currentIndex)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.currentIndex = currentIndex;
        }

        //// TODO fix the casing on these property names
        public Stream stream { get; }
        public byte[] buffer { get; }
        public int currentIndex { get; }
    }

    public ref struct ReaderTask<T>
        where T : allows ref struct
    {
        private readonly Members members;
        private readonly Func<Members, T> factory;
        private readonly bool doNothing;

        public ReaderTask(
            Members members,
            Func<Members, T> factory,
            bool doNothing)
        {
            //// TODO use `ref members` instead
            this.members = members;
            this.factory = factory;
            this.doNothing = doNothing;
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private Members members;
            private readonly Func<Members, T> factory;

            private TaskAwaiter<int> taskAwaiter;

            public Awaiter(
                Members members,
                Func<Members, T> factory)
            {
                this.members = members;
                this.factory = factory;

                this.taskAwaiter = this.members.stream.ReadAsync(this.members.buffer, 0, this.members.buffer.Length).GetAwaiter();
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
                return this.factory(this.members);
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
                this.members,
                this.factory);
        }
    }

    public ref struct SomeReader
    {
        private static readonly byte[] someBytes = [(byte)'s', (byte)'o', (byte)'m', (byte)'e'];

        public readonly Members members;

        public SomeReader(Members members)
        {
            this.members = members;
        }

        public ReaderTask<SomeReader> Read()
        {
            return new ReaderTask<SomeReader>(
                this.members,
                members => new SomeReader(members),
                false);
        }

        public bool TryMoveNext(out NextReader nextReader)
        {
            if (this.members.currentIndex + someBytes.Length > this.members.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.members.buffer.AsSpan(this.members.currentIndex, someBytes.Length).SequenceEqual(someBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new NextReader(new Members(this.members.stream, this.members.buffer, this.members.currentIndex + someBytes.Length)); //// TODO can you have a `members` constructor that takes a `ref Members` parameter and the new index?
            return true;
        }
    }

    public ref struct NextReader
    {
        private static readonly byte[] nextBytes = [(byte)'n', (byte)'e', (byte)'x', (byte)'t'];

        public readonly Members members;

        public NextReader(Members members)
        {
            this.members = members;
        }

        public ReaderTask<NextReader> Read()
        {
            return new ReaderTask<NextReader>(
                this.members,
                members => new NextReader(members),
                false);
        }

        public bool TryMoveNext(out ThatReader nextReader)
        {
            if (this.members.currentIndex + nextBytes.Length > this.members.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.members.buffer.AsSpan(this.members.currentIndex, nextBytes.Length).SequenceEqual(nextBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new ThatReader(this.members);
            return true;
        }
    }

    public ref struct ThatReader
    {
        private static readonly byte[] thatBytes = [(byte)'t', (byte)'h', (byte)'a', (byte)'t'];

        public readonly Members members;

        public ThatReader(Members members)
        {
            this.members = members;
        }

        public ReaderTask<ThatReader> Read()
        {
            return new ReaderTask<ThatReader>(
                this.members,
                members => new ThatReader(members),
                false);
        }

        public bool TryMoveNext(out Nothing nextReader)
        {
            if (this.members.currentIndex + thatBytes.Length > this.members.buffer.Length)
            {
                nextReader = default;
                return false;
            }

            if (!this.members.buffer.AsSpan(this.members.currentIndex, thatBytes.Length).SequenceEqual(thatBytes))
            {
                throw new Exception("TODO invalid payload");
            }

            nextReader = new Nothing(); ////TODO do a "nothing reader" new ThatReader(this.stream, this.buffer, this.currentIndex + thatBytes.Length);
            return true;
        }
    }
}
