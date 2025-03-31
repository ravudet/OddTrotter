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
            if (either is IAsyncEither<TLeftOld, TRightOld> eitherAsync)
            {
                //// TODO two issues (maybe more?)
                //// 1. we have to reimplement select
                //// 2. the type of `either` is not preserved in the return type
                return await eitherAsync
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

            return await Task
                .FromResult(
                    either
                        .Select(
                            left => leftSelector(left).ConfigureAwait(false).GetAwaiter().GetResult(),
                            right => rightSelector(right).ConfigureAwait(false).GetAwaiter().GetResult()))
                .ConfigureAwait(false);
        }
    }
}
