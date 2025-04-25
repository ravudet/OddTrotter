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
    using System.Net.Security;
    using static OddTrotter.Calendar.OdataCollectionResponse;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public static class QueryResultAsyncExtensions
    {
        



        //// TODO in `select`, for convenience, you have a `select` overload that *does* use a task queryresult, but *doesn't* use a task selector; that's not really relevenat for first; do you still want the "convenience method"?
        //// TODO actually, you seem to have two dimensions: is `this` a task + is the `func` a task? and you seem to want (for convenience) all 4 variations




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