namespace Fx.QueryContext
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <typeparam name="TQueryContext"></typeparam>
    public interface IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext> : IQueryContext<TResponse, TValue, TError> where TQueryContext : IQueryContext<TResponse, TValue, TError>
    {
        TQueryContext Where(Expression<Func<TValue, bool>> predicate);
    }
}
