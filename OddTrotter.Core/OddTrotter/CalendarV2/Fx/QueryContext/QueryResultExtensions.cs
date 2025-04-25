/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fx.Either;
    using Fx.Try;

    public static class QueryResultExtensions
    {
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
        public static IQueryResult<TValue, TError> Where<TValue, TError>(
            this IQueryResult<TValue, TError> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return new WhereQueryResult<TValue, TError>(source, predicate);
        }

        private sealed class WhereQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            private readonly IQueryResult<TValue, TError> source;
            private readonly Func<TValue, bool> predicate;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="predicate"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
            /// </exception>
            public WhereQueryResult(IQueryResult<TValue, TError> source, Func<TValue, bool> predicate)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(predicate);

                this.source = source;
                this.predicate = predicate;
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Nodes
            {
                get
                {
                    return this.source.Nodes.Where(predicate);
                }
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
        public static IQueryResult<TValueResult, TError> Select<TValueSource, TError, TValueResult>(
            this IQueryResult<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return new SelectQueryResult<TValueSource, TError, TValueResult>(source, selector);
        }

        private sealed class SelectQueryResult<TValueSource, TError, TValueResult> : IQueryResult<TValueResult, TError>
        {
            private readonly IQueryResult<TValueSource, TError> source;
            private readonly Func<TValueSource, TValueResult> selector;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectQueryResult(IQueryResult<TValueSource, TError> source, Func<TValueSource, TValueResult> selector)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(selector);

                this.source = source;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValueResult, TError> Nodes
            {
                get
                {
                    return this.source.Nodes.Select(selector);
                }
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/></exception>
        public static IEither<FirstOrDefault<TElement, TDefault>, TError> FirstOrDefault<TElement, TError, TDefault>(
            this IQueryResult<TElement, TError> source, 
            TDefault @default)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source
                .Nodes
                .Apply(
                    element =>
                        Either
                            .Left(
                                System.Linq.FirstOrDefault.Create(
                                    Either
                                        .Left(element.Value)
                                        .Right<TDefault>()))
                            .Right<TError>(),
                    terminal =>
                        terminal
                            .Apply(
                                error =>
                                    Either
                                        .Left<FirstOrDefault<TElement, TDefault>>()
                                        .Right(error.Value),
                                empty =>
                                    Either
                                        .Left(
                                            System.Linq.FirstOrDefault.Create(
                                                Either.Left<TElement>().Right(@default)))
                                        .Right<TError>()));
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
        /// Thrown if <paramref name="first"/> or <paramref name="second"/> <paramref name="firstErrorSelector"/> or
        /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResult<TValue, TErrorResult> Concat<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResult<TValue, TErrorFirst> first, 
            IQueryResult<TValue, TErrorSecond> second, 
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return new ConcatQueryResult<TValue, TErrorFirst, TErrorSecond, TErrorResult>(first, second, firstErrorSelector, secondErrorSelector, errorAggregator);
        }

        private sealed class ConcatQueryResult<TValue, TErrorFirst, TErrorSecond, TErrorResult> : 
            IQueryResult<TValue, TErrorResult>
        {
            private readonly IQueryResult<TValue, TErrorFirst> first;
            private readonly IQueryResult<TValue, TErrorSecond> second;
            private readonly Func<TErrorFirst, TErrorResult> firstErrorSelector;
            private readonly Func<TErrorSecond, TErrorResult> secondErrorSelector;
            private readonly Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="first"></param>
            /// <param name="second"></param>
            /// <param name="errorAggregator"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="first"/> or <paramref name="second"/> <paramref name="firstErrorSelector"/> or
            /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
            /// </exception>
            public ConcatQueryResult(
                IQueryResult<TValue, TErrorFirst> first, 
                IQueryResult<TValue, TErrorSecond> second,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
            {
                ArgumentNullException.ThrowIfNull(first);
                ArgumentNullException.ThrowIfNull(second);
                ArgumentNullException.ThrowIfNull(firstErrorSelector);
                ArgumentNullException.ThrowIfNull(secondErrorSelector);
                ArgumentNullException.ThrowIfNull(errorAggregator);

                this.first = first;
                this.second = second;
                this.firstErrorSelector = firstErrorSelector;
                this.secondErrorSelector = secondErrorSelector;
                this.errorAggregator = errorAggregator;
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TErrorResult> Nodes
            {
                get
                {
                    return this.first.Nodes.Concat(second.Nodes, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator);
                }
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
        /// Thrown if <paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="comparer"/> is
        /// <see langword="null"/>
        /// </exception>
        public static IQueryResult<TValue, TError> DistinctBy<TValue, TError, TKey>(
            this IQueryResult<TValue, TError> source, 
            Func<TValue, TKey> keySelector, 
            IEqualityComparer<TKey> comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(comparer);

            return new DistinctByResult<TValue, TError, TKey>(source, keySelector, comparer);
        }

        private sealed class DistinctByResult<TValue, TError, TKey> : IQueryResult<TValue, TError>
        {
            private readonly IQueryResult<TValue, TError> source;
            private readonly Func<TValue, TKey> keySelector;
            private readonly IEqualityComparer<TKey> comparer;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="keySelector"></param>
            /// <param name="comparer"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="comparer"/> is
            /// <see langword="null"/>
            /// </exception>
            public DistinctByResult(
                IQueryResult<TValue, TError> source, 
                Func<TValue, TKey> keySelector, 
                IEqualityComparer<TKey> comparer)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(keySelector);
                ArgumentNullException.ThrowIfNull(comparer);

                this.source = source;
                this.keySelector = keySelector;
                this.comparer = comparer;
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Nodes
            {
                get
                {
                    return this.source.Nodes.DistinctBy(this.keySelector, comparer);
                }
            }
        }

        public static IQueryResult<TResult, TError> TrySelect<TValue, TError, TResult>(this IQueryResult<TValue, TError> source, Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return new TrySelectResult<TValue, TError, TResult>(source, @try);
        }

        private sealed class TrySelectResult<TValue, TError, TResult> : IQueryResult<TResult, TError>
        {
            private readonly IQueryResult<TValue, TError> source;
            private readonly Try<TValue, TResult> @try;

            public TrySelectResult(IQueryResult<TValue, TError> source, Try<TValue, TResult> @try)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(@try);

                this.source = source;
                this.@try = @try;
            }

            public IQueryResultNode<TResult, TError> Nodes
            {
                get
                {
                    return this.source.Nodes.TrySelect(this.@try);
                }
            }
        }
    }
}
