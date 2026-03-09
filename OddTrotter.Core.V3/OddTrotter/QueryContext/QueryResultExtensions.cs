/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Try;

    using OddTrotter.Graph.CalendarEventsContext;

    public static partial class QueryResultExtensions
    {

        public static async ITask<IQueryResultAsync<TValue, TError>> Where<TValue, TError>(
            this ITask<IQueryResultAsync<TValue, TError>> source,
            Func<TValue, bool> predicate)
        {
            return (await source.ConfigureAwait(false)).Where(predicate);
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
        public static IQueryResultAsync<TValue, TError> Where<TValue, TError>(
            this IQueryResultAsync<TValue, TError> source,
            Func<TValue, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return new WhereQueryResult<TValue, TError>(source, predicate);
        }

        private sealed class WhereQueryResult<TValue, TError> : IQueryResultAsync<TValue, TError>
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
            public WhereQueryResult(IQueryResultAsync<TValue, TError> source, Func<TValue, bool> predicate)
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

        public static async ITask<IQueryResultAsync<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this ITask<IQueryResultAsync<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return (await source.ConfigureAwait(false)).Select(selector);
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
        public static IQueryResultAsync<TValueResult, TError> Select<TValueSource, TError, TValueResult>(
            this IQueryResultAsync<TValueSource, TError> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return new SelectQueryResult<TValueSource, TError, TValueResult>(source, selector);
        }

        private sealed class SelectQueryResult<TValueSource, TError, TValueResult> : IQueryResultAsync<TValueResult, TError>
        {
            private readonly IQueryResultAsync<TValueSource, TError> source;
            private readonly Func<TValueSource, TValueResult> selector;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectQueryResult(IQueryResultAsync<TValueSource, TError> source, Func<TValueSource, TValueResult> selector)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(selector);

                this.source = source;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValueResult, TError>> GetNodes()
            {
                return await (await this.source.GetNodes()).Select(selector).ConfigureAwait(false);
            }
        }

        public static async ITask<IEither<IEither<TElement, TDefault>, TError>> FirstOrDefault<TElement, TError, TDefault>(
            this ITask<IQueryResultAsync<TElement, TError>> source,
            TDefault @default) //// TODO at one point, you had a type for "first or default", but you got rid of it because you had some extensions that want to use tuple<..., ieither> and because `tuple` is a concrete type, you couldn't get type inference from `firstordefault` to `ieither` and so you needed to "select" the `firstordefault` with `aseither`, which looked a bit awkward; of course, there will still be cases when you need to do that for other types, but "first or default" is such a "small" concept, that i think it is fine to not define a type specifically for it //// TODO you should document this somewhere
        {
            return await (await source.ConfigureAwait(false)).FirstOrDefault(@default).ConfigureAwait(false);
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
        public static async ITask<IEither<IEither<TElement, TDefault>, TError>> FirstOrDefault<TElement, TError, TDefault>(
            this IQueryResultAsync<TElement, TError> source, 
            TDefault @default)
        {
            ArgumentNullException.ThrowIfNull(source);

            return (await source.GetNodes().ConfigureAwait(false))
                .Apply(
                    element =>
                        Either
                            .Right<TError>()
                            .Left(
                                Either
                                    .Right<TDefault>()
                                    .Left(element.Value)),
                    terminal =>
                        terminal
                            .Apply(
                                error =>
                                    Either
                                        .Left<Either<TElement, TDefault>>()
                                        .Right(error.Value),
                                empty =>
                                    Either
                                        .Right<TError>()
                                        .Left(
                                            Either.Left<TElement>().Right(@default))));
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
        public static IQueryResultAsync<TResult, TError> TrySelect<TValue, TError, TResult>(
            this IQueryResultAsync<TValue, TError> source, 
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return new TrySelectResult<TValue, TError, TResult>(source, @try);
        }

        private sealed class TrySelectResult<TValue, TError, TResult> : IQueryResultAsync<TResult, TError>
        {
            private readonly IQueryResultAsync<TValue, TError> source;
            private readonly Try<TValue, TResult> @try;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="try"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="try"/> is <see langword="null"/>
            /// </exception>
            public TrySelectResult(IQueryResultAsync<TValue, TError> source, Try<TValue, TResult> @try)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(@try);

                this.source = source;
                this.@try = @try;
            }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TResult, TError>> GetNodes()
            {
                return await (await this.source.GetNodes().ConfigureAwait(false)).TrySelect(this.@try).ConfigureAwait(false);
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
        public static IQueryResultAsync<TValue, TErrorResult> SelectError<TValue, TErrorSource, TErrorResult>(
            this IQueryResultAsync<TValue, TErrorSource> source,
            Func<TErrorSource, TErrorResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return new SelectErrorResult<TValue, TErrorSource, TErrorResult>(source, selector);
        }

        private sealed class SelectErrorResult<TValue, TErrorSource, TErrorResult> : IQueryResultAsync<TValue, TErrorResult>
        {
            private readonly IQueryResultAsync<TValue, TErrorSource> source;
            private readonly Func<TErrorSource, TErrorResult> selector;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="source"></param>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>
            /// </exception>
            public SelectErrorResult(
                IQueryResultAsync<TValue, TErrorSource> source,
                Func<TErrorSource, TErrorResult> selector)
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(selector);
                    
                this.source = source;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValue, TErrorResult>> GetNodes()
            {
                return (await this.source.GetNodes()).SelectError(this.selector);
            }
        }

        public static IQueryResultNodeAsync<TValue, TErrorResult> SelectError<TValue, TErrorSource, TErrorResult>(
            this IQueryResultNodeAsync<TValue, TErrorSource> source,
            Func<TErrorSource, TErrorResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return source
                .Select(
                    element =>
                        new SelectErrorElement<TValue, TErrorSource, TErrorResult>(
                            element.Value,
                            element,
                            selector),
                    terminal =>
                        terminal
                            .SelectLeft(
                                error => new SelectErrorError<TErrorResult>(selector(error.Value))))
                .ToQueryResultNodeAsync();
        }

        private sealed class SelectErrorElement<TValue, TErrrorSource, TErrorResult> : IElementAsync<TValue, TErrorResult>
        {
            private readonly IElementAsync<TValue, TErrrorSource> element;
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
                IElementAsync<TValue, TErrrorSource> element,
                Func<TErrrorSource, TErrorResult> selector)
            {
                ArgumentNullException.ThrowIfNull(selector);

                this.Value = value;
                this.element = element;
                this.selector = selector;
            }

            /// <inheritdoc/>
            public TValue Value { get; }

            /// <inheritdoc/>
            public async ITask<IQueryResultNodeAsync<TValue, TErrorResult>> Next()
            {
                return (await this.element.Next()).SelectError(this.selector);
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
