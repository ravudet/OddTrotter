using System.Threading.Tasks;
using System;

namespace Stash
{

    public static class Either2Extensions
    {
        public static MapBuilder<TLeft, TRight> Apply<TLeft, TRight>(
            this IEither2<TLeft, TRight> either)
        {
            return new MapBuilder<TLeft, TRight>(either);
        }

        public readonly ref struct MapBuilder<TLeft, TRight>
        {
            private readonly IEither2<TLeft, TRight> either;

            public MapBuilder(IEither2<TLeft, TRight> either)
            {
                this.either = either;
            }

            public SyncLeftBuilder<TLeft, TRight, TContext, TResult> LeftMap<TContext, TResult>(Func<TLeft, TContext, TResult> leftMap)
            {
                return new SyncLeftBuilder<TLeft, TRight, TContext, TResult>(this.either, leftMap);
            }

            public AsyncLeftBuilder<TLeft, TRight, TContext, TResult> LeftMap<TContext, TResult>(Func<TLeft, TContext, Task<TResult>> leftMap)
            {
                return new AsyncLeftBuilder<TLeft, TRight, TContext, TResult>(this.either, leftMap);
            }
        }

        public readonly ref struct SyncLeftBuilder<TLeft, TRight, TContext, TResult>
        {
            private readonly IEither2<TLeft, TRight> either;
            private readonly Func<TLeft, TContext, TResult> leftMap;

            public SyncLeftBuilder(IEither2<TLeft, TRight> either, Func<TLeft, TContext, TResult> leftMap)
            {
                this.either = either;
                this.leftMap = leftMap;
            }

            public Applier<TLeft, TRight, TContext, TResult, Future<TResult>.Sync> RightMap(Func<TRight, TContext, TResult> rightMap)
            {
                return new Applier<TLeft, TRight, TContext, TResult, Future<TResult>.Sync>(this.either, EitherMap2.Create(this.leftMap, rightMap));
            }
        }

        public readonly ref struct AsyncLeftBuilder<TLeft, TRight, TContext, TResult>
        {
            public AsyncLeftBuilder(IEither2<TLeft, TRight> either, Func<TLeft, TContext, Task<TResult>> leftMap)
            {
            }
        }

        public readonly ref struct Applier<TLeft, TRight, TContext, TResult, TFuture>
            where TFuture : Future<TResult>
        {
            private readonly IEither2<TLeft, TRight> either;
            private readonly EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map;

            public Applier(IEither2<TLeft, TRight> either, EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map)
            {
                this.either = either;
                this.map = map;
            }

            public TFuture Evaluate(TContext context)
            {
                return either.Apply(this.map, context);
            }
        }
    }


    public interface IEither2<TLeft, TRight> //// TODO covariance
    {
        TFuture Apply<TContext, TResult, TFuture>(
            EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map,
            TContext context)
            where TFuture : Future<TResult>;
    }

    public abstract class Either2<TLeft, TRight> : IEither2<TLeft, TRight>
    {
        private Either2()
        {
        }

        public abstract TFuture Apply<TContext, TResult, TFuture>(EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map, TContext context) where TFuture : Future<TResult>;

        public sealed class Left : Either2<TLeft, TRight>
        {
            public Left(TLeft value)
            {
                Value = value;
            }

            public TLeft Value { get; }

            public override TFuture Apply<TContext, TResult, TFuture>(EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map, TContext context)
            {
                //// TODO handle exception
                return map.LeftMap(this.Value, context);
            }
        }

        public sealed class Right : Either2<TLeft, TRight>
        {
            public Right(TRight value)
            {
                Value = value;
            }

            public TRight Value { get; }

            public override TFuture Apply<TContext, TResult, TFuture>(EitherMap2<TLeft, TRight, TContext, TResult, TFuture> map, TContext context)
            {
                //// TODo handle exception
                return map.RightMap(this.Value, context);
            }

            public TFuture Apply2<TContext, TResult, TFuture>(Func<TRight, TContext, TFuture> map, TContext context)
                where TFuture : IFuture<TResult, TFuture>
            {
                //// TODo handle exception
                return map(this.Value, context).HandleException(exception => throw new RightException(exception));
            }
        }
    }




    public static class EitherMap2
    {
        public static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Sync> Create<TLeft, TRight, TContext, TResult>(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap)
        {
            return EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Sync>.Create(leftMap, rightMap);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create<TLeft, TRight, TContext, TResult>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, TResult> rightMap)
        {
            return EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>.Create(leftMap, rightMap);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create<TLeft, TRight, TContext, TResult>(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap)
        {
            return EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>.Create(leftMap, rightMap);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create<TLeft, TRight, TContext, TResult>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap)
        {
            return EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>.Create(leftMap, rightMap);
        }
    }

