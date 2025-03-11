using Fx.QueryContext;

namespace Stash.Monad
{
    public delegate TQueryContextMonad QueryContextUnit<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>(
        TQueryContext queryContext)
        where TQueryContextMonad : IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>
        where TQueryContext : IQueryContext<TResponse, TValue, TError>;
}
