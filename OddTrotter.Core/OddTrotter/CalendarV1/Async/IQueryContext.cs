////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Threading.Tasks;
using System;

namespace Fx
{
    using Fx.Either;

    public static class QueryResultAsyncExtensions
    {
        public static async Task<IEither<TLeftResult, TRightValue>> SelectLeft
            <
                TLeftValue,
                TRightValue,
                TLeftResult
            >
            (
                this ITask<IEither<TLeftValue, TRightValue>> either,
                Func<TLeftValue, TLeftResult> leftSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return (await either.ConfigureAwait(false)).SelectLeft(leftSelector);
        }

        public static async ITask<IEither<TLeft, TRight>> SelectManyRight<TLeft, TRight>(
            this ITask<IEither<TLeft, IEither<TLeft, TRight>>> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return (await either.ConfigureAwait(false)).SelectManyRight(right => right);
        }

        public static async ITask<IEither<TLeft, TRight>> SelectManyLeft<TLeft, TRight>(
            this ITask<IEither<IEither<TLeft, TRight>, TRight>> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return (await either.ConfigureAwait(false)).SelectManyLeft(left => left);
        }
    }


}