/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    using Fx.Realizable;

    public sealed class TaskWrapper<T> : ITask<T>
    {
        private readonly Task<T> task;

        public TaskWrapper(Task<T> task)
        {
            this.task = task;
        }

        public Realizable<TResult> ContinueWith<TResult>(
            Func<T, TResult> sourceContinuation, 
            Func<Exception, TResult> exceptionContinuation, 
            Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
        {
            return new Realizable<TResult>(
                new Continuation<T, TResult>(
                    new TaskData(task),
                    sourceContinuation,
                    exceptionContinuation,
                    canceledContinuation));
        }

        private interface ITaskData<out TData>
            where TData : allows ref struct
        {
            IAwaiter<TData> GetAwaiter();

            Exception? Exception { get; }

            bool IsCanceled { get; }
        }

        private sealed class TaskData : ITaskData<T>
        {
            private readonly Task<T> task;

            public TaskData(Task<T> task)
            {
                this.task = task;
            }

            public Exception? Exception
            {
                get
                {
                    return task.Exception;
                }
            }

            public bool IsCanceled
            {
                get
                {
                    return task.IsCanceled;
                }
            }

            public IAwaiter<T> GetAwaiter()
            {
                return new TaskWrapper<T>.Awaiter(task.GetAwaiter());
            }
        }

        private sealed class Continuation<TOld, TNew> : ITask<TNew>, ITaskData<TNew>
            where TOld : allows ref struct
            where TNew : allows ref struct
        {
            private readonly ITaskData<TOld> task;
            private readonly Func<TOld, TNew> sourceContinuation;
            private readonly Func<Exception, TNew> exceptionContinuation;
            private readonly Func<OperationCanceledException, TNew> canceledContinuation;

            public Continuation(
                ITaskData<TOld> task, 
                Func<TOld, TNew> sourceContinuation,
                Func<Exception, TNew> exceptionContinuation,
                Func<OperationCanceledException, TNew> canceledContinuation)
            {
                this.task = task;
                this.sourceContinuation = sourceContinuation;
                this.exceptionContinuation = exceptionContinuation;
                this.canceledContinuation = canceledContinuation;
            }

            public Exception? Exception
            {
                get
                {
                    return task.Exception;
                }
            }

            public bool IsCanceled
            {
                get
                {
                    return task.IsCanceled;
                }
            }

            public Realizable<TResult> ContinueWith<TResult>(
                Func<TNew, TResult> sourceContinuation, 
                Func<Exception, TResult> exceptionContinuation, 
                Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                return new Realizable<TResult>(
                    new Continuation<TNew, TResult>(
                        this,
                        sourceContinuation,
                        exceptionContinuation,
                        canceledContinuation));
            }

            public IAwaiter<TNew> GetAwaiter()
            {
                return new Awaiter(
                    task,
                    this.sourceContinuation,
                    this.exceptionContinuation,
                    this.canceledContinuation);
            }

            private sealed class Awaiter : IAwaiter<TNew>
            {
                private readonly ITaskData<TOld> task;
                private readonly IAwaiter<TOld> taskAwaiter;
                private readonly Func<TOld, TNew> sourceContinuation;
                private readonly Func<Exception, TNew> exceptionContinuation;
                private readonly Func<OperationCanceledException, TNew> canceledContinuation;

                public Awaiter(
                    ITaskData<TOld> task,
                    Func<TOld, TNew> sourceContinuation,
                    Func<Exception, TNew> exceptionContinuation,
                    Func<OperationCanceledException, TNew> canceledContinuation)
                {
                    this.task = task;
                    this.sourceContinuation = sourceContinuation;
                    this.exceptionContinuation = exceptionContinuation;
                    this.canceledContinuation = canceledContinuation;

                    this.taskAwaiter = this.task.GetAwaiter();
                }

                public bool IsCompleted
                {
                    get
                    {
                        return taskAwaiter.IsCompleted;
                    }
                }

                public TNew GetResult()
                {
                    //// TODO in the `continuewith` methods of `realizable{T}` and `realizable.fromexceptiontask{T}`, you had to do some exception handling logic; do you need anything like that in this type?
                    
                    if (this.task.Exception != null)
                    {
                        var exception = this.task.Exception;
                        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
                        {
                            exception = aggregateException.InnerExceptions[0];
                        }

                        return this.exceptionContinuation(exception);
                    }
                    else if (this.task.IsCanceled)
                    {
                        return this.canceledContinuation(new OperationCanceledException("TODO"));
                    }
                    else
                    {
                        //// TODO this means that the continuation function is not run asynchronously; you can maybe do better, but maybe it's not actually an issue at all?
                        return this.sourceContinuation(this.taskAwaiter.GetResult());
                    }
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

        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter(task.GetAwaiter());
        }

        public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
        {
            private readonly Task<T> task;
            private readonly bool continueOnCapturedContext;

            public ConfiguredAwaitable(Task<T> task, bool continueOnCapturedContext)
            {
                this.task = task;
                this.continueOnCapturedContext = continueOnCapturedContext;
            }

            public IAwaiter<T> GetAwaiter()
            {
            }
        }

        private sealed class Awaiter : IAwaiter<T>
        {
            private readonly TaskAwaiter<T> taskAwaiter;
            private readonly bool? continueOnCapturedContext;

            public Awaiter(TaskAwaiter<T> taskAwaiter, bool? continueOnCapturedContext)
            {
                this.taskAwaiter = taskAwaiter;
                this.continueOnCapturedContext = continueOnCapturedContext; //// TODO write the code to leverage this
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
                return taskAwaiter.GetResult();
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
}
