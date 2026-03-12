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
                new System.Threading.Tasks.TaskExtensions2.ContinueWith2Adapter<T, TResult>(
                    task.ToFuture(),
                    ////new TaskData(task, null),
                    sourceContinuation,
                    exceptionContinuation,
                    canceledContinuation));
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
