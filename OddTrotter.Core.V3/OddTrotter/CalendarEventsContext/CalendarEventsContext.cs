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
        IQueryContextAsync
            <
                IEither
                    <
                        CalendarEvent, 
                        CalendarEventTranslationException
                    >, 
                CalendarEvent, 
                Graph.PagingError //// TODO when you use this here, either the implementation detail (that we are using graph) is leaked, or the caller is forced to parameterize this (i.e. to use `iquerycontextasync<IEither<CalendarEvent, CalendarEventTranslationException>, CalendarEvent, TError>`) (or else the *caller* will leak implementation details); do you want to try to abstract this in some way? i think to really get that correct, you would need one or two *other* implementations of a calendar events context...
            >, 
        IWhereQueryContextMixinAsync
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

        public async ITask<IQueryResultAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.PagingError>> Evaluate()
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
                        calendarEventOrError => calendarEventOrError.Apply(calendarEvent => this.where(calendarEvent), error => true));
            }

            return events;
        }

        private async ITask<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetEvents()
        {
            var instanceEvents = await this.GetInstanceEvents().ConfigureAwait(false);
            var seriesEvents = await this.GetSeriesEvents().ConfigureAwait(false);

            return instanceEvents.Concat2(
                Task.FromResult(seriesEvents)/*,
                firstError => firstError,
                secondError => secondError,
                (firstError, secondError) =>
                    new Graph.PagingException(
                            "TODO an error occurred while paging both instances events and series events",
                            new AggregateException(firstError, secondError))*/);
        }

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstanceEvents()
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

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetSeriesEvents()
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
                                //// TODO you should actually get the first *non-error* instance; however, keep in mind that, for an unending series, if there's a bug in deserializing, you won't ever first a non-error instance
                                var instances = await this.GetInstancesInSeries(seriesMaster.Id).ConfigureAwait(false);
                                return (SeriesMaster: seriesMaster, PotentialFirstInstance: await instances.FirstOrDefault(new Nothing()).SelectLeft(_ => _.AsEither()).ConfigureAwait(false)); //// TODO you need aseither because `foo2` below uses tuples which don't have covariance
                            })
                        .ConfigureAwait(false))
                .Select(
                    seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError //// TODO i think "design-wise", it makes more sense to have `ieither<ieither<event, errors>, nothing>` (i.e. the left represents the "potential" event, and its left is the actual event and its right is the ieither of errors); can you somehow make this work?
                        /*.Foo2() //// TODO you need to rename these extensions
                        .Foo3()
                        .Foo2()
                        .Foo3()
                        .Foo2()
                        .Foo3()*/
                        .Foo2Point5()
                        .Foo2Point5()
                        .Foo2Point5()
                        .SelectRight(_ => _.SelectRight(right => right.Foo5().Foo3())) //// TODO all of your lambdas need to have meaningful names; search for "_" to see what you need to address
                        ////.SelectRight(_ => _.SelectRight(_ => _.SelectLeft(_ => _.Foo5()))
                        .Foo4()
                        .Foo4()
                        .Foo4()
                        ////.SelectLeft(_ => _.SelectManyRight())
                        ////.Foo3())
                        ////.Foo4()
                        ////.Foo4()
                        )
                /*.Select(
                    seriesMasterPlusPontentialFirstInstanceOrTranslationError => seriesMasterPlusPontentialFirstInstanceOrTranslationError // what we want is ieither<(seriesmaster+firstinsatnce), ieither<seriesmastertranslationerror, ieither<instancetranslationerror, instancepagingerror>; so we are returning here and either of *that* or *nothing*
                        .Apply(
                            seriesMasterPlusPotentialFirstInstance => seriesMasterPlusPotentialFirstInstance
                                .PotentialFirstInstance
                                .Apply(
                                    potentialFirstInstanceOrError => potentialFirstInstanceOrError
                                        .Apply(
                                            firstInstanceOrError => Either
                                                .Right<Nothing>()
                                                .Left(
                                                    firstInstanceOrError
                                                        .Apply(
                                                            firstInstance => Either
                                                                .Right<Either<Graph.CalendarEventTranslationException, Either<Graph.CalendarEventTranslationException, Graph.PagingException>>>()
                                                                .Left(
                                                                    (
                                                                        SeriesMaster: seriesMasterPlusPotentialFirstInstance.SeriesMaster,
                                                                        FirstInstance: firstInstance
                                                                    )),
                                                            instanceTranslationError => Either
                                                                .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                                                .Right(
                                                                    Either
                                                                        .Left<Graph.CalendarEventTranslationException>()
                                                                        .Right(
                                                                            Either
                                                                                .Right<Graph.PagingException>()
                                                                                .Left(instanceTranslationError))))),
                                            nothing => Either
                                                .Left<Either<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), Either<Graph.CalendarEventTranslationException, Either<Graph.CalendarEventTranslationException, Graph.PagingException>>>>()
                                                .Right(nothing)),
                                    instancePagingError => Either
                                            .Right<Nothing>()
                                            .Left(
                                                Either
                                                    .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                                    .Right(
                                                        Either
                                                            .Left<Graph.CalendarEventTranslationException>()
                                                            .Right(
                                                                Either
                                                                    .Left<Graph.CalendarEventTranslationException>()
                                                                    .Right(instancePagingError))))),
                            seriesTranslationError => Either
                                .Right<Nothing>()
                                .Left(
                                    Either
                                        .Left<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance)>()
                                        .Right(
                                            Either
                                                .Right<Either<Graph.CalendarEventTranslationException, Graph.PagingException>>()
                                                .Left(seriesTranslationError)))))*/
                /*.TrySelect( //// TODO any way to get type inference here?
                    (IEither<IEither<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), IEither<Graph.CalendarEventTranslationException, IEither<Graph.CalendarEventTranslationException, Graph.PagingException>>>, Nothing> potentialSeriesMasterPlusFirstInstanceOrError, [MaybeNullWhen(false)] out IEither<(Graph.CalendarEvent SeriesMaster, Graph.CalendarEvent FirstInstance), IEither<Graph.CalendarEventTranslationException, IEither<Graph.CalendarEventTranslationException, Graph.PagingException>>> seriesMasterWithInstanceOrError) =>
                    {
                        return potentialSeriesMasterPlusFirstInstanceOrError.Decompose(out seriesMasterWithInstanceOrError, out _);
                    })*/
                .TrySelect()
                .Select(
                    seriesMasterWithInstanceOrError => seriesMasterWithInstanceOrError
                        .Foo3()
                        .Foo3()
                        .SelectLeft(
                            seriesMasterPlusInstance => new Graph.CalendarEvent(
                                seriesMasterPlusInstance.Item1.Id,
                                seriesMasterPlusInstance.Item1.Subject,
                                seriesMasterPlusInstance.Item1.Body,
                                seriesMasterPlusInstance.Item2.Start,
                                seriesMasterPlusInstance.Item1.IsCancelled,
                                seriesMasterPlusInstance.Item1.Type,
                                seriesMasterPlusInstance.Item2.End))
                        .SelectRight(_ => _.SelectRight(_ => _.Foo5()))
                        .SelectRight(
                            errors => errors.SelectManyRight())
                        .SelectRight(
                            translationErrorOrInstancePagingError => translationErrorOrInstancePagingError
                                .SelectRight(
                                    instancePagingError => new Graph.CalendarEventTranslationException("TODO include the paging error and include everything we know about the series master"))
                                .Coalesce()));

            return mastersWithInstances;
        }

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeries(string seriesMasterId)
        {
            var pageStartTime = this.startTime;
            var pageEndTime = pageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < pageEndTime)
            {
                pageEndTime = this.endTime.Value;
            }

            return await this.GetInstancesInSeries(seriesMasterId, pageStartTime, pageEndTime).ConfigureAwait(false);
        }

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeries(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var initial = await GetInstancesInSeriesWithinTimeSlice(seriesMasterId, pageStartTime, pageEndTime).ConfigureAwait(false);

            var newPageStartTime = this.startTime;
            var newPageEndTime = newPageStartTime + this.firstInstanceInSeriesLookahead;
            if (this.endTime != null && this.endTime.Value < newPageEndTime)
            {
                newPageEndTime = this.endTime.Value;
            }

            return initial.Concat2(this.GetInstancesInSeries(seriesMasterId, newPageStartTime, newPageEndTime));
        }

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetInstancesInSeriesWithinTimeSlice(string seriesMasterId, DateTime pageStartTime, DateTime pageEndTime)
        {
            var context = this.calendarSource.Events().Get(seriesMasterId).Instances(pageStartTime, pageEndTime).Get();
            if (this.isCancelled != null)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private async Task<IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError>> GetSeriesEventMasters()
        {
            var context = this
                .calendarSource
                .Events()
                .Get()
                .Filter(calendarEvent => calendarEvent.Type == "seriesMaster")
                .OrderBy(calendarEvent => calendarEvent.Start.DateTime)
                .Top(this.pageSize);

            if (this.isCancelled.HasValue)
            {
                context = context.Filter(calendarEvent => calendarEvent.IsCancelled == this.isCancelled.Value);
            }

            return await context.Evaluate().ConfigureAwait(false);
        }

        private static IQueryResultAsync<IEither<CalendarEvent, CalendarEventTranslationException>, Graph.PagingError> Translate(IQueryResultAsync<IEither<Graph.CalendarEvent, Graph.CalendarEventTranslationException>, Graph.PagingError> graphQueryResult)
        {
            return graphQueryResult
                .Select(element => element
                    .SelectRight(translationException => new CalendarEventTranslationException("TODO", translationException))
                    .SelectLeft(calendarEvent => CalendarEventsContext.Translate(calendarEvent))
                    .SelectManyLeft())
                ////.SelectError(pagingException => new PagingException("TODO", pagingException))
                ;
        }

        private static IEither<CalendarEvent, CalendarEventTranslationException> Translate(Graph.CalendarEvent calendarEvent)
        {
            /*DateTimeOffset start;
            try
            {
                start = DateTimeOffset.Parse(calendarEvent.Start.DateTime);
            }
            catch (Exception exception)
            {
                return Either.Left<CalendarEvent>().Right(new CalendarEventTranslationException("tODO", exception));
            }*/

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
        internal static IEither<TRight, TLeft> Foo5<TLeft, TRight>(
            this IEither<TLeft, TRight> either)
        {
            return either.Apply(
                left => Either.Left<TRight>().Right(left),
                right => Either.Right<TLeft>().Left(right));
        }

        internal static IEither<IEither<TLeft, TLeftInner>, TRightInner> Foo4<TLeft, TLeftInner, TRightInner>(
            this IEither<TLeft, IEither<TLeftInner, TRightInner>> either)
        {
            return either.Apply(
                left => Either.Right<TRightInner>().Left(Either.Right<TLeftInner>().Left(left)),
                right => right.Apply(
                    leftInner => Either.Right<TRightInner>().Left(Either.Left<TLeft>().Right(leftInner)),
                    rightInner => Either.Left<Either<TLeft, TLeftInner>>().Right(rightInner)));
        }

        internal static IEither<TLeft, TRight> AsEither<TLeft, TRight>(this IEither<TLeft, TRight> either)
        {
            return either;
        }

        internal static IEither<(TLeft1, TLeft2), IEither<TRightInner, TRight>> Foo2Point5<TLeft1, TLeft2, TRightInner, TRight>(
            this IEither<(TLeft1, IEither<TLeft2, TRightInner>), TRight> either)
        {
            return either.Foo2().Foo3();
        }

        internal static IEither<TLeftInner, IEither<TRightInner, TRight>> Foo3<TLeftInner, TRightInner, TRight>(
            this IEither<IEither<TLeftInner, TRightInner>, TRight> either)
        {
            return either.Apply(
                left => left.Apply(
                    leftInner => Either.Right<Either<TRightInner, TRight>>().Left(leftInner),
                    rightInner => Either.Left<TLeftInner>().Right(Either.Right<TRight>().Left(rightInner))),
                right => Either.Left<TLeftInner>().Right(Either.Left<TRightInner>().Right(right)));
        }

        internal static IEither<IEither<(TLeft1, TLeft2), TRightInner>, TRight> Foo2<TRight, TLeft1, TLeft2, TRightInner>(
            this IEither<(TLeft1, IEither<TLeft2, TRightInner>), TRight> either)
        {
            return either
                .Foo(
                    tuple => tuple
                        .Item2
                        .SelectLeft(
                            left => (tuple.Item1, left)));
        }

        internal static IEither<IEither<TLeftInner, TRightInner>, TRight> Foo<TLeft, TRight, TLeftInner, TRightInner>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, IEither<TLeftInner, TRightInner>> selector)
        {
            return either.SelectLeft(selector);
        }

        internal static IQueryResultAsync<TValue, TError> TrySelect<TValue, TError>(
            this IQueryResultAsync<IEither<TValue, Nothing>, TError> queryResult)
        {
            return queryResult.TrySelect<IEither<TValue, Nothing>, TError, TValue>((either, [MaybeNullWhen(false)] out left) => either.TryGetLeft(out left));
        }

        public static bool TryGetLeft<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left)
        {
            return either.Decompose(out left, out _);
        }

        internal static IQueryResultAsync<TResult, TError> SelectAsync<TValue, TError, TResult>(
            this IQueryResultAsync<TValue, TError> queryResult,
            Func<TValue, Task<TResult>> selector)
        {
            return new SelectQueryResult<TValue, TError, TResult>(queryResult, selector);
        }

        private sealed class SelectQueryResult<TValue, TError, TResult> : IQueryResultAsync<TResult, TError>
        {
            private readonly IQueryResultAsync<TValue, TError> queryResult;
            private readonly Func<TValue, Task<TResult>> selector;

            public SelectQueryResult(
                IQueryResultAsync<TValue, TError> queryResult,
                Func<TValue, Task<TResult>> selector)
            {
                this.queryResult = queryResult;
                this.selector = selector;
            }

            public async ITask<IQueryResultNodeAsync<TResult, TError>> GetNodes()
            {
                return new QueryResultNode(await this.queryResult.GetNodes().ConfigureAwait(false), this.selector);
            }

            private sealed class QueryResultNode : IQueryResultNodeAsync<TResult, TError>
            {
                private readonly IQueryResultNodeAsync<TValue, TError> queryResult;
                private readonly Func<TValue, Task<TResult>> selector;

                public QueryResultNode(
                    IQueryResultNodeAsync<TValue, TError> queryResult,
                    Func<TValue, Task<TResult>> selector)
                {
                    this.queryResult = queryResult;
                    this.selector = selector;
                }

                public Realizable<TResult1> ApplyAsync<TResult1, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<IElementAsync<TResult, TError>, TContext, TContinuable, TResult1> leftMap, AsyncRefContextualizedContinuableMap<IEither<IError<TError>, IEmpty>, TContext, TContinuable, TResult1> rightMap, ref TContext context)
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

                private sealed class Element : IElementAsync<TResult, TError>
                {
                    private readonly IElementAsync<TValue, TError> element;
                    private readonly Func<TValue, Task<TResult>> selector;

                    public Element(
                        TResult value,
                        IElementAsync<TValue, TError> element,
                        Func<TValue, Task<TResult>> selector)
                    {
                        Value = value;
                        this.element = element;
                        this.selector = selector;
                    }

                    public TResult Value { get; }

                    public async ITask<IQueryResultNodeAsync<TResult, TError>> Next()
                    {
                        return new QueryResultNode(await this.element.Next().ConfigureAwait(false), this.selector);
                    }
                }
            }
        }
    }
}
