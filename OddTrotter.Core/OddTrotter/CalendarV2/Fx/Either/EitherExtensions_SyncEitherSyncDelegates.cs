/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public static partial class EitherExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="leftMap"/> or <paramref name="rightMap"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="leftMap"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="rightMap"/> threw.
        /// </exception>
        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftMap);
            ArgumentNullException.ThrowIfNull(rightMap);

            return either.Apply(
                (left, context) => context.LeftMap(left),
                (right, context) => context.RightMap(right),
                new ApplyContext<TLeft, TRight, TResult>(leftMap, rightMap));
        }

        private sealed class ApplyContext<TLeft, TRight, TResult>
        {
            public ApplyContext(Func<TLeft, TResult> leftMap, Func<TRight, TResult> rightMap)
            {
                LeftMap = leftMap;
                RightMap = rightMap;
            }

            public Func<TLeft, TResult> LeftMap { get; }
            public Func<TRight, TResult> RightMap { get; }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> or <paramref name="rightSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="leftSelector"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be
        /// set to whatever exception <paramref name="rightSelector"/> threw.
        /// </exception>
        public static IEither<TLeftResult, TRightResult> Select
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult,
                TContext
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, TContext, TLeftResult> leftSelector,
                Func<TRightValue, TContext, TRightResult> rightSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return either.Apply(
                (left, context) => Either.Left(leftSelector(left, context)).Right<TRightResult>(),
                (right, context) => Either.Left<TLeftResult>().Right(rightSelector(right, context)),
                context);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="leftSelector"/> threw.
        /// </exception>
        public static IEither<TLeftResult, TRightValue> SelectLeft
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TContext
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, TContext, TLeftResult> leftSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return either.Select(
                leftSelector,
                (right, _) => right,
                context);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="rightSelector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be
        /// set to whatever exception <paramref name="rightSelector"/> threw.
        /// </exception>
        public static IEither<TLeftValue, TRightResult> SelectRight
            <
                TLeftValue,
                TRightValue,
                TRightResult,
                TContext
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TRightValue, TContext, TRightResult> rightSelector,
                TContext context
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return either.Select(
                (left, _) => left,
                rightSelector,
                context);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> or <paramref name="rightSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="leftSelector"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be
        /// set to whatever exception <paramref name="rightSelector"/> threw.
        /// </exception>
        public static IEither<TLeftResult, TRightResult> Select
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, TLeftResult> leftSelector,
                Func<TRightValue, TRightResult> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return either.Select(
                (left, _) => leftSelector(left),
                (right, _) => rightSelector(right),
                new Nothing());
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="leftSelector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set
        /// to whatever exception <paramref name="leftSelector"/> threw.
        /// </exception>
        /// <remarks>
        /// Although an alias for this method called `Select` could be provided to enable the LINQ comprehension syntax, I have
        /// chosen not to do so. This method would ultimately be more "discoverable" as a result of intellisense auto-complete
        /// and would likely create frustration for the caller when they need to disambgiuate. By having only "one way" to select
        /// left, the caller will never have to deal with a leaked detail about the disambiguation that can happen between
        /// <see cref="SelectLeft{TLeftValue, TRightValue, TLeftResult}(IEither{TLeftValue, TRightValue}, Func{TLeftValue, TLeftResult})"/>
        /// and 
        /// <see cref="SelectRight{TLeftValue, TRightValue, TRightResult}(IEither{TLeftValue, TRightValue}, Func{TRightValue, TRightResult})"/>
        /// </remarks>
        public static IEither<TLeftResult, TRightValue> SelectLeft
            <
                TLeftValue,
                TRightValue,
                TLeftResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, TLeftResult> leftSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);

            return either.Select(
                leftSelector,
                _ => _);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftValue"></typeparam>
        /// <typeparam name="TRightValue"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftSelector"></param>
        /// <param name="rightSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="rightSelector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightSelector"/> throws an exception. The <see cref="Exception.InnerException"/> will be
        /// set to whatever exception <paramref name="rightSelector"/> threw.
        /// </exception>
        /// <remarks>
        /// Although an alias for this method called `Select` could be provided to enable the LINQ comprehension syntax, I have
        /// chosen not to do so. This method would ultimately be more "discoverable" as a result of intellisense auto-complete
        /// and would likely create frustration for the caller when they need to disambgiuate. By having only "one way" to select
        /// right, the caller will never have to deal with a leaked detail about the disambiguation that can happen between
        /// <see cref="SelectLeft{TLeftValue, TRightValue, TLeftResult}(IEither{TLeftValue, TRightValue}, Func{TLeftValue, TLeftResult})"/>
        /// and 
        /// <see cref="SelectRight{TLeftValue, TRightValue, TRightResult}(IEither{TLeftValue, TRightValue}, Func{TRightValue, TRightResult})"/>
        /// </remarks>
        public static IEither<TLeftValue, TRightResult> SelectRight
            <
                TLeftValue,
                TRightValue,
                TRightResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TRightValue, TRightResult> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(rightSelector);

            return either.Select(
                _ => _,
                rightSelector);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftSource"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <typeparam name="TEither"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> or <paramref name="resultSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="selector"/> or <paramref name="resultSelector"/> throws an exception. The 
        /// <see cref="Exception.InnerException"/> will be set to whatever exception was thrown.
        /// </exception>
        /// <remarks>
        /// This method and its variants are analogous to the haskell bind. This is corroborated on [stackoverflow](https://stackoverflow.com/questions/19321868/linq-selectmany-is-bind):, and we can confirm this directly in the [haskell documentation](https://wiki.haskell.org/Monad):
        /// > class Monad m where
        /// > (>>=)  :: m a -> (  a -> m b) -> m b
        /// > (>>)   :: m a ->  m b         -> m b
        /// > return ::   a                 -> m a
        /// > ...
        /// > ...(i.e. the two varieties of bind: (>>=) and (>>))...
        /// </remarks>
        public static IEither<TLeftResult, TRight> SelectMany<TLeftSource, TRight, TEither, TLeftResult>(
            this IEither<TLeftSource, TRight> either,
            Func<TLeftSource, IEither<TEither, TRight>> selector,
            Func<TLeftSource, TEither, TLeftResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            return
                either
                    .Apply(
                        left =>
                        {
                            var selected = selector(left);
                            try
                            {
                                return
                                    selected
                                        .Apply(
                                            nestedLeft => Either.Left(resultSelector(left, nestedLeft)).Right<TRight>(),
                                            right => Either.Left<TLeftResult>().Right(right));
                            }
                            catch (LeftMapException leftMapException)
                            {
                                throw leftMapException.InnerException;
                            }
                        },
                        right =>
                            Either.Left<TLeftResult>().Right(right));
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftSource"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <typeparam name="TEither"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> or <paramref name="resultSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="selector"/> or <paramref name="resultSelector"/> throws an exception. The 
        /// <see cref="Exception.InnerException"/> will be set to whatever exception was thrown.
        /// </exception>
        /// <remarks>
        /// This is just an alias for
        /// <see cref="SelectMany{TLeftSource, TRight, TEither, TLeftResult}(IEither{TLeftSource, TRight}, Func{TLeftSource, IEither{TEither, TRight}}, Func{TLeftSource, TEither, TLeftResult})"/>
        /// that is useful for disambiguating type parameters and is generally preferred when not use the LINQ comprehension
        /// syntax.
        /// </remarks>
        public static IEither<TLeftResult, TRight> SelectManyLeft<TLeftSource, TRight, TEither, TLeftResult>(
            this IEither<TLeftSource, TRight> either,
            Func<TLeftSource, IEither<TEither, TRight>> selector,
            Func<TLeftSource, TEither, TLeftResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            return either.SelectMany(selector, resultSelector);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeftSource"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="selector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception was thrown.
        /// </exception>
        public static IEither<TLeftResult, TRight> SelectManyLeft<TLeftSource, TRight, TLeftResult>(
            this IEither<TLeftSource, TRight> either,
            Func<TLeftSource, IEither<TLeftResult, TRight>> selector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);

            return either.SelectManyLeft(selector, (left, nestedLeft) => nestedLeft);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static IEither<TLeft, TRight> SelectManyLeft<TLeft, TRight>(
            this IEither<IEither<TLeft, TRight>, TRight> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.SelectManyLeft(left => left);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRightSource"></typeparam>
        /// <typeparam name="TLeftInner"></typeparam>
        /// <typeparam name="TEither"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> or <paramref name="resultSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="selector"/> or <paramref name="resultSelector"/> throws an exception. The 
        /// <see cref="Exception.InnerException"/> will be set to whatever exception was thrown.
        /// </exception>
        public static IEither<TLeft, TRightResult> SelectMany<TLeft, TRightSource, TEither, TRightResult>(
            this IEither<TLeft, TRightSource> either,
            Func<TRightSource, IEither<TLeft, TEither>> selector,
            Func<TRightSource, TEither, TRightResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            return either
                .Apply(
                    left => Either.Left(left).Right<TRightResult>(),
                    right =>
                    {
                        var selected = selector(right);
                        try
                        {
                            return selected
                                .Apply(
                                    nestedLeft => Either.Left(nestedLeft).Right<TRightResult>(),
                                    nestedRight => Either.Left<TLeft>().Right(resultSelector(right, nestedRight)));
                        }
                        catch (RightMapException rightMapException)
                        {
                            throw rightMapException.InnerException;
                        }
                    });
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRightSource"></typeparam>
        /// <typeparam name="TLeftInner"></typeparam>
        /// <typeparam name="TEither"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> or <paramref name="resultSelector"/> is
        /// <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="selector"/> or <paramref name="resultSelector"/> throws an exception. The 
        /// <see cref="Exception.InnerException"/> will be set to whatever exception was thrown.
        /// </exception>
        /// <remarks>
        /// This is just an alias for
        /// <see cref="SelectMany{TLeft, TRightSource, TEither, TRightResult}(IEither{TLeft, TRightSource}, Func{TRightSource, IEither{TLeft, TEither}}, Func{TRightSource, TEither, TRightResult})"/>
        /// that is useful for disambiguating type parameters and is generally preferred when not use the LINQ comprehension
        /// syntax.
        /// </remarks>
        public static IEither<TLeft, TRightResult> SelectManyRight<TLeft, TRightSource, TEither, TRightResult>(
            this IEither<TLeft, TRightSource> either,
            Func<TRightSource, IEither<TLeft, TEither>> selector,
            Func<TRightSource, TEither, TRightResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            return either.SelectMany(selector, resultSelector);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRightSource"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="selector"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="selector"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception was thrown.
        /// </exception>
        public static IEither<TLeft, TRightResult> SelectManyRight<TLeft, TRightSource, TRightResult>(
            this IEither<TLeft, TRightSource> either,
            Func<TRightSource, IEither<TLeft, TRightResult>> selector)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(selector);

            return either.SelectManyRight(selector, (right, nestedRight) => nestedRight);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static IEither<TLeft, TRight> SelectManyRight<TLeft, TRight>(
            this IEither<TLeft, IEither<TLeft, TRight>> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.SelectManyRight(right => right);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static bool TryGetLeft<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left)
        {
            ArgumentNullException.ThrowIfNull(either);

            //// TODO FUTURE can you do a ref struct for these tuples?
            var result = either.Apply(
                left => (left, true),
                right => (default(TLeft), false));

            left = result.Item1;
            return result.Item2;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static bool TryGetRight<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TRight right)
        {
            ArgumentNullException.ThrowIfNull(either);

            var result = either.Apply(
                left => (default(TRight), false),
                right => (right, true));

            right = result.Item1;
            return result.Item2;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <param name="either"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static bool TryGet<TLeft>(this IEither<TLeft, Nothing> either, [MaybeNullWhen(false)] out TLeft left)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.TryGetLeft(out left);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword=""/></exception>
        public static bool TryGet<TRight>(this IEither<Nothing, TRight> either, [MaybeNullWhen(false)] out TRight right)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.TryGetRight(out right);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="coalescer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="coalescer"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="coalescer"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="coalescer"/> threw.
        /// </exception>
        /// <remarks>
        /// This is named "coalesce" to re-use the c# idiom of "null-coalescing operator". This is named "right" because, like the
        /// null-coalescing operator, if <typeparamref name="TRight"/> here were "null", then we would be "removing" the right
        /// type the same was we would be "removing" the null in a null-coalescing operator.
        /// </remarks>
        public static TLeft CoalesceRight<TLeft, TRight>(this IEither<TLeft, TRight> either, Func<TRight, TLeft> coalescer)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(coalescer);

            return either.Apply(left => left, coalescer);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="coalescer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="either"/> or <paramref name="coalescer"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="coalescer"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="coalescer"/> threw.
        /// </exception>
        public static TRight CoalesceLeft<TLeft, TRight>(this IEither<TLeft, TRight> either, Func<TLeft, TRight> coalescer)
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(coalescer);

            return either.Apply(coalescer, right => right);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static TValue Coalesce<TValue>(this IEither<TValue, TValue> either)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.CoalesceRight(right => right);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <param name="either"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static TLeft Coalesce<TLeft>(this IEither<TLeft, Nothing> either, TLeft @default)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.CoalesceRight(_ => @default);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        public static TRight Coalesce<TRight>(this IEither<Nothing, TRight> either, TRight @default)
        {
            ArgumentNullException.ThrowIfNull(either);

            return either.CoalesceLeft(_ => @default);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        /// <exception cref="TRight">Thrown if <paramref name="either"/> has the "right" value</exception>
        public static TLeft ThrowRight<TLeft, TRight>(this IEither<TLeft, TRight> either) where TRight : Exception
        {
            ArgumentNullException.ThrowIfNull(either);

            try
            {
                return either.CoalesceRight(right => throw right);
            }
            catch (RightMapException rightMapException)
            {
                throw rightMapException.InnerException;
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="either"/> is <see langword="null"/></exception>
        /// <exception cref="TLeft">Thrown if <paramref name="either"/> has the "left" value</exception>
        public static TRight ThrowLeft<TLeft, TRight>(this IEither<TLeft, TRight> either) where TLeft : Exception
        {
            ArgumentNullException.ThrowIfNull(either);

            try
            {
                return either.CoalesceLeft(left => throw left);
            }
            catch (LeftMapException leftMapException)
            {
                throw leftMapException.InnerException;
            }
        }
    }
}
