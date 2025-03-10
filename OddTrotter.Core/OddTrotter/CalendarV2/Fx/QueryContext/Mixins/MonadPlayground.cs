namespace Fx.QueryContext.Mixins
{
    using Fx.QueryContext.Monad;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public sealed class WherableAndOrderbyableContext : 
            IQueryContext<MockResponse, MockValue, MockError>, 
            IWhereQueryContextMixin<MockResponse, MockValue, MockError, WherableAndOrderbyableContext>,
            ////IOrderByQueryContextMixin<MockResponse, MockValue, MockError, OrderedResultContext>
            IOrderByQueryContextMixin<MockResponse, MockValue, MockError, WherableAndOrderbyableContext>
        {
            public ITask<IQueryResult<MockResponse, MockError>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public WherableAndOrderbyableContext OrderBy<TKey>(Expression<Func<MockResponse, TKey>> keySelector)
            {
                throw new NotImplementedException();
            }

            public WherableAndOrderbyableContext Where(Expression<Func<MockValue, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            /*public OrderedResultContext OrderBy<TKey>(Expression<Func<MockResponse, TKey>> keySelector)
            {
                throw new NotImplementedException();
            }*/
        }

        /*public sealed class OrderedResultContext : IQueryContext<MockResponse, MockValue, MockError>
        {
            public ITask<IQueryResult<MockResponse, MockError>> Evaluate()
            {
                throw new NotImplementedException();
            }
        }*/

        public sealed class OrderByThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> :
            IOrderByQueryContextMixin<TResponse, TValue, TError, OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>>,
            IQueryContextMonad<OrderByThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>, TQueryContext, TResponse, TValue, TError>
            where TQueryContext : 
                IOrderByQueryContextMixin<TResponse, TValue, TError, TQueryContext>,
                IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            public OrderByThenWhereHelpfulExtension(TQueryContext source)
            {
                this.Source = source;

                this.Self = this;
            }

            public TQueryContext Source { get; }

            public OrderByThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> Self { get; }

            public ITask<IQueryResult<TResponse, TError>> Evaluate()
            {
                return this.Source.Evaluate();
            }

            public OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> OrderBy<TKey>(Expression<Func<TResponse, TKey>> keySelector)
            {
                return OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>.Create(this.Source, keySelector);
            }

            public QueryContextUnit<OrderByThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>, TQueryContext, TResponse, TValue, TError> Unit()
            {
                return _ => new OrderByThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>(_);
            }
        }

        public sealed class OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> :
            IQueryContext<TResponse, TValue, TError>,
            IWhereQueryContextMixin<TResponse, TValue, TError, OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>>
            where TQueryContext : 
                IOrderByQueryContextMixin<TResponse, TValue, TError, TQueryContext>,
                IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            private readonly IWhereQueryContextMixin<TResponse, TValue, TError, OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>> whereable;

            public static OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> Create<TKey>(TQueryContext source, Expression<Func<TResponse, TKey>> keySelector)
            {
                return new OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>(
                    new Helper<TKey>(
                        source, 
                        keySelector, 
                        Enumerable.Empty<Expression<Func<TValue, bool>>>()));
            }

            private OrderedThenWhereHelpfulExtension(
                IWhereQueryContextMixin<TResponse, TValue, TError, OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>> whereable)
            {
                this.whereable = whereable;
            }

            public ITask<IQueryResult<TResponse, TError>> Evaluate()
            {
                return this.whereable.Evaluate();
            }

            public OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> Where(Expression<Func<TValue, bool>> predicate)
            {
                return new OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>(
                    this.whereable.Where(predicate));
            }

            private sealed class Helper<TKey> :
                IQueryContext<TResponse, TValue, TError>,
                IWhereQueryContextMixin<TResponse, TValue, TError, OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>>
            {
                private readonly TQueryContext source;
                private readonly Expression<Func<TResponse, TKey>> keySelector;
                private readonly IEnumerable<Expression<Func<TValue, bool>>> predicates;

                public Helper(
                    TQueryContext source, 
                    Expression<Func<TResponse, TKey>> keySelector, 
                    IEnumerable<Expression<Func<TValue, bool>>> predicates)
                {
                    this.source = source;
                    this.keySelector = keySelector;
                    this.predicates = predicates;
                }

                public ITask<IQueryResult<TResponse, TError>> Evaluate()
                {
                    var result = this.source;
                    foreach (var predicate in this.predicates)
                    {
                        result = result.Where(predicate);
                    }

                    return result.OrderBy(this.keySelector).Evaluate();
                }

                public OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext> Where(Expression<Func<TValue, bool>> predicate)
                {
                    return new OrderedThenWhereHelpfulExtension<TResponse, TValue, TError, TQueryContext>(
                        new Helper<TKey>(this.source, this.keySelector, this.predicates.Append(predicate)));
                }
            }
        }

        public static TQueryContextMonad Where<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>(
            this IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError> extensions,
            Expression<Func<TValue, bool>> predicate)
            where TQueryContextMonad : IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            //// TODO notice this:
            //// return extensions.Unit()(extensions.Source.Where(predicate));
            return extensions.Self.Where2<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>(predicate);
        }

        public static TQueryContextMonad Where2<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>(
            this TQueryContextMonad extensions, 
            Expression<Func<TValue, bool>> predicate)
            where TQueryContextMonad : IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            return extensions.Unit()(extensions.Source.Where(predicate));
        }

        public static TSelectedQueryContxtMonad Select2
            <
                TQueryContextMonad, 
                TSourceQueryContext, 
                TResponse, 
                TSourceValue,
                TError,
                TSelectedQueryContxtMonad,
                TResultQueryContext, 
                TResultValue
            >(
                this TQueryContextMonad extensions, 
                Expression<Func<TSourceValue, TResultValue>> selector)
            where TQueryContextMonad : IQueryContextMonad<TQueryContextMonad, TSourceQueryContext, TResponse, TSourceValue, TError>
            where TSourceQueryContext : IQueryContext<TResponse, TSourceValue, TError>, ISelectQueryContextMixin<TResponse, TSourceValue, TError, TSourceQueryContext>



            where TResultQueryContext : IQueryContext<TResponse, TResultValue, TError>
            where TSelectedQueryContxtMonad : IQueryContextMonad<TSelectedQueryContxtMonad, TResultQueryContext, TResponse, TResultValue, TError>
        {
            extensions.Unit()(extensions.Source.Select<TResultQueryContext, TResultValue>(selector));
        }

        public static void DoWork()
        {
            var context = new WherableAndOrderbyableContext();

            context.OrderBy(_ => _);
            context.Where(_ => true).OrderBy(_ => _);

            var helpfulExtension = new OrderByThenWhereHelpfulExtension
                <
                    MockResponse, 
                    MockValue, 
                    MockError, 
                    WherableAndOrderbyableContext
                >(
                    context);

            // extensions POC
            helpfulExtension.Evaluate();
            helpfulExtension.OrderBy(_ => _).Evaluate();
            helpfulExtension.OrderBy(_ => _).Where(_ => true).Evaluate();
            helpfulExtension.OrderBy(_ => _).Where(_ => true).Where(_ => false).Evaluate();

            // monad POC
            helpfulExtension
                .Where(_ => true);
            helpfulExtension
                .Where(_ => true)
                .OrderBy(_ => _);
            helpfulExtension
                .Where(_ => true)
                .Where(_ => false)
                .OrderBy(_ => _);
            helpfulExtension
                .Where(_ => true)
                .Where(_ => false)
                .OrderBy(_ => _)
                .Where(_ => true);

            //// TODO can you have a second extension so that you can ensure that the units are called recursively?
        }
    }

    public interface IOrderByQueryContextMixin<TResponse, TValue, TError, TQueryContext> : IQueryContext<TResponse, TValue, TError> where TQueryContext : IQueryContext<TResponse, TValue, TError> //// TODO if you actually ship this, tquerycontext needs to be something that, in the framework, indicates that it is ordered so that "thenby" can be called on it
    {
        TQueryContext OrderBy<TKey>(Expression<Func<TResponse, TKey>> keySelector);
    }

    public interface ISelectQueryContextMixin<TResponse, TSourceValue, TError, TSourceQueryContext> :
        IQueryContext<TResponse, TSourceValue, TError>
        where TSourceQueryContext : IQueryContext<TResponse, TSourceValue, TError>
    {
        TResultQueryContext Select<TResultQueryContext, TResultValue>(Expression<Func<TSourceValue, TResultValue>> selector)
            where TResultQueryContext : IQueryContext<TResponse, TResultValue, TError>;
    }
}
