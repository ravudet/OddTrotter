namespace OddTrotter.CalendarV1.Tokenization.Json6
{
    using System;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json5;

    using Stash;

    public static partial class Extensions
    {
        public static Move4Task<TCurrentReader, TNextReader, TValue> Move4<TCurrentReader, TNextReader, TValue>(
            this IValueReader<TCurrentReader, TNextReader, TValue> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>, allows ref struct
            where TNextReader : allows ref struct
            where TValue : allows ref struct
        {
            if (!currentReader.TryMove4(readerContext, out _, out _))
            {
                return new Move4Task<TCurrentReader, TNextReader, TValue>(readerContext.Read(), readerContext);
            }

            return new Move4Task<TCurrentReader, TNextReader, TValue>(readerContext);
        }

        public ref struct Move4Task<TCurrentReader, TNextReader, TValue>
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>, allows ref struct
            where TNextReader : allows ref struct
            where TValue : allows ref struct
        {
            private readonly ValueTask? read;
            private readonly ReaderContext readerContext;

            public Move4Task(ValueTask read, ReaderContext readerContext)
            {
                this.read = read;
                this.readerContext = readerContext;
            }

            public Move4Task(ReaderContext readerContext)
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

                public struct TaskAwaiter : ITaskAwaiter<TNextReader>
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

                    public TNextReader GetResult()
                    {
                        if (this.read == null)
                        {
                            return default!;
                        }
                        
                        this.read?.GetResult(); //// TODO this basically will just throw if needed... //// TODO you do this in other types too
                        TCurrentReader.TryMove(this.readerContext, out _, out _);

                        return default!;
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

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
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

        public static async ValueTask<TToken> Move31<TCurrentReader, TToken>(
            this ITokenReader<TCurrentReader, TToken> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            TToken token;
            while (!currentReader.TryMove3(readerContext, out token))
            {
                await readerContext.Read().ConfigureAwait(false);
            }

            return token;
        }

        public static Move3Task<TCurrentReader, TToken> Move3<TCurrentReader, TToken>(
            this ITokenReader<TCurrentReader, TToken> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>, allows ref struct
            where TToken : allows ref struct
        {
            //// TODO i don't really know why this is faster that move21; the other methods got faster when i removed the custom awaitable implementation; it's *possible* because of the closure `token`, but `move3` has a closure on `token`...
            //// TODO also worth noting though that this has a bug; the `_` should actually be returned, because otherwise you are reading the value twice
            if (currentReader.TryMove3(readerContext, out _))
            {
                return new Move3Task<TCurrentReader, TToken>(readerContext);
            }

            return new Move3Task<TCurrentReader, TToken>(readerContext.Read(), readerContext);
        }

        public readonly ref struct RefTuple<T1, T2>
            where T1 : allows ref struct
            where T2 : allows ref struct
        {
            public RefTuple(T1 item1, T2 item2)
            {
                this.Item1 = item1;
                this.Item2 = item2;
            }

            public T1 Item1 { get; }
            public T2 Item2 { get; }

            public void Deconstruct(out T1 item1, out T2 item2)
            {
                item1 = this.Item1;
                item2 = this.Item2;
            }
        }

        public static class RefTuple
        {
            public static RefTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
                where T1 : allows ref struct
                where T2 : allows ref struct
            {
                return new RefTuple<T1, T2>(item1, item2);
            }
        }

        public ref struct Move3Task<TCurrentReader, TToken>
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>, allows ref struct
            where TToken : allows ref struct
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

                public struct TaskAwaiter : ITaskAwaiter<TToken>
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

                    public TToken GetResult()
                    {
                        this.read?.GetResult(); //// TODO this basically will just throw if needed... //// TODO you do this in other types too
                        TCurrentReader.TryMove(this.readerContext, out var token);

                        return token;
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

            public ITaskAwaiter<RefTuple<ReaderContext, TToken>> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }

        public static async ValueTask<TNextReader> Move21<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            if (!currentReader.TryMove2(readerContext, out _, out _, out var context))
            {
                await readerContext.Read().ConfigureAwait(false);
                currentReader.TryContinue2(readerContext, out _, out _, ref context);
            }

            return default!;
        }

        public static Move2Task<TCurrentReader, TNextReader, TValue, TContext> Move2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> currentReader,
            ReaderContext readerContext)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            //// TODO i don't really know why this is faster that move21; the other methods got faster when i removed the custom awaitable implementation; it's *possible* because of the closure `context`, but `move3` has a closure on `token`...
            //// TODO maybe it's the `ref context`? //// TODO actually, i removed the `ref` for `context` and it didn't fix it

            if (currentReader.TryMove2(readerContext, out _, out _, out var context))
            {
                return new Move2Task<TCurrentReader, TNextReader, TValue, TContext>(readerContext);
            }

            return new Move2Task<TCurrentReader, TNextReader, TValue, TContext>(readerContext.Read(), readerContext, context);
        }

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
                await readerContext.Read().ConfigureAwait(false);
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
