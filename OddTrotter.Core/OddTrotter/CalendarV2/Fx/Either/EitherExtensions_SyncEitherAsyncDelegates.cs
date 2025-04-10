namespace Fx.Either
{
    using System;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        public static async Task<IEither<TLeftResult, TRightResult>> SelectAsync
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult,
                TContext
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, TContext, Task<TLeftResult>> leftSelector,
                Func<TRightValue, TContext, Task<TRightResult>> rightSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return await either.ApplyAsync(
                async (left, context) =>
                {
                    var newLeft = await leftSelector(left, context).ConfigureAwait(false);
                    return Either.Left(newLeft).Right<TRightResult>();
                },
                async (right, context) =>
                {
                    var newRight = await rightSelector(right, context).ConfigureAwait(false);
                    return Either.Left<TLeftResult>().Right(newRight);
                },
                context).ConfigureAwait(false);
        }

        public static async Task<IEither<TLeftResult, TRightResult>> SelectAsync
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector,
                Func<TRightValue, Task<TRightResult>> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return await either.SelectAsync(
                async (left, _) => await leftSelector(left).ConfigureAwait(false),
                async (right, _) => await rightSelector(right).ConfigureAwait(false),
                new Nothing()).ConfigureAwait(false);
        }

        public static async Task<IEither<TLeftResult, TRightValue>> SelectLeftAsync
            <
                TLeftValue,
                TRightValue,
                TLeftResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return await either.SelectAsync(
                leftSelector,
                _ => Task.FromResult(_)).ConfigureAwait(false);
        }
    }
}
