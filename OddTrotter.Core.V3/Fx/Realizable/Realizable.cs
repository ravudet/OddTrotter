namespace Fx.Realizable
{
    using System;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;



    public static class Realizable
    {
        public static Realizable<T> FromResult<T>(T value)
            where T : allows ref struct
        {
            return new Realizable<T>(value);
        }

        public static Realizable<T> FromFuture<T>(ITask<T> value)
            where T : allows ref struct
        {
            return new Realizable<T>(value);
        }

        public static Realizable<T> FromException<T>(Exception exception)
            where T : allows ref struct
        {
            return Realizable.FromFuture(new FromExceptionTask<T>(exception));
        }

        private sealed class FromExceptionTask<T> : ITask<T>
            where T : allows ref struct
        {
            private readonly Exception exception;

            public FromExceptionTask(Exception exception)
            {
                this.exception = exception;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.exception, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
            {
                private readonly Exception exception;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(Exception exception, bool continueOnCapturedContext)
                {
                    this.exception = exception;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public IAwaiter<T> GetAwaiter()
                { 
                    //// TODO maybe the task should be instantiated in the constructor of `fromexceptiontask` so that the "logic" can be shared between a configured awaitable and a non-configured one
                    return new ConfiguredAwaiter(Task.FromException(this.exception).ConfigureAwait(this.continueOnCapturedContext).GetAwaiter());
                }

                private sealed class ConfiguredAwaiter : IAwaiter<T>
                {
                    private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                    public ConfiguredAwaiter(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                    {
                        this.taskAwaiter = taskAwaiter;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return taskAwaiter.IsCompleted;
                        }
                    }

                    public T GetResult()
                    {
                        this.taskAwaiter.GetResult();
                        return default!; //// TODO not great, but this is what .NET does...
                    }

                    public void OnCompleted(Action continuation)
                    {
                        taskAwaiter.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        taskAwaiter.UnsafeOnCompleted(continuation);
                    }
                }
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                TResult result;
                try
                {
                    result = exceptionContinuation(this.exception);
                }
                catch (Exception resultException)
                {
                    return Realizable.FromException<TResult>(resultException);
                }

                return Realizable.FromResult(result);
            }

            public IAwaiter<T> GetAwaiter()
            {
                return new Awaiter(Task.FromException(this.exception).GetAwaiter());
            }

            private sealed class Awaiter : IAwaiter<T>
            {
                private readonly TaskAwaiter taskAwaiter;

                public Awaiter(TaskAwaiter taskAwaiter)
                {
                    this.taskAwaiter = taskAwaiter;
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
                    this.taskAwaiter.GetResult();
                    return default!; //// TODO not great, but this is what .NET does...
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
    }
}
