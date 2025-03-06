namespace Fx.QueryContext
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// TODO does this need to implement iquerycontext? i'm not sure if that provides any value to the implementer
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <typeparam name="TQueryContext"></typeparam>
    public interface IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext> : IQueryContext<TResponse, TValue, TError> where TQueryContext : IQueryContext<TResponse, TValue, TError> //// TODO you definitely need better names to differentiate tresponse and tvalue
    {
        TQueryContext Where(Expression<Func<TValue, bool>> predicate);
    }
}
