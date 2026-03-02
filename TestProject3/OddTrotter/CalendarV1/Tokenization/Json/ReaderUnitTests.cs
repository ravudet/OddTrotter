namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Data;
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
        private static bool TryMove2<TNextReader>(this Json2.IReader<TNextReader> currentReader, out TNextReader nextReader)
            where TNextReader : allows ref struct
        {
            if (currentReader == null)
            {
                throw new Exception("TODO");
            }

            nextReader = currentReader.TryMove(out var read);
            return read;
        }

        private static bool TryMove2<TCurrentReader, TNextReader>(this TypeHolder<TCurrentReader, TNextReader> currentReader, out TNextReader nextReader)
            where TCurrentReader : Json2.IReader<TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            nextReader = currentReader.Self.TryMove(out var read);
            return read;
        }

        private static TaskWrapper<T> ToTaskWrapper<T>(this Task<T> task)
        {
            return new TaskWrapper<T>(task);
        }

        private static TaskWrapper<Nothing> ToTaskWrapper(this Task task)
        {
            return task.ContinueWith(_ => new Nothing()).ToTaskWrapper();
        }


        private static async ITask<TNextReader> Move<TNextReader>(this ITask<Json2.IReader<TNextReader>> currentReader)
        {
            return await (await currentReader.ConfigureAwait(false)).Move1().ConfigureAwait(false);
        }

        private static ITask<TResult> FromResult<TContext, TResult>(TContext context, Func<TContext, TResult> factory)
            where TResult : allows ref struct
        {
            return new FromResultTask<TContext, TResult>(context, factory);
        }

        private sealed class FromResultTask<TContext, TResult> : ITask<TResult>
            where TResult : allows ref struct
        {
            private readonly TContext context;
            private readonly Func<TContext, TResult> factory;

            public FromResultTask(TContext context, Func<TContext, TResult> factory)
            {
                this.context = context;
                this.factory = factory;
            }

            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.context, this.factory);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TResult>
            {
                private readonly TContext context;
                private readonly Func<TContext, TResult> factory;

                public ConfiguredAwaitable(TContext context, Func<TContext, TResult> factory)
                {
                    this.context = context;
                    this.factory = factory;
                }

                public ITaskAwaiter<TResult> GetAwaiter()
                {
                    return new TaskAwaiter(this.context, this.factory);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<TResult>
                {
                    private readonly TContext context;
                    private readonly Func<TContext, TResult> factory;

                    public TaskAwaiter(TContext context, Func<TContext, TResult> factory)
                    {
                        this.context = context;
                        this.factory = factory;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return true;
                        }
                    }

                    public TResult GetResult()
                    {
                        return this.factory(this.context);
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
            }

            public ITaskAwaiter<TResult> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        private static ITask<TNextReader> Move2<TCurrentReader, TNextReader>(this TypeHolder<TCurrentReader, TNextReader> currentReader)
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            var self = currentReader.Self;
            if (self.TryMove3(out var nextReaderFactory))
            {
                return FromResult(currentReader.Self.Context, nextReaderFactory);
            }
            else
            {
                return new Move2Task<TCurrentReader, TNextReader>(self.Read().ToTaskWrapper(), self.Context);
            }
        }

        private sealed class Move2Task<TCurrentReader, TNextReader> : ITask<TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly ITask<Nothing> task;
            private readonly ReaderContext readerContext;

            public Move2Task(ITask<Nothing> task, ReaderContext readerContext)
            {
                this.task = task;
                this.readerContext = readerContext;
            }

            public IConfiguredAwaitable<TNextReader> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.task, this.readerContext, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TNextReader>
            {
                private ITask<Nothing> task;
                private readonly ReaderContext readerContext;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(
                    ITask<Nothing> task,
                    ReaderContext readerContext,
                    bool continueOnCapturedContext)
                {
                    this.task = task;
                    this.readerContext = readerContext;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public ITaskAwaiter<TNextReader> GetAwaiter()
                {
                    return new TaskAwaiter(this.task, this.readerContext, this.continueOnCapturedContext); 
                }

                private sealed class TaskAwaiter : ITaskAwaiter<TNextReader>
                {
                    private ITaskAwaiter<Nothing> task;
                    private readonly ReaderContext readerContext;
                    private readonly bool continueOnCapturedContext;

                    public TaskAwaiter(
                        ITask<Nothing> task,
                        ReaderContext readerContext,
                        bool continueOnCapturedContext)
                    {
                        this.task = task.ConfigureAwait(continueOnCapturedContext).GetAwaiter();
                        this.readerContext = readerContext;
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }


                    public bool IsCompleted
                    {
                        get
                        {
                            if (this.task.IsCompleted)
                            {
                                var currentReader = TCurrentReader.Factory(this.readerContext);
                                if (currentReader.TryMove3(out _))
                                {
                                    return true;
                                }
                                else
                                {
                                    this.task = currentReader.Read().ToTaskWrapper().ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                    return this.task.IsCompleted;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    public TNextReader GetResult()
                    {
                        TCurrentReader.Factory(this.readerContext).TryMove3(out var nextReaderFactory);
                        return nextReaderFactory(this.readerContext);
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
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        private static ITask<TNextReader> Move1<TNextReader>(this Json2.IReader<TNextReader> currentReader)
            where TNextReader : allows ref struct
        {
            if (currentReader == null)
            {
                throw new Exception("TODO");
            }

            /*TNextReader nextReader;
            while (!currentReader.TryMove2(out nextReader))
            {
                await currentReader.Read().ConfigureAwait(false);
            }

            return nextReader;*/

            return new Move1Task<TNextReader>(currentReader);
        }

        private sealed class Move1Task<TNextReader> : ITask<TNextReader>
            where TNextReader : allows ref struct
        {
            private readonly Json2.IReader<TNextReader> currentReader;

            public Move1Task(Json2.IReader<TNextReader> currentReader)
            {
                if (currentReader == null)
                {
                    throw new Exception("TODO");
                }

                this.currentReader = currentReader;
            }

            public IConfiguredAwaitable<TNextReader> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.currentReader, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TNextReader>
            {
                private readonly Json2.IReader<TNextReader> currentReader;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(Json2.IReader<TNextReader> currentReader, bool continueOnCapturedContext)
                {
                    this.currentReader = currentReader;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public ITaskAwaiter<TNextReader> GetAwaiter()
                {
                    return new TaskAwaiter(this.currentReader, this.continueOnCapturedContext);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly Json2.IReader<TNextReader> currentReader;
                    private readonly bool continueOnCapturedContext;

                    private System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter? task;

                    public TaskAwaiter(Json2.IReader<TNextReader> currentReader, bool continueOnCapturedContext)
                    {
                        if (currentReader == null)
                        {
                            throw new Exception("TODO");
                        }

                        this.currentReader = currentReader;
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (this.task != null)
                            {
                                if (!this.task.Value.IsCompleted)
                                {
                                    return false;
                                }
                                else
                                {
                                    if (this.currentReader.TryMove2(out _)) //// TODO you are making the *terrible* assumption that `trymove` is idempotent
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        this.task = this.currentReader.Read().ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                        return this.task.Value.IsCompleted;
                                    }
                                }
                            }
                            else
                            {
                                if (this.currentReader.TryMove2(out _)) //// TODO you are making the *terrible* assumption that `trymove` is idempotent
                                {
                                    return true;
                                }
                                else
                                {
                                    this.task = this.currentReader.Read().ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                    return this.task.Value.IsCompleted;
                                }
                            }
                        }
                    }

                    public TNextReader GetResult()
                    {
                        var result = this.currentReader.TryMove(out _);
                        if (result == null)
                        {
                        }

                        return result;
                    }

                    public void OnCompleted(Action continuation)
                    {
                        throw new Exception("TODO");
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        throw new Exception("TODO");
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                return new TaskAwaiter(this.currentReader);
            }

            private sealed class TaskAwaiter : ITaskAwaiter<TNextReader>
            {
                private readonly Json2.IReader<TNextReader> currentReader;

                private System.Runtime.CompilerServices.TaskAwaiter? task;

                public TaskAwaiter(Json2.IReader<TNextReader> currentReader)
                {
                    this.currentReader = currentReader;
                }

                public bool IsCompleted
                {
                    get
                    {
                        if (this.task != null)
                        {
                            if (!this.task.Value.IsCompleted)
                            {
                                return false;
                            }
                            else
                            {
                                if (this.currentReader.TryMove2(out _))
                                {
                                    return true;
                                }
                                else
                                {
                                    this.task = this.currentReader.Read().GetAwaiter();
                                    return this.task.Value.IsCompleted;
                                }
                            }
                        }
                        else
                        {
                            if (this.currentReader.TryMove2(out _))
                            {
                                return true;
                            }
                            else
                            {
                                this.task = this.currentReader.Read().GetAwaiter();
                                return this.task.Value.IsCompleted;
                            }
                        }
                    }
                }

                public TNextReader GetResult()
                {
                    var result = this.currentReader.TryMove(out _);
                    if (result == null)
                    {
                    }

                    return result;
                }

                public void OnCompleted(Action continuation)
                {
                    throw new Exception("TODO");
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    throw new Exception("TODO");
                }
            }
        }

        public static async ITask<TNextReader> Move<TNextReader>(this ITask<Json2.IReader<ValueToken<TNextReader>>> valueReader)
        {
            return await (await valueReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(this Json2.IReader<ValueToken<TNextReader>> valueReader)
        {
            var valueToken = await valueReader.Move1<ValueToken<TNextReader>>().ConfigureAwait(false);

            if (valueToken is ValueToken<TNextReader>.Array array)
            {
                return await array.Reader.Move1().Move().Move().Move().Move().Move().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.False @false)
            {
                return await @false.Reader.Move1().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Null @null)
            {
                return await @null.Reader.Move1().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Number number)
            {
                return await number.Reader.Move1().Move().Move().Move().Move().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.Object @object)
            {
                return await @object.Reader.Move1().Move().Move().Move().Move().Move().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.String @string)
            {
                return await @string.Reader.Move1().Move().Move().Move().ConfigureAwait(false);
            }
            else if (valueToken is ValueToken<TNextReader>.True @true)
            {
                return await @true.Reader.Move1().ConfigureAwait(false);
            }
            else
            {
                throw new Exception("TODO you should have an `apply` method or something on `valuetoken<T>`");
            }
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this ITask<Json2.IReader<MembersToken<TNextReader>>> subsequentArrayElementsReader)
        {
            return await (await subsequentArrayElementsReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this Json2.IReader<MembersToken<TNextReader>> subsequentArrayElementsReader)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move1<MembersToken<TNextReader>>().ConfigureAwait(false);

            return await subsequentArrayElementsToken.Apply(
                none => Task.FromResult(none).ToTaskWrapper(),
                some => some.Move1().Move().Move().Move().Move().Move().Move().Move().Move().Move().Move()).ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this ITask<Json2.IReader<SubsequentMembersToken<TNextReader>>> subsequentArrayElementsReader)
        {
            return await (await subsequentArrayElementsReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this Json2.IReader<SubsequentMembersToken<TNextReader>> subsequentArrayElementsReader)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move1<SubsequentMembersToken<TNextReader>>().ConfigureAwait(false);

            return await subsequentArrayElementsToken.Apply(
                none => Task.FromResult(none).ToTaskWrapper(),
                more => more.Move1().Move().Move().Move().Move().Move().Move().Move().Move().Move().Move().Move().Move()).ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this ITask<Json2.IReader<ExpToken<TNextReader>>> subsequentArrayElementsReader)
        {
            return await (await subsequentArrayElementsReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this Json2.IReader<ExpToken<TNextReader>> subsequentArrayElementsReader)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move1<ExpToken<TNextReader>>().ConfigureAwait(false);

            return await subsequentArrayElementsToken.Apply(
                absent => Task.FromResult(absent).ToTaskWrapper(),
                present => present.Move1().Move().Move()).ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this ITask<Json2.IReader<ArrayElementsToken<TNextReader>>> arrayElementsReader)
        {
            return await (await arrayElementsReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this Json2.IReader<ArrayElementsToken<TNextReader>> arrayElementsReader)
        {
            var arrayElementsToken = await arrayElementsReader.Move1<ArrayElementsToken<TNextReader>>().ConfigureAwait(false);

            return await arrayElementsToken.Apply(
                none => Task.FromResult(none).ToTaskWrapper(),
                some => some.Move1().Move().Move()).ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this ITask<Json2.IReader<SubsequentArrayElementsToken<TNextReader>>> subsequentArrayElementsReader)
        {
            return await (await subsequentArrayElementsReader.ConfigureAwait(false)).Move().ConfigureAwait(false);
        }

        public static async ITask<TNextReader> Move<TNextReader>(
            this Json2.IReader<SubsequentArrayElementsToken<TNextReader>> subsequentArrayElementsReader)
        {
            var subsequentArrayElementsToken = await subsequentArrayElementsReader.Move1<SubsequentArrayElementsToken<TNextReader>>().ConfigureAwait(false);

            return await subsequentArrayElementsToken.Apply(
                none => Task.FromResult(none).ToTaskWrapper(),
                more => more.Move1().Move().Move().Move().Move().Move()).ConfigureAwait(false);
        }

        private static Json2.IReader<TNextReader> AsReader<TNextReader>(this Json2.IReader<TNextReader> reader)
            where TNextReader : allows ref struct
        {
            return reader;
        }

        public static ITask<Nothing> Move(this Json2.JsonReader reader)
        {
            return reader.AsReader.Move2().Move().Move().Move();
        }
    }

    internal static class TestExtensions
    {
        internal static async Task<TNextReader> MoveInternal1<TNextReader>(this Json2.IReader<TNextReader> currentReader)
        {
            var nextReader = currentReader.TryMove(out var read);
            if (!read) //// TODO this is supposed to be a while loop
            {
                await currentReader.Read().ConfigureAwait(false);
                nextReader = currentReader.TryMove(out read);
            }

            return nextReader;
        }

        internal static Task<Func<ReaderContext, TNextReader>> MoveInternal2<TCurrentReader, TNextReader>(this TypeHolder<TCurrentReader, TNextReader> currentReader)
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            var self = currentReader.Self;
            var context = self.Context;
            if (!self.TryMove3(out var nextFactory)) //// TODO this is supposed to be a while loop
            {
                return self.Read().ContinueWith(
                    (_, context) =>
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                        var self = TCurrentReader.Factory((ReaderContext)context);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        self.TryMove3(out var nextFactory);
                        return nextFactory;
                    },
                    context);
            }

            return Task.FromResult(nextFactory);
        }
    }

    [TestClass]
    public sealed class ReaderUnitTests
    {
        [TestMethod]
        public async Task V2Broad()
        {
            //// TODO `move` implementations should also be single-execution
            //// TODO they shouldn't be allowed to call `read` unless `false` was previously returned
            //// TODO the `trygetvalue` implementations need to follow the whitespace pattern of `finished`


            //// TODO create a readtoend implementation that is specific to this test so that it uses the least amount of calls and initializations
            //// TODO "unit" readers like `objectreader` should have `trygetvalue` which returns the "known reader" chain, and then `trymove` *only* returns the "next reader"
            //// TODO change reader to use ref struct somehow (maybe there's a step between `trymove` and `ref struct` that is just `struct`
            //// TODO make sure the check the perf after the fact; it should get better with the ref structs, right?
            //// TODO commit 90833a15d48301f6528ef39209466bd0ff99a010 running `v2broad` in release mode throws an `invalidprogramexception`
            //// TODO all of these `itask` implementations can't be object allocations or it defeats the purpose

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
                await reader.Move().ConfigureAwait(false);

                stream.Position = 0;
                reader = new Json2.JsonReader(stream);

                var context = reader.Context;
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2().ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory(context);
                var valueReader = await whitespaceReader.MoveInternal1().ConfigureAwait(false);
                var valueToken = await valueReader.MoveInternal1().ConfigureAwait(false);

                if (!(valueToken is ValueToken<WhitespaceReader<Nothing>>.Object @object))
                {
                    throw new Exception("TODO");
                }

                var objectReader = await @object.Reader.MoveInternal1().ConfigureAwait(false);
                var whitespaceReader2 = await objectReader.MoveInternal1().ConfigureAwait(false);
                var membersReader = await whitespaceReader2.MoveInternal1().ConfigureAwait(false);

                var membersToken = membersReader.TryMove(out var read);
                if (!read)
                {
                    await membersReader.Read().ConfigureAwait(false);
                    membersToken = membersReader.TryMove(out read);
                }

                var firstMemberReader = membersToken.Apply(
                    _ => throw new Exception("TODO"),
                    some => some);

                // true
                var memberReader = await firstMemberReader.MoveInternal1().ConfigureAwait(false);
                var stringReader = await memberReader.MoveInternal1().ConfigureAwait(false);
                var stringDelimiterReader = await stringReader.MoveInternal1().ConfigureAwait(false);
                var charsReader = await stringDelimiterReader.MoveInternal1().ConfigureAwait(false);
                var stringDelimiterReader2 = await charsReader.MoveInternal1().ConfigureAwait(false);
                var whitespaceReader3 = await stringDelimiterReader2.MoveInternal1().ConfigureAwait(false);
                var colonReader = await whitespaceReader3.MoveInternal1().ConfigureAwait(false);
                var whitespaceReader4 = await colonReader.MoveInternal1().ConfigureAwait(false);
                var valueReader2 = await whitespaceReader4.MoveInternal1().ConfigureAwait(false);
                var valueToken2 = await valueReader2.MoveInternal1().ConfigureAwait(false);
                if (!(valueToken2 is ValueToken<SubsequentMembersReader<WhitespaceReader<ObjectEndReader<WhitespaceReader<Nothing>>>>>.True @true))
                {
                    throw new Exception("TODO");
                }

                var subsequentMembersReader = await @true.Reader.MoveInternal1().ConfigureAwait(false);
                var subsequentMembersToken = subsequentMembersReader.TryMove(out read);
                if (!read)
                {
                    await subsequentMembersReader.Read().ConfigureAwait(false);
                    subsequentMembersToken = subsequentMembersReader.TryMove(out read);
                }

                var subsequentMemberReader = subsequentMembersToken.Apply(
                    _ => throw new Exception("TODO"),
                    more => more);

                // false
                var commaReader = await subsequentMemberReader.MoveInternal1().ConfigureAwait(false);
                var whitespaceReader5 = await commaReader.MoveInternal1().ConfigureAwait(false);
                var memberReader2 = await whitespaceReader5.MoveInternal1().ConfigureAwait(false);
                var stringReader2 = await memberReader2.MoveInternal1().ConfigureAwait(false);
                var stringDelimiterReader3 = await stringReader2.MoveInternal1().ConfigureAwait(false);
                var charsReader2 = await stringDelimiterReader3.MoveInternal1();
                var stringDelimiterReader4 = await charsReader2.MoveInternal1();
                var whitespace6 = await stringDelimiterReader4.MoveInternal1();
                var colon2 = await whitespace6.MoveInternal1();
                var whitespace7 = await colon2.MoveInternal1();
                var value3 = await whitespace7.MoveInternal1();
                var valueToken3 = await value3.MoveInternal1();
                if (!(valueToken3 is ValueToken<SubsequentMembersReader<WhitespaceReader<ObjectEndReader<WhitespaceReader<Nothing>>>>>.False @false))
                {
                    throw new Exception("TODO");
                }

                var subsequentMembers2 = await @false.Reader.MoveInternal1();
                var subsequentMembersToken2 = subsequentMembers2.TryMove(out read);
                if (!read)
                {
                    await subsequentMembers2.Read();
                    subsequentMembersToken2 = subsequentMembers2.TryMove(out read);
                }

                var subsequentMember2 = subsequentMembersToken2.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // 1234
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
