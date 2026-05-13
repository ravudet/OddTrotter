namespace OddTrotter.CalendarV1.Tokenization.Json6
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json5;

    public static partial class Extensions
    {
        /*public static Move1Task<TNextReader> Move2<TCurrentReader, TNextReader>(
            this IValueReader<TCurrentReader, TNextReader> currentReader,
            ref ReaderContext readerContext)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader>
        {
            if (currentReader.TryMove(ref readerContext, out var nextReader, out _))
            {
                return new Move1Task<TNextReader>(ref readerContext);
            }

            return new Move1Task<TNextReader>(readerContext.Read());
        }*/

        public static Move1Task<TNextReader> Move1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> currentReader, 
            ref ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            if (currentReader.TryMove1(ref readerContext, out var nextReader))
            {
                return new Move1Task<TNextReader>(ref readerContext);
            }

            return new Move1Task<TNextReader>(readerContext.Read());
        }

        public ref struct Move1Task<TNextReader>
        {
            private readonly ValueTask<ReaderContext>? read;
            private readonly ref ReaderContext readerContext;

            public Move1Task(ValueTask<ReaderContext> read)
            {
                this.read = read;
            }

            public Move1Task(ref ReaderContext readerContext)
            {
                this.readerContext = ref readerContext;
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.read?.ConfigureAwait(continueOnCapturedContext), ref this.readerContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly ConfiguredValueTaskAwaitable<ReaderContext>? read;
                private readonly ref ReaderContext readerContext;

                public ConfiguredAwaitable(ConfiguredValueTaskAwaitable<ReaderContext>? read, ref ReaderContext readerContext)
                {
                    this.read = read;
                    this.readerContext = ref readerContext;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return new TaskAwaiter(this.read?.GetAwaiter(), ref this.readerContext);
                }

                public unsafe struct TaskAwaiter : ITaskAwaiter<(ReaderContext, TNextReader)>
                {
                    private readonly ConfiguredValueTaskAwaitable<ReaderContext>.ConfiguredValueTaskAwaiter? read;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private readonly ReaderContext* readerContext;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

                    public TaskAwaiter(ConfiguredValueTaskAwaitable<ReaderContext>.ConfiguredValueTaskAwaiter? read, ref ReaderContext readerContext)
                    {
                        this.read = read;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.readerContext = (ReaderContext*)Unsafe.AsPointer(ref readerContext);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
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
                        var readerContext = this.read?.GetResult() ?? Unsafe.AsRef<ReaderContext>(this.readerContext);

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
