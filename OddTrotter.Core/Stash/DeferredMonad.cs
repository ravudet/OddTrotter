using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;

namespace Stash
{
    public static class DeferredMonad
    {

    }

    public abstract class Future2<T>
    {
        private Future2()
        {
        }

        public sealed class Sync : Future2<T>
        {
            public T GetValue()
            {
                return default!;
            }
        }

        public sealed class Async : Future2<T>
        {
            public async Task<T> GetValue()
            {
                return await Task.FromResult(default(T)!).ConfigureAwait(false);
            }
        }
    }

    public struct Future<T>
    {
        /*public static implicit operator Future<T>(T value)
        {
            return new Future<T>();
        }*/

        public T Sync()
        {
            return default!;
        }
    }

    public readonly ref struct FutureFunc<T1, T2, TResult>
    {
        private readonly Func<T1, T2, TResult> func;

        public FutureFunc(Func<T1, T2, TResult> func)
        {
            this.func = func;
        }

        public Future<TResult> Invoke(T1 t1, T2 t2)
        {
            return this.func(t1, t2);
        }

        public static implicit operator FutureFunc<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return new FutureFunc<T1, T2, TResult>(func);
        }
    }

    public interface IAsyncEither<TLeft, TRight> //// TODO add covariance
    {
        Future<TResult> ApplyFuture<TResult, TContext>(
            FutureFunc<TLeft, TContext, TResult> leftMap,
            FutureFunc<TRight, TContext, TResult> rightMap,
            TContext context);

        TFuture ApplyFuture2<TResult, TContext, TFuture>(
            Map<TLeft, TRight, TResult, TContext, TFuture> map,
            TContext context)
            where TFuture : Future2<TResult>;
    }

    public static class Map //// TODO just have an empty `apply` overload on `ieither` extensions instead i think
    {
        public static MapBuilder<TLeft, TContext, TResult> Left<TLeft, TContext, TResult>(Func<TLeft, TContext, TResult> func)
        {
            return default;
        }

        public static AsyncMapBuilder<TLeft, TContext, TResult> Left<TLeft, TContext, TResult>(Func<TLeft, TContext, Task<TResult>> func)
        {
            return default;
        }

        public readonly ref struct MapBuilder<TLeft, TContext, TResult>
        {
            public Map<TLeft, TRight, TResult, TContext, Future2<TResult>.Sync> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return default;
            }

            public Map<TLeft, TRight, TResult, TContext, Future2<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func)
            {
                return default;
            }
        }

        public readonly ref struct AsyncMapBuilder<TLeft, TContext, TResult>
        {
            public Map<TLeft, TRight, TResult, TContext, Future2<TResult>.Async> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return default;
            }

            public Map<TLeft, TRight, TResult, TContext, Future2<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func)
            {
                return default;
            }
        }







        public static TResult BasicApply<TLeft, TRight, TContext, TResult>(
            this IAsyncEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            //// TODO you are here; do the `apply().left(...).right(...).context(...)` variant
            var map = Map.Left(leftMap).Right(rightMap);
            return either.ApplyFuture2(map, context).GetValue();
        }
    }

    public readonly ref struct Map<TLeft, TRight, TResult, TContext, TFuture>
        where TFuture : Future2<TResult>
    {
        public TFuture Invoke(TLeft left, TContext context)
        {
            return default!;
        }

        public TFuture Invoke(TRight right, TContext context)
        {
            return default!;
        }
    }

    public interface IMap<TValue, TContext, TResult>
    {
        TResult Map(TValue value, TContext context);
    }

    public static class AsyncEitherExtensions
    {
        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IAsyncEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            return either.ApplyFuture<TResult, TContext>(leftMap, rightMap, context).Sync();
        }

        public static class EitherMap
        {
            public static EitherMap<TValue, TContext, TResult> Create<TValue, TContext, TResult>(
                Func<TValue, TContext, Future<TResult>> @delegate)
            {
                return new EitherMap<TValue, TContext, TResult>(@delegate);
            }

            public static EitherMap<TValue, TContext, TResult> Create<TValue, TContext, TResult>(
                Func<TValue, TContext, TResult> @delegate)
            {
                return new EitherMap<TValue, TContext, TResult>((value, context) => @delegate(value, context));
            }
        }

        public readonly struct EitherMap<TValue, TContext, TResult> : IMap<TValue, TContext, Future<TResult>>
        {
            private readonly Func<TValue, TContext, Future<TResult>> @delegate;

            public EitherMap(Func<TValue, TContext, Future<TResult>> @delegate)
            {
                this.@delegate = @delegate;
            }

            public Future<TResult> Map(TValue value, TContext context)
            {
                return this.@delegate(value, context);
            }
        }

        public static Future<TResult> Apply3<TLeft, TRight, TResult, TContext>(
            this IAsyncEither<TLeft, TRight> either,
            Func<TLeft, TContext, Future<TResult>> leftMap,
            Func<TRight, TContext, Future<TResult>> rightMap,
            TContext context)
        {
            new HttpClient().GetAsync()

            return either.ApplyFuture(EitherMap.Create(leftMap), EitherMap.Create(rightMap), context);
        }

        public static Future<TResult> Apply2<TLeft, TRight, TResult, TContext>(
            this IAsyncEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, Future<TResult>> rightMap,
            TContext context)
        {
            return either.ApplyFuture(EitherMap.Create(leftMap), EitherMap.Create(rightMap), context);
        }

        public static Task<TResult> ApplyAsync<TLeft, TRight, TResult, TContext>(
            this IAsyncEither<TLeft, TRight> either,
            IMap<TLeft, TContext, Future<TResult>> leftMap,
            IMap<TRight, TContext, Future<TResult>> rightMap,
            TContext context)
        {

        }
    }
}
