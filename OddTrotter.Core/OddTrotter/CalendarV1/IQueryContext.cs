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




        /*public static Task<IQueryResult<TResult, TError>> SelectAsync<TSource, TError, TResult>(
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
        }*/
    }


}