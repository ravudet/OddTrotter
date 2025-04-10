using System.Threading.Tasks;
using System;

namespace Fx.Either
{
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
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TContext, Task<TLeftResult>> leftSelector,
                Func<TRightValue, TContext, Task<TRightResult>> rightSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return await (await either.ConfigureAwait(false)).SelectAsync(leftSelector, rightSelector, context).ConfigureAwait(false);
        }

        public static async Task<IEither<TLeftResult, TRightResult>> SelectAsync
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector,
                Func<TRightValue, Task<TRightResult>> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return await (await either.ConfigureAwait(false)).SelectAsync(leftSelector, rightSelector).ConfigureAwait(false);
        }

        public static async Task<IEither<TLeftResult, TRightValue>> SelectLeftAsync
            <
                TLeftValue,
                TRightValue,
                TLeftResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return await (await either.ConfigureAwait(false)).SelectLeftAsync(leftSelector).ConfigureAwait(false);
        }
    }
}
