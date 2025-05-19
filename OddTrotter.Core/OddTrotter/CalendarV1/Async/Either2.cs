namespace Fx.Either
{
    using Fx.Try;
    using System;
    using System.Threading.Tasks;

    public static class Either2
    {
        public static async Task<IEither<TLeft, TRight>> Create<TValue, TLeft, TRight>(
            TValue value,
            Func<TValue, bool> discriminator,
            Func<TValue, Task<TLeft>> leftFactory,
            Func<TValue, Task<TRight>> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value))
            {
                return Either.Left(await leftFactory(value).ConfigureAwait(false)).Right<TRight>();
            }
            else
            {
                var rightResult = await rightFactory(value).ConfigureAwait(false);
                return Either.Left<TLeft>().Right(rightResult);
            }
        }

        public static async Task<IEither<TLeft, TRight>> TryCreate<TValue, TResult, TLeft, TRight>(
            TValue value,
            Try<TValue, TResult> discriminator,
            Func<TValue, TResult, Task<TLeft>> leftFactory,
            Func<TValue, Task<TRight>> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value, out var leftResult))
            {
                return Either.Left(await leftFactory(value, leftResult).ConfigureAwait(false)).Right<TRight>();
            }
            else
            {
                var rightResult = await rightFactory(value).ConfigureAwait(false);
                return Either.Left<TLeft>().Right(rightResult);
            }
        }
    }
}
