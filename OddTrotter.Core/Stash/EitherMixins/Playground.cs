namespace Stash.EitherMixins
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    public interface IEither<out TLeft, out TRight>
    {
        TResult Apply<TResult, TContext>(
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context);
    }

    public interface IAsyncEither<out TLeft, out TRight>
    {
        Task<TResult> Apply<TResult, TContext>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context);

        IAsyncEitherFactory Factory { get; }
    }

    public interface IAsyncEitherFactory
    {
        Fx.Either.IEither<TLeft, TRight> CreateLeft<TLeft, TRight>(TLeft left);

        Fx.Either.IEither<TLeft, TRight> CreateRight<TLeft, TRight>(TRight right);
    }

    public sealed class AsyncEither<TLeft, TRight> : IAsyncEither<TLeft, TRight>, Fx.Either.IEither<TLeft, TRight>
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
            public Fx.Either.IEither<TLeft1, TRight1> CreateLeft<TLeft1, TRight1>(TLeft1 left)
            {
                //// TODO if the implementer doesn't care about subsequent calls, they can just do:
                return Either.Left(left).Right<TRight1>();
                ////return new AsyncEither<TLeft1, TRight1>(left);
            }

            public Fx.Either.IEither<TLeft1, TRight1> CreateRight<TLeft1, TRight1>(TRight1 right)
            {
                return new AsyncEither<TLeft1, TRight1>(right);
            }
        }

        public Task<TResult> Apply<TResult, TContext>(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap, TContext context)
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
        public static async Task<Fx.Either.IEither<TLeftNew, TRightNew>> SelectAsync<TLeftOld, TRightOld, TLeftNew, TRightNew>(
            this IAsyncEither<TLeftOld, TRightOld> either,
            Func<TLeftOld, Task<TLeftNew>> leftSelector,
            Func<TRightOld, Task<TRightNew>> rightSelector) //// TODO this should probably return an iasynceither
        {
            //// TODO are you ok with this? you now need to re-implement all of the either extensions, but for async, *and* you need to implement the above "adapter" to async...and you'll need to do this for all "core" either variants
            //// TODO i think trying this for "allows ref struct" will show if this is even feasible as a general pattern
            return await either
                    .Apply(
                        async (left, context) =>
                            either.Factory.CreateLeft<TLeftNew, TRightNew>(await leftSelector(left).ConfigureAwait(false)),
                        async (right, context) =>
                            either.Factory.CreateRight<TLeftNew, TRightNew>(await rightSelector(right).ConfigureAwait(false)),
                        new Nothing())
                    .ConfigureAwait(false);
        }
    }
}
