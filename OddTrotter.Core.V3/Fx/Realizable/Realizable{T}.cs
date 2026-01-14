/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx;
    using Fx.Either;
    using Fx.Either.Mixins;






    public sealed class Continuation<TSource, TResult> : ITask<TResult>
        where TSource : allows ref struct
        where TResult : allows ref struct
    {
        private readonly ITask<TSource> task;
        private readonly Func<TSource, TResult> sourceContinuation;

        public Continuation(ITask<TSource> task, Func<TSource, TResult> sourceContinuation)
        {
            this.task = task;
            this.sourceContinuation = sourceContinuation;
        }

        public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Realizable<TResult1> ContinueWith<TResult1>(Func<TResult, TResult1> sourceContinuation, Func<Exception, TResult1> exceptionContinuation, Func<OperationCanceledException, TResult1> canceledContinuation) where TResult1 : allows ref struct
        {
            return new Realizable<TResult1>(new Continuation<TResult, TResult1>(this, sourceContinuation));
        }

        public IAwaiter<TResult> GetAwaiter()
        {
            throw new NotImplementedException();
        }

        private sealed class Awaiter : IAwaiter<TResult>
        {
            private readonly IAwaiter<TSource> awaiter;
            private readonly Func<TSource, TResult> sourceContinuation;

            public Awaiter(IAwaiter<TSource> awaiter, Func<TSource, TResult> sourceContinuation)
            {
                this.awaiter = awaiter;
                this.sourceContinuation = sourceContinuation;
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
                return this.sourceContinuation(this.awaiter.GetResult());
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

    public sealed class Tasker<T> : ITask<T>
        where T : allows ref struct
    {
        private readonly ITask<Realizable<T>> task;

        public Tasker(ITask<Realizable<T>> task)
        {
            this.task = task;
        }

        public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
        {
            return new Realizable<TResult>(new Continuation<T, TResult>(this, sourceContinuation));
        }

        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter(this.task.GetAwaiter());
        }

        private sealed class Awaiter : IAwaiter<T>
        {
            private readonly IAwaiter<Realizable<T>> awaiter;

            public Awaiter(IAwaiter<Realizable<T>> awaiter)
            {
                this.awaiter = awaiter;
            }

            public bool IsCompleted
            {
                get
                {
                    if (this.awaiter.IsCompleted)
                    {
                        if (this.awaiter.GetResult().AsEither.Decompose(out var value, out var future))
                        {
                            return true;
                        }
                        else
                        {
                            return future.GetAwaiter().IsCompleted;
                        }
                    }

                    return false;
                }
            }

            public T GetResult()
            {
                if (this.awaiter.GetResult().AsEither.Decompose(out var value, out var future))
                {
                    return value;
                }
                else
                {
                    return future.GetAwaiter().GetResult();
                }
            }

            public void OnCompleted(Action continuation)
            {
                this.awaiter.OnCompleted(continuation); //// TODO this actually needs to be called on the awaiter of the realizable
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                this.awaiter.UnsafeOnCompleted(continuation); //// TODO this actually needs to be called on the awaiter of the realizable
            }
        }
    }



    //// TODO better name
    public readonly ref struct Realizable<T> : IContinuable<T>, IEither<Realizable<T>, T, ITask<T>>, ICastable, IDecomposeMixin<Realizable<T>, T, ITask<T>>
        where T : allows ref struct
    {
        private readonly RefEither<T, ITask<T>> either;

        private readonly ITask<Realizable<T>>? future;

        public Realizable(T value)
        {
            either = new RefEither<T, ITask<T>>(value);
        }

        public Realizable(ITask<T> future)
        {
            //// TODO do you really want `future` to be `itask` specifically, or should this be a generic on `realizable`?
            //// TODO you're asking this because `realizable` only needs `future` to be `icontinuable`, but `icontinuable` is pretty useless for the caller since it has no way to actually get the value; so, a generic would let us receive what we require (icontinuable) without the caller needing to implement `getawaiter` if they have some other way to get the value
            either = new RefEither<T, ITask<T>>(future);
        }

        public Realizable(Realizable<Realizable<T>> realizable)
        {
            //// TODO create an apply overload that is synchronous, but takes a ref parameter; use that ref parameter to store the underlying value or future, as the case may be
            //// TODO can this actually be implemented as the `unwrap` extension? https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskextensions.unwrap?view=net-10.0&redirectedfrom=MSDN#System_Threading_Tasks_TaskExtensions_Unwrap_System_Threading_Tasks_Task_System_Threading_Tasks_Task__

            realizable.ContinueWith<Realizable<T>>(
                inner =>
                {
                    var thing = inner.ContinueWith(
                       source => source,
                       _ => throw _,
                       _ => throw _);
                    if (thing.AsEither.Decompose(out var value, out var future))
                    {

                    }

                    return thing;
                }, 
                _ => throw _, 
                _ => throw _);

            if (realizable.AsEither.Decompose(out var inner, out var future))
            {
                this.either = inner.either;
            }
            else
            {
                this.future = future;
            }
        }

        public TypeHolder<Realizable<T>, T> AsContinuable()
        {
            return new TypeHolder<Realizable<T>, T>(this);
        }

        public TypeHolder<Realizable<T>, T, ITask<T>> AsEither
        {
            get
            {
                return new TypeHolder<Realizable<T>, T, ITask<T>>(this);
            }
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
        {
            if (either.AsEither.Decompose(out var value, out var future))
            {
                TResult result;
                try
                {
                    result = sourceContinuation(value);
                }
                catch (Exception sourceException)
                {
                    return Realizable.FromException<TResult>(sourceException);
                }

                return new Realizable<TResult>(result);
            }
            else
            {
                return future.ContinueWith(
                    sourceContinuation,
                    exceptionContinuation,
                    canceledContinuation);
            }
        }

        public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<T, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<ITask<T>, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return either.ApplyAsync(leftMap, rightMap, ref context);
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct
        {
            if (DecomposeMixin.TryCreate<TCasted, Realizable<T>, T, ITask<T>>(
                this,
                out casted))
            {
                return true;
            }

            casted = default;
            return false;
        }

        bool IDecomposeMixin<Realizable<T>, T, ITask<T>>.Decompose([MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right)
        {
            return either.Decompose(out left, out right);
        }
    }
}
