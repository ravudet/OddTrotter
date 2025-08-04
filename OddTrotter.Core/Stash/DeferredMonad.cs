using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;

namespace Stash
{
    public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        public abstract TFuture Apply<TResult, TContext, TFuture>(Map<TLeft, TRight, TResult, TContext, TFuture> map, TContext context) where TFuture : Future<TResult>;

        public sealed class Left : Either<TLeft, TRight>
        {
            public Left(TLeft value)
            {
                Value = value;
            }

            public TLeft Value { get; }

            public override TFuture Apply<TResult, TContext, TFuture>(Map<TLeft, TRight, TResult, TContext, TFuture> map, TContext context)
            {
                //// TODO you need to throw leftmapexception, but if this method is actually async, then that won't work correctly
                return map.Invoke(this.Value, context);
            }
        }

        public sealed class Right : Either<TLeft, TRight>
        {
            public Right(TRight value)
            {
                Value = value;
            }

            public TRight Value { get; }

            public override TFuture Apply<TResult, TContext, TFuture>(Map<TLeft, TRight, TResult, TContext, TFuture> map, TContext context)
            {
                //// TODO you need to throw rightmapexception, but if this method is actually async, then that won't work correctly
                return map.Invoke(this.Value, context);
            }
        }
    }




    public interface IEither<TLeft, TRight> //// TODO add covariance
    {
        TFuture Apply<TResult, TContext, TFuture>(
            Map<TLeft, TRight, TResult, TContext, TFuture> map,
            TContext context)
            where TFuture : Future<TResult>;
    }

    public abstract class Future<T>
    {
        private Future()
        {
        }

        public sealed class Sync : Future<T>
        {
            public T GetValue()
            {
                return default!;
            }
        }

        public sealed class Async : Future<T>
        {
            public async Task<T> GetValue()
            {
                return await Task.FromResult(default(T)!).ConfigureAwait(false);
            }
        }
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
            private readonly Func<TLeft, TContext, TResult> leftMap;

            public MapBuilder(Func<TLeft, TContext, TResult> func)
            {
                this.leftMap = func;
                //// TODO handle the default constructor
            }

            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Sync> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return new Map<TLeft, TRight, TResult, TContext, Future<TResult>.Sync>(this.leftMap, func);
            }

            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func) //// TODO you need to be able to take in more than `task` (also `valuetask`, `itask`, `future<tresult`, etc)
            {
                return default;
            }
        }

        public readonly ref struct AsyncMapBuilder<TLeft, TContext, TResult>
        {
            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return default;
            }

            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func)
            {
                return default;
            }
        }







        public static TResult BasicApply<TLeft, TRight, TContext, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            //// TODO use the `apply().left(...).right(...).context(...)` variant
            var map = Map.Left(leftMap).Right(rightMap);
            return either.Apply(map, context).GetValue();
        }
    }

    public readonly ref struct Map<TLeft, TRight, TResult, TContext, TFuture>
        where TFuture : Future<TResult>
    {
        private readonly Func<TLeft, TContext, TResult>? syncLeftMap;
        private readonly Func<TRight, TContext, TResult>? syncRightMap;
        private readonly Func<TLeft, TContext, Task<TResult>>? asyncLeftMap;
        private readonly Func<TRight, TContext, Task<TResult>>? asyncRightMap;

        public Map(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, TResult> rightMap)
        {
            this.syncLeftMap = leftMap;
            this.syncRightMap = rightMap;
        }

        public Map(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
        {
            this.syncLeftMap = leftMap;
            this.asyncRightMap = rightMap;
        }

        public Map(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, TResult> rightMap)
        {
            this.asyncLeftMap = leftMap;
            this.syncRightMap = rightMap;
        }

        public Map(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
        {
            this.asyncLeftMap = leftMap;
            this.asyncRightMap = rightMap;
        }

        public TFuture Invoke(TLeft left, TContext context)
        {
            return default!;
        }

        public TFuture Invoke(TRight right, TContext context)
        {
            return default!;
        }
    }
}
