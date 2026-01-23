/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Fx;
    using Fx.Either;
    using Fx.Try;

    public static partial class QueryResultNodeExtensions
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
        public static QueryResultNode<TValue, TError> ToQueryResultNode<TValue, TError>(
            this IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> node)
        {
            ArgumentNullException.ThrowIfNull(node);

            return new QueryResultNode<TValue, TError>(node);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TValue, TError> Where<TValue, TError>(
            this IQueryResultNode<TValue, TError> source, 
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return source
                .SelectLeft(
                    element => element
                        .Value
                        .ToEither(predicate)
                        .Select(
                            elementValue => new WhereElement<TValue, TError>(elementValue, element.Next(), predicate),
                            nothing => element.Next().Where(predicate))
                        .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private sealed class WhereElement<TValue, TError> : IElement<TValue, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Func<TValue, bool> predicate;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="predicate"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="predicate"/> is <see langword="null"/>
            /// </exception>
            public WhereElement(TValue value, IQueryResultNode<TValue, TError> next, Func<TValue, bool> predicate)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(predicate);

                this.Value = value;
                this.next = next;
                this.predicate = predicate;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Next()
            {
                return this.next.Where(predicate);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValueSource"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TValueResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TValueResult, TError> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNode<TValueSource, TError> source, 
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return source
                .SelectLeft(
                    element =>
                        new SelectElement<TValueSource, TError, TValueResult>(
                            selector(element.Value),
                            element.Next(),
                            selector))
                .ToQueryResultNode();
        }

        private sealed class SelectElement<TValueSource, TError, TValueResult> : IElement<TValueResult, TError>
        {
            private readonly IQueryResultNode<TValueSource, TError> next;
            private readonly Func<TValueSource, TValueResult> selector;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectElement(
                TValueResult value, 
                IQueryResultNode<TValueSource, TError> next, 
                Func<TValueSource, TValueResult> selector)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(selector);

                this.Value = value;
                this.next = next;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public TValueResult Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValueResult, TError> Next()
            {
                return this.next.Select(selector);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TErrorFirst"></typeparam>
        /// <typeparam name="TErrorSecond"></typeparam>
        /// <typeparam name="TErrorResult"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="errorAggregator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="first"/> or <paramref name="second"/> or <paramref name="firstErrorSelector"/> or
        /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TValue, TErrorResult> Concat<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResultNode<TValue, TErrorFirst> first, 
            IQueryResultNode<TValue, TErrorSecond> second,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return first
                .Apply(
                    element =>
                        Either
                            .Left(
                                new ConcatFirstElement<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                    element.Value, 
                                    element.Next(),
                                    second, 
                                    firstErrorSelector, 
                                    secondErrorSelector, 
                                    errorAggregator))
                            .Right<IEither<IError<TErrorResult>, IEmpty>>()
                            .ToQueryResultNode(),
                    terminal =>
                        terminal
                            .Apply(
                                error =>
                                    ConcatTraverseSecond(
                                        new Optional<TErrorFirst>(error.Value),
                                        second, 
                                        firstErrorSelector, 
                                        secondErrorSelector, 
                                        errorAggregator),
                                empty => 
                                    ConcatTraverseSecond(
                                        default,
                                        second, 
                                        firstErrorSelector, 
                                        secondErrorSelector, 
                                        errorAggregator)));
        }

        private sealed class ConcatFirstElement<TValue, TErrorFirst, TErrorSecond, TErrorResult> : IElement<TValue, TErrorResult>
        {
            private readonly IQueryResultNode<TValue, TErrorFirst> next;
            private readonly IQueryResultNode<TValue, TErrorSecond> second;
            private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
            private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
            private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="second"></param>
            /// <param name="firstErrorSelector"></param>
            /// <param name="secondErrorSelector"></param>
            /// <param name="errorAggregator"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="second"/> or <paramref name="firstErrorSelector"/> or
            /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
            /// </exception>
            public ConcatFirstElement(
                TValue value, 
                IQueryResultNode<TValue, TErrorFirst> next, 
                IQueryResultNode<TValue, TErrorSecond> second,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(second);
                ArgumentNullException.ThrowIfNull(firstErrorSelector);
                ArgumentNullException.ThrowIfNull(secondErrorSelector);
                ArgumentNullException.ThrowIfNull(errorAggregator);

                this.Value = value;
                this.next = next;
                this.second = second;
                this.firstErrorSelector = firstErrorSelector;
                this.secondErrorSelector = secondErrorSelector;
                this.errorAggregator = errorAggregator;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TErrorResult> Next()
            {
                return this.next.Concat(this.second, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TErrorFirst"></typeparam>
        /// <typeparam name="TErrorSecond"></typeparam>
        /// <typeparam name="TErrorResult"></typeparam>
        /// <param name="error"></param>
        /// <param name="second"></param>
        /// <param name="firstErrorSelector"></param>
        /// <param name="secondErrorSelector"></param>
        /// <param name="errorAggregator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="second"/> or <paramref name="firstErrorSelector"/> or
        /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
        /// </exception>
        private static IQueryResultNode<TValue, TErrorResult> ConcatTraverseSecond
            <
                TValue, 
                TErrorFirst, 
                TErrorSecond, 
                TErrorResult
            >(
                Optional<TErrorFirst> error,
                IQueryResultNode<TValue, TErrorSecond> second,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return second
                .Apply(
                    element =>
                        Either
                            .Right<IEither<IError<TErrorResult>, IEmpty>>()
                            .Left(
                                new ConcatSecondErrorElement<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                    error, 
                                    element.Value, 
                                    element.Next(),
                                    firstErrorSelector, 
                                    secondErrorSelector, 
                                    errorAggregator))
                            .ToQueryResultNode(),
                    terminal =>
                        terminal
                            .Apply(
                                secondError =>
                                    Either
                                        .Left<IElement<TValue, TErrorResult>>()
                                        .Right(
                                            Either
                                                .Right<IEmpty>()
                                                .Left(
                                                    new Error<TErrorResult>(
                                                        error.TryGetValue(out var firstError) 
                                                            ? errorAggregator(firstError, secondError.Value) 
                                                            : secondErrorSelector(secondError.Value))))
                                        .ToQueryResultNode(),
                                empty =>
                                    error.TryGetValue(out var firstError)
                                        ? Either
                                            .Left<IElement<TValue, TErrorResult>>()
                                            .Right(
                                                Either
                                                    .Right<IEmpty>()
                                                    .Left(
                                                        new Error<TErrorResult>(
                                                            firstErrorSelector(firstError))))
                                            .ToQueryResultNode()
                                        : Either
                                            .Left<IElement<TValue, TErrorResult>>()
                                            .Right(
                                                Either
                                                    .Left<IError<TErrorResult>>()
                                                    .Right(empty))
                                            .ToQueryResultNode()));
        }

        private sealed class ConcatSecondErrorElement<TValue, TErrorFirst, TErrorSecond, TErrorResult> : 
            IElement<TValue, TErrorResult>
        {
            private readonly Optional<TErrorFirst> error;
            private readonly IQueryResultNode<TValue, TErrorSecond> next;
            private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
            private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
            private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="error"></param>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="firstErrorSelector"></param>
            /// <param name="secondErrorSelector"></param>
            /// <param name="errorAggregator"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="firstErrorSelector"/> or
            /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
            /// </exception>
            public ConcatSecondErrorElement(
                Optional<TErrorFirst> error, 
                TValue value, 
                IQueryResultNode<TValue, TErrorSecond> next,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(firstErrorSelector);
                ArgumentNullException.ThrowIfNull(secondErrorSelector);
                ArgumentNullException.ThrowIfNull(errorAggregator);

                this.error = error;
                this.Value = value;
                this.next = next;
                this.firstErrorSelector = firstErrorSelector;
                this.secondErrorSelector = secondErrorSelector;
                this.errorAggregator = errorAggregator;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TErrorResult> Next()
            {
                return ConcatTraverseSecond(
                    this.error, 
                    this.next, 
                    this.firstErrorSelector, 
                    this.secondErrorSelector, 
                    this.errorAggregator);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <see langword="null"/> or <paramref name="keySelector"/> or
        /// <paramref name="comparer"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TValue, TError> DistinctBy<TValue, TError, TKey>(
            this IQueryResultNode<TValue, TError> source, 
            Func<TValue, TKey> keySelector, 
            IEqualityComparer<TKey> comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(comparer);

            var hashSet = ImmutableHashSet.Create(comparer);
            return source.DistinctBy(keySelector, hashSet);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="hashSet"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="hashSet"/> is
        /// <see langword="null"/>
        /// </exception>
        private static IQueryResultNode<TValue, TError> DistinctBy<TValue, TError, TKey>(
            this IQueryResultNode<TValue, TError> source,
            Func<TValue, TKey> keySelector,
            ImmutableHashSet<TKey> hashSet)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(hashSet);

            var originalHashset = hashSet;
            return source
                .SelectLeft(
                    element => element
                        .ToEither(
                            _element => originalHashset != (hashSet = hashSet.Add(keySelector(_element.Value))))
                        .Select(
                            element => 
                                new DistinctByElement<TValue, TError, TKey>(
                                    element.Value, 
                                    element.Next(), 
                                    keySelector, 
                                    hashSet),
                            nothing => 
                                element.Next().DistinctBy(keySelector, hashSet))
                        .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private sealed class DistinctByElement<TValue, TError, TKey> : IElement<TValue, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Func<TValue, TKey> keySelector;
            private readonly ImmutableHashSet<TKey> hashSet;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="hashSet"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="keySelector"/> or <paramref name="hashSet"/> is
            /// <see langword="null"/>
            /// </exception>
            public DistinctByElement(
                TValue value, 
                IQueryResultNode<TValue, TError> next,
                Func<TValue, TKey> keySelector, 
                ImmutableHashSet<TKey> hashSet)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(keySelector);
                ArgumentNullException.ThrowIfNull(hashSet);

                this.Value = value;
                this.next = next;
                this.keySelector = keySelector;
                this.hashSet = hashSet;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Next()
            {
                return this.next.DistinctBy(keySelector, hashSet);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="try"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="try"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TResult, TError> TrySelect<TValue, TError, TResult>(
            this IQueryResultNode<TValue, TError> source, 
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return source
                .SelectLeft(
                    element => element
                        .Value
                        .ToEither(@try)
                        .Select(
                            tried => new TrySelectElement<TValue, TError, TResult>(tried, element.Next(), @try),
                            nothing => element.Next().TrySelect(@try))
                        .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private sealed class TrySelectElement<TValue, TError, TResult> : IElement<TResult, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Try<TValue, TResult> @try;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="try"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="try"/> is <see langword="null"/>
            /// </exception>
            public TrySelectElement(TResult value, IQueryResultNode<TValue, TError> next, Try<TValue, TResult> @try)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(@try);

                Value = value;
                this.next = next;
                this.@try = @try;
            }

            /// <inheritdoc/>
            public TResult Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TResult, TError> Next()
            {
                return this.next.TrySelect(this.@try);
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TErrorSource"></typeparam>
        /// <typeparam name="TErrorResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultNode<TValue, TErrorResult> SelectError<TValue, TErrorSource, TErrorResult>(
            this IQueryResultNode<TValue, TErrorSource> source,
            Func<TErrorSource, TErrorResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return source
                .Select(
                    element => 
                        new SelectErrorElement<TValue, TErrorSource, TErrorResult>(
                            element.Value, 
                            element.Next(), 
                            selector),
                    terminal => 
                        terminal
                            .SelectLeft(
                                error => new SelectErrorError<TErrorResult>(selector(error.Value))))
                .ToQueryResultNode();
        }

        private sealed class SelectErrorElement<TValue, TErrrorSource, TErrorResult> : IElement<TValue, TErrorResult>
        {
            private readonly IQueryResultNode<TValue, TErrrorSource> next;
            private readonly Func<TErrrorSource, TErrorResult> selector;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectErrorElement(
                TValue value, 
                IQueryResultNode<TValue, TErrrorSource> next,
                Func<TErrrorSource, TErrorResult> selector)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(selector);

                this.Value = value;
                this.next = next;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TErrorResult> Next()
            {
                return this.next.SelectError(this.selector);
            }
        }

        private sealed class SelectErrorError<TError> : IError<TError>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            public SelectErrorError(TError value)
            {
                this.Value = value;
            }

            /// <inheritdoc/>
            public TError Value { get; }
        }
    }
}
