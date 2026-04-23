using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using OddTrotter.CalendarV1.Tokenization.Json3;

namespace OddTrotter.CalendarV1.Tokenization.Json4 //// TODO should be json3
{
    public static class Extensions
    {
        public static Move2Task<TToken> Move2<TCurrentReader, TToken, TContext>(
            this TypeHolder<TCurrentReader, TToken, TContext> currentReader,
            ref ReaderContext readerContext)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken, TContext>, allows ref struct
            where TToken : allows ref struct
        {
        }

        public ref struct Move2Task<TNextReader>
        {
        }

        public static Move1Task<TCurrentReader, TNextReader, TContext> Move1<TCurrentReader, TNextReader, TContext>(
            this TypeHolder<TCurrentReader, TNextReader, TContext> currentReader,
            ref ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader, TContext>, allows ref struct
            where TNextReader : new(), allows ref struct
        {
            if (!currentReader.Self.TryMove(ref readerContext, out var context))
            {
                return new Move1Task<TCurrentReader, TNextReader, TContext>(
                    new Move1ReadTask<TCurrentReader, TNextReader, TContext>(
                        readerContext.Read(),
                        ref readerContext,
                        context));
            }

            return new Move1Task<TCurrentReader, TNextReader, TContext>(
                new Move1CompletedTask<TNextReader>());
        }

        public ref struct Move1ReadTask<TCurrentReader, TNextReader, TContext>
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader, TContext>, allows ref struct
            where TNextReader : new(), allows ref struct
        {
            private readonly ValueTask task;
            private readonly ref ReaderContext readerContext;
            private readonly TContext context;

            public Move1ReadTask(
                ValueTask task,
                ref ReaderContext readerContext,
                TContext context)
            {
                this.task = task;
                this.readerContext = ref readerContext;
                this.context = context;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.task.ConfigureAwait(continueOnCapturedContext),
                    continueOnCapturedContext,
                    ref this.readerContext,
                    this.context);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable configuredAwaitable;
                private readonly bool continueOnCapturedContext;
                private readonly ref ReaderContext readerContext;
                private readonly TContext context;

                public ConfiguredAwaitable(
                    ConfiguredValueTaskAwaitable configuredAwaitable,
                    bool continueOnCapturedContext,
                    ref ReaderContext readerContext,
                    TContext context)
                {
                    this.configuredAwaitable = configuredAwaitable;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                    this.readerContext = ref readerContext;
                    this.context = context;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(
                        this.configuredAwaitable.GetAwaiter(),
                        this.continueOnCapturedContext,
                        ref this.readerContext,
                        this.context);
                }

                public unsafe struct TaskAwaiter : ITaskAwaiter<TNextReader> //// TODO readonly?
                {
                    private ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter taskAwaiter;
                    private readonly bool continueOnCapturedContext;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private readonly ReaderContext* readerContext;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private TContext context;

                    public TaskAwaiter(
                        ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter taskAwaiter,
                        bool continueOnCapturedContext,
                        ref ReaderContext readerContext,
                        TContext context)
                    {
                        this.taskAwaiter = taskAwaiter;
                        this.continueOnCapturedContext = continueOnCapturedContext;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.readerContext = (ReaderContext*)Unsafe.AsPointer(ref readerContext);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.context = context;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (!taskAwaiter.IsCompleted)
                            {
                                return false;
                            }

                            var readerContext = Unsafe.AsRef<ReaderContext>(this.readerContext);
                            var currentReader = TCurrentReader.Create(this.context);
                            if (!currentReader.TryMove(ref readerContext, out this.context!)) //// TODO !
                            {
                                this.taskAwaiter = readerContext.Read().ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                                return this.IsCompleted;
                            }

                            return true;
                        }
                    }

                    public TNextReader GetResult()
                    {
                        return new();
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
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }

        public ref struct Move1CompletedTask<TNextReader>
            where TNextReader : new(), allows ref struct
        {
            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(ValueTask.CompletedTask.ConfigureAwait(continueOnCapturedContext));
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable configuredValueTaskAwaitable;

                public ConfiguredAwaitable(ConfiguredValueTaskAwaitable configuredValueTaskAwaitable)
                {
                    this.configuredValueTaskAwaitable = configuredValueTaskAwaitable;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(this.configuredValueTaskAwaitable.GetAwaiter());
                }

                public readonly struct TaskAwaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredValueTaskAwaiter;

                    public TaskAwaiter(ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter configuredValueTaskAwaiter)
                    {
                        this.configuredValueTaskAwaiter = configuredValueTaskAwaiter;
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
                        return default!; //// TODO !
                    }

                    public void OnCompleted(Action continuation)
                    {
                        this.configuredValueTaskAwaiter.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        this.configuredValueTaskAwaiter.UnsafeOnCompleted(continuation);
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }

        public ref struct Move1Task<TCurrentReader, TNextReader, TContext>
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader, TContext>, allows ref struct
            where TNextReader : new(), allows ref struct
        {
            private readonly int type;

            private readonly Move1CompletedTask<TNextReader> completed;
            private readonly Move1ReadTask<TCurrentReader, TNextReader, TContext> read;

            public Move1Task(Move1CompletedTask<TNextReader> completed)
            {
                this.completed = completed;

                this.type = 1;
            }

            public Move1Task(Move1ReadTask<TCurrentReader, TNextReader, TContext> read)
            {
                this.read = read;

                this.type = 2;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                switch (this.type)
                {
                    case 1:
                        return new ConfiguredAwaitable(this.completed.ConfigureAwait(continueOnCapturedContext));
                    default:
                        throw new Exception("TODO");
                }
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly int type;

                private readonly Move1CompletedTask<TNextReader>.ConfiguredAwaitable completed;

                public ConfiguredAwaitable(Move1CompletedTask<TNextReader>.ConfiguredAwaitable completed)
                {
                    this.completed = completed;

                    this.type = 1;
                }

                public TaskAwaiter GetAwaiter()
                {
                    switch (this.type)
                    {
                        case 1:
                            return new TaskAwaiter(this.completed.GetAwaiter());
                        default:
                            throw new Exception("TODO");
                    }
                }

                public readonly struct TaskAwaiter : ITaskAwaiter<TNextReader>
                {
                    private readonly int type;

                    private readonly Move1CompletedTask<TNextReader>.ConfiguredAwaitable.TaskAwaiter completed;
                    private readonly Move1ReadTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable.TaskAwaiter read;

                    public TaskAwaiter(Move1CompletedTask<TNextReader>.ConfiguredAwaitable.TaskAwaiter completed)
                    {
                        this.completed = completed;

                        this.type = 1;
                    }

                    public TaskAwaiter(Move1ReadTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable.TaskAwaiter read)
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
                                    return this.completed.IsCompleted;
                                case 2:
                                    return this.read.IsCompleted;
                                default:
                                    throw new Exception("TODO");
                            }
                        }
                    }

                    public TNextReader GetResult()
                    {
                        switch (this.type)
                        {
                            case 1:
                                return this.completed.GetResult();
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
                                this.completed.OnCompleted(continuation);
                                return;
                            case 2:
                                this.read.OnCompleted(continuation);
                                return;
                            default:
                                throw new Exception("TODO");
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        switch (this.type)
                        {
                            case 1:
                                this.completed.UnsafeOnCompleted(continuation);
                                return;
                            case 2:
                                this.read.UnsafeOnCompleted(continuation);
                                return;
                            default:
                                throw new Exception("TODO");
                        }
                    }
                }
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
