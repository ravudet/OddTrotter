namespace Fx.QueryContext.Monad
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
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
        }

        public static void Play2<TResponse, TValue, TError, TQueryContext>(IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad)
            ////where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            var whered = monad.Where3(_ => true);
        }

        public static IQueryContextMonad<TQueryContext, TResponse, TValue, TError> Where3<TQueryContext, TResponse, TValue, TError>(this IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad, Expression<Func<TValue, bool>> predicate)
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            //// TODO if monad is where mixin...

            return monad.Unit()(monad.Source.Where(predicate));
        }*/
    }

    public sealed class MockResponse
    {
    }

    public sealed class MockValue
    {
    }

    public sealed class MockError
    {
    }

    public sealed class KnowsHowToWhereContext :
        IQueryContext<MockResponse, MockValue, MockError>,
        IWhereQueryContextMixin<MockResponse, MockValue, MockError, KnowsHowToWhereContext>,
        IConcatQueryContextMixin<MockResponse, MockValue, MockError, KnowsHowToWhereContext>
    {
        public IQueryContext<MockResponse, MockValue, MockError> Concat(KnowsHowToWhereContext second)
        {
            throw new NotImplementedException();
        }

        public ITask<IQueryResult<MockResponse, MockError>> Evaluate()
        {
            throw new NotImplementedException();
        }

        public KnowsHowToWhereContext Where(Expression<Func<MockValue, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class HelpfulWhereConcatExtension<TQueryContext, TResponse, TValue, TError> : 
        IQueryContext<TResponse, TValue, TError>, 
        IConcatQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        where TQueryContext : IQueryContext<TResponse, TValue, TError>, IConcatQueryContextMixin<TResponse, TValue, TError, TQueryContext>,
        IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
    {
        private readonly TQueryContext source;

        public HelpfulWhereConcatExtension(TQueryContext source)
        {
            this.source = source;
        }

        public IQueryContext<TResponse, TValue, TError> Concat(TQueryContext second)
        {
            return new Concated(this.source, second);
        }

        private sealed class Concated : IWhereQueryContextMixin<TResponse, TValue, TError, Concated>
        {
            private readonly TQueryContext source;
            private readonly TQueryContext second;

            public Concated(TQueryContext source, TQueryContext second)
            {
                this.source = source;
                this.second = second;
            }

            public ITask<IQueryResult<TResponse, TError>> Evaluate()
            {
                return this.source.Concat(this.second).Evaluate();
            }

            public Concated Where(Expression<Func<TValue, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            private sealed class Whered : IQueryContext<TResponse, TValue, TError>
            {
                private readonly TQueryContext source;
                private readonly TQueryContext second;
                private readonly Expression<Func<TValue, bool>> predicate;

                public Whered(TQueryContext source, TQueryContext second, Expression<Func<TValue, bool>> predicate)
                {
                    this.source = source;
                    this.second = second;
                    this.predicate = predicate;
                }

                public ITask<IQueryResult<TResponse, TError>> Evaluate()
                {
                    return this.source.Where(this.predicate).Concat(this.second.Where(this.predicate)).Evaluate();
                }
            }
        }

        public ITask<IQueryResult<TResponse, TError>> Evaluate()
        {
            return this.source.Evaluate();
        }
    }

    public static class QueryContextPlayground
    {
        public static void DoWork()
        {
            var knowsHowToWhereContext = new KnowsHowToWhereContext();
            var knowsHowToWhereContext2 = new KnowsHowToWhereContext();

            var concated = knowsHowToWhereContext.Where(_ => true).Concat(knowsHowToWhereContext2.Where(_ => true));

            concated = knowsHowToWhereContext.Concat(knowsHowToWhereContext2);


            var helpfullyExtended = new HelpfulWhereConcatExtension<KnowsHowToWhereContext, MockResponse, MockValue, MockError>(knowsHowToWhereContext);
            concated = helpfullyExtended.Concat(knowsHowToWhereContext2);
            concated.)
        }
    }
}
