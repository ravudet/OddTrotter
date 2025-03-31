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

    public interface IAsyncEither<out TLeft, out TRight> //// TODO should the name be the other way around?
    {
        Task<TResult> ApplyAsync<TResult, TContext>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context);
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
                //// TODO two issues (maybe more?)
                //// 2. the type of `either` is not preserved in the return type
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
            Func<TRightOld, Task<TRightNew>> rightSelector)
        {
            //// TODO are you ok with this? you now need to re-implement all of the either extensions, but for async, *and* you need to implement the above "adapter" to async...and you'll need to do this for all "core" either variants
            //// TODO i think trying this for "allows ref struct" will show if this is even feasible as a general pattern
            return await either
                    .ApplyAsync(
                        async (left, context) =>
                            Either
                                .Left(
                                    await leftSelector(left).ConfigureAwait(false))
                                .Right<TRightNew>(),
                        async (right, context) =>
                            Either
                                .Left<TLeftNew>()
                                .Right(
                                    await rightSelector(right).ConfigureAwait(false)),
                        new Nothing())
                    .ConfigureAwait(false);
        }
    }
}
