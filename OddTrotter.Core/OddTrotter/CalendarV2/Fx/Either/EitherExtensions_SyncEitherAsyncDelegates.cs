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

            //// TODO you are here
            return await either
                .Apply(
                    async (left, _) =>
                    {
                        //// TODO does using blocks instead of expression cause issues?
                        ////var newLeft = ;
                        return Either.Left(await leftSelector(left).ConfigureAwait(false)).Right<TRightResult>();
                    },
                    async (right, _) =>
                    {
                        ////var newRight = ;
                        return Either.Left<TLeftResult>().Right(await rightSelector(right).ConfigureAwait(false));
                    },
                    new Nothing());
        }
    }
}
