namespace OddTrotter.CalendarV1.Tokenization.Json6
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json5;

    public static partial class Extensions
    {
        public static async ValueTask<TNextReader> Move4<TCurrentReader, TNextReader, TValue>(
            this IValueReader<TCurrentReader, TNextReader, TValue> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            if (!currentReader.TryMove4(readerContext, out _, out _))
            {
                await readerContext.Read();
            }

            return default!;
        }

        /*public static Move1Task<TNextReader> Move4<TCurrentReader, TNextReader, TValue>(
            this IValueReader<TCurrentReader, TNextReader, TValue> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            if (currentReader.TryMove4(readerContext, out _, out _))
            {
                return new Move1Task<TNextReader>(readerContext);
            }

            return new Move1Task<TNextReader>(readerContext.Read(), readerContext);
        }*/

        public static Move3Task<TCurrentReader, TToken> Move3<TCurrentReader, TToken>(
            this ITokenReader<TCurrentReader, TToken> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            if (currentReader.TryMove3(readerContext, out var token))
            {
                return new Move3Task<TCurrentReader, TToken>(readerContext);
            }

            return new Move3Task<TCurrentReader, TToken>(readerContext.Read(), readerContext);
        }

        public ref struct Move3Task<TCurrentReader, TToken>
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            private readonly ValueTask? read;
            private readonly ReaderContext readerContext;

            public Move3Task(ValueTask read, ReaderContext readerContext)
            {
                this.read = read;
                this.readerContext = readerContext;
            }

            public Move3Task(ReaderContext readerContext)
            {
                this.readerContext = readerContext;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.read?.ConfigureAwait(continueOnCapturedContext), this.readerContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable? read;
                private readonly ReaderContext readerContext;

                public ConfiguredAwaitable(ConfiguredValueTaskAwaitable? read, ReaderContext readerContext)
                {
                    this.read = read;
                    this.readerContext = readerContext;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(this.read?.GetAwaiter(), this.readerContext);
                }

                public struct TaskAwaiter : ITaskAwaiter<(ReaderContext, TToken)>
                {
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read;
                    private readonly ReaderContext readerContext;

                    public TaskAwaiter(ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read, ReaderContext readerContext)
                    {
                        this.read = read;
                        this.readerContext = readerContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return this.read?.IsCompleted ?? true;
                        }
                    }

                    public (ReaderContext, TToken) GetResult()
                    {
                        this.read?.GetResult(); //// TODO this basically will just throw if needed... //// TODO you do this in other types too
                        TCurrentReader.TryMove(this.readerContext, out var token);

                        return (readerContext, token);
                    }

                    public void OnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.OnCompleted(continuation);
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.UnsafeOnCompleted(continuation);
                        }
                    }
                }
            }

            public ITaskAwaiter<(ReaderContext, TToken)> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }

        public static async ValueTask<TNextReader> Move2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            if (!currentReader.TryMove2(readerContext, out _, out _, out var context))
            {
                do
                {
                    await readerContext.Read();
                }
                while (!currentReader.TryContinue2(readerContext, out _, out _, ref context));
            }

            return default!;
        }

        /*public static Move2Task<TCurrentReader, TNextReader, TValue, TContext> Move2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            if (currentReader.TryMove2(readerContext, out _, out _, out var context))
            {
                return new Move2Task<TCurrentReader, TNextReader, TValue, TContext>(readerContext);
            }

            return new Move2Task<TCurrentReader, TNextReader, TValue, TContext>(readerContext.Read(), readerContext, context);
        }*/

        public ref struct Move2Task<TCurrentReader, TNextReader, TValue, TContext>
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            private readonly ValueTask? read;
            private readonly TContext? context;
            private readonly ReaderContext readerContext;

            public Move2Task(ValueTask read, ReaderContext readerContext, TContext context)
            {
                this.read = read;
                this.readerContext = readerContext;
                this.context = context;
            }

            public Move2Task(ReaderContext readerContext)
            {
                this.readerContext = readerContext;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.read?.ConfigureAwait(continueOnCapturedContext), this.context, this.readerContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable? read;
                private readonly TContext? context;
                private readonly ReaderContext readerContext;

                public ConfiguredAwaitable(ConfiguredValueTaskAwaitable? read, TContext? context, ReaderContext readerContext)
                {
                    this.read = read;
                    this.context = context;
                    this.readerContext = readerContext;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(this.read?.GetAwaiter(), this.context, this.readerContext);
                }

                public unsafe struct TaskAwaiter : ITaskAwaiter<(ReaderContext, TNextReader)>
                {
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read;
                    private readonly TContext? context;
                    private readonly ReaderContext readerContext;

                    public TaskAwaiter(ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read, TContext? context, ReaderContext readerContext)
                    {
                        this.read = read;
                        this.context = context;
                        this.readerContext = readerContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return this.read?.IsCompleted ?? true;
                        }
                    }

                    public (ReaderContext, TNextReader) GetResult()
                    {
                        if (this.read == null)
                        {
                            return (readerContext, default!);
                        }

                        this.read?.GetResult();
                        var context = this.context!; //// TODO !

                        if (!TCurrentReader.TryContinue(readerContext, out _, out _, ref context))
                        {
                            throw new Exception("TODO assuming only 1 stream read is needed"); //// TODO you do this in other types too
                        }

                        return (readerContext, default!);
                    }

                    public void OnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.OnCompleted(continuation);
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.UnsafeOnCompleted(continuation);
                        }
                    }
                }
            }

            public ITaskAwaiter<(ReaderContext, TNextReader)> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }

        public static async ValueTask<TNextReader> Move1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            while (!currentReader.TryMove1(readerContext, out _))
            {
                await readerContext.Read2();
            }

            return default!;
        }

        /*public static Move1Task<TNextReader> Move1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> currentReader, 
            ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            if (currentReader.TryMove1(readerContext, out var nextReader))
            {
                return new Move1Task<TNextReader>(readerContext);
            }

            return new Move1Task<TNextReader>(readerContext.Read(), readerContext);
        }*/

        public ref struct Move1Task<TNextReader>
        {
            private readonly ValueTask? read;
            private readonly ReaderContext readerContext;

            public Move1Task(ValueTask read, ReaderContext readerContext)
            {
                this.read = read;
                this.readerContext = readerContext;
            }

            public Move1Task(ReaderContext readerContext)
            {
                this.readerContext = readerContext;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.read?.ConfigureAwait(continueOnCapturedContext), this.readerContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable? read;
                private readonly ReaderContext readerContext;

                public ConfiguredAwaitable(ConfiguredValueTaskAwaitable? read, ReaderContext readerContext)
                {
                    this.read = read;
                    this.readerContext = readerContext;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(this.read?.GetAwaiter(), this.readerContext);
                }

                public unsafe struct TaskAwaiter : ITaskAwaiter<(ReaderContext, TNextReader)>
                {
                    private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read;
                    private readonly ReaderContext readerContext;

                    public TaskAwaiter(ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter? read, ReaderContext readerContext)
                    {
                        this.read = read;
                        this.readerContext = readerContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return this.read?.IsCompleted ?? true;
                        }
                    }

                    public (ReaderContext, TNextReader) GetResult()
                    {
                        this.read?.GetResult();

                        return (readerContext, default!);
                    }

                    public void OnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.OnCompleted(continuation);
                        }
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        if (this.read == null)
                        {
                            ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
                        }
                        else
                        {
                            this.read.Value.UnsafeOnCompleted(continuation);
                        }
                    }
                }
            }

            public ITaskAwaiter<(ReaderContext, TNextReader)> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
