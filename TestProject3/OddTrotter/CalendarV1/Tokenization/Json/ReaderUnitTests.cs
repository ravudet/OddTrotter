namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.Buffers;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.V2;
    using System.Reflection.Metadata;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NuGet.Frameworks;

    using OddTrotter.CalendarV1.Tokenization.Json2;
    using OddTrotter.CalendarV1.Tokenization.Json4;
    using OddTrotter.CalendarV1.Tokenization.Json6;

    public static class ReaderExtensions
    {
        internal static bool TryMove2<TNextReader>(this Json2.IReader<TNextReader> currentReader, out TNextReader nextReader)
            where TNextReader : allows ref struct
        {
            if (currentReader == null)
            {
                throw new Exception("TODO");
            }

            nextReader = currentReader.TryMove(out var read);
            return read;
        }

        /*private static bool TryMove2<TCurrentReader, TNextReader>(this TypeHolder<TCurrentReader, TNextReader> currentReader, out TNextReader nextReader)
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

            ////TNextReader nextReader;
            ////while (!currentReader.TryMove2(out nextReader))
            ////{
            ////await currentReader.Read().ConfigureAwait(false);
            ////}
            ////
            ////return nextReader;

            return new Move1Task<TNextReader>(currentReader);
        }

        4

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
        }*/
    }

    internal static class TestExtensions
    {
        internal static ITask<TNextReader> MoveInternal1<TNextReader>(this Json2.IReader<TNextReader> currentReader)
            where TNextReader : allows ref struct
        {
            /*var nextReader = currentReader.TryMove(out var read);
            if (!read) //// TODO this is supposed to be a while loop
            {
                await currentReader.Read().ConfigureAwait(false);
                nextReader = currentReader.TryMove(out read);
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

                    private System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? task;

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
                        return this.currentReader.TryMove(out _);
                        /*if (result == null)
                        {
                        }

                        return result;*/
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

                private System.Runtime.CompilerServices.ValueTaskAwaiter? task;

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

        internal static MoveInternal2Task<TCurrentReader, TNextReader> MoveInternal2<TCurrentReader, TNextReader>(this TypeHolder<TCurrentReader, TNextReader> currentReader, Func<TCurrentReader> factory, ref ReaderContext context)
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            //var self = currentReader.Self;
            if (!currentReader.Self.TryMove3(ref context, out var nextFactory))
            {
                //// TODO make context a `ref` field
                return new MoveInternal2Task<TCurrentReader, TNextReader>(new MoveInternal2ReadTask<TCurrentReader, TNextReader>(context.Read(), factory, ref context));

                ////throw new Exception("TODO check that moveinternal2 logic works, then improve performance as much as possible");
                /*return self.Read(context).ContinueWith(
                    async (_, context) =>
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        var readerContext = (ReaderContext)context;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        var self = factory(readerContext!);

                        return await self.AsReader.MoveInternal2(factory, readerContext!).ConfigureAwait(false);
                    },
                    context).Unwrap();*/
            }

            return new MoveInternal2Task<TCurrentReader, TNextReader>(new NewCompleted<TNextReader>(nextFactory));
        }

        internal ref struct MoveInternal2ReadTask<TCurrentReader, TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly ValueTask readTask;
            private readonly Func<TCurrentReader> factory;
            private readonly ref ReaderContext context;

            public MoveInternal2ReadTask(
                ValueTask readTask,
                Func<TCurrentReader> factory, 
                ref ReaderContext context)
            {
                this.readTask = readTask;
                this.factory = factory;
                this.context = ref context;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.readTask.ConfigureAwait(continueOnCapturedContext),
                    this.factory,
                    ref this.context,
                    continueOnCapturedContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable readTask;
                private readonly Func<TCurrentReader> factory;
                private readonly ref ReaderContext context;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(
                    ConfiguredValueTaskAwaitable readTask,
                    Func<TCurrentReader> factory,
                    ref ReaderContext context,
                    bool continueOnCapturedContext)
                {
                    this.readTask = readTask;
                    this.factory = factory;
                    this.context = ref context;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public Awaiter GetAwaiter()
                {
                    return new Awaiter(
                        this.readTask.GetAwaiter(),
                        this.factory,
                        ref this.context,
                        this.continueOnCapturedContext);
                }

                public unsafe struct Awaiter : ITaskAwaiter<Func<TNextReader>>
                {
                    private ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask;
                    private readonly Func<TCurrentReader> factory;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private readonly ReaderContext* context; //// TODO it's "mostly" ok for this to be a pointer because, for the caller to make use of the resulting `tnextreader`, the caller must maintain a reference to the `readercontext`; *however*, if that caller, let's say A, returns the task up the call stack to `B`, and `B` just wants to wait for the work to be completed, but not use the `tnextreader`, then the garbage collector could collect e.g. the stream in `readercontext` which would result in the task being defunct (not sure what the behavior would be, actually); AND THEN NOTE: that this actually isn't even possible to do because `A` cannot return the task to `B` because the compiler recognizes that the `readercontext` was passed by `ref` and therefore the task may have references to something that will leave scope; that's kind of beautiful actually
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private readonly bool continueOnCapturedContext;

                    public Awaiter(
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask,
                        Func<TCurrentReader> factory,
                        ref ReaderContext context,
                        bool continueOnCapturedContext)
                    {
                        this.readTask = readTask;
                        this.factory = factory;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.context = (ReaderContext*)Unsafe.AsPointer(ref context);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (!readTask.IsCompleted)
                            {
                                return false;
                            }

                            var context = Unsafe.AsRef<ReaderContext>(this.context);
                            var currentReader = this.factory();
                            if (!currentReader.TryMove3(ref context, out _)) //// TODO this assumes `trymove3` is idempotent, which is probably not good
                            {
                                this.readTask = context.Read().ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                return this.IsCompleted;
                            }

                            return true;
                        }
                    }

                    public Func<TNextReader> GetResult()
                    {
                        var context = Unsafe.AsRef<ReaderContext>(this.context);
                        var currentReader = this.factory();
                        currentReader.TryMove3(ref context, out var nextFactory);
                        return nextFactory;
                    }

                    public void OnCompleted(Action continuation)
                    {
                        this.readTask.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        this.readTask.UnsafeOnCompleted(continuation);
                    }
                }
            }

            public ITaskAwaiter<Func<ReaderContext, TNextReader>> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        internal ref struct MoveInternal2Task<TCurrentReader, TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TNextReader>, allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly int type;

            private readonly NewCompleted<TNextReader> newCompleted;
            private readonly MoveInternal2ReadTask<TCurrentReader, TNextReader> read;

            public MoveInternal2Task(NewCompleted<TNextReader> newCompleted)
            {
                this.newCompleted = newCompleted;

                this.type = 1;
            }

            public MoveInternal2Task(MoveInternal2ReadTask<TCurrentReader, TNextReader> read)
            {
                this.read = read;

                this.type = 2;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                switch (this.type)
                {
                    case 1:
                        return new ConfiguredAwaitable(this.newCompleted.ConfiguredAwait(continueOnCapturedContext));
                    case 2:
                        return new ConfiguredAwaitable(this.read.ConfigureAwait(continueOnCapturedContext));
                    default:
                        throw new Exception("TODO");
                }
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly int type;

                private readonly NewCompleted<TNextReader>.ConfiguredAwaitable newCompleted;
                private readonly MoveInternal2ReadTask<TCurrentReader, TNextReader>.ConfiguredAwaitable read;

                internal ConfiguredAwaitable(NewCompleted<TNextReader>.ConfiguredAwaitable newCompleted)
                {
                    this.newCompleted = newCompleted;

                    this.type = 1;
                }

                internal ConfiguredAwaitable(MoveInternal2ReadTask<TCurrentReader, TNextReader>.ConfiguredAwaitable read)
                {
                    this.read = read;

                    this.type = 2;
                }

                public Awaiter GetAwaiter()
                {
                    switch (this.type)
                    {
                        case 1:
                            return new Awaiter(this.newCompleted.GetAwaiter());
                        case 2:
                            return new Awaiter(this.read.GetAwaiter());
                        default:
                            throw new Exception("TODO");
                    }
                }

                public readonly struct Awaiter : ITaskAwaiter<Func<TNextReader>>
                {
                    private readonly int type;

                    private readonly NewCompleted<TNextReader>.ConfiguredAwaitable.Awaiter newCompleted;
                    private readonly MoveInternal2ReadTask<TCurrentReader, TNextReader>.ConfiguredAwaitable.Awaiter read;

                    internal Awaiter(NewCompleted<TNextReader>.ConfiguredAwaitable.Awaiter newCompleted)
                    {
                        this.newCompleted = newCompleted;

                        this.type = 1;
                    }

                    internal Awaiter(MoveInternal2ReadTask<TCurrentReader, TNextReader>.ConfiguredAwaitable.Awaiter read)
                    {
                        this.read = read;

                        this.type = 2;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            switch (this.type)
                            {
                                case 1:
                                    return this.newCompleted.IsCompleted;
                                case 2:
                                    return this.read.IsCompleted;
                                default:
                                    throw new Exception("TODO");
                            }
                        }
                    }

                    public Func<TNextReader> GetResult()
                    {
                        switch (this.type)
                        {
                            case 1:
                                return this.newCompleted.GetResult();
                            case 2:
                                return this.read.GetResult();
                            default:
                                throw new Exception("TODO");
                        }
                    }

                    public void OnCompleted(Action continuation)
                    {
                        switch (this.type)
                        {
                            case 1:
                                this.newCompleted.OnCompleted(continuation);
                                break;
                            case 2:
                                this.read.OnCompleted(continuation);
                                break;
                            default:
                                throw new Exception("TODO");
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        switch (this.type)
                        {
                            case 1:
                                this.newCompleted.UnsafeOnCompleted(continuation);
                                break;
                            case 2:
                                this.read.UnsafeOnCompleted(continuation);
                                break;
                            default:
                                throw new Exception("TODO");
                        }
                    }
                }
            }

            public ITaskAwaiter<Func<ReaderContext, TNextReader>> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        private static TaskWrapper<T> ToTaskWrapper<T>(this Task<T> task)
        {
            return new TaskWrapper<T>(task);
        }

        private static TaskWrapper<Nothing> ToTaskWrapper(this Task task)
        {
            return task.ContinueWith(_ => new Nothing()).ToTaskWrapper();
        }


        internal static MoveInternal3Task<TCurrentReader, TValue, TNextReader> MoveInternal3<TCurrentReader, TValue, TNextReader>(
            this TypeHolder<TCurrentReader, TValue, TNextReader> currentReader,
            ref ReaderContext readerContext,
            Func<TCurrentReader> currentReaderFactory)
            where TCurrentReader : Json2.IReader2<TCurrentReader, TValue, TNextReader>, allows ref struct
            where TValue : allows ref struct
            where TNextReader : allows ref struct
        {
            var self = currentReader.Self;
            if (self.TryGetValue3(ref readerContext, out _))
            {
                if (self.TryMove3(ref readerContext, out var nextFactory))
                {
                    return new MoveInternal3Task<TCurrentReader, TValue, TNextReader>(new NewCompleted<TNextReader>(nextFactory));
                    ////return new MoveInternal3Task<TCurrentReader, TValue, TNextReader>(new MoveInternal3TaskCompleted<TValue, TNextReader>(self.Context, nextFactory));
                }
                else
                {
                    /*var readTask = self.Read();
                    return new MoveInternal3Task<TCurrentReader, TValue, TNextReader>(new MoveInternal3TaskMove<TCurrentReader, TValue, TNextReader>(self.Context, self.Factory, readTask));*/
                    throw new Exception("TODO");
                }
            }
            else
            {
                /*var readTask = self.Read();
                return new MoveInternal3Task<TCurrentReader, TValue, TNextReader>(new MoveInternal3TaskValue<TCurrentReader, TValue, TNextReader>(self.Context, self.Factory, readTask));*/
                throw new Exception("TODO");
            }
        }

        private static ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter ConfiguredValueTaskAwaitableTrue = ValueTask.CompletedTask.ConfigureAwait(true).GetAwaiter();
        private static ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter ConfiguredValueTaskAwaitableFalse = ValueTask.CompletedTask.ConfigureAwait(false).GetAwaiter();

        public ref struct NewCompleted<TNextReader>
            where TNextReader : allows ref struct
        {
            private readonly Func<TNextReader> nextReaderFactory; //// TODO make this `ref`?

            public NewCompleted(Func<TNextReader> nextReaderFactory)
            {
                this.nextReaderFactory = nextReaderFactory;
            }

            public ConfiguredAwaitable ConfiguredAwait(bool continueOnCapturedContext)
            {
                // TODO use task.completedtask and keep a singleton of the configured awaitable
                return new ConfiguredAwaitable(this.nextReaderFactory, continueOnCapturedContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly Func<TNextReader> nextReaderFactory;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(
                    Func<TNextReader> nextReaderFactory,
                    bool continueOnCapturedContext)
                {
                    this.nextReaderFactory = nextReaderFactory;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public Awaiter GetAwaiter()
                {
                    return new Awaiter(this.nextReaderFactory, ValueTask.CompletedTask.ConfigureAwait(this.continueOnCapturedContext).GetAwaiter());
                }

                public readonly struct Awaiter : IAwaiter<Func<TNextReader>>
                {
                    private readonly Func<TNextReader> nextReaderFactory;
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredValueTaskAwaitable;

                    public Awaiter(
                        Func<TNextReader> nextReaderFactory,
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredValueTaskAwaitable)
                    {
                        this.nextReaderFactory = nextReaderFactory;
                        this.configuredValueTaskAwaitable = configuredValueTaskAwaitable;
                    }


                    public bool IsCompleted
                    {
                        get
                        {
                            return true;
                        }
                    }

                    public Func<TNextReader> GetResult()
                    {
                        return this.nextReaderFactory;
                    }

                    public void OnCompleted(Action continuation)
                    {
                        this.configuredValueTaskAwaitable.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        this.configuredValueTaskAwaitable.UnsafeOnCompleted(continuation);
                    }
                }
            }
        }

        public ref struct MoveInternal3Task<TCurrentReader, TValue, TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TValue, TNextReader>, allows ref struct
            where TValue : allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly int type;

            private readonly NewCompleted<TNextReader> newCompleted;

            internal MoveInternal3Task(NewCompleted<TNextReader> newCompleted)
            {
                this.newCompleted = newCompleted;

                this.type = 1;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                switch (this.type)
                {
                    case 1:
                        return new ConfiguredAwaitable(this.newCompleted.ConfiguredAwait(continueOnCapturedContext));
                    default:
                        throw new Exception("TODO");
                }
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly int type;

                private readonly NewCompleted<TNextReader>.ConfiguredAwaitable newCompleted;

                internal ConfiguredAwaitable(NewCompleted<TNextReader>.ConfiguredAwaitable newCompleted)
                {
                    this.newCompleted = newCompleted;

                    this.type = 1;
                }

                public Awaiter GetAwaiter()
                {
                    switch (this.type)
                    {
                        case 1:
                            return new Awaiter(this.newCompleted.GetAwaiter());
                        default:
                            throw new Exception("TODO");
                    }
                }

                public readonly struct Awaiter : ITaskAwaiter<Func<TNextReader>>
                {
                    private readonly int type;

                    private readonly NewCompleted<TNextReader>.ConfiguredAwaitable.Awaiter newCompleted;

                    internal Awaiter(NewCompleted<TNextReader>.ConfiguredAwaitable.Awaiter newCompleted)
                    {
                        this.newCompleted = newCompleted;

                        this.type = 1;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            switch (this.type)
                            {
                                case 1:
                                    return this.newCompleted.IsCompleted;
                                default:
                                    throw new Exception("TODO");
                            }
                        }
                    }

                    public Func<TNextReader> GetResult()
                    {
                        switch (this.type)
                        {
                            case 1:
                                return this.newCompleted.GetResult();
                            default:
                                throw new Exception("TODO");
                        }
                    }

                    public void OnCompleted(Action continuation)
                    {
                        switch (this.type)
                        {
                            case 1:
                                this.newCompleted.OnCompleted(continuation);
                                break;
                            default:
                                throw new Exception("TODO");
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        switch (this.type)
                        {
                            case 1:
                                this.newCompleted.UnsafeOnCompleted(continuation);
                                break;
                            default:
                                throw new Exception("TODO");
                        }
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        internal readonly ref struct MoveInternal3TaskCompleted<TValue, TNextReader>
            where TValue : allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly ReaderContext context;
            private readonly Func<ReaderContext, TNextReader> readerFactory;

            public MoveInternal3TaskCompleted(
                ReaderContext context,
                Func<ReaderContext, TNextReader> readerFactory)
            {
                this.context = context;
                this.readerFactory = readerFactory;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.context, this.readerFactory, ValueTask.CompletedTask.ConfigureAwait(continueOnCapturedContext));
            }

            public readonly ref struct ConfiguredAwaitable
            {
                private readonly ReaderContext context;
                private readonly Func<ReaderContext, TNextReader> readerFactory;
                private readonly ConfiguredValueTaskAwaitable configuredTaskAwaitable;

                public ConfiguredAwaitable(
                    ReaderContext context,
                    Func<ReaderContext, TNextReader> readerFactory,
                    ConfiguredValueTaskAwaitable configuredTaskAwaitable)
                {
                    this.context = context;
                    this.readerFactory = readerFactory;
                    this.configuredTaskAwaitable = configuredTaskAwaitable;
                }

                public Awaiter GetAwaiter()
                {
                    return new Awaiter(this.context, this.readerFactory, this.configuredTaskAwaitable.GetAwaiter());
                }

                public readonly struct Awaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly ReaderContext context;
                    private readonly Func<ReaderContext, TNextReader> readerFactory;
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredTaskAwaiter;

                    public Awaiter(
                        ReaderContext context,
                        Func<ReaderContext, TNextReader> readerFactory,
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredTaskAwaiter)
                    {
                        this.context = context;
                        this.readerFactory = readerFactory;
                        this.configuredTaskAwaiter = configuredTaskAwaiter;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return true;
                        }
                    }

                    public TNextReader GetResult()
                    {
                        return this.readerFactory(this.context);
                    }

                    public void OnCompleted(Action continuation)
                    {
                        this.configuredTaskAwaiter.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        this.configuredTaskAwaiter.UnsafeOnCompleted(continuation);
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        internal readonly ref struct MoveInternal3TaskValue<TCurrentReader, TValue, TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TValue, TNextReader>, allows ref struct
            where TValue : allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly ReaderContext context;
            private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
            private readonly ValueTask readTask;

            public MoveInternal3TaskValue(
                ReaderContext context,
                Func<ReaderContext, TCurrentReader> currentReaderFactory,
                ValueTask readTask)
            {
                this.context = context;
                this.currentReaderFactory = currentReaderFactory;
                this.readTask = readTask;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.context,
                    this.currentReaderFactory,
                    this.readTask.ConfigureAwait(continueOnCapturedContext),
                    continueOnCapturedContext);
            }

            public readonly ref struct ConfiguredAwaitable : IConfiguredAwaitable<TNextReader>
            {
                private readonly ReaderContext context;
                private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
                private readonly ConfiguredValueTaskAwaitable readTask;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(
                    ReaderContext context,
                    Func<ReaderContext, TCurrentReader> currentReaderFactory,
                    ConfiguredValueTaskAwaitable readTask,
                    bool continueOnCapturedContext)
                {
                    this.context = context;
                    this.currentReaderFactory = currentReaderFactory;
                    this.readTask = readTask;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public ITaskAwaiter<TNextReader> GetAwaiter()
                {
                    return new Awaiter(
                        this.context,
                        this.currentReaderFactory,
                        this.readTask.GetAwaiter(),
                        this.continueOnCapturedContext);
                }

                private sealed class Awaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly ReaderContext context;
                    private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
                    private ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask;
                    private readonly bool continueOnCapturedContext;
                    private bool moved;
                    private ITaskAwaiter<TNextReader>? valueTask = null;

                    public Awaiter(
                        ReaderContext context,
                        Func<ReaderContext, TCurrentReader> currentReaderFactory,
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask,
                        bool continueOnCapturedContext)
                    {
                        this.context = context;
                        this.currentReaderFactory = currentReaderFactory;
                        this.readTask = readTask;
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (this.valueTask != null)
                            {
                                return this.valueTask.IsCompleted;
                            }

                            if (!this.moved)
                            {
                                if (!this.readTask.IsCompleted)
                                {
                                    return false;
                                }

                                var context = this.context;
                                var currentReader = this.currentReaderFactory(this.context);
                                if (!currentReader.TryGetValue3(ref context, out _))
                                {
                                    this.readTask = currentReader.Read(this.context).ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                    return this.IsCompleted; //// TODO recursion probably isn't great...
                                }

                                this.moved = true;
                            }

                            if (this.moved)
                            {
                                var currentReader = this.currentReaderFactory(this.context);
                                /*Task readTask;
                                if (currentReader.TryMove3(this.context, out _))
                                {
                                    readTask = Task.CompletedTask;
                                }
                                else
                                {
                                    readTask = currentReader.Read(this.context);
                                }

                                this.valueTask = new MoveInternal3TaskMove<TCurrentReader, TValue, TNextReader>(
                                    this.context,
                                    this.currentReaderFactory,
                                    readTask)
                                    .ConfigureAwait(this.continueOnCapturedContext)
                                    .GetAwaiter();*/

                                return this.IsCompleted;
                            }

                            throw new Exception("TODO");
                        }
                    }

                    public TNextReader GetResult()
                    {
                        return this.valueTask!.GetResult();
                    }

                    public void OnCompleted(Action continuation)
                    {
                        //// TODO keep the list of continuations until  you create `this.valuetask` and then call it?
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        //// TODO keep the list of continuations until  you create `this.valuetask` and then call it?

                        this.readTask.UnsafeOnCompleted(() => this.Recurse(continuation));

                        /*continuation();

                        if (this.valueTask != null)
                        {
                            this.valueTask.UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.continuation = continuation;
                        }*/
                    }

                    private void Recurse(Action continuation)
                    {
                        if (this.IsCompleted)
                        {
                            this.valueTask!.UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.readTask.UnsafeOnCompleted(() => this.Recurse(continuation));
                        }
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        internal readonly ref struct MoveInternal3TaskMove<TCurrentReader, TValue, TNextReader>
            where TCurrentReader : Json2.IReader2<TCurrentReader, TValue, TNextReader>, allows ref struct
            where TValue : allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly ReaderContext context;
            private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
            private readonly ValueTask readTask;

            public MoveInternal3TaskMove(
                ReaderContext context,
                Func<ReaderContext, TCurrentReader> currentReaderFactory,
                ValueTask readTask)
            {
                this.context = context;
                this.currentReaderFactory = currentReaderFactory;
                this.readTask = readTask;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.context,
                    this.currentReaderFactory,
                    this.readTask.ConfigureAwait(continueOnCapturedContext),
                    continueOnCapturedContext);
            }

            public readonly ref struct ConfiguredAwaitable : IConfiguredAwaitable<TNextReader>
            {
                private readonly ReaderContext context;
                private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
                private readonly ConfiguredValueTaskAwaitable readTask;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(
                    ReaderContext context,
                    Func<ReaderContext, TCurrentReader> currentReaderFactory,
                    ConfiguredValueTaskAwaitable readTask,
                    bool continueOnCapturedContext)
                {
                    this.context = context;
                    this.currentReaderFactory = currentReaderFactory;
                    this.readTask = readTask;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public ITaskAwaiter<TNextReader> GetAwaiter()
                {
                    return new Awaiter(
                        this.context,
                        this.currentReaderFactory,
                        this.readTask.GetAwaiter(),
                        this.continueOnCapturedContext);
                }

                private sealed class Awaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly ReaderContext context;
                    private readonly Func<ReaderContext, TCurrentReader> currentReaderFactory;
                    private ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask;
                    private readonly bool continueOnCapturedContext;
                    private Func<TNextReader>? nextReaderFactory = null;

                    public Awaiter(
                        ReaderContext context,
                        Func<ReaderContext, TCurrentReader> currentReaderFactory,
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter readTask,
                        bool continueOnCapturedContext)
                    {
                        this.context = context;
                        this.currentReaderFactory = currentReaderFactory;
                        this.readTask = readTask;
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (this.nextReaderFactory != null)
                            {
                                return true;
                            }

                            if (!this.readTask.IsCompleted)
                            {
                                return false;
                            }

                            var currentReader = this.currentReaderFactory(this.context);
                            /*if (currentReader.TryMove3(this.context, out this.nextReaderFactory))
                            {
                                return true;
                            }
                            else*/
                            {
                                this.readTask = currentReader.Read(this.context).ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                return this.IsCompleted;
                            }
                        }
                    }

                    public TNextReader GetResult()
                    {
                        return this.nextReaderFactory!();
                    }

                    public void OnCompleted(Action continuation)
                    {
                        //// TODO wait for the last `readtask` and then add all the continuations to that
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        //// TODO wait for the last `readtask` and then add all the continuations to that

                        this.readTask.UnsafeOnCompleted(() => this.Recurse(continuation));
                    }

                    private void Recurse(Action continuation)
                    {
                        if (this.IsCompleted)
                        {
                            this.readTask.UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.readTask.UnsafeOnCompleted(() => this.Recurse(continuation));
                        }
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }
    }

    [TestClass]
    public sealed class ReaderUnitTests
    {
        private const string data =
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

        /*[TestMethod]
        public async Task V2Broad()
        {
            //// TODO i think it "means" something that your implementations all have no instance members; they could be static extensions on `readercontext` probably somehow, and not even have allocations on the stack, i think


            //// TODO `move` implementations should also be single-execution
            //// TODO they shouldn't be allowed to call `read` unless `false` was previously returned
            //// TODO the `trygetvalue` implementations need to follow the whitespace pattern of `finished`



            //// TODO 522d8139ea5a8695e2bb52a76895052f535fa360 was the jsonreader to ref struct commit
            //// TODO you are at ~209 after valuereader
            //// TODO with apply, you are at 210 mostly, getting as low as 208
            //// TODO ~208 range, getting as low as 206 with valuetoken
            //// TODO after updating to valuetask and stuff, you are now generally at 207, as low as 206
        
            //// TODO at some point, you decided to remove the "state" from the readers, so that stuff like whitespace reader and falsereader, etc. wouldn't need to keep track of "where they are"; you did this because you made the `nextreaderfactory`s stateless so that you could re-use them, which you need in the move extension tasks to loop the stream reading; however, you *could* have trygetvalue and trymove give back the "new" factory (which either has the state closed or returns the parameterized state with the method) whenever `false` is returned

            //// TODO you are realizing the the "nextfactory" can't be re-used to create the reader because, for something like whitespace or false (and a bunch more), there's state in the current reader instance (like the whitespaces that have already been read or which character in "false" we are at)
            //// TODO remove the `read` method from the reader
            //// TODO once you've removed all of the state, the only thing left is the next reader factory, which itself can be removed if you have a generic type constraint requiring a `new` default constructor
            //// TODO does readercontext.read2() make sense now with valuetask?

            //// TODO "unit" readers like `objectreader` should have `trygetvalue` which returns the "known reader" chain, and then `trymove` *only* returns the "next reader"
            //// TODO change reader to use ref struct somehow (maybe there's a step between `trymove` and `ref struct` that is just `struct`
            //// TODO make sure the check the perf after the fact; it should get better with the ref structs, right?
            //// TODO compare to utf8jsonreader; have a case where the entire stream is read to memory, and another where it's read as you go
            //// TODO remove the use of `ireader2.factory`
            //// TODO make all of the lambdas in the `jsonreader.cs` file `static` to ensure you aren't creating any closures
            //// TODO commit 90833a15d48301f6528ef39209466bd0ff99a010 running `v2broad` in release mode throws an `invalidprogramexception`
            //// TODO all of these `itask` implementations can't be object allocations or it defeats the purpose

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 1000;
                for (int i = 0; i < iterations; ++i)
                {
                    stream.Position = 0;
                    var reader = new Json2.JsonReader(stream);
                    await reader.Move().ConfigureAwait(false);

                    Assert.AreEqual(stream.Length, stream.Position);
                }
            }
        }*/

        [TestMethod]
        public async Task RefStructs()
        {
            //// TODO do the static method thing
            //// TODO can you have ref structs but never instantiate them

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await RefStructs(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static async Task RefStructs(Stream stream)
        {
            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }

            {
                stream.Position = 0;
                var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
                var reader = new Json3.JsonReader();

                var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!valueToken.TryObject(out var @object))
                {
                    throw new Exception("TODO");
                }

                var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
                if (!membersToken.TrySome(out var firstMemberReader))
                {
                    throw new Exception("TODO");
                }

                var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
                var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task NoRefStructs()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await NoRefStructs(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static async Task NoRefStructs(Stream stream)
        {
            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }

            {
                stream.Position = 0;

                var context = new ReaderContext(stream, new byte[20], 0, 0);
                await context.Read().ConfigureAwait(false);

                var reader = new Json2.JsonReader(stream);
                var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
                var whitespaceReader = whitespaceReaderFactory();

                var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

                var valueReader = valueReaderFactory();
                var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
                var valueToken = valueTokenFactory();
                if (!valueToken.TryObject(out var objectFactory))
                {
                    throw new Exception("TODO");
                }

                var @object = objectFactory();
                var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
                var objectstart = objectstartfactory();
                var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);
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
            }
        }

        [TestMethod]
        public async Task V3Broad()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await V3DoWork(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static async Task V3DoWork(Stream stream)
        {
            stream.Position = 0;
            var context = await Json3.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
            var reader = new Json3.JsonReader();

            var whitespaceReader = await reader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueReader = await whitespaceReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueToken = await valueReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!valueToken.TryObject(out var @object))
            {
                throw new Exception("TODO");
            }

            var objectStart = await @object.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespacereader2 = await objectStart.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var membersReader = await whitespacereader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var membersToken = await membersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!membersToken.TrySome(out var firstMemberReader))
            {
                throw new Exception("TODO");
            }

            var memberReader = await firstMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringReader = await memberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiterReader = await stringReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var charsReader = await stringDelimiterReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiterReader2 = await charsReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespaceReader3 = await stringDelimiterReader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var colonReader = await whitespaceReader3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespaceReader4 = await colonReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueReader2 = await whitespaceReader4.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueToken2 = await valueReader2.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!valueToken2.TryTrue(out var @true))
            {
                throw new Exception("TODO");
            }

            var subsequentMembersReader = await @true.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var subsequentMembersToken = await subsequentMembersReader.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!subsequentMembersToken.TryMore(out var subsequentMemberReader))
            {
                throw new Exception("TODO");
            }

            var commaReader = await subsequentMemberReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespaceReader5 = await commaReader.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var memberReader2 = await whitespaceReader5.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringReader2 = await memberReader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiterReader3 = await stringReader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var charsReader2 = await stringDelimiterReader3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiterReader4 = await charsReader2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespace6 = await stringDelimiterReader4.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var colon2 = await whitespace6.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespace7 = await colon2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var value3 = await whitespace7.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueToken3 = await value3.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!valueToken3.TryFalse(out var @false))
            {
                throw new Exception("TODO");
            }

            var subsequentMembers2 = await @false.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var subsequentMembersToken2 = await subsequentMembers2.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!subsequentMembersToken2.TryMore(out var subsequentMember2))
            {
                throw new Exception("TODO");
            }

            // 1234
            var comma2 = await subsequentMember2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespace8 = await comma2.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var member3 = await whitespace8.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var string3 = await member3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiter5 = await string3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var chars3 = await stringDelimiter5.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var stringDelimiter6 = await chars3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespace9 = await stringDelimiter6.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var colon3 = await whitespace9.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var whitespace10 = await colon3.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var value4 = await whitespace10.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var valueToken4 = await value4.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!valueToken4.TryNumber(out var number))
            {
                throw new Exception("TODO");
            }

            var sign = await number.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var @int = await sign.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var frac = await @int.AsMoveReader.Move1(ref context).ConfigureAwait(false);
            var fracToken = await frac.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!fracToken.TryAbsent(out var exp))
            {
                throw new Exception("TODO");
            }

            var expToken = await exp.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!expToken.TryAbsent(out var subsequentMembers3))
            {
                throw new Exception("TODO");
            }

            var subsequentMembersToken3 = await subsequentMembers3.AsTokenReader.Move2(ref context).ConfigureAwait(false);
            if (!subsequentMembersToken3.TryMore(out var subsequentMember3))
            {
                throw new Exception("TODO");
            }


            Assert.AreEqual(80, stream.Position);
            Assert.AreEqual(1, context.CurrentByteIndex);
        }

        [TestMethod]
        public async Task DotNetFullRead()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var buffer = new byte[stream.Length];
                Assert.AreEqual(buffer.Length, await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false));

                var timer = new System.Diagnostics.Stopwatch();
                for (int i = 0; i < iterations; ++i)
                {
                    timer.Start();
                    DotNetFullRead(buffer);
                    timer.Stop();
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static void DotNetFullRead(byte[] buffer)
        {
            /*stream.Position = 0;
            var buffer = new byte[stream.Length];
            Assert.AreEqual(buffer.Length, await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false));*/

            var reader = new System.Text.Json.Utf8JsonReader(buffer);
            while (reader.Read())
            {
            }
        }

        [TestMethod]
        public async Task StaticOnlyFullRead()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
                var timer = new System.Diagnostics.Stopwatch();
                for (int i = 0; i < iterations; ++i)
                {
                    timer.Start();
                    StaticOnlyFullRead(buffer); // right around 5000 is the cost of doing the bare minimum setup (creating the reader context and having the initial jsonreader to kick everything off);
                    timer.Stop();
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static void StaticOnlyFullRead(byte[] buffer)
        {
            //// TODO your design makes feature gaps explicit
            //// TODO your design is strongly typed and so it allows correct by construction
            //// TODO your design is strongly typed and so it allows helper methods that don't require (non-null) validations (note that this also means that other designs have to expose more details than they might otherwise; for example, the .net json reader *doesn't* expose the reader state in a way that gives you all of the information needed, so you have to pass the ref struct reader, which means you have to jump through hoops to get async methods)
            //// TODO your design allows for more fine granularity without breaking changes (e.g. a urlreader could be a ivaluereader<url>, and in the future *also* implement a imovereader<schemereader<domainreader<portreader<segmentsreader<queryoptionsreader<fragmentreader>>>>>>)

            /*stream.Position = 0;
            var context = await Json5.ReaderContext.FromStream(stream, new byte[stream.Length]).ConfigureAwait(false);*/
            var context = Json5.ReaderContext.FromBuffer(buffer);
            var reader = Readers.Create();

            reader.MoveTry1(context).MoveTry2(context).MoveTry3(context).TryObject(out var @object);
            @object.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TrySome(out var firstMemberReader);

            // true
            firstMemberReader.MoveTry1(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryTrue(out var @true);
            @true.MoveTry2(context).MoveTry3(context).TryMore(out var subsequentMemberReader);
            var whitespaceReader5 = subsequentMemberReader.MoveTry1(context).MoveTry4(context);

            // false
            whitespaceReader5.MoveTry2(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryFalse(out var @false);
            @false.MoveTry2(context).MoveTry3(context).TryMore(out var subsequentMember2);

            // 1234
            subsequentMember2.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryNumber(out var number);
            var sign = number.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryAbsent(out var exp);
            var expToken = exp.MoveTry3(context).TryAbsent(out var subsequentMembers3);
            var subsequentMembersToken3 = subsequentMembers3!.MoveTry3(context).TryMore(out var subsequentMember3);

            // asdf
            subsequentMember3.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryString(out var string5);
            string5.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry3(context).TryMore(out var subsequentMember4);

            // null
            subsequentMember4.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryNull(out var @null);
            @null.MoveTry2(context).MoveTry3(context).TryMore(out var subsequentMember5);

            // object
            subsequentMember5.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry1(context).MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TryObject(out var object2);
            var objectStart2 = object2.MoveTry1(context).MoveTry4(context).MoveTry2(context).MoveTry3(context).TrySome(out var nestedFirstMember);

            Json5.SubsequentMembersReader<Json5.WhitespaceReader<Json5.ObjectEndReader<Json5.WhitespaceReader<Nothing>>>> subsequentMembers6;
            {
                // true
                var nestedMember = nestedFirstMember.MoveTry1(context);
                var nestedString = nestedMember.MoveTry1(context);
                var nestedStringDelimiter = nestedString.MoveTry1(context);
                var nestedChars = nestedStringDelimiter.MoveTry4(context);
                var nestedStringDeliminter2 = nestedChars.MoveTry2(context);
                var nestedWhitespace = nestedStringDeliminter2.MoveTry4(context);
                var nestedColon = nestedWhitespace.MoveTry2(context);
                var nestedWhispace2 = nestedColon.MoveTry4(context);
                var nestedValue = nestedWhispace2.MoveTry2(context);
                var nestedValueToken = nestedValue.MoveTry3(context);
                nestedValueToken.TryTrue(out var nestedTrue);
                var nestedSubsequentMembersReader = nestedTrue.MoveTry2(context);
                var nestedSubsequentMembersToken = nestedSubsequentMembersReader.MoveTry3(context);
                nestedSubsequentMembersToken.TryMore(out var nestedsubsequentMemberReader);

                // false
                var nestedcommaReader = nestedsubsequentMemberReader.MoveTry1(context);
                var nestedwhitespaceReader5 = nestedcommaReader.MoveTry4(context);
                var nestedmemberReader2 = nestedwhitespaceReader5.MoveTry2(context);
                var nestedstringReader2 = nestedmemberReader2.MoveTry1(context);
                var nestedstringDelimiterReader3 = nestedstringReader2.MoveTry1(context);
                var nestedcharsReader2 = nestedstringDelimiterReader3.MoveTry4(context);
                var nestedstringDelimiterReader4 = nestedcharsReader2.MoveTry2(context);
                var nestedwhitespace6 = nestedstringDelimiterReader4.MoveTry4(context);
                var nestedcolon2 = nestedwhitespace6.MoveTry2(context);
                var nestedwhitespace7 = nestedcolon2.MoveTry4(context);
                var nestedvalue3 = nestedwhitespace7.MoveTry2(context);
                var nestedvalueToken3 = nestedvalue3.MoveTry3(context);
                nestedvalueToken3.TryFalse(out var nestedfalse);

                var nestedsubsequentMembers2 = nestedfalse.MoveTry2(context);
                var nestedsubsequentMembersToken2 = nestedsubsequentMembers2.MoveTry3(context);
                nestedsubsequentMembersToken2.TryMore(out var nestedsubsequentMember2);

                // 1234
                var nestedcomma2 = nestedsubsequentMember2.MoveTry1(context);
                var nestedwhitespace8 = nestedcomma2.MoveTry4(context);
                var nestedmember3 = nestedwhitespace8.MoveTry2(context);
                var nestedstring3 = nestedmember3.MoveTry1(context);
                var nestedstringDelimiter5 = nestedstring3.MoveTry1(context);
                var nestedchars3 = nestedstringDelimiter5.MoveTry4(context);
                var nestedstringDelimiter6 = nestedchars3.MoveTry2(context);
                var nestedwhitespace9 = nestedstringDelimiter6.MoveTry4(context);
                var nestedcolon3 = nestedwhitespace9.MoveTry2(context);
                var nestedwhitespace10 = nestedcolon3.MoveTry4(context);
                var nestedvalue4 = nestedwhitespace10.MoveTry2(context);
                var nestedvalueToken4 = nestedvalue4.MoveTry3(context);
                nestedvalueToken4.TryNumber(out var nestednumber);
                var nestedsign = nestednumber.MoveTry1(context);
                var nestedint = nestedsign.MoveTry4(context);
                var nestedfrac = nestedint.MoveTry2(context);
                var nestedfracToken = nestedfrac.MoveTry3(context);
                nestedfracToken.TryAbsent(out var nestedexp);
                var nestedexpToken = nestedexp.MoveTry3(context);
                nestedexpToken.TryAbsent(out var nestedsubsequentMembers3);
                var nestedsubsequentMembersToken3 = nestedsubsequentMembers3!.MoveTry3(context);
                nestedsubsequentMembersToken3.TryMore(out var nestedsubsequentMember3);

                // asdf
                var nestedcomma3 = nestedsubsequentMember3.MoveTry1(context);
                var nestedwhitespace11 = nestedcomma3.MoveTry4(context);
                var nestedmember4 = nestedwhitespace11.MoveTry2(context);
                var nestedstring4 = nestedmember4.MoveTry1(context);
                var nestedstringDelimiter7 = nestedstring4.MoveTry1(context);
                var nestedchars4 = nestedstringDelimiter7.MoveTry4(context);
                var nestedstringDelimiter8 = nestedchars4.MoveTry2(context);
                var nestedwhitespace12 = nestedstringDelimiter8.MoveTry4(context);
                var nestedcolon4 = nestedwhitespace12.MoveTry2(context);
                var nestedwhitespace13 = nestedcolon4.MoveTry4(context);
                var nestedvalue5 = nestedwhitespace13.MoveTry2(context);
                var nestedvalueToken5 = nestedvalue5.MoveTry3(context);
                nestedvalueToken5.TryString(out var nestedstring5);
                var nestedstringDelimiter9 = @nestedstring5.MoveTry1(context);
                var nestedchars5 = nestedstringDelimiter9.MoveTry4(context);
                var nestedstringDelimiter10 = nestedchars5.MoveTry2(context);
                var nestedsubsequentMembers4 = nestedstringDelimiter10.MoveTry4(context);
                var nestedsubsequentMembersToken4 = nestedsubsequentMembers4.MoveTry3(context);
                nestedsubsequentMembersToken4.TryMore(out var nestedsubsequentMember4);

                // null
                var nestedcomma4 = nestedsubsequentMember4.MoveTry1(context);
                var nestedwhitespace14 = nestedcomma4.MoveTry4(context);
                var nestedmember5 = nestedwhitespace14.MoveTry2(context);
                var nestedstring6 = nestedmember5.MoveTry1(context);
                var nestedstringDelimiter11 = nestedstring6.MoveTry1(context);
                var nestedchars6 = nestedstringDelimiter11.MoveTry4(context);
                var nestedstringDelimiter12 = nestedchars6.MoveTry2(context);
                var nestedwhitespace15 = nestedstringDelimiter12.MoveTry4(context);
                var nestedcolon5 = nestedwhitespace15.MoveTry2(context);
                var nestedwhitespace16 = nestedcolon5.MoveTry4(context);
                var nestedvalue6 = nestedwhitespace16.MoveTry2(context);
                var nestedvalueToken6 = nestedvalue6.MoveTry3(context);
                nestedvalueToken6.TryNull(out var nestednull);

                var nestedsubsequentMembers5 = @nestednull.MoveTry2(context);
                var nestedsubsequentMembersToken5 = nestedsubsequentMembers5.MoveTry3(context);
                nestedsubsequentMembersToken5.TryNone(out var nestedsubsequentMember5);

                var nestedobjectEnd = nestedsubsequentMember5!.MoveTry2(context);
                subsequentMembers6 = nestedobjectEnd.MoveTry4(context);
            }

            var subsequentMembersToken6 = subsequentMembers6.MoveTry3(context);
            subsequentMembersToken6.TryMore(out var subsequentMember6);

            // emptyobject
            var comma6 = subsequentMember6.MoveTry1(context);
            var whitespace21 = comma6.MoveTry4(context);
            var member7 = whitespace21.MoveTry2(context);
            var string8 = member7.MoveTry1(context);
            var stringDelimiter15 = string8.MoveTry1(context);
            var chars8 = stringDelimiter15.MoveTry4(context);
            var stringDelimiter16 = chars8.MoveTry2(context);
            var whitespace22 = stringDelimiter16.MoveTry4(context);
            var colon7 = whitespace22.MoveTry2(context);
            var whitespace23 = colon7.MoveTry4(context);
            var value8 = whitespace23.MoveTry2(context);
            var valueToken8 = value8.MoveTry3(context);
            valueToken8.TryObject(out var object3);
            var objectStart3 = object3.MoveTry1(context);
            var whitespace24 = objectStart3.MoveTry4(context);
            var members3 = whitespace24.MoveTry2(context);
            var membersToken3 = members3.MoveTry3(context);
            membersToken3.TryNone(out var whitespace25);
            var objectEnd = whitespace25!.MoveTry2(context);
            var subsequentMembers7 = objectEnd.MoveTry4(context);
            var subsequentMembersToken7 = subsequentMembers7.MoveTry3(context);
            subsequentMembersToken7.TryMore(out var subsequentMember8);

            // emptyarray
            var comma7 = subsequentMember8.MoveTry1(context);
            var whitespace26 = comma7.MoveTry4(context);
            var member8 = whitespace26.MoveTry2(context);
            var string9 = member8.MoveTry1(context);
            var stringDelimiter17 = string9.MoveTry1(context);
            var chars9 = stringDelimiter17.MoveTry4(context);
            var stringDelimiter18 = chars9.MoveTry2(context);
            var whitespace27 = stringDelimiter18.MoveTry4(context);
            var colon8 = whitespace27.MoveTry2(context);
            var whitespace28 = colon8.MoveTry4(context);
            var value9 = whitespace28.MoveTry2(context);
            var valueToken9 = value9.MoveTry3(context);
            valueToken9.TryArray(out var array);
            var arrayStart = array.MoveTry1(context);
            var whitespace29 = arrayStart.MoveTry4(context);
            var arrayElements = whitespace29.MoveTry2(context);
            var arrayElementsToken = arrayElements.MoveTry3(context);
            arrayElementsToken.TryNone(out var whitespace30);
            var arrayEnd = whitespace30.MoveTry2(context);
            var subsequentMembers9 = arrayEnd.MoveTry4(context);
            var subsequentMembersToken9 = subsequentMembers9.MoveTry3(context);
            subsequentMembersToken9.TryMore(out var subsequentMember10);

            // array
            var comma8 = subsequentMember10.MoveTry1(context);
            var whitespace31 = comma8.MoveTry4(context);
            var member9 = whitespace31.MoveTry2(context);
            var string10 = member9.MoveTry1(context);
            var stringDelimiter19 = string10.MoveTry1(context);
            var chars10 = stringDelimiter19.MoveTry4(context);
            var stringDelimiter20 = chars10.MoveTry2(context);
            var whitespace32 = stringDelimiter20.MoveTry4(context);
            var colon9 = whitespace32.MoveTry2(context);
            var whitespace33 = colon9.MoveTry4(context);
            var value10 = whitespace33.MoveTry2(context);
            var valueToken10 = value10.MoveTry3(context);
            valueToken10.TryArray(out var array2);
            var arrayStart2 = array2.MoveTry1(context);
            var whitespace34 = arrayStart2.MoveTry4(context);
            var arrayElements2 = whitespace34.MoveTry2(context);
            var arrayElementsToken2 = arrayElements2.MoveTry3(context);
            arrayElementsToken2.TrySome(out var arrayElement);
            var value11 = arrayElement.MoveTry1(context);
            var valueToken11 = value11.MoveTry3(context);
            valueToken11.TryObject(out var object4);
            var objectStart4 = object4.MoveTry1(context);
            var whitespace35 = objectStart4.MoveTry4(context);
            var members4 = whitespace35.MoveTry2(context);
            var membersToken4 = members4.MoveTry3(context);
            membersToken4.TrySome(out var firstMember4);
            Json5.SubsequentArrayElementsReader<Json5.WhitespaceReader<Json5.ArrayEndReader<Json5.SubsequentMembersReader<Json5.WhitespaceReader<Json5.ObjectEndReader<Json5.WhitespaceReader<Nothing>>>>>>> subsequentArrayElements;
            {
                // true
                var nestedMember = firstMember4.MoveTry1(context);
                var nestedString = nestedMember.MoveTry1(context);
                var nestedStringDelimiter = nestedString.MoveTry1(context);
                var nestedChars = nestedStringDelimiter.MoveTry4(context);
                var nestedStringDeliminter2 = nestedChars.MoveTry2(context);
                var nestedWhitespace = nestedStringDeliminter2.MoveTry4(context);
                var nestedColon = nestedWhitespace.MoveTry2(context);
                var nestedWhispace2 = nestedColon.MoveTry4(context);
                var nestedValue = nestedWhispace2.MoveTry2(context);
                var nestedValueToken = nestedValue.MoveTry3(context);
                nestedValueToken.TryTrue(out var nestedTrue);
                var nestedSubsequentMembersReader = nestedTrue.MoveTry2(context);
                var nestedSubsequentMembersToken = nestedSubsequentMembersReader.MoveTry3(context);
                nestedSubsequentMembersToken.TryMore(out var nestedsubsequentMemberReader);

                // false
                var nestedcommaReader = nestedsubsequentMemberReader.MoveTry1(context);
                var nestedwhitespaceReader5 = nestedcommaReader.MoveTry4(context);
                var nestedmemberReader2 = nestedwhitespaceReader5.MoveTry2(context);
                var nestedstringReader2 = nestedmemberReader2.MoveTry1(context);
                var nestedstringDelimiterReader3 = nestedstringReader2.MoveTry1(context);
                var nestedcharsReader2 = nestedstringDelimiterReader3.MoveTry4(context);
                var nestedstringDelimiterReader4 = nestedcharsReader2.MoveTry2(context);
                var nestedwhitespace6 = nestedstringDelimiterReader4.MoveTry4(context);
                var nestedcolon2 = nestedwhitespace6.MoveTry2(context);
                var nestedwhitespace7 = nestedcolon2.MoveTry4(context);
                var nestedvalue3 = nestedwhitespace7.MoveTry2(context);
                var nestedvalueToken3 = nestedvalue3.MoveTry3(context);
                nestedvalueToken3.TryFalse(out var nestedfalse);
                var nestedsubsequentMembers2 = nestedfalse.MoveTry2(context);
                var nestedsubsequentMembersToken2 = nestedsubsequentMembers2.MoveTry3(context);
                nestedsubsequentMembersToken2.TryMore(out var nestedsubsequentMember2);

                // 1234
                var nestedcomma2 = nestedsubsequentMember2.MoveTry1(context);
                var nestedwhitespace8 = nestedcomma2.MoveTry4(context);
                var nestedmember3 = nestedwhitespace8.MoveTry2(context);
                var nestedstring3 = nestedmember3.MoveTry1(context);
                var nestedstringDelimiter5 = nestedstring3.MoveTry1(context);
                var nestedchars3 = nestedstringDelimiter5.MoveTry4(context);
                var nestedstringDelimiter6 = nestedchars3.MoveTry2(context);
                var nestedwhitespace9 = nestedstringDelimiter6.MoveTry4(context);
                var nestedcolon3 = nestedwhitespace9.MoveTry2(context);
                var nestedwhitespace10 = nestedcolon3.MoveTry4(context);
                var nestedvalue4 = nestedwhitespace10.MoveTry2(context);
                var nestedvalueToken4 = nestedvalue4.MoveTry3(context);
                nestedvalueToken4.TryNumber(out var nestednumber);
                var nestedsign = nestednumber.MoveTry1(context);
                var nestedint = nestedsign.MoveTry4(context);
                var nestedfrac = nestedint.MoveTry2(context);
                var nestedfracToken = nestedfrac.MoveTry3(context);
                nestedfracToken.TryAbsent(out var nestedexp);
                var nestedexpToken = nestedexp.MoveTry3(context);
                nestedexpToken.TryAbsent(out var nestedsubsequentMembers3);
                var nestedsubsequentMembersToken3 = nestedsubsequentMembers3!.MoveTry3(context);
                nestedsubsequentMembersToken3.TryMore(out var nestedsubsequentMember3);

                // asdf
                var nestedcomma3 = nestedsubsequentMember3.MoveTry1(context);
                var nestedwhitespace11 = nestedcomma3.MoveTry4(context);
                var nestedmember4 = nestedwhitespace11.MoveTry2(context);
                var nestedstring4 = nestedmember4.MoveTry1(context);
                var nestedstringDelimiter7 = nestedstring4.MoveTry1(context);
                var nestedchars4 = nestedstringDelimiter7.MoveTry4(context);
                var nestedstringDelimiter8 = nestedchars4.MoveTry2(context);
                var nestedwhitespace12 = nestedstringDelimiter8.MoveTry4(context);
                var nestedcolon4 = nestedwhitespace12.MoveTry2(context);
                var nestedwhitespace13 = nestedcolon4.MoveTry4(context);
                var nestedvalue5 = nestedwhitespace13.MoveTry2(context);
                var nestedvalueToken5 = nestedvalue5.MoveTry3(context);
                nestedvalueToken5.TryString(out var nestedstring5);
                var nestedstringDelimiter9 = @nestedstring5.MoveTry1(context);
                var nestedchars5 = nestedstringDelimiter9.MoveTry4(context);
                var nestedstringDelimiter10 = nestedchars5.MoveTry2(context);
                var nestedsubsequentMembers4 = nestedstringDelimiter10.MoveTry4(context);
                var nestedsubsequentMembersToken4 = nestedsubsequentMembers4.MoveTry3(context);
                nestedsubsequentMembersToken4.TryMore(out var nestedsubsequentMember4);

                // null
                var nestedcomma4 = nestedsubsequentMember4.MoveTry1(context);
                var nestedwhitespace14 = nestedcomma4.MoveTry4(context);
                var nestedmember5 = nestedwhitespace14.MoveTry2(context);
                var nestedstring6 = nestedmember5.MoveTry1(context);
                var nestedstringDelimiter11 = nestedstring6.MoveTry1(context);
                var nestedchars6 = nestedstringDelimiter11.MoveTry4(context);
                var nestedstringDelimiter12 = nestedchars6.MoveTry2(context);
                var nestedwhitespace15 = nestedstringDelimiter12.MoveTry4(context);
                var nestedcolon5 = nestedwhitespace15.MoveTry2(context);
                var nestedwhitespace16 = nestedcolon5.MoveTry4(context);
                var nestedvalue6 = nestedwhitespace16.MoveTry2(context);
                var nestedvalueToken6 = nestedvalue6.MoveTry3(context);
                nestedvalueToken6.TryNull(out var nestednull);

                var nestedsubsequentMembers5 = @nestednull.MoveTry2(context);
                var nestedsubsequentMembersToken5 = nestedsubsequentMembers5.MoveTry3(context);
                nestedsubsequentMembersToken5.TryNone(out var nestedsubsequentMember5);

                var nestedobjectEnd = nestedsubsequentMember5!.MoveTry2(context);
                subsequentArrayElements = nestedobjectEnd.MoveTry4(context);
            }

            var subsequentArrayElementsToken = subsequentArrayElements.MoveTry3(context);
            subsequentArrayElementsToken.TryNone(out var whitespace36);
            var arrayEnd2 = whitespace36.MoveTry2(context);
            var subsequentMembers10 = arrayEnd2.MoveTry4(context);
            var subsequentMembersToken10 = subsequentMembers10.MoveTry3(context);
            subsequentMembersToken10.TryNone(out var whitespace37);
            var objectEnd2 = whitespace37!.MoveTry2(context);
            var whitespace38 = objectEnd2.MoveTry4(context);
            var nothing = whitespace38.MoveTry2(context);

            Assert.AreEqual(new Nothing(), nothing);
        }

        [TestMethod]
        public async Task DotNet()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await DotNet(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static async Task DotNet(Stream stream)
        {
            stream.Position = 0;
            var buffer = new byte[20];

            var state = new System.Text.Json.JsonReaderState();
            long bytesConsumed = buffer.Length;
            bool final = false;
            var valueRead = true;
            while (!final)
            {
                var startIndex = (int)(buffer.Length - bytesConsumed);
                if (!valueRead)
                {
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, bytesConsumed, newBuffer, 0, buffer.Length - bytesConsumed);
                    buffer = newBuffer;
                }
                else
                {
                    if (bytesConsumed < buffer.Length)
                    {
                        int newLocation = 0;
                        for (long oldLocation = bytesConsumed; oldLocation < buffer.Length; ++oldLocation)
                        {
                            buffer[newLocation] = buffer[oldLocation];
                            ++newLocation;
                        }
                    }
                }

                valueRead = false;
                var length = buffer.Length - startIndex;
                var read = await stream.ReadAsync(buffer, startIndex, length).ConfigureAwait(false);
                System.Text.Json.Utf8JsonReader reader;
                var sequence = new ReadOnlySequence<byte>(buffer, 0, startIndex + read);
                final = read != length;
                reader = new System.Text.Json.Utf8JsonReader(sequence, final, state);
                
                while (reader.Read())
                {
                    valueRead = true;
                }

                state = reader.CurrentState;
                bytesConsumed = reader.BytesConsumed;
            }
        }

        [TestMethod]
        public async Task StaticOnly()
        {

            //// TODO i think you are about ready to start moving forward with this design and productizing
            //// first of all, remember that making values ref structs seems to have made things a bit slower for `staticonly`; it *may* have made `staticonlyfullread` *slightly* faster; so maybe revert that change
            


            //// TODO you are here
            //// TODO static only is fastest per your last tests (you should probably run this one more time just to be sure)
            //// TODO now you need to compare it to .net
            //// TODO and optimize during .net comparison
            

            //// TODO can you have ref structs but never instantiate them

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await StaticOnly(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        public static async Task StaticOnly(Stream stream)
        {
            stream.Position = 0;
            var context = await Json5.ReaderContext.FromStream(stream, new byte[20]).ConfigureAwait(false);
            var reader = Readers.Create();

            var whitespaceReader = await reader.Move1(context).ConfigureAwait(false);
            (context, var valueReader) = await whitespaceReader.Move2(context).ConfigureAwait(false);
            //var valueReader = await whitespaceReader.Move21(context).ConfigureAwait(false);
            var valueToken = await valueReader.Move3(context).ConfigureAwait(false);
            //var valueToken = await valueReader.Move31(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken.TryObject(out var @object));
            var objectStart = await @object.Move1(context).ConfigureAwait(false);
            var whitespacereader2 = await objectStart.Move4(context).ConfigureAwait(false);
            (context, var membersReader) = await whitespacereader2.Move2(context).ConfigureAwait(false);
            //var membersReader = await whitespacereader2.Move21(context).ConfigureAwait(false);
            var membersToken = await membersReader.Move3(context).ConfigureAwait(false);
            //var membersToken = await membersReader.Move31(context).ConfigureAwait(false);
            Assert.IsTrue(membersToken.TrySome(out var firstMemberReader));

            // true
            var memberReader = await firstMemberReader.Move1(context).ConfigureAwait(false);
            var stringReader = await memberReader.Move1(context).ConfigureAwait(false);
            var stringDelimiterReader = await stringReader.Move1(context).ConfigureAwait(false);
            var charsReader = await stringDelimiterReader.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiterReader2) = await charsReader.Move2(context).ConfigureAwait(false);
            //var stringDelimiterReader2 = await charsReader.Move21(context).ConfigureAwait(false);
            var whitespaceReader3 = await stringDelimiterReader2.Move4(context).ConfigureAwait(false);
            (context, var colonReader) = await whitespaceReader3.Move2(context).ConfigureAwait(false);
            //var colonReader = await whitespaceReader3.Move21(context).ConfigureAwait(false);
            var whitespaceReader4 = await colonReader.Move4(context).ConfigureAwait(false);
            (context, var valueReader2) = await whitespaceReader4.Move2(context).ConfigureAwait(false);
            //var valueReader2 = await whitespaceReader4.Move21(context).ConfigureAwait(false);
            var valueToken2 = await valueReader2.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken2.TryTrue(out var @true));
            (context, var subsequentMembersReader) = await @true.Move2(context).ConfigureAwait(false);
            var subsequentMembersToken = await subsequentMembersReader.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken.TryMore(out var subsequentMemberReader));
            var commaReader = await subsequentMemberReader.Move1(context).ConfigureAwait(false);
            var whitespaceReader5 = await commaReader.Move4(context).ConfigureAwait(false);

            // false
            (context, var memberReader2) = await whitespaceReader5.Move2(context).ConfigureAwait(false);
            //var memberReader2 = await whitespaceReader5.Move21(context).ConfigureAwait(false);
            var stringReader2 = await memberReader2.Move1(context).ConfigureAwait(false);
            var stringDelimterReader3 = await stringReader2.Move1(context).ConfigureAwait(false);
            var charsReader2 = await stringDelimterReader3.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiterReader4) = await charsReader2.Move2(context).ConfigureAwait(false);
            var whitespace6 = await stringDelimiterReader4.Move4(context).ConfigureAwait(false);
            (context, var colon2) = await whitespace6.Move2(context).ConfigureAwait(false);
            //var colon2 = await whitespace6.Move21(context).ConfigureAwait(false);
            var whitespace7 = await colon2.Move4(context).ConfigureAwait(false);
            (context, var value3) = await whitespace7.Move2(context).ConfigureAwait(false);
            //var value3 = await whitespace7.Move21(context).ConfigureAwait(false);
            var valueToken3 = await value3.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken3.TryFalse(out var @false));
            (context, var subsequentMembers2) = await @false.Move2(context).ConfigureAwait(false);
            var subsequentMembersToken2 = await subsequentMembers2.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken2.TryMore(out var subsequentMember2));

            // 1234
            var comma2 = await subsequentMember2.Move1(context).ConfigureAwait(false);
            var whitespace8 = await comma2.Move4(context).ConfigureAwait(false);
            (context, var member3) = await whitespace8.Move2(context).ConfigureAwait(false);
            //var member3 = await whitespace8.Move21(context).ConfigureAwait(false);
            var string3 = await member3.Move1(context).ConfigureAwait(false);
            var stringDelimiter5 = await string3.Move1(context).ConfigureAwait(false);
            var chars3 = await stringDelimiter5.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter6) = await chars3.Move2(context).ConfigureAwait(false);
            var whitespace9 = await stringDelimiter6.Move4(context).ConfigureAwait(false);
            (context, var colon3) = await whitespace9.Move2(context).ConfigureAwait(false);
            //var colon3 = await whitespace9.Move21(context).ConfigureAwait(false);
            var whitespace10 = await colon3.Move4(context).ConfigureAwait(false);
            (context, var value4) = await whitespace10.Move2(context).ConfigureAwait(false);
            //var value4 = await whitespace10.Move21(context).ConfigureAwait(false);
            var valueToken4 = await value4.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken4.TryNumber(out var number));
            var sign = await number.Move1(context).ConfigureAwait(false);
            var @int = await sign.Move4(context).ConfigureAwait(false);
            (context, var frac) = await @int.Move2(context).ConfigureAwait(false);
            var fracToken = await frac.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(fracToken.TryAbsent(out var exp));
            var expToken = await exp.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(expToken.TryAbsent(out var subsequentMembers3));
            var subsequentMembersToken3 = await subsequentMembers3.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken3.TryMore(out var subsequentMember3));

            // asdf
            var comma3 = await subsequentMember3.Move1(context).ConfigureAwait(false);
            var whitespace11 = await comma3.Move4(context).ConfigureAwait(false);
            (context, var member4) = await whitespace11.Move2(context).ConfigureAwait(false);
            //var member4 = await whitespace11.Move21(context).ConfigureAwait(false);
            var string4 = await member4.Move1(context).ConfigureAwait(false);
            var stringDelimiter7 = await string4.Move1(context).ConfigureAwait(false);
            var chars4 = await stringDelimiter7.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter8) = await chars4.Move2(context).ConfigureAwait(false);
            var whitespace12 = await stringDelimiter8.Move4(context).ConfigureAwait(false);
            (context, var colon4) = await whitespace12.Move2(context).ConfigureAwait(false);
            //var colon4 = await whitespace12.Move21(context).ConfigureAwait(false);
            var whitespace13 = await colon4.Move4(context).ConfigureAwait(false);
            (context, var value5) = await whitespace13.Move2(context).ConfigureAwait(false);
            var valueToken5 = await value5.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken5.TryString(out var @string5));
            var stringDelimiter9 = await string5.Move1(context).ConfigureAwait(false);
            var chars5 = await stringDelimiter9.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter10) = await chars5.Move2(context).ConfigureAwait(false);
            var subsequentMembers4 = await stringDelimiter10.Move4(context).ConfigureAwait(false);
            var subsequentMembersToken4 = await subsequentMembers4.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken4.TryMore(out var subsequentMember4));

            // null
            var comma4 = await subsequentMember4.Move1(context).ConfigureAwait(false);
            var whitespace14 = await comma4.Move4(context).ConfigureAwait(false);
            (context, var member5) = await whitespace14.Move2(context).ConfigureAwait(false);
            var string6 = await member5.Move1(context).ConfigureAwait(false);
            var stringDelimiter11 = await string6.Move1(context).ConfigureAwait(false);
            var chars6 = await stringDelimiter11.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter12) = await chars6.Move2(context).ConfigureAwait(false);
            var whitespace15 = await stringDelimiter12.Move4(context).ConfigureAwait(false);
            (context, var colon5) = await whitespace15.Move2(context).ConfigureAwait(false);
            var whitespace16 = await colon5.Move4(context).ConfigureAwait(false);
            (context, var value6) = await whitespace16.Move2(context).ConfigureAwait(false);
            var valueToken6 = await value6.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken6.TryNull(out var @null));
            (context, var subsequentMembers5) = await @null.Move2(context).ConfigureAwait(false);
            var subsequentMembersToken5 = await subsequentMembers5.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken5.TryMore(out var subsequentMember5));


            // object
            var comma5 = await subsequentMember5.Move1(context).ConfigureAwait(false);
            var whitespace17 = await comma5.Move4(context).ConfigureAwait(false);
            //var member6 = await whitespace17.AsReader.MoveInternal3(whitespace17.Factory).ConfigureAwait(false);
            (context, var member6) = await whitespace17.Move2(context).ConfigureAwait(false);
            var string7 = await member6.Move1(context).ConfigureAwait(false);
            var stringDelimiter13 = await string7.Move1(context).ConfigureAwait(false);
            var chars7 = await stringDelimiter13.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter14) = await chars7.Move2(context).ConfigureAwait(false);
            var whitespace18 = await stringDelimiter14.Move4(context).ConfigureAwait(false);
            //var colon6 = await whitespace18.AsReader.MoveInternal3(whitespace18.Factory).ConfigureAwait(false);
            (context, var colon6) = await whitespace18.Move2(context).ConfigureAwait(false);
            var whitespace19 = await colon6.Move4(context).ConfigureAwait(false);
            //var value7 = await whitespace19.AsReader.MoveInternal3(whitespace19.Factory).ConfigureAwait(false);
            (context, var value7) = await whitespace19.Move2(context).ConfigureAwait(false);
            var valueToken7 = await value7.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken7.TryObject(out var object2));
            var objectStart2 = await object2.Move1(context).ConfigureAwait(false);
            var whitespace20 = await objectStart2.Move4(context).ConfigureAwait(false);
            //var nestedMembers = await whitespace20.AsReader.MoveInternal3(whitespace20.Factory).ConfigureAwait(false);
            (context, var nestedMembers) = await whitespace20.Move2(context).ConfigureAwait(false);
            var nestedMembersToken = await nestedMembers.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(nestedMembersToken.TrySome(out var nestedFirstMember));

            Json5.SubsequentMembersReader<Json5.WhitespaceReader<Json5.ObjectEndReader<Json5.WhitespaceReader<Nothing>>>> subsequentMembers6;
            {
                // true
                var nestedMember = await nestedFirstMember.Move1(context).ConfigureAwait(false);
                var nestedString = await nestedMember.Move1(context).ConfigureAwait(false);
                var nestedStringDelimiter = await nestedString.Move1(context).ConfigureAwait(false);
                var nestedChars = await nestedStringDelimiter.Move4(context).ConfigureAwait(false);
                (context, var nestedStringDeliminter2) = await nestedChars.Move2(context).ConfigureAwait(false);
                var nestedWhitespace = await nestedStringDeliminter2.Move4(context).ConfigureAwait(false);
                //var nestedColon = await nestedWhitespace.AsReader.MoveInternal3(nestedWhitespace.Factory).ConfigureAwait(false);
                (context, var nestedColon) = await nestedWhitespace.Move2(context).ConfigureAwait(false);
                var nestedWhispace2 = await nestedColon.Move4(context).ConfigureAwait(false);
                //var nestedValue = await nestedWhispace2.AsReader.MoveInternal3(nestedWhispace2.Factory).ConfigureAwait(false);
                (context, var nestedValue) = await nestedWhispace2.Move2(context).ConfigureAwait(false);
                var nestedValueToken = await nestedValue.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedValueToken.TryTrue(out var nestedTrue));
                (context, var nestedSubsequentMembersReader) = await nestedTrue.Move2(context).ConfigureAwait(false);
                var nestedSubsequentMembersToken = await nestedSubsequentMembersReader.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedSubsequentMembersToken.TryMore(out var nestedsubsequentMemberReader));

                // false
                var nestedcommaReader = await nestedsubsequentMemberReader.Move1(context).ConfigureAwait(false);
                var nestedwhitespaceReader5 = await nestedcommaReader.Move4(context).ConfigureAwait(false);
                //var nestedmemberReader2 = await nestedwhitespaceReader5.AsReader.MoveInternal3(nestedwhitespaceReader5.Factory).ConfigureAwait(false);
                (context, var nestedmemberReader2) = await nestedwhitespaceReader5.Move2(context).ConfigureAwait(false);
                var nestedstringReader2 = await nestedmemberReader2.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiterReader3 = await nestedstringReader2.Move1(context).ConfigureAwait(false);
                var nestedcharsReader2 = await nestedstringDelimiterReader3.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiterReader4) = await nestedcharsReader2.Move2(context).ConfigureAwait(false);
                var nestedwhitespace6 = await nestedstringDelimiterReader4.Move4(context).ConfigureAwait(false);
                //var nestedcolon2 = await nestedwhitespace6.AsReader.MoveInternal3(nestedwhitespace6.Factory).ConfigureAwait(false);
                (context, var nestedcolon2) = await nestedwhitespace6.Move2(context).ConfigureAwait(false);
                var nestedwhitespace7 = await nestedcolon2.Move4(context).ConfigureAwait(false);
                //var nestedvalue3 = await nestedwhitespace7.AsReader.MoveInternal3(nestedwhitespace7.Factory).ConfigureAwait(false);
                (context, var nestedvalue3) = await nestedwhitespace7.Move2(context).ConfigureAwait(false);
                var nestedvalueToken3 = await nestedvalue3.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken3.TryFalse(out var nestedfalse));

                (context, var nestedsubsequentMembers2) = await nestedfalse.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken2 = await nestedsubsequentMembers2.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken2.TryMore(out var nestedsubsequentMember2));

                // 1234
                var nestedcomma2 = await nestedsubsequentMember2.Move1(context).ConfigureAwait(false);
                var nestedwhitespace8 = await nestedcomma2.Move4(context).ConfigureAwait(false);
                //var nestedmember3 = await nestedwhitespace8.AsReader.MoveInternal3(nestedwhitespace8.Factory).ConfigureAwait(false);
                (context, var nestedmember3) = await nestedwhitespace8.Move2(context).ConfigureAwait(false);
                var nestedstring3 = await nestedmember3.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter5 = await nestedstring3.Move1(context).ConfigureAwait(false);
                var nestedchars3 = await nestedstringDelimiter5.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter6) = await nestedchars3.Move2(context).ConfigureAwait(false);
                var nestedwhitespace9 = await nestedstringDelimiter6.Move4(context).ConfigureAwait(false);
                //var nestedcolon3 = await nestedwhitespace9.AsReader.MoveInternal3(nestedwhitespace9.Factory).ConfigureAwait(false);
                (context, var nestedcolon3) = await nestedwhitespace9.Move2(context).ConfigureAwait(false);
                var nestedwhitespace10 = await nestedcolon3.Move4(context).ConfigureAwait(false);
                //var nestedvalue4 = await nestedwhitespace10.AsReader.MoveInternal3(nestedwhitespace10.Factory).ConfigureAwait(false);
                (context, var nestedvalue4) = await nestedwhitespace10.Move2(context).ConfigureAwait(false);
                var nestedvalueToken4 = await nestedvalue4.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken4.TryNumber(out var nestednumber));
                var nestedsign = await nestednumber.Move1(context).ConfigureAwait(false);
                var nestedint = await nestedsign.Move4(context).ConfigureAwait(false);
                (context, var nestedfrac) = await nestedint.Move2(context).ConfigureAwait(false);
                var nestedfracToken = await nestedfrac.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedfracToken.TryAbsent(out var nestedexp));
                var nestedexpToken = await nestedexp.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedexpToken.TryAbsent(out var nestedsubsequentMembers3));
                var nestedsubsequentMembersToken3 = await nestedsubsequentMembers3.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken3.TryMore(out var nestedsubsequentMember3));

                // asdf
                var nestedcomma3 = await nestedsubsequentMember3.Move1(context).ConfigureAwait(false);
                var nestedwhitespace11 = await nestedcomma3.Move4(context).ConfigureAwait(false);
                //var nestedmember4 = await nestedwhitespace11.AsReader.MoveInternal3(nestedwhitespace11.Factory).ConfigureAwait(false);
                (context, var nestedmember4) = await nestedwhitespace11.Move2(context).ConfigureAwait(false);
                var nestedstring4 = await nestedmember4.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter7 = await nestedstring4.Move1(context).ConfigureAwait(false);
                var nestedchars4 = await nestedstringDelimiter7.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter8) = await nestedchars4.Move2(context).ConfigureAwait(false);
                var nestedwhitespace12 = await nestedstringDelimiter8.Move4(context).ConfigureAwait(false);
                //var nestedcolon4 = await nestedwhitespace12.AsReader.MoveInternal3(nestedwhitespace12.Factory).ConfigureAwait(false);
                (context, var nestedcolon4) = await nestedwhitespace12.Move2(context).ConfigureAwait(false);
                var nestedwhitespace13 = await nestedcolon4.Move4(context).ConfigureAwait(false);
                //var nestedvalue5 = await nestedwhitespace13.AsReader.MoveInternal3(nestedwhitespace13.Factory).ConfigureAwait(false);
                (context, var nestedvalue5) = await nestedwhitespace13.Move2(context).ConfigureAwait(false);
                var nestedvalueToken5 = await nestedvalue5.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken5.TryString(out var nestedstring5));
                var nestedstringDelimiter9 = await @nestedstring5.Move1(context).ConfigureAwait(false);
                var nestedchars5 = await nestedstringDelimiter9.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter10) = await nestedchars5.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembers4 = await nestedstringDelimiter10.Move4(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken4 = await nestedsubsequentMembers4.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken4.TryMore(out var nestedsubsequentMember4));

                // null
                var nestedcomma4 = await nestedsubsequentMember4.Move1(context).ConfigureAwait(false);
                var nestedwhitespace14 = await nestedcomma4.Move4(context).ConfigureAwait(false);
                //var nestedmember5 = await nestedwhitespace14.AsReader.MoveInternal3(nestedwhitespace14.Factory).ConfigureAwait(false);
                (context, var nestedmember5) = await nestedwhitespace14.Move2(context).ConfigureAwait(false);
                var nestedstring6 = await nestedmember5.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter11 = await nestedstring6.Move1(context).ConfigureAwait(false);
                var nestedchars6 = await nestedstringDelimiter11.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter12) = await nestedchars6.Move2(context).ConfigureAwait(false);
                var nestedwhitespace15 = await nestedstringDelimiter12.Move4(context).ConfigureAwait(false);
                //var nestedcolon5 = await nestedwhitespace15.AsReader.MoveInternal3(nestedwhitespace15.Factory).ConfigureAwait(false);
                (context, var nestedcolon5) = await nestedwhitespace15.Move2(context).ConfigureAwait(false);
                var nestedwhitespace16 = await nestedcolon5.Move4(context).ConfigureAwait(false);
                //var nestedvalue6 = await nestedwhitespace16.AsReader.MoveInternal3(nestedwhitespace16.Factory).ConfigureAwait(false);
                (context, var nestedvalue6) = await nestedwhitespace16.Move2(context).ConfigureAwait(false);
                var nestedvalueToken6 = await nestedvalue6.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken6.TryNull(out var nestednull));

                (context, var nestedsubsequentMembers5) = await @nestednull.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken5 = await nestedsubsequentMembers5.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken5.TryNone(out var nestedsubsequentMember5));

                //var nestedobjectEnd = await nestedsubsequentMember5.AsReader.MoveInternal3(nestedsubsequentMember5.Factory).ConfigureAwait(false);
                (context, var nestedobjectEnd) = await nestedsubsequentMember5.Move2(context).ConfigureAwait(false);
                subsequentMembers6 = await nestedobjectEnd.Move4(context).ConfigureAwait(false);
            }

            var subsequentMembersToken6 = await subsequentMembers6.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken6.TryMore(out var subsequentMember6));

            // emptyobject
            var comma6 = await subsequentMember6.Move1(context).ConfigureAwait(false);
            var whitespace21 = await comma6.Move4(context).ConfigureAwait(false);
            //var member7 = await whitespace21.AsReader.MoveInternal3(whitespace21.Factory).ConfigureAwait(false);
            (context, var member7) = await whitespace21.Move2(context).ConfigureAwait(false);
            var string8 = await member7.Move1(context).ConfigureAwait(false);
            var stringDelimiter15 = await string8.Move1(context).ConfigureAwait(false);
            var chars8 = await stringDelimiter15.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter16) = await chars8.Move2(context).ConfigureAwait(false);
            var whitespace22 = await stringDelimiter16.Move4(context).ConfigureAwait(false);
            //var colon7 = await whitespace22.AsReader.MoveInternal3(whitespace22.Factory).ConfigureAwait(false);
            (context, var colon7) = await whitespace22.Move2(context).ConfigureAwait(false);
            var whitespace23 = await colon7.Move4(context).ConfigureAwait(false);
            //var value8 = await whitespace23.AsReader.MoveInternal3(whitespace23.Factory).ConfigureAwait(false);
            (context, var value8) = await whitespace23.Move2(context).ConfigureAwait(false);
            var valueToken8 = await value8.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken8.TryObject(out var object3));
            var objectStart3 = await object3.Move1(context).ConfigureAwait(false);
            var whitespace24 = await objectStart3.Move4(context).ConfigureAwait(false);
            //var members3 = await whitespace24.AsReader.MoveInternal3(whitespace24.Factory).ConfigureAwait(false);
            (context, var members3) = await whitespace24.Move2(context).ConfigureAwait(false);
            var membersToken3 = await members3.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(membersToken3.TryNone(out var whitespace25));
            //var objectEnd = await whitespace25.AsReader.MoveInternal3(whitespace25.Factory).ConfigureAwait(false);
            (context, var objectEnd) = await whitespace25.Move2(context).ConfigureAwait(false);
            var subsequentMembers7 = await objectEnd.Move4(context).ConfigureAwait(false);
            var subsequentMembersToken7 = await subsequentMembers7.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken7.TryMore(out var subsequentMember8));

            // emptyarray
            var comma7 = await subsequentMember8.Move1(context).ConfigureAwait(false);
            var whitespace26 = await comma7.Move4(context).ConfigureAwait(false);
            //var member8 = await whitespace26.AsReader.MoveInternal3(whitespace26.Factory).ConfigureAwait(false);
            (context, var member8) = await whitespace26.Move2(context).ConfigureAwait(false);
            var string9 = await member8.Move1(context).ConfigureAwait(false);
            var stringDelimiter17 = await string9.Move1(context).ConfigureAwait(false);
            var chars9 = await stringDelimiter17.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter18) = await chars9.Move2(context).ConfigureAwait(false);
            var whitespace27 = await stringDelimiter18.Move4(context).ConfigureAwait(false);
            //var colon8 = await whitespace27.AsReader.MoveInternal3(whitespace27.Factory).ConfigureAwait(false);
            (context, var colon8) = await whitespace27.Move2(context).ConfigureAwait(false);
            var whitespace28 = await colon8.Move4(context).ConfigureAwait(false);
            //var value9 = await whitespace28.AsReader.MoveInternal3(whitespace28.Factory).ConfigureAwait(false);
            (context, var value9) = await whitespace28.Move2(context).ConfigureAwait(false);
            var valueToken9 = await value9.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken9.TryArray(out var array));
            var arrayStart = await array.Move1(context).ConfigureAwait(false);
            var whitespace29 = await arrayStart.Move4(context).ConfigureAwait(false);
            //var arrayElements = await whitespace29.AsReader.MoveInternal3(whitespace29.Factory).ConfigureAwait(false);
            (context, var arrayElements) = await whitespace29.Move2(context).ConfigureAwait(false);
            var arrayElementsToken = await arrayElements.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(arrayElementsToken.TryNone(out var whitespace30));
            //var arrayEnd = await whitespace30.AsReader.MoveInternal3(whitespace30.Factory).ConfigureAwait(false);
            (context, var arrayEnd) = await whitespace30.Move2(context).ConfigureAwait(false);
            var subsequentMembers9 = await arrayEnd.Move4(context).ConfigureAwait(false);
            var subsequentMembersToken9 = await subsequentMembers9.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken9.TryMore(out var subsequentMember10));

            // array
            var comma8 = await subsequentMember10.Move1(context).ConfigureAwait(false);
            var whitespace31 = await comma8.Move4(context).ConfigureAwait(false);
            //var member9 = await whitespace31.AsReader.MoveInternal3(whitespace31.Factory).ConfigureAwait(false);
            (context, var member9) = await whitespace31.Move2(context).ConfigureAwait(false);
            var string10 = await member9.Move1(context).ConfigureAwait(false);
            var stringDelimiter19 = await string10.Move1(context).ConfigureAwait(false);
            var chars10 = await stringDelimiter19.Move4(context).ConfigureAwait(false);
            (context, var stringDelimiter20) = await chars10.Move2(context).ConfigureAwait(false);
            var whitespace32 = await stringDelimiter20.Move4(context).ConfigureAwait(false);
            //var colon9 = await whitespace32.AsReader.MoveInternal3(whitespace32.Factory).ConfigureAwait(false);
            (context, var colon9) = await whitespace32.Move2(context).ConfigureAwait(false);
            var whitespace33 = await colon9.Move4(context).ConfigureAwait(false);
            //var value10 = await whitespace33.AsReader.MoveInternal3(whitespace33.Factory).ConfigureAwait(false);
            (context, var value10) = await whitespace33.Move2(context).ConfigureAwait(false);
            var valueToken10 = await value10.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken10.TryArray(out var array2));
            var arrayStart2 = await array2.Move1(context).ConfigureAwait(false);
            var whitespace34 = await arrayStart2.Move4(context).ConfigureAwait(false);
            //var arrayElements2 = await whitespace34.AsReader.MoveInternal3(whitespace34.Factory).ConfigureAwait(false);
            (context, var arrayElements2) = await whitespace34.Move2(context).ConfigureAwait(false);
            var arrayElementsToken2 = await arrayElements2.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(arrayElementsToken2.TrySome(out var arrayElement));
            var value11 = await arrayElement.Move1(context).ConfigureAwait(false);
            var valueToken11 = await value11.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(valueToken11.TryObject(out var object4));
            var objectStart4 = await object4.Move1(context).ConfigureAwait(false);
            var whitespace35 = await objectStart4.Move4(context).ConfigureAwait(false);
            //var members4 = await whitespace35.AsReader.MoveInternal3(whitespace35.Factory).ConfigureAwait(false);
            (context, var members4) = await whitespace35.Move2(context).ConfigureAwait(false);
            var membersToken4 = await members4.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(membersToken4.TrySome(out var firstMember4));
            Json5.SubsequentArrayElementsReader<Json5.WhitespaceReader<Json5.ArrayEndReader<Json5.SubsequentMembersReader<Json5.WhitespaceReader<Json5.ObjectEndReader<Json5.WhitespaceReader<Nothing>>>>>>> subsequentArrayElements;
            {
                // true
                var nestedMember = await firstMember4.Move1(context).ConfigureAwait(false);
                var nestedString = await nestedMember.Move1(context).ConfigureAwait(false);
                var nestedStringDelimiter = await nestedString.Move1(context).ConfigureAwait(false);
                var nestedChars = await nestedStringDelimiter.Move4(context).ConfigureAwait(false);
                (context, var nestedStringDeliminter2) = await nestedChars.Move2(context).ConfigureAwait(false);
                var nestedWhitespace = await nestedStringDeliminter2.Move4(context).ConfigureAwait(false);
                //var nestedColon = await nestedWhitespace.AsReader.MoveInternal3(nestedWhitespace.Factory).ConfigureAwait(false);
                (context, var nestedColon) = await nestedWhitespace.Move2(context).ConfigureAwait(false);
                var nestedWhispace2 = await nestedColon.Move4(context).ConfigureAwait(false);
                //var nestedValue = await nestedWhispace2.AsReader.MoveInternal3(nestedWhispace2.Factory).ConfigureAwait(false);
                (context, var nestedValue) = await nestedWhispace2.Move2(context).ConfigureAwait(false);
                var nestedValueToken = await nestedValue.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedValueToken.TryTrue(out var nestedTrue));
                (context, var nestedSubsequentMembersReader) = await nestedTrue.Move2(context).ConfigureAwait(false);
                var nestedSubsequentMembersToken = await nestedSubsequentMembersReader.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedSubsequentMembersToken.TryMore(out var nestedsubsequentMemberReader));

                // false
                var nestedcommaReader = await nestedsubsequentMemberReader.Move1(context).ConfigureAwait(false);
                var nestedwhitespaceReader5 = await nestedcommaReader.Move4(context).ConfigureAwait(false);
                //var nestedmemberReader2 = await nestedwhitespaceReader5.AsReader.MoveInternal3(nestedwhitespaceReader5.Factory).ConfigureAwait(false);
                (context, var nestedmemberReader2) = await nestedwhitespaceReader5.Move2(context).ConfigureAwait(false);
                var nestedstringReader2 = await nestedmemberReader2.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiterReader3 = await nestedstringReader2.Move1(context).ConfigureAwait(false);
                var nestedcharsReader2 = await nestedstringDelimiterReader3.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiterReader4) = await nestedcharsReader2.Move2(context).ConfigureAwait(false);
                var nestedwhitespace6 = await nestedstringDelimiterReader4.Move4(context).ConfigureAwait(false);
                //var nestedcolon2 = await nestedwhitespace6.AsReader.MoveInternal3(nestedwhitespace6.Factory).ConfigureAwait(false);
                (context, var nestedcolon2) = await nestedwhitespace6.Move2(context).ConfigureAwait(false);
                var nestedwhitespace7 = await nestedcolon2.Move4(context).ConfigureAwait(false);
                //var nestedvalue3 = await nestedwhitespace7.AsReader.MoveInternal3(nestedwhitespace7.Factory).ConfigureAwait(false);
                (context, var nestedvalue3) = await nestedwhitespace7.Move2(context).ConfigureAwait(false);
                var nestedvalueToken3 = await nestedvalue3.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken3.TryFalse(out var nestedfalse));
                (context, var nestedsubsequentMembers2) = await nestedfalse.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken2 = await nestedsubsequentMembers2.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken2.TryMore(out var nestedsubsequentMember2));

                // 1234
                var nestedcomma2 = await nestedsubsequentMember2.Move1(context).ConfigureAwait(false);
                var nestedwhitespace8 = await nestedcomma2.Move4(context).ConfigureAwait(false);
                //var nestedmember3 = await nestedwhitespace8.AsReader.MoveInternal3(nestedwhitespace8.Factory).ConfigureAwait(false);
                (context, var nestedmember3) = await nestedwhitespace8.Move2(context).ConfigureAwait(false);
                var nestedstring3 = await nestedmember3.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter5 = await nestedstring3.Move1(context).ConfigureAwait(false);
                var nestedchars3 = await nestedstringDelimiter5.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter6) = await nestedchars3.Move2(context).ConfigureAwait(false);
                var nestedwhitespace9 = await nestedstringDelimiter6.Move4(context).ConfigureAwait(false);
                //var nestedcolon3 = await nestedwhitespace9.AsReader.MoveInternal3(nestedwhitespace9.Factory).ConfigureAwait(false);
                (context, var nestedcolon3) = await nestedwhitespace9.Move2(context).ConfigureAwait(false);
                var nestedwhitespace10 = await nestedcolon3.Move4(context).ConfigureAwait(false);
                //var nestedvalue4 = await nestedwhitespace10.AsReader.MoveInternal3(nestedwhitespace10.Factory).ConfigureAwait(false);
                (context, var nestedvalue4) = await nestedwhitespace10.Move2(context).ConfigureAwait(false);
                var nestedvalueToken4 = await nestedvalue4.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken4.TryNumber(out var nestednumber));
                var nestedsign = await nestednumber.Move1(context).ConfigureAwait(false);
                var nestedint = await nestedsign.Move4(context).ConfigureAwait(false);
                (context, var nestedfrac) = await nestedint.Move2(context).ConfigureAwait(false);
                var nestedfracToken = await nestedfrac.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedfracToken.TryAbsent(out var nestedexp));
                var nestedexpToken = await nestedexp.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedexpToken.TryAbsent(out var nestedsubsequentMembers3));
                var nestedsubsequentMembersToken3 = await nestedsubsequentMembers3.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken3.TryMore(out var nestedsubsequentMember3));

                // asdf
                var nestedcomma3 = await nestedsubsequentMember3.Move1(context).ConfigureAwait(false);
                var nestedwhitespace11 = await nestedcomma3.Move4(context).ConfigureAwait(false);
                //var nestedmember4 = await nestedwhitespace11.AsReader.MoveInternal3(nestedwhitespace11.Factory).ConfigureAwait(false);
                (context, var nestedmember4) = await nestedwhitespace11.Move2(context).ConfigureAwait(false);
                var nestedstring4 = await nestedmember4.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter7 = await nestedstring4.Move1(context).ConfigureAwait(false);
                var nestedchars4 = await nestedstringDelimiter7.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter8) = await nestedchars4.Move2(context).ConfigureAwait(false);
                var nestedwhitespace12 = await nestedstringDelimiter8.Move4(context).ConfigureAwait(false);
                //var nestedcolon4 = await nestedwhitespace12.AsReader.MoveInternal3(nestedwhitespace12.Factory).ConfigureAwait(false);
                (context, var nestedcolon4) = await nestedwhitespace12.Move2(context).ConfigureAwait(false);
                var nestedwhitespace13 = await nestedcolon4.Move4(context).ConfigureAwait(false);
                //var nestedvalue5 = await nestedwhitespace13.AsReader.MoveInternal3(nestedwhitespace13.Factory).ConfigureAwait(false);
                (context, var nestedvalue5) = await nestedwhitespace13.Move2(context).ConfigureAwait(false);
                var nestedvalueToken5 = await nestedvalue5.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken5.TryString(out var nestedstring5));
                var nestedstringDelimiter9 = await @nestedstring5.Move1(context).ConfigureAwait(false);
                var nestedchars5 = await nestedstringDelimiter9.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter10) = await nestedchars5.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembers4 = await nestedstringDelimiter10.Move4(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken4 = await nestedsubsequentMembers4.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken4.TryMore(out var nestedsubsequentMember4));

                // null
                var nestedcomma4 = await nestedsubsequentMember4.Move1(context).ConfigureAwait(false);
                var nestedwhitespace14 = await nestedcomma4.Move4(context).ConfigureAwait(false);
                //var nestedmember5 = await nestedwhitespace14.AsReader.MoveInternal3(nestedwhitespace14.Factory).ConfigureAwait(false);
                (context, var nestedmember5) = await nestedwhitespace14.Move2(context).ConfigureAwait(false);
                var nestedstring6 = await nestedmember5.Move1(context).ConfigureAwait(false);
                var nestedstringDelimiter11 = await nestedstring6.Move1(context).ConfigureAwait(false);
                var nestedchars6 = await nestedstringDelimiter11.Move4(context).ConfigureAwait(false);
                (context, var nestedstringDelimiter12) = await nestedchars6.Move2(context).ConfigureAwait(false);
                var nestedwhitespace15 = await nestedstringDelimiter12.Move4(context).ConfigureAwait(false);
                //var nestedcolon5 = await nestedwhitespace15.AsReader.MoveInternal3(nestedwhitespace15.Factory).ConfigureAwait(false);
                (context, var nestedcolon5) = await nestedwhitespace15.Move2(context).ConfigureAwait(false);
                var nestedwhitespace16 = await nestedcolon5.Move4(context).ConfigureAwait(false);
                //var nestedvalue6 = await nestedwhitespace16.AsReader.MoveInternal3(nestedwhitespace16.Factory).ConfigureAwait(false);
                (context, var nestedvalue6) = await nestedwhitespace16.Move2(context).ConfigureAwait(false);
                var nestedvalueToken6 = await nestedvalue6.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedvalueToken6.TryNull(out var nestednull));

                (context, var nestedsubsequentMembers5) = await @nestednull.Move2(context).ConfigureAwait(false);
                var nestedsubsequentMembersToken5 = await nestedsubsequentMembers5.Move3(context).ConfigureAwait(false);
                Assert.IsTrue(nestedsubsequentMembersToken5.TryNone(out var nestedsubsequentMember5));

                //var nestedobjectEnd = await nestedsubsequentMember5.AsReader.MoveInternal3(nestedsubsequentMember5.Factory).ConfigureAwait(false);
                (context, var nestedobjectEnd) = await nestedsubsequentMember5.Move2(context).ConfigureAwait(false);
                subsequentArrayElements = await nestedobjectEnd.Move4(context).ConfigureAwait(false);
            }

            var subsequentArrayElementsToken = await subsequentArrayElements.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentArrayElementsToken.TryNone(out var whitespace36));
            //var arrayEnd2 = await whitespace36.AsReader.MoveInternal3(whitespace36.Factory).ConfigureAwait(false);
            (context, var arrayEnd2) = await whitespace36.Move2(context).ConfigureAwait(false);
            var subsequentMembers10 = await arrayEnd2.Move4(context).ConfigureAwait(false);
            var subsequentMembersToken10 = await subsequentMembers10.Move3(context).ConfigureAwait(false);
            Assert.IsTrue(subsequentMembersToken10.TryNone(out var whitespace37));
            //var objectEnd2 = await whitespace37.AsReader.MoveInternal3(whitespace37.Factory).ConfigureAwait(false);
            (context, var objectEnd2) = await whitespace37.Move2(context).ConfigureAwait(false);
            var whitespace38 = await objectEnd2.Move4(context).ConfigureAwait(false);
            //// TODO remove this when you fix moveinternal3
            ////context.Read().ConfigureAwait(false).GetAwaiter().GetResult();
            (context, var nothing) = await whitespace38.Move2(context).ConfigureAwait(false);

            Assert.AreEqual(new Nothing(), nothing);


            //// TODO now that you're not using `ref struct`, you can have intermediate helper methods
            ////Assert.AreEqual($"{stream.Length}:0", $"{stream.Position}:{context.CurrentByteIndex}");
            /*Assert.AreEqual(stream.Length, stream.Position);
            Assert.AreEqual(0, context.CurrentByteIndex);*/
        }

        [TestMethod]
        public async Task V2Broad2()
        {
            //// TODO it seems that `readonly` ref structs are a *sometimes* a liability for performance; you will probably need to do a whole bunch of testing to decide which ones should be `readonly`; just keep them all *not* `readonly` for now, though
            //// TODO also test `readonly` on any task `ref struct`s that you created

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var iterations = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < iterations; ++i)
                {
                    await DoWork(stream).ConfigureAwait(false);
                }

                Console.WriteLine(timer.ElapsedTicks);
            }
        }

        /*public static TestExtensions.MoveInternal2Task<JsonReader, WhitespaceReader2<ValueReader2<WhitespaceReader2<Nothing>>>> DoWork2(Stream stream)
        {
            stream.Position = 0;
            var reader = new Json2.JsonReader(stream);

            var context = reader.Context;
            return reader.AsReader.MoveInternal2(reader.Factory, ref context);
        }*/

        public static async Task DoWork(Stream stream)
        {
            stream.Position = 0;

            var context = new ReaderContext(stream, new byte[20], 0, 0);
            await context.Read().ConfigureAwait(false);

            var reader = new Json2.JsonReader(stream);
            var whitespaceReaderFactory = await reader.AsReader.MoveInternal2(reader.Factory, ref context).ConfigureAwait(false);
            var whitespaceReader = whitespaceReaderFactory();

            var valueReaderFactory = await whitespaceReader.AsReader.MoveInternal3(ref context, whitespaceReaderFactory).ConfigureAwait(false);

            var valueReader = valueReaderFactory();
            var valueTokenFactory = await valueReader.AsReader.MoveInternal2(valueReaderFactory, ref context).ConfigureAwait(false);
            var valueToken = valueTokenFactory();
            if (!valueToken.TryObject(out var objectFactory))
            {
                throw new Exception("TODO");
            }

            var @object = objectFactory();
            var objectstartfactory = await @object.AsReader.MoveInternal2(objectFactory, ref context).ConfigureAwait(false);
            var objectstart = objectstartfactory();
            var whitespaceReader2 = await objectstart.MoveInternal1().ConfigureAwait(false);

            /*var objectReader = await @object.MoveInternal1().ConfigureAwait(false);
            var whitespaceReader2 = await objectReader.MoveInternal1().ConfigureAwait(false);*/
            //var membersReader = await whitespaceReader2.AsReader.MoveInternal3(whitespaceReader2.Factory).ConfigureAwait(false);
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
            //var colonReader = await whitespaceReader3.AsReader.MoveInternal3(whitespaceReader3.Factory).ConfigureAwait(false);
            var colonReader = await whitespaceReader3.MoveInternal1().ConfigureAwait(false);
            var whitespaceReader4 = await colonReader.MoveInternal1().ConfigureAwait(false);
            //var valueReader2 = await whitespaceReader4.AsReader.MoveInternal3(whitespaceReader4.Factory).ConfigureAwait(false);
            var valueReader2 = await whitespaceReader4.MoveInternal1().ConfigureAwait(false);
            var valueToken2 = await valueReader2.MoveInternal1().ConfigureAwait(false);
            if (!valueToken2.TryTrue().TryGetValue(out var @true))
            {
                throw new Exception("TODO");
            }

            var subsequentMembersReader = await @true.MoveInternal1().ConfigureAwait(false);

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
            //var memberReader2 = await whitespaceReader5.AsReader.MoveInternal3(whitespaceReader5.Factory).ConfigureAwait(false);
            var memberReader2 = await whitespaceReader5.MoveInternal1().ConfigureAwait(false);
            var stringReader2 = await memberReader2.MoveInternal1().ConfigureAwait(false);
            var stringDelimiterReader3 = await stringReader2.MoveInternal1().ConfigureAwait(false);
            var charsReader2 = await stringDelimiterReader3.MoveInternal1().ConfigureAwait(false);
            var stringDelimiterReader4 = await charsReader2.MoveInternal1().ConfigureAwait(false);
            var whitespace6 = await stringDelimiterReader4.MoveInternal1().ConfigureAwait(false);
            //var colon2 = await whitespace6.AsReader.MoveInternal3(whitespace6.Factory).ConfigureAwait(false);
            var colon2 = await whitespace6.MoveInternal1().ConfigureAwait(false);
            var whitespace7 = await colon2.MoveInternal1().ConfigureAwait(false);
            //var value3 = await whitespace7.AsReader.MoveInternal3(whitespace7.Factory).ConfigureAwait(false);
            var value3 = await whitespace7.MoveInternal1().ConfigureAwait(false);
            var valueToken3 = await value3.MoveInternal1().ConfigureAwait(false);
            if (!valueToken3.TryFalse().TryGetValue(out var @false))
            {
                throw new Exception("TODO");
            }

            //// TODO
            /*context.CurrentByteIndex = 15;
            if (!@false.TryGetValue3(ref context, out _, out _, out _))
            {
                throw new Exception("tODO");
            }

            if (!@false.TryMove3(ref context, out var more1))
            {
                throw new Exception("tODO");
            }

            var subsequentMembers2 = more1();*/
            var subsequentMembers2 = await @false.MoveInternal1().ConfigureAwait(false);
            var subsequentMembersToken2 = subsequentMembers2.TryMove(out read);
            if (!read)
            {
                await subsequentMembers2.Read().ConfigureAwait(false);
                subsequentMembersToken2 = subsequentMembers2.TryMove(out read);
            }

            var subsequentMember2 = subsequentMembersToken2.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // 1234
            var comma2 = await subsequentMember2.MoveInternal1().ConfigureAwait(false);
            var whitespace8 = await comma2.MoveInternal1().ConfigureAwait(false);
            //var member3 = await whitespace8.AsReader.MoveInternal3(whitespace8.Factory).ConfigureAwait(false);
            var member3 = await whitespace8.MoveInternal1().ConfigureAwait(false);
            var string3 = await member3.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter5 = await string3.MoveInternal1().ConfigureAwait(false);
            var chars3 = await stringDelimiter5.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter6 = await chars3.MoveInternal1().ConfigureAwait(false);
            var whitespace9 = await stringDelimiter6.MoveInternal1().ConfigureAwait(false);
            var colon3 = await whitespace9.MoveInternal1().ConfigureAwait(false);
            //var colon3 = await whitespace9.AsReader.MoveInternal3(whitespace9.Factory).ConfigureAwait(false);
            var whitespace10 = await colon3.MoveInternal1().ConfigureAwait(false);
            //var value4 = await whitespace10.AsReader.MoveInternal3(whitespace10.Factory).ConfigureAwait(false);
            var value4 = await whitespace10.MoveInternal1().ConfigureAwait(false);
            var valueToken4 = await value4.MoveInternal1().ConfigureAwait(false);
            if (!valueToken4.TryNumber().TryGetValue(out var number))
            {
                throw new Exception("TODO");
            }

            var sign = await number.MoveInternal1().ConfigureAwait(false);
            var @int = await sign.MoveInternal1().ConfigureAwait(false);
            var frac = await @int.MoveInternal1().ConfigureAwait(false);
            var exp = await frac.MoveInternal1().ConfigureAwait(false);
            var expToken = exp.TryMove(out read);
            if (!read)
            {
                await exp.Read().ConfigureAwait(false);
                expToken = exp.TryMove(out read);
            }

            var subsequentMembers3 = expToken.Apply(
                _ => _,
                _ => throw new Exception("tODO"));
            var subsequentMembersToken3 = subsequentMembers3.TryMove(out read);
            if (!read)
            {
                await subsequentMembers3.Read().ConfigureAwait(false);
                subsequentMembersToken3 = subsequentMembers3.TryMove(out read);
            }

            var subsequentMember3 = subsequentMembersToken3.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // asdf
            var comma3 = await subsequentMember3.MoveInternal1().ConfigureAwait(false);
            var whitespace11 = await comma3.MoveInternal1().ConfigureAwait(false);
            //var member4 = await whitespace11.AsReader.MoveInternal3(whitespace11.Factory).ConfigureAwait(false);
            var member4 = await whitespace11.MoveInternal1().ConfigureAwait(false);
            var string4 = await member4.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter7 = await string4.MoveInternal1().ConfigureAwait(false);
            var chars4 = await stringDelimiter7.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter8 = await chars4.MoveInternal1().ConfigureAwait(false);
            var whitespace12 = await stringDelimiter8.MoveInternal1().ConfigureAwait(false);
            //var colon4 = await whitespace12.AsReader.MoveInternal3(whitespace12.Factory).ConfigureAwait(false);
            var colon4 = await whitespace12.MoveInternal1().ConfigureAwait(false);
            var whitespace13 = await colon4.MoveInternal1().ConfigureAwait(false);
            //var value5 = await whitespace13.AsReader.MoveInternal3(whitespace13.Factory).ConfigureAwait(false);
            var value5 = await whitespace13.MoveInternal1().ConfigureAwait(false);
            var valueToken5 = await value5.MoveInternal1().ConfigureAwait(false);
            if (!valueToken5.TryString().TryGetValue(out var @string5))
            {
                throw new Exception("TODO");
            }

            var stringDelimiter9 = await @string5.MoveInternal1().ConfigureAwait(false);
            var chars5 = await stringDelimiter9.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter10 = await chars5.MoveInternal1().ConfigureAwait(false);
            var subsequentMembers4 = await stringDelimiter10.MoveInternal1().ConfigureAwait(false);

            var subsequentMembersToken4 = subsequentMembers4.TryMove(out read);
            if (!read)
            {
                await subsequentMembers4.Read().ConfigureAwait(false);
                subsequentMembersToken4 = subsequentMembers4.TryMove(out read);
            }

            var subsequentMember4 = subsequentMembersToken4.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // null
            var comma4 = await subsequentMember4.MoveInternal1().ConfigureAwait(false);
            var whitespace14 = await comma4.MoveInternal1().ConfigureAwait(false);
            //var member5 = await whitespace14.AsReader.MoveInternal3(whitespace14.Factory).ConfigureAwait(false);
            var member5 = await whitespace14.MoveInternal1().ConfigureAwait(false);
            var string6 = await member5.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter11 = await string6.MoveInternal1().ConfigureAwait(false);
            var chars6 = await stringDelimiter11.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter12 = await chars6.MoveInternal1().ConfigureAwait(false);
            var whitespace15 = await stringDelimiter12.MoveInternal1().ConfigureAwait(false);
            //var colon5 = await whitespace15.AsReader.MoveInternal3(whitespace15.Factory).ConfigureAwait(false);
            var colon5 = await whitespace15.MoveInternal1().ConfigureAwait(false);
            var whitespace16 = await colon5.MoveInternal1().ConfigureAwait(false);
            //var value6 = await whitespace16.AsReader.MoveInternal3(whitespace16.Factory).ConfigureAwait(false);
            var value6 = await whitespace16.MoveInternal1().ConfigureAwait(false);
            var valueToken6 = await value6.MoveInternal1().ConfigureAwait(false);
            if (!valueToken6.TryNull().TryGetValue(out var @null))
            {
                throw new Exception("TODO");
            }

            var subsequentMembers5 = await @null.MoveInternal1().ConfigureAwait(false);
            var subsequentMembersToken5 = subsequentMembers5.TryMove(out read);
            if (!read)
            {
                await subsequentMembers5.Read().ConfigureAwait(false);
                subsequentMembersToken5 = subsequentMembers5.TryMove(out read);
            }

            var subsequentMember5 = subsequentMembersToken5.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // object
            var comma5 = await subsequentMember5.MoveInternal1().ConfigureAwait(false);
            var whitespace17 = await comma5.MoveInternal1().ConfigureAwait(false);
            //var member6 = await whitespace17.AsReader.MoveInternal3(whitespace17.Factory).ConfigureAwait(false);
            var member6 = await whitespace17.MoveInternal1().ConfigureAwait(false);
            var string7 = await member6.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter13 = await string7.MoveInternal1().ConfigureAwait(false);
            var chars7 = await stringDelimiter13.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter14 = await chars7.MoveInternal1().ConfigureAwait(false);
            var whitespace18 = await stringDelimiter14.MoveInternal1().ConfigureAwait(false);
            //var colon6 = await whitespace18.AsReader.MoveInternal3(whitespace18.Factory).ConfigureAwait(false);
            var colon6 = await whitespace18.MoveInternal1().ConfigureAwait(false);
            var whitespace19 = await colon6.MoveInternal1().ConfigureAwait(false);
            //var value7 = await whitespace19.AsReader.MoveInternal3(whitespace19.Factory).ConfigureAwait(false);
            var value7 = await whitespace19.MoveInternal1().ConfigureAwait(false);
            var valueToken7 = await value7.MoveInternal1().ConfigureAwait(false);
            if (!valueToken7.TryObject().TryGetValue(out var object2))
            {
                throw new Exception("TODO");
            }

            var objectStart2 = await object2.MoveInternal1().ConfigureAwait(false);
            var whitespace20 = await objectStart2.MoveInternal1().ConfigureAwait(false);
            //var nestedMembers = await whitespace20.AsReader.MoveInternal3(whitespace20.Factory).ConfigureAwait(false);
            var nestedMembers = await whitespace20.MoveInternal1().ConfigureAwait(false);
            var nestedMembersToken = nestedMembers.TryMove(out read);
            if (!read)
            {
                await nestedMembers.Read().ConfigureAwait(false);
                nestedMembersToken = nestedMembers.TryMove(out read);
            }

            var nestedFirstMember = nestedMembersToken.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            SubsequentMembersReader<WhitespaceReader<ObjectEndReader<WhitespaceReader2<Nothing>>>> subsequentMembers6;
            {
                // true
                var nestedMember = await nestedFirstMember.MoveInternal1().ConfigureAwait(false);
                var nestedString = await nestedMember.MoveInternal1().ConfigureAwait(false);
                var nestedStringDelimiter = await nestedString.MoveInternal1().ConfigureAwait(false);
                var nestedChars = await nestedStringDelimiter.MoveInternal1().ConfigureAwait(false);
                var nestedStringDeliminter2 = await nestedChars.MoveInternal1().ConfigureAwait(false);
                var nestedWhitespace = await nestedStringDeliminter2.MoveInternal1().ConfigureAwait(false);
                //var nestedColon = await nestedWhitespace.AsReader.MoveInternal3(nestedWhitespace.Factory).ConfigureAwait(false);
                var nestedColon = await nestedWhitespace.MoveInternal1().ConfigureAwait(false);
                var nestedWhispace2 = await nestedColon.MoveInternal1().ConfigureAwait(false);
                //var nestedValue = await nestedWhispace2.AsReader.MoveInternal3(nestedWhispace2.Factory).ConfigureAwait(false);
                var nestedValue = await nestedWhispace2.MoveInternal1().ConfigureAwait(false);
                var nestedValueToken = await nestedValue.MoveInternal1().ConfigureAwait(false);
                if (!nestedValueToken.TryTrue().TryGetValue(out var nestedTrue))
                {
                    throw new Exception("TODO");
                }

                var nestedSubsequentMembersReader = await nestedTrue.MoveInternal1().ConfigureAwait(false);
                var nestedSubsequentMembersToken = nestedSubsequentMembersReader.TryMove(out read);
                if (!read)
                {
                    await nestedSubsequentMembersReader.Read().ConfigureAwait(false);
                    nestedSubsequentMembersToken = nestedSubsequentMembersReader.TryMove(out read);
                }

                var nestedsubsequentMemberReader = nestedSubsequentMembersToken.Apply(
                    _ => throw new Exception("TODO"),
                    more => more);

                // false
                var nestedcommaReader = await nestedsubsequentMemberReader.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespaceReader5 = await nestedcommaReader.MoveInternal1().ConfigureAwait(false);
                //var nestedmemberReader2 = await nestedwhitespaceReader5.AsReader.MoveInternal3(nestedwhitespaceReader5.Factory).ConfigureAwait(false);
                var nestedmemberReader2 = await nestedwhitespaceReader5.MoveInternal1().ConfigureAwait(false);
                var nestedstringReader2 = await nestedmemberReader2.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiterReader3 = await nestedstringReader2.MoveInternal1().ConfigureAwait(false);
                var nestedcharsReader2 = await nestedstringDelimiterReader3.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiterReader4 = await nestedcharsReader2.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace6 = await nestedstringDelimiterReader4.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon2 = await nestedwhitespace6.AsReader.MoveInternal3(nestedwhitespace6.Factory).ConfigureAwait(false);
                var nestedcolon2 = await nestedwhitespace6.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace7 = await nestedcolon2.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue3 = await nestedwhitespace7.AsReader.MoveInternal3(nestedwhitespace7.Factory).ConfigureAwait(false);
                var nestedvalue3 = await nestedwhitespace7.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken3 = await nestedvalue3.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken3.TryFalse().TryGetValue(out var nestedfalse))
                {
                    throw new Exception("TODO");
                }

                //// TODO
                /*context.CurrentByteIndex = 3;
                if (!nestedfalse.TryGetValue3(ref context, out _, out _, out _))
                {
                    throw new Exception("tODO");
                }

                if (!nestedfalse.TryMove3(ref context, out var _more1))
                {
                    throw new Exception("tODO");
                }

                var nestedsubsequentMembers2 = _more1();*/
                var nestedsubsequentMembers2 = await nestedfalse.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembersToken2 = nestedsubsequentMembers2.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers2.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken2 = nestedsubsequentMembers2.TryMove(out read);
                }

                var nestedsubsequentMember2 = nestedsubsequentMembersToken2.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // 1234
                var nestedcomma2 = await nestedsubsequentMember2.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace8 = await nestedcomma2.MoveInternal1().ConfigureAwait(false);
                //var nestedmember3 = await nestedwhitespace8.AsReader.MoveInternal3(nestedwhitespace8.Factory).ConfigureAwait(false);
                var nestedmember3 = await nestedwhitespace8.MoveInternal1().ConfigureAwait(false);
                var nestedstring3 = await nestedmember3.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter5 = await nestedstring3.MoveInternal1().ConfigureAwait(false);
                var nestedchars3 = await nestedstringDelimiter5.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter6 = await nestedchars3.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace9 = await nestedstringDelimiter6.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon3 = await nestedwhitespace9.AsReader.MoveInternal3(nestedwhitespace9.Factory).ConfigureAwait(false);
                var nestedcolon3 = await nestedwhitespace9.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace10 = await nestedcolon3.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue4 = await nestedwhitespace10.AsReader.MoveInternal3(nestedwhitespace10.Factory).ConfigureAwait(false);
                var nestedvalue4 = await nestedwhitespace10.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken4 = await nestedvalue4.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken4.TryNumber().TryGetValue(out var nestednumber))
                {
                    throw new Exception("TODO");
                }

                var nestedsign = await nestednumber.MoveInternal1().ConfigureAwait(false);
                var nestedint = await nestedsign.MoveInternal1().ConfigureAwait(false);
                var nestedfrac = await nestedint.MoveInternal1().ConfigureAwait(false);
                var nestedexp = await nestedfrac.MoveInternal1().ConfigureAwait(false);
                var nestedexpToken = nestedexp.TryMove(out read);
                if (!read)
                {
                    await nestedexp.Read().ConfigureAwait(false);
                    nestedexpToken = nestedexp.TryMove(out read);
                }

                var nestedsubsequentMembers3 = nestedexpToken.Apply(
                    _ => _,
                    _ => throw new Exception("tODO"));
                var nestedsubsequentMembersToken3 = nestedsubsequentMembers3.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers3.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken3 = nestedsubsequentMembers3.TryMove(out read);
                }

                var nestedsubsequentMember3 = nestedsubsequentMembersToken3.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // asdf
                var nestedcomma3 = await nestedsubsequentMember3.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace11 = await nestedcomma3.MoveInternal1().ConfigureAwait(false);
                //var nestedmember4 = await nestedwhitespace11.AsReader.MoveInternal3(nestedwhitespace11.Factory).ConfigureAwait(false);
                var nestedmember4 = await nestedwhitespace11.MoveInternal1().ConfigureAwait(false);
                var nestedstring4 = await nestedmember4.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter7 = await nestedstring4.MoveInternal1().ConfigureAwait(false);
                var nestedchars4 = await nestedstringDelimiter7.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter8 = await nestedchars4.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace12 = await nestedstringDelimiter8.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon4 = await nestedwhitespace12.AsReader.MoveInternal3(nestedwhitespace12.Factory).ConfigureAwait(false);
                var nestedcolon4 = await nestedwhitespace12.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace13 = await nestedcolon4.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue5 = await nestedwhitespace13.AsReader.MoveInternal3(nestedwhitespace13.Factory).ConfigureAwait(false);
                var nestedvalue5 = await nestedwhitespace13.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken5 = await nestedvalue5.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken5.TryString().TryGetValue(out var nestedstring5))
                {
                    throw new Exception("TODO");
                }

                var nestedstringDelimiter9 = await @nestedstring5.MoveInternal1().ConfigureAwait(false);
                var nestedchars5 = await nestedstringDelimiter9.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter10 = await nestedchars5.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembers4 = await nestedstringDelimiter10.MoveInternal1().ConfigureAwait(false);

                var nestedsubsequentMembersToken4 = nestedsubsequentMembers4.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers4.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken4 = nestedsubsequentMembers4.TryMove(out read);
                }

                var nestedsubsequentMember4 = nestedsubsequentMembersToken4.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // null
                var nestedcomma4 = await nestedsubsequentMember4.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace14 = await nestedcomma4.MoveInternal1().ConfigureAwait(false);
                //var nestedmember5 = await nestedwhitespace14.AsReader.MoveInternal3(nestedwhitespace14.Factory).ConfigureAwait(false);
                var nestedmember5 = await nestedwhitespace14.MoveInternal1().ConfigureAwait(false);
                var nestedstring6 = await nestedmember5.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter11 = await nestedstring6.MoveInternal1().ConfigureAwait(false);
                var nestedchars6 = await nestedstringDelimiter11.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter12 = await nestedchars6.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace15 = await nestedstringDelimiter12.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon5 = await nestedwhitespace15.AsReader.MoveInternal3(nestedwhitespace15.Factory).ConfigureAwait(false);
                var nestedcolon5 = await nestedwhitespace15.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace16 = await nestedcolon5.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue6 = await nestedwhitespace16.AsReader.MoveInternal3(nestedwhitespace16.Factory).ConfigureAwait(false);
                var nestedvalue6 = await nestedwhitespace16.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken6 = await nestedvalue6.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken6.TryNull().TryGetValue(out var nestednull))
                {
                    throw new Exception("TODO");
                }

                var nestedsubsequentMembers5 = await @nestednull.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembersToken5 = nestedsubsequentMembers5.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers5.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken5 = nestedsubsequentMembers5.TryMove(out read);
                }

                var nestedsubsequentMember5 = nestedsubsequentMembersToken5.Apply(
                    _ => _,
                    _ => throw new Exception("TODO"));

                //var nestedobjectEnd = await nestedsubsequentMember5.AsReader.MoveInternal3(nestedsubsequentMember5.Factory).ConfigureAwait(false);
                var nestedobjectEnd = await nestedsubsequentMember5.MoveInternal1().ConfigureAwait(false);
                subsequentMembers6 = await nestedobjectEnd.MoveInternal1().ConfigureAwait(false);
            }

            var subsequentMembersToken6 = subsequentMembers6.TryMove(out read);
            if (!read)
            {
                await subsequentMembers6.Read().ConfigureAwait(false);
                subsequentMembersToken6 = subsequentMembers6.TryMove(out read);
            }

            var subsequentMember6 = subsequentMembersToken6.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // emptyobject
            var comma6 = await subsequentMember6.MoveInternal1().ConfigureAwait(false);
            var whitespace21 = await comma6.MoveInternal1().ConfigureAwait(false);
            //var member7 = await whitespace21.AsReader.MoveInternal3(whitespace21.Factory).ConfigureAwait(false);
            var member7 = await whitespace21.MoveInternal1().ConfigureAwait(false);
            var string8 = await member7.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter15 = await string8.MoveInternal1().ConfigureAwait(false);
            var chars8 = await stringDelimiter15.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter16 = await chars8.MoveInternal1().ConfigureAwait(false);
            var whitespace22 = await stringDelimiter16.MoveInternal1().ConfigureAwait(false);
            //var colon7 = await whitespace22.AsReader.MoveInternal3(whitespace22.Factory).ConfigureAwait(false);
            var colon7 = await whitespace22.MoveInternal1().ConfigureAwait(false);
            var whitespace23 = await colon7.MoveInternal1().ConfigureAwait(false);
            //var value8 = await whitespace23.AsReader.MoveInternal3(whitespace23.Factory).ConfigureAwait(false);
            var value8 = await whitespace23.MoveInternal1().ConfigureAwait(false);
            var valueToken8 = await value8.MoveInternal1().ConfigureAwait(false);
            if (!valueToken8.TryObject().TryGetValue(out var object3))
            {
                throw new Exception("TODO");
            }

            var objectStart3 = await object3.MoveInternal1().ConfigureAwait(false);
            var whitespace24 = await objectStart3.MoveInternal1().ConfigureAwait(false);
            //var members3 = await whitespace24.AsReader.MoveInternal3(whitespace24.Factory).ConfigureAwait(false);
            var members3 = await whitespace24.MoveInternal1().ConfigureAwait(false);
            var membersToken3 = members3.TryMove(out read);
            if (!read)
            {
                await members3.Read().ConfigureAwait(false);
                membersToken3 = members3.TryMove(out read);
            }

            var whitespace25 = membersToken3.Apply(_ => _, _ => throw new Exception("TODO"));
            //var objectEnd = await whitespace25.AsReader.MoveInternal3(whitespace25.Factory).ConfigureAwait(false);
            var objectEnd = await whitespace25.MoveInternal1().ConfigureAwait(false);
            var subsequentMembers7 = await objectEnd.MoveInternal1().ConfigureAwait(false);
            var subsequentMembersToken7 = subsequentMembers7.TryMove(out read);
            if (!read)
            {
                await subsequentMembers7.Read().ConfigureAwait(false);
                subsequentMembersToken7 = subsequentMembers7.TryMove(out read);
            }

            var subsequentMember8 = subsequentMembersToken7.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // emptyarray
            var comma7 = await subsequentMember8.MoveInternal1().ConfigureAwait(false);
            var whitespace26 = await comma7.MoveInternal1().ConfigureAwait(false);
            //var member8 = await whitespace26.AsReader.MoveInternal3(whitespace26.Factory).ConfigureAwait(false);
            var member8 = await whitespace26.MoveInternal1().ConfigureAwait(false);
            var string9 = await member8.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter17 = await string9.MoveInternal1().ConfigureAwait(false);
            var chars9 = await stringDelimiter17.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter18 = await chars9.MoveInternal1().ConfigureAwait(false);
            var whitespace27 = await stringDelimiter18.MoveInternal1().ConfigureAwait(false);
            //var colon8 = await whitespace27.AsReader.MoveInternal3(whitespace27.Factory).ConfigureAwait(false);
            var colon8 = await whitespace27.MoveInternal1().ConfigureAwait(false);
            var whitespace28 = await colon8.MoveInternal1().ConfigureAwait(false);
            //var value9 = await whitespace28.AsReader.MoveInternal3(whitespace28.Factory).ConfigureAwait(false);
            var value9 = await whitespace28.MoveInternal1().ConfigureAwait(false);
            var valueToken9 = await value9.MoveInternal1().ConfigureAwait(false);
            if (!valueToken9.TryArray().TryGetValue(out var array))
            {
                throw new Exception("TODO");
            }

            var arrayStart = await array.MoveInternal1().ConfigureAwait(false);
            var whitespace29 = await arrayStart.MoveInternal1().ConfigureAwait(false);
            //var arrayElements = await whitespace29.AsReader.MoveInternal3(whitespace29.Factory).ConfigureAwait(false);
            var arrayElements = await whitespace29.MoveInternal1().ConfigureAwait(false);
            var arrayElementsToken = arrayElements.TryMove(out read);
            if (!read)
            {
                await arrayElements.Read().ConfigureAwait(false);
                arrayElementsToken = arrayElements.TryMove(out read);
            }

            var whitespace30 = arrayElementsToken.Apply(_ => _, _ => throw new Exception("TODO"));
            //var arrayEnd = await whitespace30.AsReader.MoveInternal3(whitespace30.Factory).ConfigureAwait(false);
            var arrayEnd = await whitespace30.MoveInternal1().ConfigureAwait(false);
            var subsequentMembers9 = await arrayEnd.MoveInternal1().ConfigureAwait(false);
            var subsequentMembersToken9 = subsequentMembers9.TryMove(out read);
            if (!read)
            {
                await subsequentMembers9.Read().ConfigureAwait(false);
                subsequentMembersToken9 = subsequentMembers9.TryMove(out read);
            }

            var subsequentMember10 = subsequentMembersToken9.Apply(
                _ => throw new Exception("TODO"),
                _ => _);

            // array
            var comma8 = await subsequentMember10.MoveInternal1().ConfigureAwait(false);
            var whitespace31 = await comma8.MoveInternal1().ConfigureAwait(false);
            //var member9 = await whitespace31.AsReader.MoveInternal3(whitespace31.Factory).ConfigureAwait(false);
            var member9 = await whitespace31.MoveInternal1().ConfigureAwait(false);
            var string10 = await member9.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter19 = await string10.MoveInternal1().ConfigureAwait(false);
            var chars10 = await stringDelimiter19.MoveInternal1().ConfigureAwait(false);
            var stringDelimiter20 = await chars10.MoveInternal1().ConfigureAwait(false);
            var whitespace32 = await stringDelimiter20.MoveInternal1().ConfigureAwait(false);
            //var colon9 = await whitespace32.AsReader.MoveInternal3(whitespace32.Factory).ConfigureAwait(false);
            var colon9 = await whitespace32.MoveInternal1().ConfigureAwait(false);
            var whitespace33 = await colon9.MoveInternal1().ConfigureAwait(false);
            //var value10 = await whitespace33.AsReader.MoveInternal3(whitespace33.Factory).ConfigureAwait(false);
            var value10 = await whitespace33.MoveInternal1().ConfigureAwait(false);
            var valueToken10 = await value10.MoveInternal1().ConfigureAwait(false);
            if (!valueToken10.TryArray().TryGetValue(out var array2))
            {
                throw new Exception("TODO");
            }

            var arrayStart2 = await array2.MoveInternal1().ConfigureAwait(false);
            var whitespace34 = await arrayStart2.MoveInternal1().ConfigureAwait(false);
            //var arrayElements2 = await whitespace34.AsReader.MoveInternal3(whitespace34.Factory).ConfigureAwait(false);
            var arrayElements2 = await whitespace34.MoveInternal1().ConfigureAwait(false);
            var arrayElementsToken2 = arrayElements2.TryMove(out read);
            if (!read)
            {
                await arrayElements2.Read().ConfigureAwait(false);
                arrayElementsToken2 = arrayElements2.TryMove(out read);
            }

            var arrayElement = arrayElementsToken2.Apply(_ => throw new Exception("TODO"), _ => _);
            var value11 = await arrayElement.MoveInternal1().ConfigureAwait(false);
            var valueToken11 = await value11.MoveInternal1().ConfigureAwait(false);
            if (!valueToken11.TryObject().TryGetValue(out var object4))
            {
                throw new Exception("TODO");
            }

            var objectStart4 = await object4.MoveInternal1().ConfigureAwait(false);
            var whitespace35 = await objectStart4.MoveInternal1().ConfigureAwait(false);
            //var members4 = await whitespace35.AsReader.MoveInternal3(whitespace35.Factory).ConfigureAwait(false);
            var members4 = await whitespace35.MoveInternal1().ConfigureAwait(false);
            var membersToken4 = members4.TryMove(out read);
            if (!read)
            {
                await members4.Read().ConfigureAwait(false);
                membersToken4 = members4.TryMove(out read);
            }

            var firstMember4 = membersToken4.Apply(_ => throw new Exception("TODO"), _ => _);
            SubsequentArrayElementsReader
                <
                    WhitespaceReader<ArrayEndReader<SubsequentMembersReader<WhitespaceReader<ObjectEndReader<WhitespaceReader2<Nothing>>>>>>
                >
                subsequentArrayElements;
            {
                // true
                var nestedMember = await firstMember4.MoveInternal1().ConfigureAwait(false);
                var nestedString = await nestedMember.MoveInternal1().ConfigureAwait(false);
                var nestedStringDelimiter = await nestedString.MoveInternal1().ConfigureAwait(false);
                var nestedChars = await nestedStringDelimiter.MoveInternal1().ConfigureAwait(false);
                var nestedStringDeliminter2 = await nestedChars.MoveInternal1().ConfigureAwait(false);
                var nestedWhitespace = await nestedStringDeliminter2.MoveInternal1().ConfigureAwait(false);
                //var nestedColon = await nestedWhitespace.AsReader.MoveInternal3(nestedWhitespace.Factory).ConfigureAwait(false);
                var nestedColon = await nestedWhitespace.MoveInternal1().ConfigureAwait(false);
                var nestedWhispace2 = await nestedColon.MoveInternal1().ConfigureAwait(false);
                //var nestedValue = await nestedWhispace2.AsReader.MoveInternal3(nestedWhispace2.Factory).ConfigureAwait(false);
                var nestedValue = await nestedWhispace2.MoveInternal1().ConfigureAwait(false);
                var nestedValueToken = await nestedValue.MoveInternal1().ConfigureAwait(false);
                if (!nestedValueToken.TryTrue().TryGetValue(out var nestedTrue))
                {
                    throw new Exception("TODO");
                }

                var nestedSubsequentMembersReader = await nestedTrue.MoveInternal1().ConfigureAwait(false);
                var nestedSubsequentMembersToken = nestedSubsequentMembersReader.TryMove(out read);
                if (!read)
                {
                    await nestedSubsequentMembersReader.Read().ConfigureAwait(false);
                    nestedSubsequentMembersToken = nestedSubsequentMembersReader.TryMove(out read);
                }

                var nestedsubsequentMemberReader = nestedSubsequentMembersToken.Apply(
                    _ => throw new Exception("TODO"),
                    more => more);

                // false
                var nestedcommaReader = await nestedsubsequentMemberReader.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespaceReader5 = await nestedcommaReader.MoveInternal1().ConfigureAwait(false);
                //var nestedmemberReader2 = await nestedwhitespaceReader5.AsReader.MoveInternal3(nestedwhitespaceReader5.Factory).ConfigureAwait(false);
                var nestedmemberReader2 = await nestedwhitespaceReader5.MoveInternal1().ConfigureAwait(false);
                var nestedstringReader2 = await nestedmemberReader2.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiterReader3 = await nestedstringReader2.MoveInternal1().ConfigureAwait(false);
                var nestedcharsReader2 = await nestedstringDelimiterReader3.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiterReader4 = await nestedcharsReader2.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace6 = await nestedstringDelimiterReader4.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon2 = await nestedwhitespace6.AsReader.MoveInternal3(nestedwhitespace6.Factory).ConfigureAwait(false);
                var nestedcolon2 = await nestedwhitespace6.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace7 = await nestedcolon2.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue3 = await nestedwhitespace7.AsReader.MoveInternal3(nestedwhitespace7.Factory).ConfigureAwait(false);
                var nestedvalue3 = await nestedwhitespace7.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken3 = await nestedvalue3.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken3.TryFalse().TryGetValue(out var nestedfalse))
                {
                    throw new Exception("TODO");
                }

                //// TODO
                /*context.CurrentByteIndex = 15;
                if (!nestedfalse.TryGetValue3(ref context, out _, out _, out _))
                {
                    throw new Exception("tODO");
                }

                if (!nestedfalse.TryMove3(ref context, out var _more1))
                {
                    throw new Exception("tODO");
                }

                var nestedsubsequentMembers2 = _more1();*/
                var nestedsubsequentMembers2 = await nestedfalse.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembersToken2 = nestedsubsequentMembers2.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers2.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken2 = nestedsubsequentMembers2.TryMove(out read);
                }

                var nestedsubsequentMember2 = nestedsubsequentMembersToken2.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // 1234
                var nestedcomma2 = await nestedsubsequentMember2.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace8 = await nestedcomma2.MoveInternal1().ConfigureAwait(false);
                //var nestedmember3 = await nestedwhitespace8.AsReader.MoveInternal3(nestedwhitespace8.Factory).ConfigureAwait(false);
                var nestedmember3 = await nestedwhitespace8.MoveInternal1().ConfigureAwait(false);
                var nestedstring3 = await nestedmember3.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter5 = await nestedstring3.MoveInternal1().ConfigureAwait(false);
                var nestedchars3 = await nestedstringDelimiter5.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter6 = await nestedchars3.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace9 = await nestedstringDelimiter6.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon3 = await nestedwhitespace9.AsReader.MoveInternal3(nestedwhitespace9.Factory).ConfigureAwait(false);
                var nestedcolon3 = await nestedwhitespace9.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace10 = await nestedcolon3.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue4 = await nestedwhitespace10.AsReader.MoveInternal3(nestedwhitespace10.Factory).ConfigureAwait(false);
                var nestedvalue4 = await nestedwhitespace10.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken4 = await nestedvalue4.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken4.TryNumber().TryGetValue(out var nestednumber))
                {
                    throw new Exception("TODO");
                }

                var nestedsign = await nestednumber.MoveInternal1().ConfigureAwait(false);
                var nestedint = await nestedsign.MoveInternal1().ConfigureAwait(false);
                var nestedfrac = await nestedint.MoveInternal1().ConfigureAwait(false);
                var nestedexp = await nestedfrac.MoveInternal1().ConfigureAwait(false);
                var nestedexpToken = nestedexp.TryMove(out read);
                if (!read)
                {
                    await nestedexp.Read().ConfigureAwait(false);
                    nestedexpToken = nestedexp.TryMove(out read);
                }

                var nestedsubsequentMembers3 = nestedexpToken.Apply(
                    _ => _,
                    _ => throw new Exception("tODO"));
                var nestedsubsequentMembersToken3 = nestedsubsequentMembers3.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers3.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken3 = nestedsubsequentMembers3.TryMove(out read);
                }

                var nestedsubsequentMember3 = nestedsubsequentMembersToken3.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // asdf
                var nestedcomma3 = await nestedsubsequentMember3.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace11 = await nestedcomma3.MoveInternal1().ConfigureAwait(false);
                //var nestedmember4 = await nestedwhitespace11.AsReader.MoveInternal3(nestedwhitespace11.Factory).ConfigureAwait(false);
                var nestedmember4 = await nestedwhitespace11.MoveInternal1().ConfigureAwait(false);
                var nestedstring4 = await nestedmember4.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter7 = await nestedstring4.MoveInternal1().ConfigureAwait(false);
                var nestedchars4 = await nestedstringDelimiter7.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter8 = await nestedchars4.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace12 = await nestedstringDelimiter8.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon4 = await nestedwhitespace12.AsReader.MoveInternal3(nestedwhitespace12.Factory).ConfigureAwait(false);
                var nestedcolon4 = await nestedwhitespace12.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace13 = await nestedcolon4.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue5 = await nestedwhitespace13.AsReader.MoveInternal3(nestedwhitespace13.Factory).ConfigureAwait(false);
                var nestedvalue5 = await nestedwhitespace13.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken5 = await nestedvalue5.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken5.TryString().TryGetValue(out var nestedstring5))
                {
                    throw new Exception("TODO");
                }

                var nestedstringDelimiter9 = await @nestedstring5.MoveInternal1().ConfigureAwait(false);
                var nestedchars5 = await nestedstringDelimiter9.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter10 = await nestedchars5.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembers4 = await nestedstringDelimiter10.MoveInternal1().ConfigureAwait(false);

                var nestedsubsequentMembersToken4 = nestedsubsequentMembers4.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers4.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken4 = nestedsubsequentMembers4.TryMove(out read);
                }

                var nestedsubsequentMember4 = nestedsubsequentMembersToken4.Apply(
                    _ => throw new Exception("TODO"),
                    _ => _);

                // null
                var nestedcomma4 = await nestedsubsequentMember4.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace14 = await nestedcomma4.MoveInternal1().ConfigureAwait(false);
                //var nestedmember5 = await nestedwhitespace14.AsReader.MoveInternal3(nestedwhitespace14.Factory).ConfigureAwait(false);
                var nestedmember5 = await nestedwhitespace14.MoveInternal1().ConfigureAwait(false);
                var nestedstring6 = await nestedmember5.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter11 = await nestedstring6.MoveInternal1().ConfigureAwait(false);
                var nestedchars6 = await nestedstringDelimiter11.MoveInternal1().ConfigureAwait(false);
                var nestedstringDelimiter12 = await nestedchars6.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace15 = await nestedstringDelimiter12.MoveInternal1().ConfigureAwait(false);
                //var nestedcolon5 = await nestedwhitespace15.AsReader.MoveInternal3(nestedwhitespace15.Factory).ConfigureAwait(false);
                var nestedcolon5 = await nestedwhitespace15.MoveInternal1().ConfigureAwait(false);
                var nestedwhitespace16 = await nestedcolon5.MoveInternal1().ConfigureAwait(false);
                //var nestedvalue6 = await nestedwhitespace16.AsReader.MoveInternal3(nestedwhitespace16.Factory).ConfigureAwait(false);
                var nestedvalue6 = await nestedwhitespace16.MoveInternal1().ConfigureAwait(false);
                var nestedvalueToken6 = await nestedvalue6.MoveInternal1().ConfigureAwait(false);
                if (!nestedvalueToken6.TryNull().TryGetValue(out var nestednull))
                {
                    throw new Exception("TODO");
                }

                var nestedsubsequentMembers5 = await @nestednull.MoveInternal1().ConfigureAwait(false);
                var nestedsubsequentMembersToken5 = nestedsubsequentMembers5.TryMove(out read);
                if (!read)
                {
                    await nestedsubsequentMembers5.Read().ConfigureAwait(false);
                    nestedsubsequentMembersToken5 = nestedsubsequentMembers5.TryMove(out read);
                }

                var nestedsubsequentMember5 = nestedsubsequentMembersToken5.Apply(
                    _ => _,
                    _ => throw new Exception("TODO"));

                //var nestedobjectEnd = await nestedsubsequentMember5.AsReader.MoveInternal3(nestedsubsequentMember5.Factory).ConfigureAwait(false);
                var nestedobjectEnd = await nestedsubsequentMember5.MoveInternal1().ConfigureAwait(false);
                subsequentArrayElements = await nestedobjectEnd.MoveInternal1().ConfigureAwait(false);
            }

            var subsequentArrayElementsToken = subsequentArrayElements.TryMove(out read);
            if (!read)
            {
                await subsequentArrayElements.Read().ConfigureAwait(false);
                subsequentArrayElementsToken = subsequentArrayElements.TryMove(out read);
            }

            var whitespace36 = subsequentArrayElementsToken.Apply(
                _ => _,
                _ => throw new Exception("TODO"));
            //var arrayEnd2 = await whitespace36.AsReader.MoveInternal3(whitespace36.Factory).ConfigureAwait(false);
            var arrayEnd2 = await whitespace36.MoveInternal1().ConfigureAwait(false);
            var subsequentMembers10 = await arrayEnd2.MoveInternal1().ConfigureAwait(false);
            var subsequentMembersToken10 = subsequentMembers10.TryMove(out read);
            if (!read)
            {
                await subsequentMembers10.Read().ConfigureAwait(false);
                subsequentMembersToken10 = subsequentMembers10.TryMove(out read);
            }

            var whitespace37 = subsequentMembersToken10.Apply(
                _ => _,
                _ => throw new Exception("TODO"));
            //var objectEnd2 = await whitespace37.AsReader.MoveInternal3(whitespace37.Factory).ConfigureAwait(false);
            var objectEnd2 = await whitespace37.MoveInternal1().ConfigureAwait(false);
            var whitespace38 = await objectEnd2.MoveInternal1().ConfigureAwait(false);
            //// TODO remove this when you fix moveinternal3
            context.Read().ConfigureAwait(false).GetAwaiter().GetResult();
            var nothingFactory = await whitespace38.AsReader.MoveInternal3(ref context, Foo).ConfigureAwait(false);
            var nothing = nothingFactory();

            Assert.AreEqual(new Nothing(), nothing);

            Assert.AreEqual(stream.Length, stream.Position);
            Assert.AreEqual(0, context.CurrentByteIndex);
        }

        static WhitespaceReader2<Nothing> Foo()
        {
            throw new Exception("TODO");
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
