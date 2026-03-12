namespace OddTrotter.CalendarEventsContext
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;
    using Fx.QueryContext.Mixins;
    using Fx.Realizable;

    using OddTrotter.Graph.CalendarEventsContext;
    using OddTrotter.Graph.CalendarEventsSource;

    using Graph = OddTrotter.Graph.CalendarEventsContext;

    internal sealed class CalendarEventsContextSettings
    {
        private CalendarEventsContextSettings(uint pageSize, TimeSpan firstInstanceInSeriesLookahead)
        {
            PageSize = pageSize;
            FirstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
        }

        public static CalendarEventsContextSettings Default { get; } = new CalendarEventsContextSettings(
            10,
            TimeSpan.FromDays(14));

        public uint PageSize { get; }
        public TimeSpan FirstInstanceInSeriesLookahead { get; }
    }

    internal sealed class CalendarEventsContext : //// TODO i think you need to rethink some of your concrete implementation names; for example, this is an implementation of a calendar event context that *leverages graph*; shouldn't the graph part be in the name?
        IQueryContext
            <
                IEither
                    <
                        CalendarEvent, 
                        CalendarEventTranslationException
                    >, 
                CalendarEvent, 
                Graph.PagingError //// TODO when you use this here, either the implementation detail (that we are using graph) is leaked, or the caller is forced to parameterize this (i.e. to use `iquerycontextasync<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, TError>`) (or else the *caller* will leak implementation details); do you want to try to abstract this in some way? i think to really get that correct, you would need one or two *other* implementations of a calendar events context...
            >, 
        IWhereQueryContextMixin
            <
                IEither
                    <
                        CalendarEvent,
                        CalendarEventTranslationException
                    >,
                CalendarEvent,
                Graph.PagingError,
                CalendarEventsContext
            >
    {
        private readonly ICalendarSource calendarSource;
        private readonly DateTime startTime;
        private readonly uint pageSize;
        private readonly TimeSpan firstInstanceInSeriesLookahead;
        private readonly bool? isCancelled;
        private readonly DateTime? endTime;
        private readonly Func<CalendarEvent, bool>? where;
        private readonly Func<Graph.CalendarEvent, bool>? seriesMasterPredicate;

        internal CalendarEventsContext(ICalendarSource calendarSource, DateTime startTime)
            : this(calendarSource, startTime, CalendarEventsContextSettings.Default)
        {
        }

        internal CalendarEventsContext(ICalendarSource calendarSource, DateTime startTime, CalendarEventsContextSettings settings)
            : this(
                  calendarSource,
                  startTime,
                  settings.PageSize,
                  settings.FirstInstanceInSeriesLookahead,
                  null,
                  null,
                  null,
                  null)
        {
        }

        private CalendarEventsContext(
            ICalendarSource calendarSource, 
            DateTime startTime,
            uint pageSize,
            TimeSpan firstInstanceInSeriesLookahead,
            bool? isCancelled,
            DateTime? endTime,
            Func<CalendarEvent, bool>? where,
            Func<Graph.CalendarEvent, bool>? seriesMasterPredicate)
        {
            this.calendarSource = calendarSource;
            this.startTime = startTime;
            this.pageSize = pageSize;
            this.firstInstanceInSeriesLookahead = firstInstanceInSeriesLookahead;
            this.isCancelled = isCancelled;
            this.endTime = endTime;
            this.where = where;
            this.seriesMasterPredicate = seriesMasterPredicate;
        }

        public async ITask<IQueryResult<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.PagingError>> Evaluate()
        {
            var events = await this.GetEvents()
                .Select(element => element
                    .SelectRight(translationException => new CalendarEventTranslationException("TODO", translationException))
                    .SelectLeft(calendarEvent => CalendarEventsContext.Translate(calendarEvent))
                    .SelectManyLeft())
                .Where(
                    calendarEventOrTranslationError => calendarEventOrTranslationError
                        .Apply(
                            calendarEvent => calendarEvent.Start > this.startTime, // there's a bug in the graph api; it treats gt as ge, so we need to do this extra check locally
                            error => true))
                .ConfigureAwait(false);
            if (where != null)
            {
                events = events
                    .Where(
                        calendarEventOrError => calendarEventOrError
                            .Apply(
                                calendarEvent => this.where(calendarEvent), 
                                error => true));
            }

            return events;
        }

        private async ITask<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetEvents()
        {
            var instanceEvents = await this.GetInstanceEvents().ConfigureAwait(false);
            var seriesEvents = await this.GetSeriesEvents().ConfigureAwait(false);

            return instanceEvents.Concat(
                seriesEvents,
                firstError => firstError,
                secondError => secondError,
                (firstError, secondError) => new Graph.PagingError.Context(
                    new Uri("https://todo.com"), 
                    new Graph.ContextException("TODO an error occurred while paging both instances events and series events")));
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstanceEvents()
        {
            var context = this
                .calendarSource
                .Events()
                .Get()
                .Filter(calendarEvent => calendarEvent.Type == "singleInstance")
                .Filter(calendarEvent => calendarEvent.Start.DateTime > this.startTime) //// TODO i can't decide if `timestructure.datetime` should be a string and we should call `this.startTime.ToString()` here, or if `timestructure.datetime` is supposed to be a datetime; look at the csdl probably...
                .Top(this.pageSize)
                .OrderBy(calendarEvent => calendarEvent.Start.DateTime);

            if (this.endTime != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.End.DateTime < this.endTime.Value);
            }

            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetSeriesEvents()
        {
            var seriesEventMasters = await this.GetSeriesEventMasters().ConfigureAwait(false);
            if (this.seriesMasterPredicate != null)
            {
                seriesEventMasters = seriesEventMasters
                    .Where(
                        seriesEventMasterOrError => seriesEventMasterOrError
                            .Apply(
                                seriesEventMaster => this.seriesMasterPredicate(seriesEventMaster),
                                error => true));
            }

            var mastersWithInstances = seriesEventMasters
                .SelectAsync(
                    async seriesMasterOrTranslationError => await seriesMasterOrTranslationError
                        .SelectLeft(
                            async seriesMaster =>
                            {
                                // this selector is trying to accomplish a lot; ultimately, the entire *method* is trying to get a series master and its first instance, so that we can see if the series has any instances in the time period that's been configured by the caller
                                // by the time we get to this selector, we have the series masters, so we are now trying to get the first instance
                                // once we "have" the instances (it is lazily evaluated), we are going to try to get the first one; *but* what if the first one has a translation error? well, we could just try to get the second one; *but*, what if there's a bug in our translation code? then *all* of the instances will have errors, and if its an unending series, we will loop forever trying to find the an instance that will never exist
                                // so, we can't take the first non-error instance of *all* of the instances; we need to put a cap on it, so we use a `take`
                                // then, we go ahead and get the first instance whether or not it has an error; we will use this in the case where *all* of the instances have an error
                                // now that we have that in our back pocket just in case, we try to see if there are any *non-error* instances using a `where`; we get the first instance of *those*
                                // if there are none, then we go back to the first error instance
                                // and now we have the potential first instance, so we may return
                                var instances = await this
                                    .GetInstancesInSeries(seriesMaster.Id)
                                    .Take(100) //// TODO configure this;
                                    .ConfigureAwait(false);
                                var potentialFirstInstance = await instances.FirstOrDefault(new Nothing()).ConfigureAwait(false);

                                //// TODO perf-wise, this is no different from enumerable, but maybe you could do better
                                var nonErrorInstance = await instances
                                    .Where(potentialInstance => potentialInstance.Apply(instance => true, error => false))
                                    .FirstOrDefault(new Nothing()).ConfigureAwait(false);
                                if (!nonErrorInstance.TryGetRight(out _))
                                {
                                    potentialFirstInstance = nonErrorInstance;
                                }

                                return
                                    (
                                        SeriesMaster: seriesMaster,
                                        PotentialFirstInstance: potentialFirstInstance
                                    );
                            })
                        .ConfigureAwait(false))
                .Select(
                    seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError
                        // we want to filter out series masters that don't have future instances (we *don't* want to filter errors, since they *might* represent future instances); we will do this later with a `tryselect`, so we need to get the `nothing` instances to the "right" side of the either; we are also looking to get non-error cases to the left side of the either; so, we should end up with something like `ieither<ieither<...<ieither<(seriesmaster, firstinstance), error>, error>,...> nothing>`
                        .LiftSequence() // pull the paging error out of the tuple
                        .Associate() // move the tuple left
                        .LiftSequence() // pull the nothing out of the tuple
                        .Associate() // move the tuple left
                        .LiftSequence() // pull the translation error out of the tuple
                        .Associate() // move the tuple left
                        .SelectRight(
                            errorCases => errorCases
                                .SelectRight(
                                    nothingOrErrors => nothingOrErrors
                                        .Swap() // move nothing to the right side
                                    ))
                        .Unassociate() // move nothing to the right
                        .Unassociate() // move nothing to the right
                        .SelectLeft( // get all of the eithers nested on the left
                            seriesMasterPlusPontentialFirstInstanceOrErrorCases => seriesMasterPlusPontentialFirstInstanceOrErrorCases
                                .Associate())
                        )
                .TrySelect()
                .Select(
                    seriesMasterPlusInstanceOrError => seriesMasterPlusInstanceOrError
                        .SelectLeft(
                            seriesMasterPlusInstance => 
                                // combine the series master and the first instance into a "canonical" calendar event; this allows the caller to see meaningful timestamps while preserving the "series" nature of the event (for things like canceling and accepting the event);
                                // NOTE: there's an argument to be made that this class should actually return all future instances of the series event, and not preserve the data about the series, but i'm not clear what the design of the (non-graph) `calendarevent` class would look like in that case, for situations like canceling, declining, or accepting a series
                                new Graph.CalendarEvent(
                                    seriesMasterPlusInstance.Item1.Id,
                                    seriesMasterPlusInstance.Item1.Subject,
                                    seriesMasterPlusInstance.Item1.Body,
                                    seriesMasterPlusInstance.Item2.Start,
                                    seriesMasterPlusInstance.Item1.IsCancelled,
                                    seriesMasterPlusInstance.Item1.Type,
                                    seriesMasterPlusInstance.Item2.End))
                        .SelectRight(
                            // reorder the error cases so that you can combine the different translation errors
                            errorCases => errorCases
                                .SelectRight(
                                    pagingOrTranslation => pagingOrTranslation
                                        .Swap()))
                        .SelectRight(
                            errors => errors.SelectManyRight())
                        .SelectRight(
                            translationErrorOrInstancePagingError => translationErrorOrInstancePagingError
                                .SelectRight(
                                    instancePagingError => new Graph.CalendarEventTranslationException("TODO include the paging error and include everything we know about the series master"))
                                .Coalesce()));

            return mastersWithInstances;
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeries(string seriesMasterId)
        {
            var pageStartTime = this.startTime;
            var pageEndTime = pageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < pageEndTime)
            {
                pageEndTime = this.endTime.Value;
            }

            return await this.GetInstancesInSeries(seriesMasterId, pageStartTime, pageEndTime).ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeries(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var newPageStartTime = this.startTime;
            var newPageEndTime = newPageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < newPageEndTime)
            {
                newPageEndTime = this.endTime.Value;
            }

            return await this
                .GetInstancesInSeriesWithinTimeSlice(seriesMasterId, pageStartTime, pageEndTime)
                .Concat(
                    this.GetInstancesInSeries(seriesMasterId, newPageStartTime, newPageEndTime),
                    firstError => firstError,
                    secondError => secondError,
                    (firstError, secondError) => 
                        new Graph.PagingError.Context(
                            new Uri("https://todo.com"), 
                            new Graph.ContextException("TODO an error occurred within this time slice *and* the next one")))
                .ConfigureAwait(false);
        }

        private async ITask<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeriesWithinTimeSlice(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var context = this
                .calendarSource
                .Events()
                .Get(seriesMasterId)
                .Instances(pageStartTime, pageEndTime)
                .Get();
            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private async Task<IQueryResult<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetSeriesEventMasters()
        {
            var context = this
                .calendarSource
                .Events()
                .Get()
                .Filter(calendarEvent => calendarEvent.Type == "seriesMaster")
                .OrderBy(calendarEvent => calendarEvent.Start.DateTime)
                .Top(this.pageSize);

            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private static IEither<CalendarEvent, CalendarEventTranslationException> Translate(Graph.CalendarEvent calendarEvent)
        {
            return Either
                .Right<CalendarEventTranslationException>()
                .Left(
                    new CalendarEvent(
                        calendarEvent.Id,
                        calendarEvent.Subject, 
                        calendarEvent.Body.Content,
                        calendarEvent.Start.DateTime, 
                        calendarEvent.IsCancelled));
        }

        public CalendarEventsContext Where(Expression<Func<CalendarEvent, bool>> predicate)
        {
            if (object.ReferenceEquals(predicate, StartLessThanNow))
            {
                var now = DateTime.UtcNow;
                if (this.endTime != null && this.endTime < now)
                {
                    // we logically can see that this will always happen (they can only call set `endTime` to `DateTime.UtcNow`, so `now` will always been more in the future than `endTime`) 
                    return this;
                }

                return new CalendarEventsContext(this.calendarSource, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, this.isCancelled, now,
                    this.where, this.seriesMasterPredicate);
            }
            else if (object.ReferenceEquals(predicate, IsNotCancelled))
            {
                if (this.isCancelled != null)
                {
                    // the caller can only provide `IsNotCancelled` right now, so if `isCancelled` is already set, it won't be changing
                    return this;
                }

                return new CalendarEventsContext(this.calendarSource, this.startTime, this.pageSize, this.firstInstanceInSeriesLookahead, false, this.endTime, this.where, this.seriesMasterPredicate);
            }

            if (TryTranslateToSeriesMaster(predicate, out var seriesMasterPredicate))
            {
                return new CalendarEventsContext(
                    this.calendarSource,
                    this.startTime,
                    this.pageSize,
                    this.firstInstanceInSeriesLookahead,
                    this.isCancelled,
                    this.endTime,
                    this.where,
                    this.seriesMasterPredicate == null ? seriesMasterPredicate : calendarEvent => this.seriesMasterPredicate(calendarEvent) && seriesMasterPredicate(calendarEvent));
            }

            var compiledPredicate = predicate.Compile();
            return new CalendarEventsContext(
                this.calendarSource,
                this.startTime,
                this.pageSize,
                this.firstInstanceInSeriesLookahead,
                this.isCancelled,
                this.endTime,
                this.where == null ? compiledPredicate : calendarEvent => this.where(calendarEvent) && compiledPredicate(calendarEvent),
                this.seriesMasterPredicate);
        }

        private static bool TryTranslateToSeriesMaster(Expression<Func<CalendarEvent, bool>> predicate, [MaybeNullWhen(false)] out Func<Graph.CalendarEvent, bool> seriesMasterPredicate)
        {
            //// TODO you have a sample of this in `playgroundtests.ParsePredicate`
            if (object.ReferenceEquals(predicate, SubjectIsTodoList))
            {
                seriesMasterPredicate = calendarEvent => calendarEvent.Subject == "todo list";
                return true;
            }

            seriesMasterPredicate = default;
            return false;
        }

        public static Expression<Func<CalendarEvent, bool>> SubjectIsTodoList { get; } = calendarEvent => calendarEvent.Subject == "todo list";

        public static Expression<Func<CalendarEvent, bool>> StartLessThanNow { get; } = calendarEvent => calendarEvent.Start < DateTime.UtcNow; //// TODO will "now" constantly change?

        public static Expression<Func<CalendarEvent, bool>> IsNotCancelled { get; } = calendarEvent => !calendarEvent.IsCancelled;
    }

    internal static class Extensions
    {

        internal static IEither<TRight, TLeft> Swap<TLeft, TRight>(
            this IEither<TLeft, TRight> either)
        {
            // TODO haskell calls this "swap": https://hackage.haskell.org/package/assoc-1.1.1/docs/Data-Bifunctor-Swap.html

            return either.Apply(
                left => Either.Left<TRight>().Right(left),
                right => Either.Right<TLeft>().Left(right));
        }

        internal static IEither<IEither<TLeft, TLeftInner>, TRightInner> Unassociate<TLeft, TLeftInner, TRightInner>(
            this IEither<TLeft, IEither<TLeftInner, TRightInner>> either)
        {
            // TODO haskell calls this "unassoc": https://hackage.haskell.org/package/assoc-1.1.1/docs/Data-Bifunctor-Assoc.html

            return either.Apply(
                left => Either.Right<TRightInner>().Left(Either.Right<TLeftInner>().Left(left)),
                right => right.Apply(
                    leftInner => Either.Right<TRightInner>().Left(Either.Left<TLeft>().Right(leftInner)),
                    rightInner => Either.Left<Either<TLeft, TLeftInner>>().Right(rightInner)));
        }

        internal static IEither<TLeftResult, Nothing> Propagate<TLeftSource, TLeftResult>(
            this IEither<TLeftSource, Nothing> either,
            Func<TLeftSource, TLeftResult?> propagator)
        {
            //// TODO you don't need this method yet, but it does seem useful

            return either.SelectLeft(propagator).SelectLeft(_ => _.ToEither()).SelectManyLeft();
        }

        internal static IEither<TValue, Nothing> ToEither<TValue>(this TValue? value)
        {
            if (value is null)
            {
                return Either.Left<TValue>().Right(new Nothing());
            }
            else
            {
                return Either.Right<Nothing>().Left(value);
            }
        }
        
        internal static IEither<TLeftInner, IEither<TRightInner, TRight>> Associate<TLeftInner, TRightInner, TRight>(
            this IEither<IEither<TLeftInner, TRightInner>, TRight> either)
        {
            //// TODO haskell calls this `assoc`: https://hackage.haskell.org/package/assoc-1.1.1/docs/Data-Bifunctor-Assoc.html (the "Assoc Either" section)

            return either.Apply(
                left => left.Apply(
                    leftInner => Either.Right<Either<TRightInner, TRight>>().Left(leftInner),
                    rightInner => Either.Left<TLeftInner>().Right(Either.Right<TRight>().Left(rightInner))),
                right => Either.Left<TLeftInner>().Right(Either.Left<TRightInner>().Right(right)));
        }

        internal static IEither<IEither<(TLeft1, TLeft2), TRightInner>, TRight> LiftSequence<TRight, TLeft1, TLeft2, TRightInner>(
            this IEither<(TLeft1, IEither<TLeft2, TRightInner>), TRight> either)
        {
            //// TODO this operation is equivalent to haskells `fmap sequence`; i am calling it "lift" because `fmap` is a "lift": https://wiki.haskell.org/Lifting ; i supposed i *could* call it `selectsequence` because i'm using "select" as a continuation of the c# idiom that "select" means "fmap"

            return either.SelectLeft(tuple => tuple.Sequence());
        }

        internal static IEither<(T1, TLeft), TRight> Sequence<T1, TLeft, TRight>(
            this (T1, IEither<TLeft, TRight>) tuple)
        {
            //// TODO haskell calls this "sequence": https://hackage.haskell.org/package/base-4.21.0.0/docs/Data-Traversable.html

            return tuple.Item2.SelectLeft(left => (tuple.Item1, left));
        }


        internal static async ITask<IQueryResult<TValue, TError>> Take<TValue, TError>(
            this Task<IQueryResult<TValue, TError>> queryResult,
            int count)
        {
            //// TODO note somewhere that if you find `count` elements before getting to the end, you don't end up preserving any `terror` that might have occurred

            return new TakeQueryResult<TValue, TError>(await queryResult.ConfigureAwait(false), count);
        }

        private sealed class TakeQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            private readonly IQueryResult<TValue, TError> queryResult;
            private readonly int count;

            public TakeQueryResult(
                IQueryResult<TValue, TError> queryResult,
                int count)
            {
                this.queryResult = queryResult;
                this.count = count;
            }

            public async ITask<IQueryResultNode<TValue, TError>> GetNodes()
            {
                return new Node(await this.queryResult.GetNodes().ConfigureAwait(false), this.count);
            }

            private sealed class Node : IQueryResultNode<TValue, TError>
            {
                private readonly IQueryResultNode<TValue, TError> queryResult;
                private readonly int count;

                public Node(
                    IQueryResultNode<TValue, TError> queryResult,
                    int count)
                {
                    this.queryResult = queryResult;
                    this.count = count;
                }

                public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TValue, TError>, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult> rightMap, ref TContext context)
                    where TResult : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult>, allows ref struct
                {
                    return this.queryResult.ApplyAsync<TResult, TContext, TContinuable>(
                        (element, ref context) =>
                        {
                            return leftMap(new Element(element, this.count), ref context);
                        },
                        (terminal, ref context) =>
                        {
                            return rightMap(terminal, ref context);
                        },
                        ref context);
                }

                private sealed class Element : IElement<TValue, TError>
                {
                    private readonly IElement<TValue, TError> element;
                    private readonly int count;

                    public Element(IElement<TValue, TError> element, int count)
                    {
                        this.element = element;
                        this.count = count;
                    }

                    public TValue Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    public async ITask<IQueryResultNode<TValue, TError>> Next()
                    {
                        return new Node(await this.element.Next().ConfigureAwait(false), count - 1);
                    }
                }
            }
        }


        internal static IQueryResult<TValue, TError> TrySelect<TValue, TError>(
            this IQueryResult<IEither<TValue, Nothing>, TError> queryResult)
        {
            return queryResult.TrySelect<IEither<TValue, Nothing>, TError, TValue>((either, [MaybeNullWhen(false)] out left) => either.TryGetLeft(out left));
        }

        public static bool TryGetLeft<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left)
        {
            return either.Decompose(out left, out _);
        }

        internal static IQueryResult<TResult, TError> SelectAsync<TValue, TError, TResult>(
            this IQueryResult<TValue, TError> queryResult,
            Func<TValue, Task<TResult>> selector)
        {
            return new SelectQueryResult<TValue, TError, TResult>(queryResult, selector);
        }

        private sealed class SelectQueryResult<TValue, TError, TResult> : IQueryResult<TResult, TError>
        {
            private readonly IQueryResult<TValue, TError> queryResult;
            private readonly Func<TValue, Task<TResult>> selector;

            public SelectQueryResult(
                IQueryResult<TValue, TError> queryResult,
                Func<TValue, Task<TResult>> selector)
            {
                this.queryResult = queryResult;
                this.selector = selector;
            }

            public async ITask<IQueryResultNode<TResult, TError>> GetNodes()
            {
                return new QueryResultNode(await this.queryResult.GetNodes().ConfigureAwait(false), this.selector);
            }

            private sealed class QueryResultNode : IQueryResultNode<TResult, TError>
            {
                private readonly IQueryResultNode<TValue, TError> queryResult;
                private readonly Func<TValue, Task<TResult>> selector;

                public QueryResultNode(
                    IQueryResultNode<TValue, TError> queryResult,
                    Func<TValue, Task<TResult>> selector)
                {
                    this.queryResult = queryResult;
                    this.selector = selector;
                }

                public Realizable<TResult1> ApplyAsync<TResult1, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElement<TResult, TError>, TContext, TContinuable, TResult1> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult1> rightMap, ref TContext context)
                    where TResult1 : allows ref struct
                    where TContext : allows ref struct
                    where TContinuable : IContinuable<TResult1>, allows ref struct
                {
                    if (this.queryResult.Decompose(out var element, out var terminal)) //// TODO you shouldn't need to use decompose
                    {
                        return this.selector(element.Value)
                            .ToTaskWrapper()
                            .ContinueWith(
                                selected => new Element(selected, element, this.selector),
                                _ => throw _,
                                _ => throw _)
                            .ContinueWith(
                                element =>
                                {
                                    var fakeContext = default(TContext)!;
                                    ref TContext toPass = ref Unsafe.AsRef(ref fakeContext); //// TODO use the real context here...

                                    return leftMap(element, ref toPass).ContinueWith(_ => _, _ => throw _, _ => throw _);
                                },
                                _ => throw _,
                                _ => throw _)
                            .Unwrap();
                    }
                    else
                    {
                        return rightMap(terminal, ref context).ContinueWith(_ => _, _ => throw _, _ => throw _);
                    }
                }

                private sealed class Element : IElement<TResult, TError>
                {
                    private readonly IElement<TValue, TError> element;
                    private readonly Func<TValue, Task<TResult>> selector;

                    public Element(
                        TResult value,
                        IElement<TValue, TError> element,
                        Func<TValue, Task<TResult>> selector)
                    {
                        Value = value;
                        this.element = element;
                        this.selector = selector;
                    }

                    public TResult Value { get; }

                    public async ITask<IQueryResultNode<TResult, TError>> Next()
                    {
                        return new QueryResultNode(await this.element.Next().ConfigureAwait(false), this.selector);
                    }
                }
            }
        }
    }
}
