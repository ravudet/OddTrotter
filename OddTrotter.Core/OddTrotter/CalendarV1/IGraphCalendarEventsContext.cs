namespace OddTrotter.Calendar
{
    using System;
    using System.Collections;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fx.Either;

    public interface IGraphCalendarEventsContext
    {
        //// TODO i think you like having the "context" *and* "evaluator" types because the context is pretty specific to GET requests (is this true? what about a `$select` on a `POST` request?) //// TODO as i typed this i became less convinced, but we should try it anyway

        Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> Evaluate();

        IGraphCalendarEventsContext Filter(Expression<Func<GraphCalendarEvent, bool>> filter);

        IGraphCalendarEventsContext Top(int top);

        IGraphCalendarEventsContext OrderBy<TOrder>(Expression<Func<GraphCalendarEvent, TOrder>> orderBy);
    }

    public sealed class GraphCalendarEventsContext : IGraphCalendarEventsContext
    {
        private readonly IGraphCalendarEventsEvaluator evaluator;
        private readonly UriPath calendarRoot;

        private readonly string? filter;
        private readonly string? orderBy;
        private readonly string? top;

        public GraphCalendarEventsContext(IGraphCalendarEventsEvaluator evaluator, UriPath calendarRoot)
            : this(evaluator, calendarRoot, null, null, null)
        {
        }

        private GraphCalendarEventsContext(
            IGraphCalendarEventsEvaluator evaluator, 
            UriPath calendarRoot, 
            string? filter, 
            string? orderBy,
            string? top)
        {
            this.evaluator = evaluator;
            this.calendarRoot = calendarRoot;
            this.filter = filter;
            this.orderBy = orderBy;
            this.top = top;
        }

        public async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> Evaluate()
        {
            //// TODO do you need a queryresult extension that select errors?
            //// TODO should the context or the evaluator be the one that knows about `/events`?
            var uri = new Uri($"{this.calendarRoot.Path}/events", UriKind.Relative).ToRelativeUri();
            var query = new GraphQuery.GetEvents(uri);
            return await this
                .evaluator
                .Page(
                    query,
                    nextLink => nextLink.StartsWith(this.evaluator.ServiceRoot) ? this.evaluator : throw new Exception("TODO you need a new exception type for this probably?")) //// TODO this contextgenerator stuff was because you didn't have serviceroot on the context interface, so you wanted the caller to pass it in; now that you have it in the interface, instead of a generator, you should probably just take in the "dictionary"; the reason this is coming up is because otherwise the `page` method needs to describe how the generator should return (or throw) in the even that a context cannot be found by the caller; you really don't want the generator to throw because that defeats the purpose of the queryresult stuff)
                .ConfigureAwait(false);
        }

        public IGraphCalendarEventsContext Filter(Expression<Func<GraphCalendarEvent, bool>> filter)
        {
            string? filterExpression = null;
            if (filter == TypeEqualsSingleInstance)
            {
                filterExpression = "type eq 'singleInstance'";
            }
            else if (filter == IsCancelled)
            {
                filterExpression = "isCancelled eq true";
            }
            else if (filter == IsNotCancelled)
            {
                filterExpression = "isCancelled eq false";
            }
            else if (filter.Parameters.Count == 1)
            {
                var parameterName = filter.Parameters[0].Name;
                if (parameterName != null)
                {
                    if (parameterName.StartsWith(nameof(StartTimeGreaterThan)))
                    {
                        if (long.TryParse(parameterName.Substring(nameof(StartTimeGreaterThan).Length), out var startTimeTicks))
                        {
                            var startTime = new DateTime(startTimeTicks);
                            filterExpression = $"start/dateTime gt '{startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.000000")}'"; //// TODO does touniversal time mess up if you're already in utc?
                        }
                    }
                    else if (parameterName.StartsWith(nameof(EndTimeLessThan)))
                    {
                        if (long.TryParse(parameterName.Substring(nameof(EndTimeLessThan).Length), out var endTimeTicks))
                        {
                            var endTime = new DateTime(endTimeTicks);
                            filterExpression = $"end/dateTime lt '{endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.000000")}'";
                        }
                    }
                }
            }

            if (filterExpression == null)
            {
                throw new NotImplementedException("TODO");
            }

            if (this.filter == null)
            {
                return new GraphCalendarEventsContext(
                    this.evaluator, 
                    this.calendarRoot, 
                    filterExpression, 
                    this.orderBy,
                    this.top);
            }
            else
            {
                return new GraphCalendarEventsContext(
                    this.evaluator, 
                    this.calendarRoot, 
                    this.filter + " and " + filterExpression, 
                    this.orderBy,
                    this.top);
            }
        }

        public IGraphCalendarEventsContext OrderBy<TOrder>(Expression<Func<GraphCalendarEvent, TOrder>> orderBy)
        {
            string orderByExpression;
            if (orderBy is Expression<Func<GraphCalendarEvent, string>> asString && asString == StartTime)
            {
                orderByExpression = "start/dateTime";
            }
            else
            {
                throw new NotImplementedException("TODO");
            }

            if (this.orderBy == null)
            {
                return new GraphCalendarEventsContext(
                    this.evaluator,
                    this.calendarRoot, 
                    this.filter,
                    orderByExpression,
                    this.top);
            }
            else
            {
                return new GraphCalendarEventsContext(
                    this.evaluator,
                    this.calendarRoot,
                    this.filter,
                    this.orderBy + "," + orderByExpression,
                    this.top);
            }
        }

        public IGraphCalendarEventsContext Top(int top)
        {
            if (this.top != null)
            {
                throw new Exception("TODO invalidoperationexception");
            }

            return new GraphCalendarEventsContext(
                this.evaluator,
                this.calendarRoot,
                this.filter,
                this.orderBy,
                top.ToString());
        }

        internal static Expression<Func<GraphCalendarEvent, bool>> TypeEqualsSingleInstance { get; } = calendarEvent => true; //// TODO how should you handle the fact that `calendarEvent/type` won't get selected? it still needs to be a property on `graphcalendarevent` so that you can write this expression

        internal static Expression<Func<GraphCalendarEvent, bool>> StartTimeGreaterThan(DateTime dateTime)
        {
            Expression<Func<GraphCalendarEvent, bool>> foo = calendarEvent => true;
            var ticks = Expression.Parameter(typeof(GraphCalendarEvent), nameof(StartTimeGreaterThan) + dateTime.Ticks.ToString());
            foo.Update(foo.Body, new[] { ticks });

            return foo;
        }

        internal static Expression<Func<GraphCalendarEvent, bool>> EndTimeLessThan(DateTime dateTime)
        {
            Expression<Func<GraphCalendarEvent, bool>> foo = calendarEvent => true;
            var ticks = Expression.Parameter(typeof(GraphCalendarEvent), nameof(EndTimeLessThan) + dateTime.Ticks.ToString());
            foo.Update(foo.Body, new[] { ticks });

            return foo;
        }

        internal static Expression<Func<GraphCalendarEvent, bool>> IsCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == true;

        internal static Expression<Func<GraphCalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => calendarEvent.IsCancelled == false;

        internal static Expression<Func<GraphCalendarEvent, string>> StartTime { get; } = calendarEvent => calendarEvent.Start.DateTime;
    }
}