    public sealed class EitherMap2<TLeft, TRight, TContext, TResult, TFuture>
         where TFuture : Future<TResult>
    {
        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Sync> Create(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap)
        {
            var left = (TLeft left, TContext context) => Future.Create(() => leftMap(left, context));
            var right = (TRight right, TContext context) => Future.Create(() => rightMap(right, context));
            return new EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Sync>(left, right);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, TResult> rightMap)
        {
            var left = (TLeft left, TContext context) => Future.Create(async () => await leftMap(left, context).ConfigureAwait(false));
            var right = (TRight right, TContext context) => Future.Create(async () => await Task.FromResult(rightMap(right, context)).ConfigureAwait(false)); //// TODO we are making this decision for the caller (how they want to handle the sync map when the other is async)
            return new EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>(left, right);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap)
        {
            var left = (TLeft left, TContext context) => Future.Create(async () => await Task.FromResult(leftMap(left, context)).ConfigureAwait(false));
            var right = (TRight right, TContext context) => Future.Create(async () => await rightMap(right, context).ConfigureAwait(false));
            return new EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>(left, right);
        }

        internal static EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async> Create(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap)
        {
            var left = (TLeft left, TContext context) => Future.Create(async () => await leftMap(left, context).ConfigureAwait(false));
            var right = (TRight right, TContext context) => Future.Create(async () => await rightMap(right, context).ConfigureAwait(false));
            return new EitherMap2<TLeft, TRight, TContext, TResult, Future<TResult>.Async>(left, right);
        }

        private EitherMap2(Func<TLeft, TContext, TFuture> leftMap, Func<TRight, TContext, TFuture> rightMap)
        {
            this.LeftMap = leftMap;
            this.RightMap = rightMap;
        }

        public Func<TLeft, TContext, TFuture> LeftMap { get; }
        public Func<TRight, TContext, TFuture> RightMap { get; }
    }


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

        public static TFutureResult Adapt<TResult, TFutureSource, TFutureResult>(this TFutureSource source)
            where TFutureSource : Future<TResult>
            where TFutureResult : Future<TResult>
        {
            if (source is Future<TResult>.Sync sync)
            {
                if (typeof(TFutureResult) == typeof(Future<TResult>.Sync))
                {
                    return (sync as TFutureResult)!;
                }
                else if (typeof(TFutureResult) == typeof(Future<TResult>.Async))
                {
                    var asyncResult = Future.Create(async () => await Task.FromResult(sync.GetValue()).ConfigureAwait(false));
                    return (asyncResult as TFutureResult)!;
                }
                else
                {
                    throw new Exception("tODO");
                }
            }
            else if (source is Future<TResult>.Async async)
            {
                if (typeof(TFutureResult) == typeof(Future<TResult>.Sync))
                {
                    throw new Exception("TODO can't convert from async to sync");
                }
                else if (typeof(TFutureResult) == typeof(Future<TResult>.Async))
                {
                    return (async as TFutureResult)!;
                }
                else
                {
                    throw new Exception("tODO");
                }
            }
            else
            {
                throw new Exception("tODO");
            }
        }
    }

    public interface IFuture<T, TFuture> where TFuture : IFuture<T, TFuture>
    {
        TFuture HandleException(Func<Exception, T> sync);
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
                //// TODO someone could still instantiate a `sync` with `func<task<T>>`; i'm not clear if this is a problem though
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






































    public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        public abstract TFuture Apply<TResult, TContext, TFuture>(EitherMap<TLeft, TRight, TResult, TContext, TFuture> map, TContext context) where TFuture : Future<TResult>;

        public sealed class Left : Either<TLeft, TRight>
        {
            public Left(TLeft value)
            {
                Value = value;
            }

            public TLeft Value { get; }

            public override TFuture Apply<TResult, TContext, TFuture>(EitherMap<TLeft, TRight, TResult, TContext, TFuture> map, TContext context)
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

            public override TFuture Apply<TResult, TContext, TFuture>(EitherMap<TLeft, TRight, TResult, TContext, TFuture> map, TContext context)
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

    public static class Map
    {
        public static Map<TSource, TContext, TResult, Future<TResult>.Sync> Create<TSource, TContext, TResult>(Func<TSource, TContext, TResult> func)
        {
            return Map<TSource, TContext, TResult, Future<TResult>.Sync>.Create(func);
        }

        public static Map<TSource, TContext, TResult, Future<TResult>.Async> Create<TSource, TContext, TResult>(Func<TSource, TContext, Task<TResult>> func)
        {
            return Map<TSource, TContext, TResult, Future<TResult>.Async>.Create(func);
        }
    }

