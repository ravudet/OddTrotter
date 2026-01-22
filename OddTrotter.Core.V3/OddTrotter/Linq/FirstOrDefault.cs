/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Linq
{
    using Fx.Either;

    public static class FirstOrDefault
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static FirstOrDefault<TFirst, TDefault> Create<TFirst, TDefault>(IEither<TFirst, TDefault> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return new FirstOrDefault<TFirst, TDefault>(either);
        }
    }
}
