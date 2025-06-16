/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Fx.Either;
    using Fx.Try;
    using Stash.Monad;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public static partial class QueryResultAsyncExtensions
    {
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
        public static async Task<IQueryResult<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this Task<IQueryResult<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return (await source.ConfigureAwait(false)).Select(selector);
        }

        public static IQueryResultAsync<TValueResult, TError> Select<TValueSource, TError, TValueResult>(
            this IQueryResultAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return new SelectQueryResultAsync<TValueSource, TError, TValueResult>(source, selector);
        }

        private sealed class SelectQueryResultAsync<TValueSource, TError, TValueResult> : IQueryResultAsync<TValueResult, TError>
        {
            private readonly IQueryResultAsync<TValueSource, TError> source;
            private readonly Func<TValueSource, TValueResult> selector;

            public SelectQueryResultAsync(IQueryResultAsync<TValueSource, TError> source, Func<TValueSource, TValueResult> selector)
            {
                this.source = source;
                this.selector = selector;
            }

            public async ITask<IQueryResultNodeAsync<TValueResult, TError>> GetNodes()
            {
                return
                    await
                        this
                            .source
                            .GetNodes()
                            .Select(this.selector)
                    .ConfigureAwait(false);
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
        public static async Task<IQueryResult<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this Task<IQueryResult<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return (await source.ConfigureAwait(false)).TrySelect(@try);
        }

        public static IQueryResultAsync<TResult, TError> TrySelect<TValue, TError, TResult>(
            this IQueryResultAsync<TValue, TError> source,
            Try<TValue, TResult> @try)
        {
            return new TrySelectQueryResultAsync<TValue, TError, TResult>(source, @try);
        }

        private sealed class TrySelectQueryResultAsync<TValue, TError, TResult> : IQueryResultAsync<TResult, TError>
        {
            private readonly IQueryResultAsync<TValue, TError> source;
            private readonly Try<TValue, TResult> @try;

            public TrySelectQueryResultAsync(
                IQueryResultAsync<TValue, TError> source,
                Try<TValue, TResult> @try)
            {
                this.source = source;
                this.@try = @try;
            }

            public async ITask<IQueryResultNodeAsync<TResult, TError>> GetNodes()
            {
                return
                    await
                        this
                            .source
                            .GetNodes()
                            .TrySelect(this.@try)
                    .ConfigureAwait(false);
            }
        }

        public static async ITask<IQueryResultNodeAsync<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this ITask<IQueryResultNodeAsync<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            return await (await source.ConfigureAwait(false)).TrySelect(@try).ConfigureAwait(false);
        }

        public static async ITask<IQueryResultNodeAsync<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this IQueryResultNodeAsync<TValue, TError> source,
            Try<TValue, TResult> @try)
        {
            return (await source
                .SelectLeft(
                    async element => await element
                        .Value
                        .ToEither(@try)
                        .Select(
                            async tried => new TrySelectElementAsync<TValue, TError, TResult>(tried, await element.Next().ConfigureAwait(false), @try),
                            async nothing => await (await element.Next().ConfigureAwait(false)).TrySelect(@try).ConfigureAwait(false))
                        .SelectManyRight()
                        .ConfigureAwait(false))
                .SelectManyLeft()
                .ConfigureAwait(false))
                .ToQueryResultNodeAsync();
        }

        private sealed class TrySelectElementAsync<TValue, TError, TResult> : IElementAsync<TResult, TError>
        {
            private readonly IQueryResultNodeAsync<TValue, TError> next;
            private readonly Try<TValue, TResult> @try;

            public TrySelectElementAsync(TResult value, IQueryResultNodeAsync<TValue, TError> next, Try<TValue, TResult> @try)
            {
                Value = value;
                this.next = next;
                this.@try = @try;
            }

            public TResult Value { get; }

            public async ITask<IQueryResultNodeAsync<TResult, TError>> Next()
            {
                return await this.next.TrySelect(this.@try).ConfigureAwait(false);
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
        /// Thrown if <paramref name="first"/> or <paramref name="second"/> <paramref name="firstErrorSelector"/> or
        /// <paramref name="secondErrorSelector"/> or <paramref name="errorAggregator"/> is <see langword="null"/>
        /// </exception>
        public static IQueryResultAsync<TValue, TErrorResult> Concat<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResult<TValue, TErrorFirst> first,
            IQueryResultAsync<TValue, TErrorSecond> second,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return new ConcatQueryResultAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult>(first, second, firstErrorSelector, secondErrorSelector, errorAggregator);
        }

        private sealed class ConcatQueryResultAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult> :
            IQueryResultAsync<TValue, TErrorResult>
        {
            private readonly IQueryResult<TValue, TErrorFirst> first;
            private readonly IQueryResultAsync<TValue, TErrorSecond> second;
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
            public ConcatQueryResultAsync(
                IQueryResult<TValue, TErrorFirst> first,
                IQueryResultAsync<TValue, TErrorSecond> second,
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
            public async ITask<IQueryResultNodeAsync<TValue, TErrorResult>> GetNodes()
            {
                return await this.first.Nodes.Concat(await second.GetNodes().ConfigureAwait(false), this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator).ConfigureAwait(false);
            }
        }

        public static async ITask<TResult> Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, ITask<TResult>> leftMap,
            Func<TRight, ITask<TResult>> rightMap)
        {
            return await either.Apply((left, nothing) => leftMap(left), (right, nothing) => rightMap(right), new Nothing()).ConfigureAwait(false);
        }

        public static async ITask<IQueryResultNodeAsync<TValue, TErrorResult>> Concat<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
            this IQueryResultNode<TValue, TErrorFirst> first,
            IQueryResultNodeAsync<TValue, TErrorSecond> second,
            Func<TErrorFirst, TErrorResult> firstErrorSelector,
            Func<TErrorSecond, TErrorResult> secondErrorSelector,
            Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return await first
                .Apply(
                    async element => await Task
                        .FromResult(
                            Either
                                .Left(
                                    new ConcatFirstElementAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                        element.Value,
                                        element.Next(),
                                        second,
                                        firstErrorSelector,
                                        secondErrorSelector,
                                        errorAggregator))
                                .Right<IEither<IError<TErrorResult>, IEmpty>>()
                                .ToQueryResultNodeAsync())
                        .ConfigureAwait(false),
                    async terminal => await 
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
                                        errorAggregator))
                            .ConfigureAwait(false));
        }

        private sealed class ConcatFirstElementAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult> : IElementAsync<TValue, TErrorResult>
        {
            private readonly IQueryResultNode<TValue, TErrorFirst> next;
            private readonly IQueryResultNodeAsync<TValue, TErrorSecond> second;
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
            public ConcatFirstElementAsync(
                TValue value,
                IQueryResultNode<TValue, TErrorFirst> next,
                IQueryResultNodeAsync<TValue, TErrorSecond> second,
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
            public ITask<IQueryResultNodeAsync<TValue, TErrorResult>> Next()
            {
                return this.next.Concat(this.second, this.firstErrorSelector, this.secondErrorSelector, this.errorAggregator);
            }
        }

        private static async ITask<IQueryResultNodeAsync<TValue, TErrorResult>> ConcatTraverseSecond
            <
                TValue,
                TErrorFirst,
                TErrorSecond,
                TErrorResult
            >(
                Optional<TErrorFirst> error,
                IQueryResultNodeAsync<TValue, TErrorSecond> second,
                Func<TErrorFirst, TErrorResult> firstErrorSelector,
                Func<TErrorSecond, TErrorResult> secondErrorSelector,
                Func<TErrorFirst, TErrorSecond, TErrorResult> errorAggregator)
        {
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(firstErrorSelector);
            ArgumentNullException.ThrowIfNull(secondErrorSelector);
            ArgumentNullException.ThrowIfNull(errorAggregator);

            return await second
                .Apply(
                    async element =>
                        Either
                            .Left(
                                new ConcatSecondErrorElementAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                    error,
                                    element.Value,
                                    await element.Next().ConfigureAwait(false),
                                    firstErrorSelector,
                                    secondErrorSelector,
                                    errorAggregator))
                            .Right<IEither<IError<TErrorResult>, IEmpty>>()
                            .ToQueryResultNodeAsync(),
                    terminal =>
                        Task.FromResult(
                        terminal
                            .Apply(
                                secondError =>
                                    Either
                                        .Left<IElementAsync<TValue, TErrorResult>>()
                                        .Right(
                                            Either
                                                .Left(
                                                    new Error<TErrorResult>(
                                                        error.TryGetValue(out var firstError)
                                                            ? errorAggregator(firstError, secondError.Value)
                                                            : secondErrorSelector(secondError.Value)))
                                                .Right<IEmpty>())
                                        .ToQueryResultNodeAsync(),
                                empty =>
                                    error.TryGetValue(out var firstError)
                                        ? Either
                                            .Left<IElementAsync<TValue, TErrorResult>>()
                                            .Right(
                                                Either
                                                    .Left(
                                                        new Error<TErrorResult>(
                                                            firstErrorSelector(firstError)))
                                                    .Right<IEmpty>())
                                            .ToQueryResultNodeAsync()
                                        : Either
                                            .Left<IElementAsync<TValue, TErrorResult>>()
                                            .Right(
                                                Either
                                                    .Left<IError<TErrorResult>>()
                                                    .Right(empty))
                                            .ToQueryResultNodeAsync())))
                .ConfigureAwait(false);
        }

        private sealed class ConcatSecondErrorElementAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult> :
            IElementAsync<TValue, TErrorResult>
        {
            private readonly Optional<TErrorFirst> error;
            private readonly IQueryResultNodeAsync<TValue, TErrorSecond> next;
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
            public ConcatSecondErrorElementAsync(
                Optional<TErrorFirst> error,
                TValue value,
                IQueryResultNodeAsync<TValue, TErrorSecond> next,
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
            public ITask<IQueryResultNodeAsync<TValue, TErrorResult>> Next()
            {
                return ConcatTraverseSecond(
                    this.error,
                    this.next,
                    this.firstErrorSelector,
                    this.secondErrorSelector,
                    this.errorAggregator);
            }
        }






        public static IQueryResultAsync<TValue, TError> Where<TValue, TError>(
            this IQueryResultAsync<TValue, TError> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return new WhereQueryResultAsync<TValue, TError>(source, predicate);
        }

        private sealed class WhereQueryResultAsync<TValue, TError> : IQueryResultAsync<TValue, TError>
        {
            private readonly IQueryResultAsync<TValue, TError> source;
            private readonly Func<TValue, bool> predicate;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="predicate"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
            /// </exception>
            public WhereQueryResultAsync(IQueryResultAsync<TValue, TError> source, Func<TValue, bool> predicate)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(predicate);

                this.source = source;
                this.predicate = predicate;
            }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValue, TError>> GetNodes()
            {
                return await (await this.source.GetNodes().ConfigureAwait(false)).Where(predicate).ConfigureAwait(false);
            }
        }

        public static async ITask<IQueryResultNodeAsync<TValue, TError>> Where<TValue, TError>(
            this IQueryResultNodeAsync<TValue, TError> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return (await source
                .SelectLeft(
                    async element => await element
                        .Value
                        .ToEither(predicate)
                        .Select(
                            async value => new WhereElementAsync<TValue, TError>(value, await element.Next().ConfigureAwait(false), predicate),
                            async nothing => await (await element.Next().ConfigureAwait(false)).Where(predicate).ConfigureAwait(false))
                        .SelectManyRight()
                        .ConfigureAwait(false))
                .SelectManyLeft()
                .ConfigureAwait(false))
                .ToQueryResultNodeAsync();
        }

        private sealed class WhereElementAsync<TValue, TError> : IElementAsync<TValue, TError>
        {
            private readonly IQueryResultNodeAsync<TValue, TError> next;
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
            public WhereElementAsync(TValue value, IQueryResultNodeAsync<TValue, TError> next, Func<TValue, bool> predicate)
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
            public async ITask<IQueryResultNodeAsync<TValue, TError>> Next()
            {
                return await this.next.Where(predicate).ConfigureAwait(false);
            }
        }
    }
}
