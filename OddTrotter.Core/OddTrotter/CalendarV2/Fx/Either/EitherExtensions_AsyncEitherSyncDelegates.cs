using System.Threading.Tasks;
using System;

namespace Fx.Either
{
    public static partial class EitherExtensions
    {
        public static async Task<IEither<TLeftResult, TRightResult>> Select
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult,
                TContext
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TContext, TLeftResult> leftSelector,
                Func<TRightValue, TContext, TRightResult> rightSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return (await either.ConfigureAwait(false)).Select(leftSelector, rightSelector, context);
        }

        public static async Task<IEither<TLeftResult, TRightResult>> Select
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TLeftResult> leftSelector,
                Func<TRightValue, TRightResult> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return (await either.ConfigureAwait(false)).Select(leftSelector, rightSelector);
        }

        public static async Task<IEither<TLeftResult, TRightValue>> SelectLeft
            <
                TLeftValue,
                TRightValue,
                TLeftResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TLeftResult> leftSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return (await either.ConfigureAwait(false)).SelectLeft(leftSelector);
        }
    }
}
