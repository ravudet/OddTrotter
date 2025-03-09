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
    public interface IConcatQueryContextMixin<TResponse, TValue, TError, TQueryContext> : IQueryContext<TResponse, TValue, TError> //// TODO you definitely need better names to differentiate tresponse and tvalue
        where TQueryContext : IQueryContext<TResponse, TValue, TError>
    {
        IQueryContext<TResponse, TValue, TError> Concat(TQueryContext second); //// TODO i'm unclear about this method signature
    }
}
