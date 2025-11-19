namespace Fx.Realizable
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;



    public static class Realizable
    {
        public static Realizable<T> FromResult<T>(T value)
            where T : allows ref struct
        {
            return new Realizable<T>(value);
        }

        public static Realizable<T> FromException<T>(Exception exception)
            where T : allows ref struct
        {
            return new Realizable<T>(new FromExceptionTask<T>(exception));
        }

        private sealed class FromExceptionTask<T> : ITask<T>
            where T : allows ref struct
        {
            private readonly Exception exception;

            public FromExceptionTask(Exception exception)
            {
                this.exception = exception;
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> source, Func<Exception, TResult> exception, Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
            {
                return Realizable.FromResult(exception(this.exception));
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
                    throw new Exception("TODO");
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
