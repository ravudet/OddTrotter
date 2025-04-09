namespace Fx.Either
{
    using System;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        public static async ValueTask<Either<TLeftResult, TRightResult>> SelectAsync //// TODO should return `ieither`
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

            //// TODO you are here
            return await either
                .ApplyAsync(
                    async (left, _) =>
                    {
                        //// TODO does using blocks instead of expression cause issues?
                        var newLeft = await leftSelector(left).ConfigureAwait(false);
                        return Either.Left(newLeft).Right<TRightResult>();
                    },
                    async (right, _) =>
                    {
                        var newRight = await rightSelector(right).ConfigureAwait(false);
                        return Either.Left<TLeftResult>().Right(newRight);
                    },
                    new Nothing())
                .ConfigureAwait(false);
        }
    }
}
