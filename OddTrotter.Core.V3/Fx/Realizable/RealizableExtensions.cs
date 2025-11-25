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
            //// TODO actually use `continueoncapturedcontext`
            if (realizable.TypeHolder.Decompose(out var left, out var right))
            {
                return new TaskWrapper<T>(Task.FromResult(left)).GetAwaiter();
            }
            else
            {
                return right.GetAwaiter();
            }
        }

        public static Realizable<TResult> Select<TSource, TResult>(this Realizable<TSource> realizable, Func<TSource, TResult> selector)
            where TSource : allows ref struct
            where TResult : allows ref struct
        {
            //// TODO you are here
            return realizable.Apply(
                value => new Realizable<TResult>(selector(value)),
                future => future.ContinueWith(
                    selector,
                    _ => _,
                    _ => _));
        }
    }
}
