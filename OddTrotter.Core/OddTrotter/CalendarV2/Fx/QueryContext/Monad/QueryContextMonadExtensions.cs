namespace Fx.QueryContext.Monad
{
    using System;
    using System.Linq.Expressions;
    using static OddTrotter.Calendar.OdataCollectionResponse;

    public static class QueryContextMonadExtensions
    {
        public static QueryContextUnit<TQueryContext, TResponse, TValue, TError> Unit<TQueryContext, TResponse, TValue, TError>(this IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad) where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            //// TODO how much value does this extension *really* provide?
            return monad.Unit<TQueryContext, TResponse, TValue, TError>();
        }

        /*public static TQueryContext Where2<TResponse, TValue, TError, TQueryContext>(this TQueryContext whereQueryContextMixin, Expression<Func<TValue, bool>> predicate)
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            return whereQueryContextMixin.Where(predicate);
        }

        public static void Play<TResponse, TValue, TError, TQueryContext>(TQueryContext whereQueryContextMixin)
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            var whered2 = whereQueryContextMixin.Where(_ => true);
            whered2 = whered2.Where(_ => false);

            var whered = whereQueryContextMixin.Where2<TResponse, TValue, TError, TQueryContext>(_ => true);
            whered = whered.Where2<TResponse, TValue, TError, TQueryContext>(_ => false);
        }*/

        public static void Play2<TResponse, TValue, TError, TQueryContext>(IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad)
            ////where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            var whered = monad.Where3(_ => true);
        }

        public static IQueryContextMonad<TQueryContext, TResponse, TValue, TError> Where3<TQueryContext, TResponse, TValue, TError>(this IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad, Expression<Func<TValue, bool>> predicate)
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            return monad.Unit()(monad.Source.Where(predicate));
        }
    }
}
