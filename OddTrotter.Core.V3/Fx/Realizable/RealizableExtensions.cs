/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;

    public static class RealizableExtensions
    {
        public static IAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable)
        {
            return realizable.GetAwaiter(null);
        }

        public static ConfiguredAwaiter<T> ConfigureAwait<T>(this Realizable<T> realizable, bool continueOnCapturedContext)
        {
            return new ConfiguredAwaiter<T>(realizable, continueOnCapturedContext);
        }

        public readonly ref struct ConfiguredAwaiter<T>
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
            //// TODO you are here

            
            //// TODO make sure you call  `configureawait` on all of your awaitables

            ITask<T> task;
            if (realizable.TypeHolder.Decompose(out var left, out var right))
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
            return realizable.TypeHolder.Apply(
                value => new Realizable<TResult>(selector(value)),
                future => future.ContinueWith(
                    selector,
                    _ => throw _,
                    _ => throw _));
        }
    }
}
