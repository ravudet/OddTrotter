using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OddTrotter.CalendarV1.Tokenization.Json3
{
    public static class Extensions
    {
        public static Move1Task<TCurrentReader, TNextReader, TContext> Move1<TCurrentReader, TNextReader, TContext>(
            this TypeHolder<TCurrentReader, TNextReader, TContext> currentReader,
            ref ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader, TContext>, allows ref struct
            where TNextReader : allows ref struct
        {
            if (!currentReader.Self.TryMove(ref readerContext, out var nextReader, out var context, out var factory))
            {
            }
        }

        public ref struct Move1CompletedTask<TCurrentReader, TNextReader, TContext>
            where TCurrentReader : allows ref struct
            where TNextReader : allows ref struct
        {
            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new System.NotImplementedException();
            }

            public ref struct ConfiguredAwaitable
            {
                public TaskAwaiter GetAwaiter()
                {
                    throw new System.NotImplementedException();
                }

                public struct TaskAwaiter : ITaskAwaiter<TNextReader>
                {
                    public bool IsCompleted => throw new NotImplementedException();

                    public TNextReader GetResult()
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
            }

            public ITaskAwaiter<TNextReader> GetAwaiter()
            {
                throw new System.NotImplementedException();
            }
        }

        public ref struct Move1Task<TCurrentReader, TNextReader, TContext>
            where TCurrentReader : allows ref struct
            where TNextReader : allows ref struct
        {
            private readonly int type;

            private readonly Move1CompletedTask<TCurrentReader, TNextReader, TContext> completed;

            public Move1Task(Move1CompletedTask<TCurrentReader, TNextReader, TContext> completed)
            {
                this.completed = completed;

                this.type = 1;
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

                private readonly Move1CompletedTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable completed;

                public ConfiguredAwaitable(Move1CompletedTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable completed)
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

                    private readonly Move1CompletedTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable.TaskAwaiter completed;

                    public TaskAwaiter(Move1CompletedTask<TCurrentReader, TNextReader, TContext>.ConfiguredAwaitable.TaskAwaiter completed)
                    {
                        this.completed = completed;

                        this.type = 1;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            switch (this.type)
                            {
                                case 1:
                                    return this.completed.IsCompleted;
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
