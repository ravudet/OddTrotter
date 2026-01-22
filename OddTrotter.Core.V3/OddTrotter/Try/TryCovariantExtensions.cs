/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Try
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public static class TryCovariantExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="try"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="try"/> is <see langword="null"/></exception>
        public static bool Try<TInput, TOutput>(
            this TryCovariant<TInput, TOutput> @try, 
            TInput input,
            [MaybeNullWhen(false)] out TOutput output)
        {
            ArgumentNullException.ThrowIfNull(@try);

            output = @try(input, out var tried);
            return tried;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="try"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="try"/> is <see langword="null"/></exception>
        public static Try<TInput, TOutput> ToTry<TInput, TOutput>(this TryCovariant<TInput, TOutput> @try)
        {
            ArgumentNullException.ThrowIfNull(@try);

            return @try.Try;
        }
    }
}
