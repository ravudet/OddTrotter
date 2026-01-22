/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    using Fx.Realizable;

    public sealed class TaskWrapper<T> : ITask<T>, IConfigurableFuture<T>
    {
        private readonly Task<T> task;

        public Exception? Exception => this.task.Exception;

        public bool IsCanceled => this.task.IsCanceled;

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
                    new TaskData(task, null),
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

            ITaskData<TData> ConfigureAwait(bool continueOnCapturedContext);
        }

        private sealed class TaskData : ITaskData<T>
        {
            private readonly Task<T> task;
            private readonly bool? continueOnCapturedContext;

            public TaskData(Task<T> task, bool? continueOnCapturedContext)
            {
                this.task = task;
                this.continueOnCapturedContext = continueOnCapturedContext;
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

            public ITaskData<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new TaskData(this.task, continueOnCapturedContext);
            }

            public IAwaiter<T> GetAwaiter()
            {
                if (this.continueOnCapturedContext == null)
                {
                    return new TaskWrapper<T>.Awaiter(task.GetAwaiter());
                }
                else
                {
                    return new TaskWrapper<T>.ConfiguredAwaiter(task.ConfigureAwait(this.continueOnCapturedContext.Value).GetAwaiter());
                }
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

            public IConfiguredAwaitable<TNew> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    new Continuation<TOld, TNew>.Awaiter(
                        this.task.ConfigureAwait(continueOnCapturedContext),
                        this.sourceContinuation,
                        this.exceptionContinuation,
                        this.canceledContinuation));
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TNew>
            {
                private readonly Continuation<TOld, TNew>.Awaiter awaiter;

                public ConfiguredAwaitable(Continuation<TOld, TNew>.Awaiter awaiter)
                {
                    this.awaiter = awaiter;
                }

                public IAwaiter<TNew> GetAwaiter()
                {
                    return this.awaiter;
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

            ITaskData<TNew> ITaskData<TNew>.ConfigureAwait(bool continueOnCapturedContext)
            {
                return new Continuation<TOld, TNew>(
                    this.task.ConfigureAwait(continueOnCapturedContext),
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
                    if (this.task.Exception != null)
                    {
                        var exception = this.task.Exception;
                        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
                        {
                            exception = aggregateException.InnerExceptions[0];
                        }

                        return this.exceptionContinuation(exception); //// TODO you need to implement some way for `exceptioncontinuation` to rethrow without losing the stack trace; .NET rebuilds the stack trace with something called `restoredispatchstate`: https://source.dot.net/#System.Private.CoreLib/src/System/Exception.CoreCLR.cs,50a6552033907120,references; so maybe way you could do is have `exceptionContinuation == null` indicate to rethrow; it would be something like:
                        // TOld old;
                        // try
                        // {
                        //   old = this.taskAwaiter.GetResult();
                        // }
                        // catch (OperationCanceledException operationCanceledException) when (this.task.IsCanceled) // needed so that you can differentiate the task being canceled from the underlying delegate happening to throw an unrelated `operationcanceledexception`
                        // {
                        //   if (this.canceledContinuation == null)
                        //   {
                        //     throw;
                        //   }
                        // 
                        //   return this.canceledContinuation(operationCanceledException);
                        // }
                        // catch (Exception exception)
                        // {
                        //   if (this.exceptionContinuation == null)
                        //   {
                        //     throw;
                        //   }
                        // 
                        //   return this.exceptionContinuation(exception);
                        // }
                        // 
                        // return this.sourceContinuation(old);
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
            return new ConfiguredAwaitable(this.task.ConfigureAwait(continueOnCapturedContext));
        }

        IFuture<T> IConfigurableFuture<T>.ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        private sealed class ConfiguredFuture : IFuture<T>
        {
            private readonly Task<T> task;

            public ConfiguredFuture(Task<T> task)
            {
                this.task = task;
            }

            public Exception? Exception => this.task.Exception;

            public bool IsCanceled => this.task.IsCanceled;

            public IAwaiter<T> GetAwaiter()
            {
                return new ConfiguredAwaitable(this.task.ConfigureAwait(false)).GetAwaiter();
            }
        }

        private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
        {
            private readonly ConfiguredTaskAwaitable<T> task;

            public ConfiguredAwaitable(ConfiguredTaskAwaitable<T> task)
            {
                this.task = task;
            }

            public IAwaiter<T> GetAwaiter()
            {
                return new ConfiguredAwaiter(this.task.GetAwaiter());
            }
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

        private sealed class ConfiguredAwaiter : IAwaiter<T>
        {
            private readonly ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter taskAwaiter;

            public ConfiguredAwaiter(ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter taskAwaiter)
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
