/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Fx;
    using Fx.Either;

    public static class QueryResultNodeExtensions
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
                    element => Either
                        .Create(
                            element,
                            element => predicate(element.Value),
                            element => new WhereElement<TValue, TError>(element.Value, element.Next(), predicate),
                            element => element.Next().Where(predicate))
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
                                        new RealNullable<TErrorFirst>(error.Value),
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
                RealNullable<TErrorFirst> error,
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
                            .Left(
                                new ConcatSecondErrorElement<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                    error, 
                                    element.Value, 
                                    element.Next(),
                                    firstErrorSelector, 
                                    secondErrorSelector, 
                                    errorAggregator))
                            .Right<IEither<IError<TErrorResult>, IEmpty>>()
                            .ToQueryResultNode(),
                    terminal =>
                        terminal
                            .Apply(
                                secondError =>
                                    Either
                                        .Left<IElement<TValue, TErrorResult>>()
                                        .Right(
                                            Either
                                                .Left(
                                                    new Error<TErrorResult>(
                                                        error.TryGetValue(out var firstError) 
                                                            ? errorAggregator(firstError, secondError.Value) 
                                                            : secondErrorSelector(secondError.Value)))
                                                .Right<IEmpty>())
                                        .ToQueryResultNode(),
                                empty =>
                                    error.TryGetValue(out var firstError)
                                        ? Either
                                            .Left<IElement<TValue, TErrorResult>>()
                                            .Right(
                                                Either
                                                    .Left(
                                                        new Error<TErrorResult>(
                                                            firstErrorSelector(firstError)))
                                                    .Right<IEmpty>())
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
            private readonly RealNullable<TErrorFirst> error;
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
                RealNullable<TErrorFirst> error, 
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

        private static IQueryResultNode<TValue, TError> DistinctBy<TValue, TError, TKey>(
            this IQueryResultNode<TValue, TError> source,
            Func<TValue, TKey> keySelector,
            ImmutableHashSet<TKey> hashSet)
        {
            return source
                .SelectLeft(
                    element => Either
                        .Create(
                            element,
                            element =>
                            {
                                var key = keySelector(element.Value);
                                if (!hashSet.Contains(key))
                                {
                                    hashSet = hashSet.Add(key); //// TODO does immutable hashset do something like return the original instance if the item already existed?
                                    return true;
                                }

                                return false;
                            },
                            element => new DistinctByElement<TValue, TError, TKey>(element.Value, element.Next(), keySelector, hashSet),
                            element => element.Next().DistinctBy(keySelector, hashSet))
                        .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private sealed class DistinctByElement<TValue, TError, TKey> : IElement<TValue, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Func<TValue, TKey> keySelector;
            private readonly ImmutableHashSet<TKey> hashSet;

            private readonly Lazy<IQueryResultNode<TValue, TError>> nextResult;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            /// <param name="next"></param>
            /// <param name="hashSet"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="next"/> or <paramref name="hashSet"/> is <see langword="null"/>
            /// </exception>
            public DistinctByElement(TValue value, IQueryResultNode<TValue, TError> next, Func<TValue, TKey> keySelector, ImmutableHashSet<TKey> hashSet)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(hashSet);

                this.Value = value;
                this.next = next;
                this.keySelector = keySelector;
                this.hashSet = hashSet;

                this.nextResult = new Lazy<IQueryResultNode<TValue, TError>>(() => this.NextImpl());
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Next()
            {
                return this.nextResult.Value;
            }

            private IQueryResultNode<TValue, TError> NextImpl()
            {
                return this.next.DistinctBy(keySelector, hashSet);
            }
        }
    }
}
