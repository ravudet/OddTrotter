/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    using Fx.Either;
    using Fx.Try;

    public static class EnumerableExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public static FirstOrDefault<TElement, TElement?> EitherFirstOrDefault<TElement>(
            this IEnumerable<TElement> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.EitherFirstOrDefault(default(TElement));
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        /// <remarks>
        /// I wasn't sure if I liked having a second <typeparamref name="TDefault"/> type parameter, but ultimately it just gives
        /// callers more flexibility (i.e. cases where <typeparamref name="TElement"/> and <typeparamref name="TDefault"/> are
        /// the same get resolved by the caller to simply call this method anyway). Even if we only had one type parameter, we
        /// could still implement the LINQ overloads by delegating to this overload, and this method also provides the
        /// flexibility in cases where the type parameter is the same of diffentiating between and first and a default. 
        /// </remarks>
        public static FirstOrDefault<TElement, TDefault> EitherFirstOrDefault<TElement, TDefault>(
            this IEnumerable<TElement> source,
            TDefault @default)
        {
            ArgumentNullException.ThrowIfNull(source);

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return FirstOrDefault.Create(Either.Left<TElement>().Right(@default));
                }

                return FirstOrDefault.Create(Either.Left(enumerator.Current).Right<TDefault>());
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="try"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="try"/> is <see langword="null"/>
        /// </exception>
        public static IEnumerable<TResult> TrySelect<TElement, TResult>(
            this IEnumerable<TElement> source,
            Try<TElement, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return TrySelectIterator(source, @try);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source">assumed to not be <see langword="null"/></param>
        /// <param name="try">assumed to not be <see langword="null"/></param>
        /// <returns></returns>
        private static IEnumerable<TResult> TrySelectIterator<TElement, TResult>(
            this IEnumerable<TElement> source,
            Try<TElement, TResult> @try)
        {
            foreach (var element in source)
            {
                if (@try(element, out var result))
                {
                    yield return result;
                }
            }
        }
    }
}
