using System.Threading.Tasks;
using System;

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
                return map
                    .LeftMap
                    .HandleException(exception => throw new LeftException(exception))
                    .Invoke(this.Value, context);
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
                return map
                    .RightMap
                    .HandleException(exception => throw new LeftException(exception))
                    .Invoke(this.Value, context);
            }
        }
    }

    public sealed class LeftException : Exception
    {
        public LeftException(Exception exception)
            : base(null, exception)
        {
        }
    }

    public sealed class RightException : Exception
    {
        public RightException(Exception exception)
            : base(null, exception)
        {
        }
    }

    public readonly struct Map<TSource, TContext, TResult, TFuture> //// TODO can you make this a ref struct?
        where TFuture : Future<TResult> //// TODO you need to make sure that this is the correct derived type based on the fields
    {
        private readonly Func<TSource, TContext, TResult>? sync;
        private readonly Func<TSource, TContext, Task<TResult>>? async;

        public Map(Func<TSource, TContext, TResult> func)
        {
            this.sync = func;
        }

        public Map(Func<TSource, TContext, Task<TResult>> func)
        {
            this.async = func;
        }

        public TFuture Invoke(TSource source, TContext context)
        {
            if (this.sync != null)
            {
                var self = this;
                var future = Future.Create(() => self.sync(source, context));
                return (future as TFuture)!; //// TODo get rid of null forgiveness //// TODO make a note for yourself if you can't get rid of the null forgiveness that this type is the only type that should require it in that case
            }
            else if (this.async != null)
            {
                var self = this;
                var future = Future.Create(async () => await self.async(source, context).ConfigureAwait(false));
                return (future as TFuture)!; //// TODo get rid of null forgiveness
            }
            else
            {
                throw new Exception("TODO visitor maybe?");
            }
        }

        public Map<TSource, TContext, TResult, TFuture> HandleException(Func<Exception, TResult> handler)
        {
            //// TODO how do you compose maps?
            if (this.sync != null)
            {
                var self = this;
                return new Map<TSource, TContext, TResult, TFuture>((source, context) =>
                {
                    try
                    {
                        return self.sync(source, context);
                    }
                    catch (Exception exception)
                    {
                        return handler(exception);
                    }
                });
            }
            else if (this.async != null)
            {
                var self = this;
                return new Map<TSource, TContext, TResult, TFuture>(async (source, context) =>
                {
                    try
                    {
                        return await self.async(source, context).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        return handler(exception);
                    }
                });
            }
            else
            {
                throw new Exception("TODO use visitor?");
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



    //// TODO `\|=` is listed as an operator here: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading#overloadable-operators i think it's a bug and supposed to be `|=`

    public static class Future
    {
        public static Future<T>.Sync Create<T>(Func<T> promise)
        {
            return new Future<T>.Sync(promise);
        }

        public static Future<T>.Async Create<T>(Func<Task<T>> promise)
        {
            return new Future<T>.Async(promise);
        }
    }

    public abstract class Future<T>
    {
        private Future()
        {
        }

        public sealed class Sync : Future<T>
        {
            private readonly Func<T> promise;

            public Sync(Func<T> promise)
            {
                this.promise = promise;
            }

            public T GetValue()
            {
                return this.promise();
            }
        }

        public sealed class Async : Future<T>
        {
            private readonly Func<Task<T>> promise;

            public Async(Func<Task<T>> promise)
            {
                this.promise = promise;
            }

            public async Task<T> GetValue()
            {
                return await this.promise().ConfigureAwait(false);
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

        public static async Task<TResult> ApplyAsync<TLeft, TRight, TContext, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context)
        {
            var map = Map.Left(leftMap).Right(rightMap);
            return await either.Apply(map, context).GetValue().ConfigureAwait(false);
        }



        public static IEither<TLeftFuture, TRightFuture> Select<TLeftSource, TRightSource, TLeftResult, TRightResult, TLeftFuture, TRightFuture>(
            this IEither<TLeftSource, TRightSource> either,
            Map<TLeftSource, object, TLeftResult, TLeftFuture> leftMap,
            Map<TRightSource, object, TRightResult, TRightFuture> rightMap)
            where TLeftFuture : Future<TLeftResult>
            where TRightFuture : Future<TRightResult>
        {
            var map = new Map<TLeftSource, TRightSource, IEither<TLeftFuture, TRightFuture>, object, Future<IEither<TLeftFuture, TRightFuture>>.Sync>( //// TODO you are having the result be a sync future of an either; but maybe the return type of this method should actually be a future itself? //// TODO i'm not really sure; i think that creating this maps should probably be sync because this isnt' really part of the operation that the caller is requesting, these are just transformations; and it would also mean that assertions are sync as well
                new Map<TLeftSource, object, IEither<TLeftFuture, TRightFuture>, Future<IEither<TLeftFuture, TRightFuture>>.Sync>((left, nothing) => new Either<TLeftFuture, TRightFuture>.Left(leftMap.Invoke(left, nothing))),
                new Map<TRightSource, object, IEither<TLeftFuture, TRightFuture>, Future<IEither<TLeftFuture, TRightFuture>>.Sync>((right, nothing) => new Either<TLeftFuture, TRightFuture>.Right(rightMap.Invoke(right, nothing))));

            return either.Apply(map, new object()).GetValue();
        }


        //// TODO implement the existing select on top of that
        //// TODO implement the async variants of the existing select
    }

    public readonly struct Map<TLeft, TRight, TResult, TContext, TFuture> //// TODO cna you make this a ref struct?
        where TFuture : Future<TResult> //// TODO you need to make sure that this is the correct derived type based on the fields
    {
        private readonly Map<TLeft, TContext, TResult, TFuture> leftMap;
        private readonly Map<TRight, TContext, TResult, TFuture> rightMap;

        public Map(Map<TLeft, TContext, TResult, TFuture> leftMap, Map<TRight, TContext, TResult, TFuture> rightMap)
        {
            this.leftMap = leftMap;
            this.rightMap = rightMap;
        }

        public Map(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, TResult> rightMap)
            : this(new Map<TLeft, TContext, TResult, TFuture>(leftMap), new Map<TRight, TContext, TResult, TFuture>(rightMap))
        {
        }

        public Map(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
            : this(new Map<TLeft, TContext, TResult, TFuture>(leftMap), new Map<TRight, TContext, TResult, TFuture>(rightMap))
        {
        }

        public Map(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, TResult> rightMap)
            : this(new Map<TLeft, TContext, TResult, TFuture>(leftMap), new Map<TRight, TContext, TResult, TFuture>(rightMap))
        {
        }

        public Map(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
            : this(new Map<TLeft, TContext, TResult, TFuture>(leftMap), new Map<TRight, TContext, TResult, TFuture>(rightMap))
        {
        }

        public Map<TLeft, TContext, TResult, TFuture> LeftMap => leftMap;

        public Map<TRight, TContext, TResult, TFuture> RightMap => rightMap;

        public Map<TLeft, TRight, TResult, TContext, TFuture> HandleLeftException(Func<TLeft, TContext, Exception, TResult> handler)
        {
            //// TODO what if htis is called more than once?

            return new Map<TLeft, TRight, TResult, TContext, TFuture>(
                this.syncLeftMap, 
                this.syncRightMap, 
                this.asyncLeftMap,
                this.asyncRightMap, 
                handler);


            //// TODO could this actually be done at the caller level?
            //// TODO you're really going to have to implement a bunch of stuff that's equivalent to `func` if you want this to work well
            //// TODO this type is actually doing two things: it is abstracting the return type *and* it is being a DU func; maybe have separate types for the two purposes //// TODO i think having the single function map above will also let you have implicit conversions so that you don't have to do so much with map builders (maybe?)
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
            //// TODO what if left and right are the same type?

            return this.LeftMap.Invoke(left, context);

            if (this.LeftMap != null)
            {
                var map = this.LeftMap;
                if (this.leftHandleException != null)
                {
                    var self = this;
                    map = (left, context) =>
                    {
                        try
                        {
                            return map(left, context);
                        }
                        catch (Exception exception)
                        {
                            return self.leftHandleException(left, context, exception);
                        }
                    };
                }

                var sync = Future.Create<TResult>(map);
                return (sync as TFuture)!; //// TODO can you avoid null forgiveness? //// TODO in all 4 branches
            }
            else if (this.asyncLeftMap != null)
            {
                //// TODO handle exceptions
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
            return this.RightMap.Invoke(right, context);

            if (this.syncRightMap != null)
            {
                //// TODO handle exceptions
                var sync = Future<TResult>.Create(this.syncRightMap(right, context));
                return (sync as TFuture)!;
            }
            else if (this.asyncRightMap != null)
            {
                //// TODO handle exceptions
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
