/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext.Mixins
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDataStoreValue"></typeparam>
    /// <typeparam name="TClientValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <typeparam name="TQueryContext"></typeparam>
    public interface IWhereQueryContextMixinAsync<TClientValue, TDataStoreValue, TError, TQueryContext> : IQueryContextAsync<TClientValue, TDataStoreValue, TError> where TQueryContext : IQueryContextAsync<TClientValue, TDataStoreValue, TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if <paramref name="predicate"/> represents a filtering that is not supported by the backing data source
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown if support for <paramref name="predicate"/> is not yet implemented; this *could* mean that
        /// <paramref name="predicate"/> is not supported by the underyling data store and a validity check is not yet
        /// implemented, or it could mean that <paramref name="predicate"/> *is* supported by the underlying data store, but
        /// converting it to the proper query hasn't been implemented yet
        /// </exception>
        TQueryContext Where(Expression<Func<TDataStoreValue, bool>> predicate);
    }
}
