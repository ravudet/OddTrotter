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
                TRightResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector,
                Func<TRightValue, Task<TRightResult>> rightSelector
            )
        {
            return await (await either.ConfigureAwait(false)).SelectAsync(leftSelector, rightSelector).ConfigureAwait(false);
        }
    }
}
