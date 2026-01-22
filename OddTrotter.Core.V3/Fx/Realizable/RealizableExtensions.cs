/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;

    public static class RealizableExtensions
    {



        public static Realizable<TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight>> AsEither<TLeft, TRight>(
            this Realizable<RefEither<TLeft, TRight>> realizable)
        {
            //// TODO are you really happy with the name of this extension?
            return realizable
                .AsEither
                .Apply( //// TODO should be `selectleft`
                    either => new Realizable<TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight>>(either.AsEither),
                    future => future.ContinueWith(either => either.AsEither, _ => throw _, _ => throw _));
        }






        public static Realizable<T> Unwrap<T>(this Realizable<Realizable<T>> realizable)
            where T : allows ref struct
        {
            //// TODO i think `realizable` could implement `itask<T>` (even though `await` can't actually be used with it), and then this method could actually work for all nested awaitables; for `await` to still work for `realizable`s without `ref struct` type parameters, i think you will need to implement `itask` *explicitly` so that the `getawaiter` extension is found, and not the "native" implementation on `realizable`

            if (realizable.AsEither.Decompose(out var inner, out var future))
            {
                return inner;
            }
            else
            {
                return new Realizable<T>(new Tasker<T>(future));
            }
        }

        public sealed class Continuation<TSource, TResult> : ITask<TResult> //// TODO better name //// TODO somehow consolidate this with taskwrapper.continuation //// TODO if this remains public, it should go outside of the extensions class and in its own file
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
                return new ConfiguredAwaitable(this.task, this.sourceContinuation, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TResult>
            {
                private readonly ITask<TSource> task;
                private readonly Func<TSource, TResult> sourceContinuation;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(ITask<TSource> task, Func<TSource, TResult> sourceContinuation, bool continueOnCapturedContext)
                {
                    this.task = task;
                    this.sourceContinuation = sourceContinuation;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public IAwaiter<TResult> GetAwaiter()
                {
                    return new Awaiter(this.task.ConfigureAwait(this.continueOnCapturedContext).GetAwaiter(), this.sourceContinuation);
                }
            }

            public Realizable<TResult1> ContinueWith<TResult1>(Func<TResult, TResult1> sourceContinuation, Func<Exception, TResult1> exceptionContinuation, Func<OperationCanceledException, TResult1> canceledContinuation) where TResult1 : allows ref struct
            {
                return new Realizable<TResult1>(new Continuation<TResult, TResult1>(this, sourceContinuation));
            }

            public IAwaiter<TResult> GetAwaiter()
            {
                return new Awaiter(this.task.GetAwaiter(), this.sourceContinuation);
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

        private sealed class Tasker<T> : ITask<T> //// TODO better name
            where T : allows ref struct
        {
            private readonly ITask<Realizable<T>> task;

            public Tasker(ITask<Realizable<T>> task)
            {
                this.task = task;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.task, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
            {
                private readonly ITask<Realizable<T>> task;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(ITask<Realizable<T>> task, bool continueOnCapturedContext)
                {
                    this.task = task;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public IAwaiter<T> GetAwaiter()
                {
                    return new Awaiter(this.task.ConfigureAwait(this.continueOnCapturedContext).GetAwaiter());
                }
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                return Realizable.FromFuture(
                    this.ToFuture().ContinueWith2(sourceContinuation, exceptionContinuation, canceledContinuation));

                //// TODO `continuation` doesn't take exceptions and cancellations into account
                ////return new Realizable<TResult>(new Continuation<T, TResult>(this, sourceContinuation));
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

        public readonly ref struct RealizableAwaitable<T> : ITask<ConfiguredAwaiter<T>, IAwaiter<T>, T>
        {
            private readonly Realizable<T> realizable;

            public RealizableAwaitable(Realizable<T> realizable)
            {
                this.realizable = realizable;
            }

            public TypeHolder<RealizableAwaitable<T>, ConfiguredAwaiter<T>, IAwaiter<T>, T> AsAwaitable()
            {
                return new TypeHolder<RealizableAwaitable<T>, ConfiguredAwaiter<T>, IAwaiter<T>, T>(this);
            }

            public ConfiguredAwaiter<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return this.realizable.ConfigureAwait(continueOnCapturedContext);
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                return this.realizable.ContinueWith(sourceContinuation, exceptionContinuation, canceledContinuation);
            }

            public IAwaiter<T> GetAwaiter()
            {
                return this.realizable.GetAwaiter();
            }
        }

        public static RealizableAwaitable<T> ToAwaitable<T>(this Realizable<T> realizable)
        {
            //// TODO you should be able to convert *any* continuable into an awaitable
            return new RealizableAwaitable<T>(realizable);
        }

        public static IAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable)
        {
            return realizable.GetAwaiter(null);
        }

        public static ConfiguredAwaiter<T> ConfigureAwait<T>(this Realizable<T> realizable, bool continueOnCapturedContext)
        {
            return new ConfiguredAwaiter<T>(realizable, continueOnCapturedContext);
        }

        public readonly ref struct ConfiguredAwaiter<T> : IConfiguredAwaitable<T>
        {
            private readonly Realizable<T> realizable;
            private readonly bool continueOnCapturedContext;

            public ConfiguredAwaiter(Realizable<T> realizable, bool continueOnCapturedContext)
            {
                this.realizable = realizable;
                this.continueOnCapturedContext = continueOnCapturedContext;
            }

            public IAwaiter<T> GetAwaiter()
            {
                return this.realizable.GetAwaiter(this.continueOnCapturedContext);
            }
        }

        private static IAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable, bool? continueOnCapturedContext)
        {
            ITask<T> task;
            if (realizable.AsEither.Decompose(out var left, out var right))
            {
                task = new TaskWrapper<T>(Task.FromResult(left));
            }
            else
            {
                task = right;
            }

            if (continueOnCapturedContext != null)
            {
                return task.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter();
            }
            else
            {
                return task.GetAwaiter();
            }
        }

        public static Realizable<TResult> Select<TSource, TResult>(this Realizable<TSource> realizable, Func<TSource, TResult> selector)
            where TSource : allows ref struct
            where TResult : allows ref struct
        {
            return realizable.AsEither.Apply(
                value => new Realizable<TResult>(selector(value)),
                future => future.ContinueWith(
                    selector,
                    _ => throw _,
                    _ => throw _));
        }
    }
}
