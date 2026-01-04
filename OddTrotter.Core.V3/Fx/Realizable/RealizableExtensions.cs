/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;

    public static class RealizableExtensions
    {
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
