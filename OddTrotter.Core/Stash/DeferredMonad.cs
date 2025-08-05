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




    public interface IEither<TLeft, TRight> //// TODO add covariance //// TODO i think you could use an internal method on a public ifuture interface to accomplish this?
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

        public static Future<T>.Sync Create(T value)
        {
            return new Sync(value);
        }

        public static Future<T>.Async Create(Task<T> value)
        {
            return new Async(value);
        }

        public sealed class Sync : Future<T>
        {
            private readonly T value;

            public Sync(T value)
            {
                this.value = value;
            }

            public T GetValue()
            {
                return this.value;
            }
        }

        public sealed class Async : Future<T>
        {
            private readonly Task<T> value;

            public Async(Task<T> value)
            {
                this.value = value;
            }

            public async Task<T> GetValue()
            {
                return await this.value.ConfigureAwait(false);
            }
        }
    }




    public static class Map //// TODO just have an empty `apply` overload on `ieither` extensions instead i think
    {
        public static MapBuilder<TLeft, TContext, TResult> Left<TLeft, TContext, TResult>(Func<TLeft, TContext, TResult> func)
        {
            return new MapBuilder<TLeft, TContext, TResult>(func);
        }

        public static AsyncMapBuilder<TLeft, TContext, TResult> Left<TLeft, TContext, TResult>(Func<TLeft, TContext, Task<TResult>> func)
        {
            return new AsyncMapBuilder<TLeft, TContext, TResult>(func);
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

                //// TODO you are here
                //// TODO you are trying to actually implement an either with this new pattern
                //// TODO you need to be satisfied with all of the todos in here before you can move on
                return new Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
            }
        }

        public readonly ref struct AsyncMapBuilder<TLeft, TContext, TResult>
        {
            private readonly Func<TLeft, TContext, Task<TResult>> leftMap;

            public AsyncMapBuilder(Func<TLeft, TContext, Task<TResult>> func)
            {
                this.leftMap = func;
                //// TODO handle the default constructor
            }

            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return new Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
            }

            public Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func)
            {
                return new Map<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
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
        where TFuture : Future<TResult> //// TODO you need to make sure that this is the correct derived type based on the fields
    {
        private readonly Func<TLeft, TContext, TResult>? syncLeftMap;
        private readonly Func<TRight, TContext, TResult>? syncRightMap;
        private readonly Func<TLeft, TContext, Task<TResult>>? asyncLeftMap;
        private readonly Func<TRight, TContext, Task<TResult>>? asyncRightMap;

        private readonly Func<TLeft, TContext, Exception, TResult>? leftHandleException;
        private readonly Func<TRight, TContext, Exception, TResult>? rightHandleException;

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

        private Map(
            Func<TLeft, TContext, TResult>? syncLeftMap,
            Func<TRight, TContext, TResult>? syncRightMap,
            Func<TLeft, TContext, Task<TResult>>? asyncLeftMap,
            Func<TRight, TContext, Task<TResult>>? asyncRightMap,
            Func<TLeft, TContext, Exception, TResult>? leftHandleException)
        {
            this.syncLeftMap = syncLeftMap;
            this.syncRightMap = syncRightMap;
            this.asyncLeftMap = asyncLeftMap;
            this.asyncRightMap = asyncRightMap;
            this.leftHandleException = leftHandleException;
        }

        private Map(
            Func<TLeft, TContext, TResult>? syncLeftMap,
            Func<TRight, TContext, TResult>? syncRightMap,
            Func<TLeft, TContext, Task<TResult>>? asyncLeftMap,
            Func<TRight, TContext, Task<TResult>>? asyncRightMap,
            Func<TRight, TContext, Exception, TResult>? rightHandleException)
        {
            this.syncLeftMap = syncLeftMap;
            this.syncRightMap = syncRightMap;
            this.asyncLeftMap = asyncLeftMap;
            this.asyncRightMap = asyncRightMap;
            this.rightHandleException = rightHandleException;
        }

        public Map<TLeft, TRight, TResult, TContext, TFuture> HandleLeftException(Func<TLeft, TContext, Exception, TResult> handler)
        {
            //// TODO what if htis is called more than once?

            return new Map<TLeft, TRight, TResult, TContext, TFuture>(
                this.syncLeftMap, 
                this.syncRightMap, 
                this.asyncLeftMap,
                this.asyncRightMap, 
                handler);
        }

        public Map<TLeft, TRight, TResult, TContext, TFuture> HandleRightException(Func<TRight, TContext, Exception, TResult> handler)
        {
            //// TODO what if htis is called more than once?

            return new Map<TLeft, TRight, TResult, TContext, TFuture>(
                this.syncLeftMap,
                this.syncRightMap,
                this.asyncLeftMap,
                this.asyncRightMap,
                handler);
        }

        public TFuture Invoke(TLeft left, TContext context)
        {
            if (this.syncLeftMap != null)
            {
                var sync = Future<TResult>.Create(this.syncLeftMap(left, context));
                return (sync as TFuture)!; //// TODO can you avoid null forgiveness? //// TODO in all 4 branches
            }
            else if (this.asyncLeftMap != null)
            {
                var async = Future<TResult>.Create(this.asyncLeftMap(left, context));
                return (async as TFuture)!;
            }
            else
            {
                throw new Exception("TODO maybe a visitor?");
            }
        }

        public TFuture Invoke(TRight right, TContext context)
        {
            if (this.syncRightMap != null)
            {
                var sync = Future<TResult>.Create(this.syncRightMap(right, context));
                return (sync as TFuture)!;
            }
            else if (this.asyncRightMap != null)
            {
                var async = Future<TResult>.Create(this.asyncRightMap(right, context));
                return (async as TFuture)!;
            }
            else
            {
                throw new Exception("TODO maybe a visitor?");
            }
        }
    }
}
