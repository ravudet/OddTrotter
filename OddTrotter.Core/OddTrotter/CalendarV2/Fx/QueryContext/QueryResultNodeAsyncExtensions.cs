/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Try;

    public static partial class QueryResultNodeAsyncExtensions
    {
        public static async ITask<TResult> Apply<TValue, TError, TResult>(
            this IQueryResultNodeAsync<TValue, TError> queryResultNode,
            Func<IElementAsync<TValue, TError>, ITask<TResult>> elementMap,
            Func<IEither<IError<TError>, IEmpty>, TResult> terminalMap)
        {
            ArgumentNullException.ThrowIfNull(queryResultNode);
            ArgumentNullException.ThrowIfNull(elementMap);
            ArgumentNullException.ThrowIfNull(terminalMap);

            return await
                queryResultNode
                    .Apply(
                        async (element, nothing) => await elementMap(element).ConfigureAwait(false),
                        async (terminal, nothing) => await Task.FromResult(terminalMap(terminal)).ConfigureAwait(false),
                        new Nothing())
                    .ConfigureAwait(false);
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
        public static async ITask<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNodeAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return await source
                .SelectLeft(
                    async element =>
                        new SelectElementAsync<TValueSource, TError, TValueResult>(
                            selector(element.Value),
                            await element.Next().ConfigureAwait(false),
                            selector))
                .ToQueryResultNodeAsync()
                .ConfigureAwait(false);
        }

        private sealed class SelectElementAsync<TValueSource, TError, TValueResult> : IElementAsync<TValueResult, TError>
        {
            private readonly IQueryResultNodeAsync<TValueSource, TError> next;
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
            public SelectElementAsync(
                TValueResult value,
                IQueryResultNodeAsync<TValueSource, TError> next,
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
            public async ITask<IQueryResultNodeAsync<TValueResult, TError>> Next()
            {
                return await this.next.Select(this.selector).ConfigureAwait(false);
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
        public static async ITask<IQueryResultNodeAsync<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this IQueryResultNodeAsync<TValue, TError> source,
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return await source
                .SelectLeft(
                    async element => await element
                        .Value
                        .ToEither(@try)
                        .Select(
                            async tried =>
                                new TrySelectElementAsync<TValue, TError, TResult>(
                                    tried,
                                    await element.Next().ConfigureAwait(false),
                                    @try),
                            async nothing => await element.Next().TrySelect(@try).ConfigureAwait(false))
                        .SelectManyRight()
                        .ConfigureAwait(false))
                .SelectManyLeft()
                .ToQueryResultNodeAsync()
                .ConfigureAwait(false);
        }

        private sealed class TrySelectElementAsync<TValue, TError, TResult> : IElementAsync<TResult, TError>
        {
            private readonly IQueryResultNodeAsync<TValue, TError> next;
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
            public TrySelectElementAsync(TResult value, IQueryResultNodeAsync<TValue, TError> next, Try<TValue, TResult> @try)
            {
                ArgumentNullException.ThrowIfNull(next);
                ArgumentNullException.ThrowIfNull(@try);

                this.Value = value;
                this.next = next;
                this.@try = @try;
            }

            /// <inheritdoc/>
            public TResult Value { get; }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TResult, TError>> Next()
            {
                return await this.next.TrySelect(this.@try).ConfigureAwait(false);
            }
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
        public static async ITask<IQueryResultNodeAsync<TValue, TError>> Where<TValue, TError>(
            this IQueryResultNodeAsync<TValue, TError> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return await source
                .SelectLeft(
                    async element => await element
                        .Value
                        .ToEither(predicate)
                        .Select(
                            async value => 
                                new WhereElementAsync<TValue, TError>(
                                    value, 
                                    await element.Next().ConfigureAwait(false), 
                                    predicate),
                            async nothing => await element.Next().Where(predicate).ConfigureAwait(false))
                        .SelectManyRight()
                        .ConfigureAwait(false))
                .SelectManyLeft()
                .ToQueryResultNodeAsync()
                .ConfigureAwait(false);
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
                    element => Either
                        .Left(
                            new ConcatFirstElementAsync<TValue, TErrorFirst, TErrorSecond, TErrorResult>(
                                element.Value,
                                element.Next(),
                                second,
                                firstErrorSelector,
                                secondErrorSelector,
                                errorAggregator))
                        .Right<IEither<IError<TErrorResult>, IEmpty>>()
                        .ToQueryResultNodeAsync(),
                    async terminal => await 
                        ConcatTraverseSecond(
                            terminal
                                .Apply(
                                    error => new Optional<TErrorFirst>(error.Value),
                                    empty => new Optional<TErrorFirst>()),
                            second,
                            firstErrorSelector,
                            secondErrorSelector,
                            errorAggregator)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
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
                        Either
                            .Left<IElementAsync<TValue, TErrorResult>>()
                            .Right(
                                terminal
                                    .Apply(
                                        secondError =>
                                            Either
                                                .Left(
                                                    new Error<TErrorResult>(
                                                        error.TryGetValue(out var firstError)
                                                            ? errorAggregator(firstError, secondError.Value)
                                                            : secondErrorSelector(secondError.Value)))
                                                .Right<IEmpty>(),
                                        empty =>
                                            error
                                                .TryGetValue(out var firstError)
                                                    ? Either
                                                        .Left(
                                                            new Error<TErrorResult>(
                                                                firstErrorSelector(firstError)))
                                                        .Right<IEmpty>()
                                                    : Either
                                                        .Left<Error<TErrorResult>>()
                                                        .Right(empty)))
                            .ToQueryResultNodeAsync())
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
    }
}
