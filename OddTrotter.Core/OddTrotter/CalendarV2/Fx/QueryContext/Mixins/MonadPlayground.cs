namespace Fx.QueryContext.Mixins
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public static class MonadPlayground
    {
        public sealed class MockResponse
        {

        }

        public sealed class MockError
        {
        }

        public sealed class MockValue
        {
        }

        public sealed class WherableAndOrderbyableContext : IQueryContext<MockResponse, MockValue, MockError>, IWhereQueryContextMixin<MockResponse, MockValue, MockError, WherableAndOrderbyableContext>, IOrderByQueryContextMixin<MockResponse, MockValue, MockError, OrderedResultContext>
        {
            public ITask<IQueryResult<MockResponse, MockError>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public WherableAndOrderbyableContext Where(Expression<Func<MockValue, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            public OrderedResultContext OrderBy<TKey>(Expression<Func<MockResponse, TKey>> keySelector)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class OrderedResultContext : IQueryContext<MockResponse, MockValue, MockError>
        {
            public ITask<IQueryResult<MockResponse, MockError>> Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public static void DoWork()
        {
            var context = new WherableAndOrderbyableContext();

            context.OrderBy(_ => _);
            context.Where(_ => true).OrderBy(_ => _);

            //// TODO now write an "heplful extension" that allows a where after the orderby is called
        }
    }

    public interface IOrderByQueryContextMixin<TResponse, TValue, TError, TQueryContext> : IQueryContext<TResponse, TValue, TError> where TQueryContext : IQueryContext<TResponse, TValue, TError> //// TODO if you actually ship this, tquerycontext needs to be something that, in the framework, indicates that it is ordered so that "thenby" can be called on it
    {
        TQueryContext OrderBy<TKey>(Expression<Func<TResponse, TKey>> keySelector);
    }
}
