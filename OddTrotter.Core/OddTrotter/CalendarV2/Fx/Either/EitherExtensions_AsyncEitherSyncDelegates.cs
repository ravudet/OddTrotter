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
                TRightResult
            >
            (
                this Task<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TLeftResult> leftSelector,
                Func<TRightValue, TRightResult> rightSelector
            )
        {
            return (await either.ConfigureAwait(false)).Select(leftSelector, rightSelector);
        }
    }
}
