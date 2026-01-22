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






        public static IConfigurableFuture<T> ToFuture<T>(this Task<T> task)
        {
            return new TaskToFuture<T>(task);
        }

        private sealed class TaskToFuture<T> : IConfigurableFuture<T>
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

            public IFuture<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new Configured(this.task, continueOnCapturedContext);
            }

            private sealed class Configured : IFuture<T>
            {
                private readonly Task<T> task;
                private readonly bool continueOnCapturedContext;

                public Configured(
                    Task<T> task,
                    bool continueOnCapturedContext)
                {
                    this.task = task;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public Exception? Exception => this.task.Exception;

                public bool IsCanceled => this.task.IsCanceled;

                public IAwaiter<T> GetAwaiter()
                {
                    return new Awaiter(this.task.ConfigureAwait(this.continueOnCapturedContext).GetAwaiter());
                }

                private sealed class Awaiter : IAwaiter<T>
                {
                    private readonly ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter;

                    public Awaiter(ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter)
                    {
                        this.awaiter = awaiter;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return this.awaiter.IsCompleted;
                        }
                    }

                    public T GetResult()
                    {
                        return this.awaiter.GetResult();
                    }

                    public void OnCompleted(Action continuation)
                    {
                        this.awaiter.OnCompleted(continuation);
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        this.awaiter.UnsafeOnCompleted(continuation);
                    }
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

        private sealed class ContinueWith2Adapter<TSource, TResult> : ITask<TResult>, IConfigurableFuture<TResult>, IConfiguredAwaitable<TResult>, IFuture<TResult>
            where TSource : allows ref struct
            where TResult : allows ref struct
        {
            private readonly IConfigurableFuture<TSource> future;
            private readonly Func<TSource, TResult> sourceContinuation;
            private readonly Func<Exception, TResult> exceptionContinuation;
            private readonly Func<OperationCanceledException, TResult> canceledContinuation;
            private readonly bool? continueOnCapturedContext;

            public ContinueWith2Adapter(
                IConfigurableFuture<TSource> future,
                Func<TSource, TResult> sourceContinuation,
                Func<Exception, TResult> exceptionContinuation,
                Func<OperationCanceledException, TResult> canceledContinuation)
                : this(
                      future,
                      sourceContinuation,
                      exceptionContinuation,
                      canceledContinuation,
                      null)
            {
            }

            private ContinueWith2Adapter(
                IConfigurableFuture<TSource> future, 
                Func<TSource, TResult> sourceContinuation,
                Func<Exception, TResult> exceptionContinuation,
                Func<OperationCanceledException, TResult> canceledContinuation,
                bool? continueOnCapturedContext)
            {
                this.future = future;
                this.sourceContinuation = sourceContinuation;
                this.exceptionContinuation = exceptionContinuation;
                this.canceledContinuation = canceledContinuation;
                this.continueOnCapturedContext = continueOnCapturedContext;
            }

            public Exception? Exception
            {
                get
                {
                    //// TODO is this supposed to throw if the task isn't completed yet? or is it `null` until we encounter an exception?

                    IAwaiter<TResult> awaiter;
                    if (this.continueOnCapturedContext == null)
                    {
                        awaiter = this.GetAwaiter();
                    }
                    else
                    {
                        awaiter = this.ConfigureAwait(this.continueOnCapturedContext.Value).GetAwaiter();
                    }

                    if (!awaiter.IsCompleted)
                    {
                        return null;
                    }

                    try
                    {
                        awaiter.GetResult();
                        return null;
                    }
                    catch (Exception exception)
                    {
                        return exception;
                    }
                }
            }

            public bool IsCanceled
            {
                get
                {
                    //// TODO is this supposed to throw if the task isn't completed yet? or is it `false` until we encounter an exception?

                    return false; // this can't ever be canceled; if `future` is canceled, we run an uncancelable `func` on the result
                }
            }

            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                return this.ConfigureAwaitImpl(continueOnCapturedContext);
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
                return new Awaiter(
                    this.future,
                    this.future.GetAwaiter(),
                    this.sourceContinuation, 
                    this.exceptionContinuation, 
                    this.canceledContinuation);
            }

            private sealed class Awaiter : IAwaiter<TResult>
            {
                private readonly IConfigurableFuture<TSource> future;
                private readonly IAwaiter<TSource> awaiter;
                private readonly Func<TSource, TResult> sourceContinuation;
                private readonly Func<Exception, TResult> exceptionContinuation;
                private readonly Func<OperationCanceledException, TResult> canceledContinuation;

                public Awaiter(
                    IConfigurableFuture<TSource> future,
                    IAwaiter<TSource> awaiter,
                    Func<TSource, TResult> sourceContinuation,
                    Func<Exception, TResult> exceptionContinuation,
                    Func<OperationCanceledException, TResult> canceledContinuation)
                {
                    this.future = future;
                    this.awaiter = awaiter;
                    this.sourceContinuation = sourceContinuation;
                    this.exceptionContinuation = exceptionContinuation;
                    this.canceledContinuation = canceledContinuation;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.awaiter.IsCompleted;
                    }
                }

                public TResult GetResult()
                {
                    if (this.future.Exception != null)
                    {
                        var exception = this.future.Exception;
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
                    else if (this.future.IsCanceled)
                    {
                        return this.canceledContinuation(new OperationCanceledException("TODO"));
                    }
                    else
                    {
                        //// TODO this means that the continuation function is not run asynchronously; you can maybe do better, but maybe it's not actually an issue at all? //// TODO i think you can use a mixin for this maybe?
                        return this.sourceContinuation(this.awaiter.GetResult());
                    }
                }

                public void OnCompleted(Action continuation)
                {
                    this.awaiter.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.awaiter.UnsafeOnCompleted(continuation);
                }
            }

            IFuture<TResult> IConfigurableFuture<TResult>.ConfigureAwait(bool continueOnCapturedContext)
            {
                return this.ConfigureAwaitImpl(continueOnCapturedContext);
            }

            private ContinueWith2Adapter<TSource, TResult> ConfigureAwaitImpl(bool continueOnCapturedContext)
            {
                return new ContinueWith2Adapter<TSource, TResult>(
                    this.future,
                    this.sourceContinuation,
                    this.exceptionContinuation,
                    this.canceledContinuation,
                    continueOnCapturedContext);
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
