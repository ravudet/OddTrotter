/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Try;

    public static partial class QueryResultNodeAsyncExtensions
    {
        public static IEither<TResult, Nothing> ToEither<TValue, TResult>(this TValue value, Try<TValue, TResult> @try)
        {
            //// TODO wrong class
            ArgumentNullException.ThrowIfNull(@try);

            if (@try(value, out var output))
            {
                return Either.Right<Nothing>().Left(output);
            }
            else
            {
                return Either.Left<TResult>().Right(new Nothing());
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
        public static async ITask<IQueryResultNodeAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this IQueryResultNodeAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return await source
                .SelectLeft(
                    async element =>
                        (IElementAsync<TValueResult, TError>)new SelectElementAsync<TValueSource, TError, TValueResult>(
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
                        .SelectAsync(
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
    }
}