    public readonly struct Map<TSource, TContext, TResult, TFuture> //// TODO can you make this a ref struct?
        : IMap<TSource, TContext, TResult, TFuture>
        where TFuture : Future<TResult>
    {
        private readonly Func<TSource, TContext, TResult>? sync;
        private readonly Func<TSource, TContext, Task<TResult>>? async;

        internal static Map<TSource, TContext, TResult, Future<TResult>.Sync> Create(Func<TSource, TContext, TResult> func)
        {
            return new Map<TSource, TContext, TResult, Future<TResult>.Sync>(func);
        }

        private Map(Func<TSource, TContext, TResult> func)
        {
            this.sync = func;
        }

        internal static Map<TSource, TContext, TResult, Future<TResult>.Async> Create(Func<TSource, TContext, Task<TResult>> func)
        {
            return new Map<TSource, TContext, TResult, Future<TResult>.Async>(func);
        }

        private Map(Func<TSource, TContext, Task<TResult>> func)
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

        public IMap<TSource, TContext, TResult, TFuture> HandleException(Func<Exception, TResult> handler)
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
            EitherMap<TLeft, TRight, TResult, TContext, TFuture> map,
            TContext context)
            where TFuture : Future<TResult>;
    }



    //// TODO `\|=` is listed as an operator here: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading#overloadable-operators i think it's a bug and supposed to be `|=`

    




    public static class EitherMap //// TODO just have an empty `apply` overload on `ieither` extensions instead i think
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

            public EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Sync> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return new EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Sync>(this.leftMap, func);
            }

            public EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func) //// TODO you need to be able to take in more than `task` (also `valuetask`, `itask`, `future<tresult`, etc)
            {
                //// TODO you are trying to actually implement an either with this new pattern
                //// TODO you need to be satisfied with all of the todos in here before you can move on
                return new EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
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

            public EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, TResult> func)
            {
                return new EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
            }

            public EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async> Right<TRight>(Func<TRight, TContext, Task<TResult>> func)
            {
                return new EitherMap<TLeft, TRight, TResult, TContext, Future<TResult>.Async>(this.leftMap, func);
            }
        }







        public static TResult BasicApply<TLeft, TRight, TContext, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            //// TODO use the `apply().left(...).right(...).context(...)` variant
            var map = EitherMap.Left(leftMap).Right(rightMap);
            return either.Apply(map, context).GetValue();
        }

        public static async Task<TResult> ApplyAsync<TLeft, TRight, TContext, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context)
        {
            var map = EitherMap.Left(leftMap).Right(rightMap);
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
                Map.Create((left, nothing) => new Either<TLeftFuture, TRightFuture>.Left(leftMap.Invoke(left, nothing))),
                Map.Create((right, nothing) => new Either<TLeftFuture, TRightFuture>.Right(rightMap.Invoke(right, nothing))));

            return either.Apply(map, new object()).GetValue();
        }


        //// TODO implement the existing select on top of that
        //// TODO implement the async variants of the existing select
    }

    public readonly struct EitherMap<TLeft, TRight, TResult, TContext, TFuture> //// TODO cna you make this a ref struct?
        where TFuture : Future<TResult>//// TODO you need to make sure that this is the correct derived type based on the fields
    {

        public EitherMap(IMap<TLeft, TContext, TResult, Future<TResult>> leftMap, IMap<TRight, TContext, TResult, Future<TResult>> rightMap)
        {
            this.LeftMap = leftMap;
            this.RightMap = rightMap;
        }

        private EitherMap(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, TResult> rightMap)
            : this(Map.Create(leftMap), Map.Create(rightMap))
        {
            //// TODO you are here and you've just had the realization that the `func` variation of `map`, while it might be useful to standalone for some other use case, is not useful for eithers
        }

        private EitherMap(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
            : this(Map.Create(leftMap), Map.Create(rightMap))
        {
        }

        private EitherMap(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, TResult> rightMap)
            : this(Map.Create(leftMap), Map.Create(rightMap))
        {
        }

        private EitherMap(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap)
            : this(Map.Create(leftMap), Map.Create(rightMap))
        {
        }

        public IMap<TLeft, TContext, TResult, Future<TResult>> LeftMap { get; }

        public IMap<TRight, TContext, TResult, Future<TResult>> RightMap { get; }

        public TFuture Invoke(TLeft source, TContext context);
        {
            
        }
    }



    public interface IMap<TSource, TContext, TResult, out TFuture>
        where TFuture : Future<TResult>
    {
        TFuture Invoke(TSource source, TContext context);

        IMap<TSource, TContext, TResult, TFuture> HandleException(Func<Exception, TResult> handler);
    }
}
