namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        public static IEither<TLeftResult, TRightResult> Select
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
            return either.Select(
                (left, _) => leftSelector(left),
                (right, _) => rightSelector(right),
                new Nothing());
        }
    }
}
