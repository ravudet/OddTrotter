/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    using Fx.Realizable;

    public sealed class TaskWrapper<T> : IContinuable<T>
    {
        private readonly Task<T> task;

        public TaskWrapper(Task<T> task)
        {
            this.task = task;
        }

        public Realizable<TResult> ContinueWith<TResult>(
            Func<T, TResult> source, 
            Func<Exception, TResult> exception, 
            Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
        {
            return new Realizable<TResult>(
                new Continuation<T, TResult>(
                    new TaskData(task),
                    source,
                    exception,
                    canceled));
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
            private readonly Func<TOld, TNew> source;
            private readonly Func<Exception, TNew> exception;
            private readonly Func<OperationCanceledException, TNew> canceled;

            public Continuation(
                ITaskData<TOld> task, 
                Func<TOld, TNew> source,
                Func<Exception, TNew> exception,
                Func<OperationCanceledException, TNew> canceled)
            {
                this.task = task;
                this.source = source;
                this.exception = exception;
                this.canceled = canceled;
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
                Func<TNew, TResult> source, 
                Func<Exception, TResult> exception, 
                Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
            {
                return new Realizable<TResult>(
                    new Continuation<TNew, TResult>(
                        this,
                        source,
                        exception,
                        canceled));
            }

            public IAwaiter<TNew> GetAwaiter()
            {
                return new Awaiter(
                    task,
                    source,
                    exception,
                    canceled);
            }

            private sealed class Awaiter : IAwaiter<TNew>
            {
                private readonly ITaskData<TOld> task;
                private readonly IAwaiter<TOld> taskAwaiter;
                private readonly Func<TOld, TNew> source;
                private readonly Func<Exception, TNew> exception;
                private readonly Func<OperationCanceledException, TNew> canceled;

                public Awaiter(
                    ITaskData<TOld> task,
                    Func<TOld, TNew> source,
                    Func<Exception, TNew> exception,
                    Func<OperationCanceledException, TNew> canceled)
                {
                    this.task = task;
                    this.source = source;
                    this.exception = exception;
                    this.canceled = canceled;

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

                        return this.exception(exception);
                    }
                    else if (this.task.IsCanceled)
                    {
                        return this.canceled(new OperationCanceledException("TODO"));
                    }
                    else
                    {
                        //// TODO this means that the continuation function is not run asynchronously; you can maybe do better, but maybe it's not actually an issue at all?
                        return this.source(this.taskAwaiter.GetResult());
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

        private sealed class Awaiter : IAwaiter<T>
        {
            private readonly TaskAwaiter<T> taskAwaiter;

            public Awaiter(TaskAwaiter<T> taskAwaiter)
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
