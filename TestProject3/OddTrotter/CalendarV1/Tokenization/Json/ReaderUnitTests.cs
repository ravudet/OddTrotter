namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.CalendarV1.Tokenization.Json2;
    using OddTrotter.CalendarV1.Tokenization.Readers;

    public static class ReaderExtensions
    {
        private static async Task ReadToEnd4<TNextReader>(this Json2.IReader<TNextReader> currentReader, Func<TNextReader, Task> readToEnd)
        {
            var nextReader = await currentReader.Move().ConfigureAwait(false);
            await readToEnd(nextReader).ConfigureAwait(false);
        }

        public static async Task ReadToEnd(this Json2.JsonReader reader)
        {
            var whitespaceReader = await reader.Move().ConfigureAwait(false);
            await whitespaceReader.ReadToEnd4(
                async valueReader => await valueReader.ReadToEnd2(
                    async whitespaceReader => await whitespaceReader.ReadToEnd4(
                        nothing => Task.CompletedTask)));
        }

        public static async Task ReadToEnd3<TNextReader>(
            this Json2.ArrayElementsReader<TNextReader> arrayElementsReader,
            Func<TNextReader, Task> noneReadToEnd,
            Func<Json2.ArrayElementReader<SubsequentArrayElementsReader<TNextReader>>, Task> someReadToEnd)
        {
            var arrayElementsToken = await arrayElementsReader.Move();
            if (arrayElementsToken is ArrayElementsToken<TNextReader>.None none)
            {
                await noneReadToEnd(none.Reader);
            }
            else if (arrayElementsToken is ArrayElementsToken<TNextReader>.Some some)
            {
                await someReadToEnd(some.Reader);
            }
            else
            {
                throw new Exception("TODO implement apply");
            }
        }

        public static async Task ReadToEnd5<TNextReader>(
            this Json2.SubsequentArrayElementsReader<TNextReader> subsequentArrayElementsReader,
            Func<TNextReader, Task> noneReadToEnd,
            Func<Json2.SubsequentArrayElementReader<SubsequentArrayElementsReader<TNextReader>>, Task> moreReadToEnd)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move();
            if (subsequentArrayElementsToken is SubsequentArrayElementsToken<TNextReader>.None none)
            {
                await noneReadToEnd(none.Reader);
            }
            else if (subsequentArrayElementsToken is SubsequentArrayElementsToken<TNextReader>.More more)
            {
                await moreReadToEnd(more.Reader);
            }
            else
            {
                throw new Exception("TODO implement apply");
            }
        }

        public static async Task ReadToEnd6<TNextReader>(
            this Json2.SubsequentArrayElementsReader<TNextReader> subsequentArrayElementsReader,
            Func<TNextReader, Task> readToEnd)
        {
            await subsequentArrayElementsReader.ReadToEnd5(
                async nextReader => await readToEnd(nextReader),
                async subsequentArrayElementReader => await subsequentArrayElementReader.ReadToEnd4(
                    async commaReader => await commaReader.ReadToEnd4(
                        async whitespaceReader => await whitespaceReader.ReadToEnd4(
                            async arrayElementReader => await arrayElementReader.ReadToEnd4(
                                async valueReader => await valueReader.ReadToEnd2(
                                    async subsequentArrayElementsReader => await subsequentArrayElementsReader.ReadToEnd6(readToEnd)))))));
        }

        public static async Task ReadToEnd7<TNextReader>(
            this Json2.ExpReader<TNextReader> expReader,
            Func<TNextReader, Task> absentReadToEnd,
            Func<Json2.EReader<ExpSignReader<DigitsReader<TNextReader>>>, Task> presentReadToEnd)
        {
            var expToken = await expReader.Move();
            if (expToken is ExpToken<TNextReader>.Absent absent)
            {
                await absentReadToEnd(absent.Reader);
            }
            else if (expToken is ExpToken<TNextReader>.Present present)
            {
                await presentReadToEnd(present.Reader);
            }
            else
            {
                throw new Exception("TODO implement apply");
            }
        }

        public static async Task ReadToEnd8<TNextReader>(
            this Json2.MembersReader<TNextReader> membersReader,
            Func<TNextReader, Task> noneReadToEnd,
            Func<Json2.FirstMemberReader<TNextReader>, Task> someReadToEnd)
        {
            var membersToken = await membersReader.Move();
            if (membersToken is MembersToken<TNextReader>.None none)
            {
                await noneReadToEnd(none.Reader);
            }
            else if (membersToken is MembersToken<TNextReader>.Some some)
            {
                await someReadToEnd(some.Reader);
            }
            else
            {
                throw new Exception("TODO implement apply");
            }
        }

        public static async Task ReadToEnd9<TNextReader>(
            this Json2.SubsequentMembersReader<TNextReader> subsequentMembersReader,
            Func<TNextReader, Task> noneReadToEnd,
            Func<Json2.SubsequentMemberReader<SubsequentMembersReader<TNextReader>>, Task> moreReadToEnd)
        {
            var subsequentMembersToken = await subsequentMembersReader.Move();
            if (subsequentMembersToken is SubsequentMembersToken<TNextReader>.None none)
            {
                await noneReadToEnd(none.Reader);
            }
            else if (subsequentMembersToken is SubsequentMembersToken<TNextReader>.More more)
            {
                await moreReadToEnd(more.Reader);
            }
            else
            {
                throw new Exception("TODO implement apply");
            }
        }

        public static async Task ReadToEnd11<TNextReader>(
            this Json2.SubsequentMembersReader<TNextReader> subsequentMembersReader,
            Func<TNextReader, Task> readToEnd)
        {
            await subsequentMembersReader.ReadToEnd9(
                async whitespaceReader => await readToEnd(whitespaceReader),
                async subsequentMemberReader => await subsequentMemberReader.ReadToEnd4(
                    async commaReader => await commaReader.ReadToEnd4(
                        async whitespaceReader => await whitespaceReader.ReadToEnd4(
                            async memberReader => await memberReader.ReadToEnd10(
                                async subsequentMembersReader => await subsequentMembersReader.ReadToEnd11(
                                    async nextReader => await readToEnd(nextReader)))))));
        }

        public static async Task ReadToEnd10<TNextReader>(
            this Json2.MemberReader<TNextReader> memberReader,
            Func<TNextReader, Task> readToEnd)
        {
            await memberReader.ReadToEnd4(
                async stringReader => await stringReader.ReadToEnd4(
                    async stringDelimiterReader => await stringDelimiterReader.ReadToEnd4(
                        async charsReader => await charsReader.ReadToEnd4(
                            async stringDelimiterReader => await stringDelimiterReader.ReadToEnd4(
                                async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                    async colonReader => await colonReader.ReadToEnd4(
                                        async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                            async valueReader => await valueReader.ReadToEnd2(
                                                async nextReader => await readToEnd(nextReader))))))))));
        }

        public static async Task ReadToEnd2<TNextReader>(
            this Json2.ValueReader<TNextReader> valueReader,
            Func<TNextReader, Task> readToEnd)
        {
            await valueReader.ReadToEnd1(
                    async arrayReader => await arrayReader.ReadToEnd4(
                        async arrayStartReader => await arrayStartReader.ReadToEnd4(
                            async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                async arrayElementsReader => await arrayElementsReader.ReadToEnd3(
                                    async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                        async arrayEndReader => await arrayEndReader.ReadToEnd4(
                                            async nextReader => await readToEnd(nextReader))),
                                    async arrayElementReader => await arrayElementReader.ReadToEnd4(
                                        async valueReader => await valueReader.ReadToEnd2(
                                            async subsequentArrayElementsReader => await subsequentArrayElementsReader.ReadToEnd6(
                                                async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                                    async arrayEndReader => await arrayEndReader.ReadToEnd4(
                                                        async nextReader => await readToEnd(nextReader)))))))))),
                    async falseReader => await falseReader.ReadToEnd4(
                        async nextReader => await readToEnd(nextReader)),
                    async nullReader => await nullReader.ReadToEnd4(
                        async nextReader => await readToEnd(nextReader)),
                    async numberReader => await numberReader.ReadToEnd4(
                        async signReader => await signReader.ReadToEnd4(
                            async intReader => await intReader.ReadToEnd4(
                                async fracReader => await fracReader.ReadToEnd4(
                                    async expReader => await expReader.ReadToEnd7(
                                        async nextReader => await readToEnd(nextReader),
                                        async eReader => await eReader.ReadToEnd4(
                                            async expSignReader => await expSignReader.ReadToEnd4(
                                                async digitsReader => await digitsReader.ReadToEnd4(
                                                    async nextReader => await readToEnd(nextReader))))))))),
                    async objectReader => await objectReader.ReadToEnd4(
                        async objectStartReader => await objectStartReader.ReadToEnd4(
                            async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                async membersReader => await membersReader.ReadToEnd8(
                                    async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                        async objectEndReader => await objectEndReader.ReadToEnd4(
                                            async nextReader => await readToEnd(nextReader))),
                                    async firstMemberReader => await firstMemberReader.ReadToEnd4(
                                        async memberReader => await memberReader.ReadToEnd10(
                                            async subsequentMembersReader => await subsequentMembersReader.ReadToEnd11(
                                                async whitespaceReader => await whitespaceReader.ReadToEnd4(
                                                    async objectEndReader => await objectEndReader.ReadToEnd4(
                                                        async nextReader => await readToEnd(nextReader)))))))))),
                    async stringReader => await stringReader.ReadToEnd4(
                        async stringDelimiterReader => await stringDelimiterReader.ReadToEnd4(
                            async charsReader => await charsReader.ReadToEnd4(
                                async stringDelimiterReader => await stringDelimiterReader.ReadToEnd4(
                                    async nextReader => await readToEnd(nextReader))))),
                    async trueReader => await trueReader.ReadToEnd4(
                        async nextReader => await readToEnd(nextReader)));
        }

        public static async Task ReadToEnd1<TNextReader>(
            this Json2.ValueReader<TNextReader> valueReader,
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

    }

    [TestClass]
    public sealed class ReaderUnitTests
    {
        [TestMethod]
        public async Task V2Broad()
        {
            //// TODO `move` implementations should also be single-execution
            
            //// TODO implement buffers for readers
            //// TODO change reader interface so that async is only used when the buffer is expanded
            //// TODO change reader to use ref struct somehow (maybe there's a step between `trymove` and `ref struct` that is just `struct`

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
    "emptyObject": {},
    "emptyArray": [],
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
                await reader.ReadToEnd().ConfigureAwait(false);
            }
        }

        

        /*private static async Task ReadToEnd<TNextReader>(Json2.StringReader<TNextReader> stringReader, Func<TNextReader, Task> readToEnd)
        {
            var stringDelimiterReader = await stringReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                stringDelimiterReader,
                async charsReader => await ReadToEnd(
                    charsReader,
                    async stringDelimiterReader => await ReadToEnd(
                        stringDelimiterReader,
                        async nextReader => await readToEnd(
                            nextReader))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ObjectReader<TNextReader> objectReader, Func<TNextReader, Task> readToEnd)
        {
            var objectStartReader = await objectReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                objectStartReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async membersReader => await ReadToEnd(
                        membersReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            async objectEndReader => await ReadToEnd(
                                objectEndReader,
                                async nextReader => await readToEnd(
                                    nextReader))))));
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

        private static async Task ReadToEnd<TNextReader>(Json2.MembersReader<TNextReader> membersReader, Func<TNextReader, Task> readToEnd)
        {
            var membersToken = await membersReader.Move().ConfigureAwait(false);
            if (membersToken is MembersToken<TNextReader>.None none)
            {
                await readToEnd(none.Reader);
            }
            else if (membersToken is MembersToken<TNextReader>.Some some)
            {
                await ReadToEnd(
                    some.Reader,
                    async nextReader => await readToEnd(
                        nextReader));
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        private static async Task ReadToEnd<TNextReader>(Json2.FirstMemberReader<TNextReader> firstMemberReader, Func<TNextReader, Task> readToEnd)
        {
            var memberReader = await firstMemberReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                memberReader,
                async subsequentMembersReader => await ReadToEnd(
                    subsequentMembersReader,
                    async nextReader => await readToEnd(
                        nextReader)));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.SubsequentMembersReader<TNextReader> subsequentMembersReader, Func<TNextReader, Task> readToEnd)
        {
            var subsequentMembersToken = await subsequentMembersReader.Move().ConfigureAwait(false);
            if (subsequentMembersToken is SubsequentMembersToken<TNextReader>.None none)
            {
                await readToEnd(none.Reader);
            }
            else if (subsequentMembersToken is SubsequentMembersToken<TNextReader>.More more)
            {
                await ReadToEnd(
                    more.Reader,
                    async subsequentMemberReader => await ReadToEnd(
                        subsequentMemberReader,
                        async subsequentMembersReader => await ReadToEnd(
                            subsequentMemberReader,
                            async nextReader => await readToEnd(
                                nextReader))));
            }
            else
            {
                throw new Exception("TODO visitor");
            }
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
            var expToken = await expReader.Move().ConfigureAwait(false);
            if (expToken is ExpToken<TNextReader>.Absent absent)
            {
                await readToEnd(absent.Reader);
            }
            else if (expToken is ExpToken<TNextReader>.Present present)
            {
                await ReadToEnd(
                    present.Reader,
                    async expSignReader => await ReadToEnd(
                        expSignReader,
                        async digitsReader => await ReadToEnd(
                            digitsReader,
                            async nextReader => await readToEnd(
                                nextReader))));
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayReader<TNextReader> arrayReader, Func<TNextReader, Task> readToEnd)
        {
            var arrayStartReader = await arrayReader.Move().ConfigureAwait(false);
            await ReadToEnd(
                arrayStartReader,
                async whitespaceReader => await ReadToEnd(
                    whitespaceReader,
                    async arrayElementsReader => await ReadToEnd(
                        arrayElementsReader,
                        async whitespaceReader => await ReadToEnd(
                            whitespaceReader,
                            async arrayEndReader => await ReadToEnd(
                                arrayEndReader,
                                async nextReader => await readToEnd(
                                    nextReader))))));
        }

        private static async Task ReadToEnd<TNextReader>(Json2.ArrayElementsReader<TNextReader> arrayElementsReader, Func<TNextReader, Task> readToEnd)
        {
            var arrayElementsToken = await arrayElementsReader.Move().ConfigureAwait(false);
            if (arrayElementsToken is ArrayElementsToken<TNextReader>.None none)
            {
                await readToEnd(none.Reader).ConfigureAwait(false);
            }
            else if (arrayElementsToken is ArrayElementsToken<TNextReader>.Some some)
            {
                await ReadToEnd(
                    some.Reader,
                    async subsequentArrayElementsReader => await ReadToEnd(
                        subsequentArrayElementsReader,
                        async nextReader => await readToEnd(
                            nextReader)));
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        private static async Task ReadToEnd<TNextReader>(Json2.SubsequentArrayElementsReader<TNextReader> subsequentArrayElementsReader, Func<TNextReader, Task> readToEnd)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move().ConfigureAwait(false);
            if (subsequentArrayElementsToken is SubsequentArrayElementsToken<TNextReader>.None none)
            {
                await readToEnd(none.Reader);
            }
            else if (subsequentArrayElementsToken is SubsequentArrayElementsToken<TNextReader>.More more)
            {
                await ReadToEnd(
                    more.Reader,
                    async subsequentArrayElementsReader => await ReadToEnd(
                        subsequentArrayElementsReader,
                        async nextReader => await readToEnd(
                            nextReader)));
            }
            else
            {
                throw new Exception("TODO visitor");
            }
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
        }*/

        /*[TestMethod]
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
        }*/
    }
}
