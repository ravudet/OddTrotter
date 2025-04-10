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

            return await either
                .Apply(
                    async (left, _) =>
                    {
                        var newLeft = await leftSelector(left).ConfigureAwait(false);
                        return Either.Left(newLeft).Right<TRightResult>();
                    },
                    async (right, _) =>
                    {
                        var newRight = await rightSelector(right).ConfigureAwait(false);
                        return Either.Left<TLeftResult>().Right(newRight);
                    },
                    new Nothing());
        }
    }
}
