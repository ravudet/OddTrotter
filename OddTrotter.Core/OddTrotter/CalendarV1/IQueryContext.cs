////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace OddTrotter.Calendar
{
    using OddTrotter.TodoList;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using static OddTrotter.Calendar.QueryResultExtensions;

    using Fx.Either;
    using Fx.QueryContext;
    using System.Net.Http.Headers;
    using Fx.Try;
    using System.Linq;


    public static class QueryResultAsyncExtensions
    {
        


        public static async Task<IQueryResult<TResult, TError>> TrySelectAsync<TValue, TError, TResult>(this Task<IQueryResult<TValue, TError>> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return (await queryResult).TrySelect(@try);
        }


        //// TODO in `select`, for convenience, you have a `select` overload that *does* use a task queryresult, but *doesn't* use a task selector; that's not really relevenat for first; do you still want the "convenience method"?
        //// TODO actually, you seem to have two dimensions: is `this` a task + is the `func` a task? and you seem to want (for convenience) all 4 variations



        public static async Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this Task<IQueryResult<TSource, TError>> queryResult,
            Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(queryResult);
            ArgumentNullException.ThrowIfNull(selector);

            return (await queryResult.ConfigureAwait(false)).Select(selector);
        }

        public static Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
            this IQueryResult<TSource, TError> queryResult,
            Func<TSource, Task<TResult>> selector)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return
                Task.FromResult(
                        queryResult
                            .Select(
                                element => selector(element).ConfigureAwait(false).GetAwaiter().GetResult())); //// TODO make this actually async
        }
    }


    public static class QueryResultExtensions
    {

        //// TODO do you really want a tryselect overload that pretends ieithers are trys
        







        public static IQueryResult<TResult, TError> TrySelect<TValue, TError, TResult>(this IQueryResult<TValue, TError> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return new TrySelectQueryResult<TResult, TError>(queryResult.Nodes.TrySelectIterator(@try));
        }

        private sealed class TrySelectQueryResult<TResult, TError> : IQueryResult<TResult, TError>
        {
            public TrySelectQueryResult(IQueryResultNode<TResult, TError> nodes)
            {
                Nodes = nodes;
            }


            public IQueryResultNode<TResult, TError> Nodes { get; }
        }

        private static IQueryResultNode<TResult, TError> TrySelectIterator<TValue, TError, TResult>(this IQueryResultNode<TValue, TError> queryResult, Fx.Try.Try<TValue, TResult> @try)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException(nameof(queryResult));
            }

            if (@try == null)
            {
                throw new ArgumentNullException(nameof(@try));
            }

            return queryResult.SelectLeft(
                element =>
                    TryCreate(
                        element,
                        Lift<TValue, TResult, IElement<TValue, TError>>(@try, element => element.Value), ///// TODO not sure that this is a life, and not sure that the lack of the type inference makes this useful in any way
                        (element, result) => new TrySelectElement<TValue, TError, TResult>(result, element.Next(), @try),
                        element => element.Next().TrySelectIterator(@try))
                    .SelectManyRight())
                .SelectManyLeft()
                .ToQueryResultNode();
        }

        private static Try<TOther, TResult> Lift<TSource, TResult, TOther>(Try<TSource, TResult> @try, Func<TOther, TSource> selector)
        {
            return (TOther other, [MaybeNullWhen(false)] out TResult result) => @try(selector(other), out result);
        }

        private static IEither<TLeft, TRight> TryCreate<TValue, TResult, TLeft, TRight>( //// TODO this should go in the `either` factory methods class, if you choose to keep it
            TValue value,
            Try<TValue, TResult> discriminator,
            Func<TValue, TResult, TLeft> leftFactory,
            Func<TValue, TRight> rightFactory)
        {
            ArgumentNullException.ThrowIfNull(discriminator);
            ArgumentNullException.ThrowIfNull(leftFactory);
            ArgumentNullException.ThrowIfNull(rightFactory);

            if (discriminator(value, out var result))
            {
                return Either.Left(leftFactory(value, result)).Right<TRight>();
            }
            else
            {
                return Either.Left<TLeft>().Right(rightFactory(value));
            }
        }

        private sealed class TrySelectElement<TValue, TError, TResult> : IElement<TResult, TError>
        {
            private readonly IQueryResultNode<TValue, TError> next;
            private readonly Try<TValue, TResult> @try;

            public TrySelectElement(TResult value, IQueryResultNode<TValue, TError> next, Fx.Try.Try<TValue, TResult> @try)
            {
                Value = value;
                this.next = next;
                this.@try = @try;
            }

            public TResult Value { get; }

            public IQueryResultNode<TResult, TError> Next()
            {
                return this.next.TrySelectIterator(this.@try);
            }
        }


        public static Fx.QueryContext.IQueryResult<TValue, TErrorEnd> ErrorSelect<TValue, TErrorStart, TErrorEnd>(this IQueryResult<TValue, TErrorStart> queryResult, Func<TErrorStart, TErrorEnd> selector)
        {
            //// TODO do you like this method name? do you want to normalize with names used in `either`?
            return new ErrorSelectQueryResult<TValue, TErrorEnd>(queryResult.Nodes.ErrorSelectIterator(selector));
        }

        private static Fx.QueryContext.IQueryResultNode<TValue, TErrorEnd> ErrorSelectIterator<TValue, TErrorStart, TErrorEnd>(this IQueryResultNode<TValue, TErrorStart> queryResult, Func<TErrorStart, TErrorEnd> selector)
        {
            return queryResult
                .Select(
                    element => new ErrorSelectElement<TValue, TErrorStart, TErrorEnd>(element, selector),
                    terminal => terminal.SelectLeft(error => new ErrorSelectError<TErrorEnd>(selector(error.Value))))
                .ToQueryResultNode();
        }

        private sealed class ErrorSelectElement<TValue, TErrorStart, TErrorEnd> : IElement<TValue, TErrorEnd>
        {
            private readonly IElement<TValue, TErrorStart> element;
            private readonly Func<TErrorStart, TErrorEnd> selector;

            public ErrorSelectElement(IElement<TValue, TErrorStart> element, Func<TErrorStart, TErrorEnd> selector)
            {
                this.element = element;
                this.selector = selector;
            }

            public TValue Value
            {
                get
                {
                    return this.element.Value;
                }
            }

            public IQueryResultNode<TValue, TErrorEnd> Next()
            {
                return this.element.Next().ErrorSelectIterator(this.selector);
            }
        }

        private sealed class ErrorSelectError<TErrorEnd> : IError<TErrorEnd>
        {
            public ErrorSelectError(TErrorEnd value)
            {
                Value = value;
            }

            public TErrorEnd Value { get; }
        }

        private sealed class ErrorSelectQueryResult<TValue, TErrorEnd> : Fx.QueryContext.IQueryResult<TValue, TErrorEnd>
        {
            public ErrorSelectQueryResult(IQueryResultNode<TValue, TErrorEnd> nodes)
            {
                Nodes = nodes;
            }

            public IQueryResultNode<TValue, TErrorEnd> Nodes { get; }
        }




        //// TODO you previously had an `oftype` extension that went unused, likely because it had the type inference problem; maybe your `type<T>` solution can work here?


    }
}