/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> or <paramref name="rightSelector"/> is <see langword="null"/></exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="leftSelector"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be
        /// set to whatever exception <paramref name="rightSelector"/> threw.
        /// </exception>
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
