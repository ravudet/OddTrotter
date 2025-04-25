/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Try;

    public static partial class QueryResultExtensions
    {
        public static async Task<IQueryResult<TValueResult, TError>> Select<TValueSource, TError, TValueResult>(
            this Task<IQueryResult<TValueSource, TError>> source,
            Func<TValueSource, TValueResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return (await source.ConfigureAwait(false)).Select(selector);
        }

        public static async Task<IQueryResult<TResult, TError>> TrySelect<TValue, TError, TResult>(
            this Task<IQueryResult<TValue, TError>> source,
            Try<TValue, TResult> @try)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(@try);

            return (await source.ConfigureAwait(false)).TrySelect(@try);
        }
    }
}
