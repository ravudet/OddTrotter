namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        public static Task<Either<TLeftResult, TRightResult>> SelectAsync //// TODO should return `ieither`
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
            return either.Apply(
                async (left, _) => Either.Left(await leftSelector(left).ConfigureAwait(false)).Right<TRightResult>(),
                async (right, _) => Either.Left<TLeftResult>().Right(await rightSelector(right).ConfigureAwait(false)),
                new Nothing());
        }
    }
}
