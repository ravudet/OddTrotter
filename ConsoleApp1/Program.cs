namespace ConsoleApp1
{
    using System;
    using System.Net.Security;
    using System.Threading.Tasks;

    using Fx.Either;

    public static class Program
    {
        public static void Main(string[] args)
        {
        }
    }

    public interface IEither<out TLeft, out TRight>
    {
        TResult Apply<TResult, TContext>(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context);
    }

    public interface IAsyncEither<out TLeft, out TRight> : Fx.Either.IEither<TLeft, TRight>
    {
        Task<TResult> ApplyAsync<TResult, TContext>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context);

        IAsyncEitherFactory Factory { get; }
    }

    public interface IAsyncEitherFactory
    {
        IAsyncEither<TLeft, TRight> CreateLeft<TLeft, TRight>(TLeft left);

        IAsyncEither<TLeft, TRight> CreateRight<TLeft, TRight>(TRight right);
    }

    public sealed class AsyncEither<TLeft, TRight> : IAsyncEither<TLeft, TRight>
    {
        public AsyncEither(TLeft left)
        {
        }

        public AsyncEither(TRight right)
        {
        }

        public IAsyncEitherFactory Factory { get; } = new AsyncEitherFactory(); //// TODO you should almost certainly do this in the "basic" either interface as well

        private sealed class AsyncEitherFactory : IAsyncEitherFactory
        {
            public IAsyncEither<TLeft1, TRight1> CreateLeft<TLeft1, TRight1>(TLeft1 left)
            {
                //// TODO if the implementer doesn't care about subsequent calls, they can just do:
                //// return Either.Left(left).Right<TRight1>();
                return new AsyncEither<TLeft1, TRight1>(left);
            }

            public IAsyncEither<TLeft1, TRight1> CreateRight<TLeft1, TRight1>(TRight1 right)
            {
                return new AsyncEither<TLeft1, TRight1>(right);
            }
        }

        public Task<TResult> ApplyAsync<TResult, TContext>(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap, TContext context)
        {
            throw new NotImplementedException();
        }

        public TResult Apply<TResult, TContext>(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, TResult> rightMap, TContext context)
        {
            throw new NotImplementedException();
        }
    }

    public static class EitherExtensions
    {
        public static async Task<Fx.Either.IEither<TLeftNew, TRightNew>> SelectAsync<TLeftOld, TRightOld, TLeftNew, TRightNew>(
            this Fx.Either.IEither<TLeftOld, TRightOld> either,
            Func<TLeftOld, Task<TLeftNew>> leftSelector,
            Func<TRightOld, Task<TRightNew>> rightSelector)
        {
            if (either is IAsyncEither<TLeftOld, TRightOld> asyncEither)
            {
                return await asyncEither
                    .SelectAsync(
                        leftSelector,
                        rightSelector)
                    .ConfigureAwait(false);
            }

            return await Task
                .FromResult(
                    either
                        .Select(
                            left => leftSelector(left).ConfigureAwait(false).GetAwaiter().GetResult(),
                            right => rightSelector(right).ConfigureAwait(false).GetAwaiter().GetResult()))
                .ConfigureAwait(false);
        }
    }

    public static class AsyncEitherExtensions
    {
        public static async Task<IAsyncEither<TLeftNew, TRightNew>> SelectAsync<TLeftOld, TRightOld, TLeftNew, TRightNew>(
            this IAsyncEither<TLeftOld, TRightOld> either,
            Func<TLeftOld, Task<TLeftNew>> leftSelector,
            Func<TRightOld, Task<TRightNew>> rightSelector)
        {
            //// TODO are you ok with this? you now need to re-implement all of the either extensions, but for async, *and* you need to implement the above "adapter" to async...and you'll need to do this for all "core" either variants
            //// TODO i think trying this for "allows ref struct" will show if this is even feasible as a general pattern
            return await either
                    .ApplyAsync(
                        async (left, context) =>
                            either.Factory.CreateLeft<TLeftNew, TRightNew>(await leftSelector(left).ConfigureAwait(false)),
                        async (right, context) =>
                            either.Factory.CreateRight<TLeftNew, TRightNew>(await rightSelector(right).ConfigureAwait(false)),
                        new Nothing())
                    .ConfigureAwait(false);
        }
    }

    public interface IRefStructResultEither<TLeft, TRight> : IEither<TLeft, TRight>
    {
        new TResult Apply<TResult, TContext>( //// TODO will using `new` here cause problems for callers?
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context) where TResult : allows ref struct;

        IRefStructResultEitherFactory Factory { get; }
    }

    public interface IRefStructResultEitherFactory
    {
        IRefStructResultEither<TLeft, TRight> CreateLeft<TLeft, TRight>(TLeft left);

        IRefStructResultEither<TLeft, TRight> CreateRight<TLeft, TRight>(TRight right);
    }

    public sealed class RefStructResultEither<TLeft, TRight> : IRefStructResultEither<TLeft, TRight>
    {
        public RefStructResultEither(TLeft left)
        {
        }

        public RefStructResultEither(TRight right)
        {
        }

        public IRefStructResultEitherFactory Factory => throw new NotImplementedException();

        private sealed class RefStructEitherFactory : IRefStructResultEitherFactory
        {
            public IRefStructResultEither<TLeft1, TRight1> CreateLeft<TLeft1, TRight1>(TLeft1 left)
            {
                //// TODO you are here, implementing the factory to finihs the select query below; then you need to implement the "converter" extension in `EitherExtensions`
                throw new NotImplementedException();
            }

            public IRefStructResultEither<TLeft1, TRight1> CreateRight<TLeft1, TRight1>(TRight1 right)
            {
                throw new NotImplementedException();
            }
        }

        public TResult Apply<TResult, TContext>(Func<TLeft, TContext, TResult> leftMap, Func<TRight, TContext, TResult> rightMap, TContext context) where TResult : allows ref struct
        {
            throw new NotImplementedException();
        }

        TResult IEither<TLeft, TRight>.Apply<TResult, TContext>(System.Func<TLeft, TContext, TResult> leftMap, System.Func<TRight, TContext, TResult> rightMap, TContext context)
        {
            throw new NotImplementedException();
        }
    }

    public static class ResultStructResultEitherExtensions
    {
        public static Fx.Either.IEither<TLeftNew, TRightNew> Select<TLeftOld, TRightOld, TLeftNew, TRightNew>(
            this IRefStructResultEither<TLeftOld, TRightOld> either,
            Func<TLeftOld, TLeftNew> leftSelector,
            Func<TRightOld, TRightNew> rightSelector) //// TODO this should probably return an IRefStructResultEither
        {
            return either
                .Apply(
                    (left, context) => Either.Left(leftSelector(left)).Right<TRightNew>(),
                    (right, context) => Either.Left<TLeftNew>().Right(rightSelector(right)),
                    new Nothing());
        }
    }
}