namespace Fx.QueryContext.Monad
{
    public delegate IQueryContextMonad<TQueryContext, TResponse, TValue, TError> QueryContextUnit<TQueryContext, TResponse, TValue, TError>(
        TQueryContext queryContext)
        where TQueryContext : IQueryContext<TResponse, TValue, TError>;
}
