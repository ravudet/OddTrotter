////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Fx.Either;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OddTrotter.Calendar
{
    public static class EitherAsyncExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLeftOld"></typeparam>
        /// <typeparam name="TRightOld"></typeparam>
        /// <typeparam name="TLeftNew"></typeparam>
        /// <typeparam name="TRightNew"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> or <paramref name="rightSelector"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that <paramref name="leftSelector"/> or <paramref name="rightSelector"/> can throw</exception> //// TODO is this good?
        public static async Task<IEither<TLeftNew, TRightNew>> SelectAsync<TLeftOld, TRightOld, TLeftNew, TRightNew>(
            this Fx.Either.IEither<TLeftOld, TRightOld> either,
            Func<TLeftOld, Task<TLeftNew>> leftSelector,
            Func<TRightOld, Task<TRightNew>> rightSelector)
        {
            if (either == null)
            {
                throw new ArgumentNullException(nameof(either));
            }

            if (leftSelector == null)
            {
                throw new ArgumentNullException(nameof(leftSelector));
            }

            if (rightSelector == null)
            {
                throw new ArgumentNullException(nameof(rightSelector));
            }

            return await either.ApplyAsync(
                async (left, context) =>
                {
                    var newLeft = await leftSelector(left).ConfigureAwait(false);
                    return Either.Left(newLeft).Right<TRightNew>();
                },
                async (right, context) =>
                {
                    var newRight = await rightSelector(right).ConfigureAwait(false);
                    return Either.Left<TLeftNew>().Right(newRight);
                },
                new Nothing()).ConfigureAwait(false);

            /*return await either
                .VisitAsync(
                    async (left, context) => Either.Right<TRightNew>().Left(await leftSelector(left.Value).ConfigureAwait(false)),
                    async (right, context) => Either.Left<TLeftNew>().Right(await rightSelector(right.Value).ConfigureAwait(false)),
                    new Nothing())
                .ConfigureAwait(false);*/
        }
    }
}
