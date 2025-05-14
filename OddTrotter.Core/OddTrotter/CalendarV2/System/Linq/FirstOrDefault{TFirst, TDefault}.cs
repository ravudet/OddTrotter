/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Linq
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TDefault"></typeparam>
    /// <remarks>
    /// You were tempted to nest this in <see cref="EnumerableExtensions"/>, but realized that its name would conflict with the
    /// <see cref="EnumerableExtensions.EitherFirstOrDefault{TElement}(Collections.Generic.IEnumerable{TElement})"/> if you had
    /// named *that* method how you wanted to, but you couldn't because then it would conflict with the existing LINQ extension.
    /// So, we are choosing to not compound mistakes and are putting this in its own file.
    /// </remarks>
    public sealed class FirstOrDefault<TFirst, TDefault> : IEither<TFirst, TDefault>
    {
        private readonly IEither<TFirst, TDefault> either;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="either"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public FirstOrDefault(IEither<TFirst, TDefault> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            this.either = either;
        }

        /// <inheritdoc/>
        public TResult Apply<TResult, TContext>(
            Func<TFirst, TContext, TResult> leftMap,
            Func<TDefault, TContext, TResult> rightMap,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return this.either.Apply(leftMap, rightMap, context);
        }

        /// <inheritdoc/>
        public async Task<TResult> Apply<TResult, TContext>(
            Func<TFirst, TContext, Task<TResult>> leftMap,
            Func<TDefault, TContext, Task<TResult>> rightMap, 
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return await this.either.Apply(leftMap, rightMap, context).ConfigureAwait(false);
        }
    }
}
