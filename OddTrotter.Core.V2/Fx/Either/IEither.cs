/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    public interface IEither<out TLeft, out TRight>
    {
        //// TODO FUTURE there are the other `apply` variants as used by the visitor pattern in the concrete implementation:
        //// async
        //// unsafe
        //// result allows ref struct
        //// context allows ref struct
        //// context by reference
        //// there are likely others
        ////
        //// are these mixins? are they standalone types? what is the best way to handle this? is there a kernel? for example,
        //// most of the others appear that they can be built on top of an async unsafe implementation that allows ref structs
        //// and takes the context by reference; would mixins then let you do everything else?


        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="leftMap"/> or <paramref name="rightMap"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="leftMap"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="rightMap"/> threw.
        /// </exception>
        /// <remarks>
        /// This is named `apply`. Other names proposed:
        /// 1. `visit` - This leaks the design detail that the visitor pattern is used to implement the method; it's a fine name,
        /// but if we can do better, we should.
        /// 2. `aggregate`/`fold` - This is just definitely not a `fold`. The return type being a completely new, non-`ieither`
        /// value distracted me when considering this option, but ultimately there is only a single value in the `ieither`
        /// structure, so there is really no traversal happening that is essential to a `fold`, as noted 
        /// [here](https://en.wikipedia.org/wiki/Fold_(higher-order_function):
        /// > functions that analyze a recursive data structure and through use of a given combining operation, recombine the
        /// > results of recursively processing its constituent parts
        /// 3. `fmap` - Haskell has an `fmap` function
        /// ([reference](https://en.wikipedia.org/wiki/Functor#Computer_implementations)) that takes a functor. A functor maps
        /// [morphisms](https://en.wikipedia.org/wiki/Morphism) and morphisms are structure-preserving. In this method,
        /// <paramref name="leftMap"/> and <paramref name="rightMap"/> are the components of the piecewise function and (together
        /// or individually) they do *not* preserve structure (though they may be written in a way which *does* preserve
        /// structure). As a result, that piecewise function is *not* a functor, and therefore this is *not* `fmap`.
        /// 4. `morph` - This was an option because it seemed to be the "verb" form (and therefore more idiomatic to c#) of
        /// "morphism". However, as described above in `fmap`, <paramref name="leftMap"/> and <paramref name="rightMap"/> form a
        /// piecewise function that is *not* structure preserving and therefore is not a morphism.
        /// 5. `switch` - Similar to `visit`, this leaks the design detail that a discriminated union is being used to implement
        /// the method. Although this works well as an analog to the c#
        /// [switch expression](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression),
        /// the name obfuscates the monadic nature of `ieither`.
        /// 
        /// [`apply`](https://en.wikipedia.org/wiki/Apply) was chosen because this method is applying the piecewise map composed
        /// of <paramref name="leftMap"/> and <paramref name="rightMap"/> to that map's `ieither` argument.
        /// 
        /// This method throws <see cref="LeftMapException"/> or <see cref="RightMapException"/>. This is a divergence from how
        /// the LINQ APIs document delegates. With LINQ, the expectation is that the delegates don't throw. However, this is by
        /// convention. If it's documented, it is documented in a general LINQ document rather than on the individual APIs,
        /// making it more difficult to discover. My intention with wrapping the thrown exceptions into two new exception types
        /// is 4-fold:
        /// 1. I treat interfaces as contracts, and as a result, I document *at the interface level* what exceptions can be
        /// thrown. If an exception is not documented, the expectation should be that that exception will not be thrown.
        /// Following this logic, if <see cref="Apply"/> did not document *anything* for the cases where
        /// <paramref name="leftMap"/> or <paramref name="rightMap"/> throw, then callers should expect that no exceptions will
        /// be thrown in those cases.
        /// 2. There are use-cases where it is very useful for <paramref name="leftMap"/> or <paramref name="rightMap"/> to throw
        /// (consider the <see cref="Fx.Either.EitherExtensions.ThrowRight"/> method), so narrowing the scope of this method to
        /// only maps that don't throw does not match the intended function.
        /// 3. The caller of <see cref="Apply"/> is not necessarily the author of the functions provided for the maps. As a
        /// result, they will not know which exceptions they need to catch unless they restrict their own callers to only provide
        /// functions that conform to a certain contract. This option was *also* considered for <see cref="Apply"/>, though it
        /// was rejected as well (see next point).
        /// 4. I could introduce a new interface that specifies the allowed exceptions for <paramref name="leftMap"/> and
        /// <paramref name="rightMap"/> and then take instances of those interfaces as parameters of <see cref="Apply"/>.
        /// However, doing this would now require all map authors to essentially wrap their functions in a try catch and adapt
        /// their natural exceptions to conform to the contract. This is effectively what implementers of
        /// <see cref="IEither{TLeft, TRight}"/> will need to do, but having just the implementers of the interface do it, 
        /// instead of every caller, is less error-prone and reduces the barrier to entry.
        /// </remarks>
        TResult Apply<TResult, TContext>(
            Func<TLeft, TContext, TResult> leftMap, 
            Func<TRight, TContext, TResult> rightMap,
            TContext context);

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="leftMap"/> or <paramref name="rightMap"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="leftMap"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="rightMap"/> threw.
        /// </exception>
        ITask<TResult> Apply<TResult, TContext>(
            Func<TLeft, TContext, ITask<TResult>> leftMap,
            Func<TRight, TContext, ITask<TResult>> rightMap,
            TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;
    }
}
