namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    using Fx.Realizable;

    public static class TaskExtensions
    {
        public static TaskWrapper<T> ToTaskWrapper<T>(this Task<T> task)
        {
            return new TaskWrapper<T>(task);
        }

        public static TaskWrapper<Nothing> ToTaskWrapper(this Task task)
        {
            //// TODO is this really the best way to accomplish this?
            return task.ContinueWith(_ => new Nothing()).ToTaskWrapper();
        }






        public static IFuture<T> ToFuture<T>(this Task<T> task)
        {
            return new TaskToFuture<T>(task);
        }

        private sealed class TaskToFuture<T> : IFuture<T>
        {
            private readonly Task<T> task;

            public TaskToFuture(Task<T> task)
            {
                this.task = task;
            }

            public Exception? Exception
            {
                get
                {
                    return this.task.Exception;
                }
            }

            public bool IsCanceled
            {
                get
                {
                    return this.task.IsCanceled;
                }
            }

            public IAwaiter<T> GetAwaiter()
            {
                return new Awaiter(this.task.GetAwaiter());
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
                        return this.taskAwaiter.IsCompleted;
                    }
                }

                public T GetResult()
                {
                    return this.taskAwaiter.GetResult();
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




        //// TODO is it ok to put `task` and `itask` extensions in the same class?
        




        public static ITask<TResult> ContinueWith2<TSource, TResult>(
            this IConfigurableFuture<TSource> task,
            Func<TSource, TResult> sourceContinuation,
            Func<Exception, TResult> exceptionContinuation,
            Func<OperationCanceledException, TResult> canceledContinuation)
            where TSource : allows ref struct
            where TResult : allows ref struct
        {
            return new ContinueWith2Adapter<TSource, TResult>(
                task, 
                sourceContinuation,
                exceptionContinuation,
                canceledContinuation);
        }

        private sealed class ContinueWith2Adapter<TSource, TResult> : ITask<TResult>, IConfigurableFuture<TResult>
            where TSource : allows ref struct
            where TResult : allows ref struct
        {
            private readonly IConfigurableFuture<TSource> future;
            private readonly Func<TSource, TResult> sourceContinuation;
            private readonly Func<Exception, TResult> exceptionContinuation;
            private readonly Func<OperationCanceledException, TResult> canceledContinuation;

            public ContinueWith2Adapter(
                IConfigurableFuture<TSource> future, 
                Func<TSource, TResult> sourceContinuation,
                Func<Exception, TResult> exceptionContinuation,
                Func<OperationCanceledException, TResult> canceledContinuation)
            {
                this.future = future;
                this.sourceContinuation = sourceContinuation;
                this.exceptionContinuation = exceptionContinuation;
                this.canceledContinuation = canceledContinuation;
            }

            public Exception? Exception
            {
                get
                {
                    //// TODO is this supposed to throw if the task isn't completed yet? or is it `null` until we encounter an exception?

                    if (this.future.Exception != null)
                    {
                        try
                        {
                            this.exceptionContinuation(this.future.Exception);
                            return null;
                        }
                        catch (Exception exception)
                        {
                            return exception;
                        }
                    }
                    else
                    {
                        try
                        {
                            this.GetAwaiter().GetResult();
                            return null;
                        }
                        catch (Exception exception)
                        {
                            return exception;
                        }
                    }
                }
            }

            public bool IsCanceled => throw new NotImplementedException();

            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public Realizable<TResult1> ContinueWith<TResult1>(
                Func<TResult, TResult1> sourceContinuation, 
                Func<Exception, TResult1> exceptionContinuation, 
                Func<OperationCanceledException, TResult1> canceledContinuation) where TResult1 : allows ref struct
            {
                return Realizable.FromFuture(
                    new ContinueWith2Adapter<TResult, TResult1>(
                        this,
                        sourceContinuation,
                        exceptionContinuation,
                        canceledContinuation));
            }

            public IAwaiter<TResult> GetAwaiter()
            {
                throw new NotImplementedException();
            }

            private sealed class Awaiter : IAwaiter<TResult>
            {
                public Awaiter()
                {
                }
            }

            IFuture<TResult> IConfigurableFuture<TResult>.ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IFuture<out T> //// TODO `itask` should be called `iawaitable` and this should be called `itask` (though, this doesn't have a `continuewith`, so maybe this shouldn't be called `itask`; i'm nervous about calling it `ifuture` though, because that's a name with a real mathematical meaning, and i didn't do any diligence to ensure i followed that meaning)
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter(); //// TODO this should implement `iawaitable`, i think

        Exception? Exception { get; }

        bool IsCanceled { get; }
    }

    public interface IConfigurableFuture<out T>
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter(); //// TODO this should implement `iawaitable`, i think

        Exception? Exception { get; }

        bool IsCanceled { get; }

        IFuture<T> ConfigureAwait(bool continueOnCapturedContext);
    }
}
