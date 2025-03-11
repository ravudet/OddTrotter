namespace OddTrotter.Calendar
{
    using Fx.Either;
    using Fx.QueryContext;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.V2;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public static class LeftOverEitherExtensions
    {
        public static Fx.Either.IEither<TLeftResult, TRightResult> Zip<TLeftFirst, TRightFirst, TLeftSecond, TRightSecond, TLeftResult, TRightResult>(
            this Fx.Either.IEither<TLeftFirst, TRightFirst> first,
            Fx.Either.IEither<TLeftSecond, TRightSecond> second,
            Func<TLeftFirst, TLeftSecond, TLeftResult> leftSelector,
            Func<TLeftFirst, TRightSecond, TRightResult> leftFirstRightSecondSelector,
            Func<TRightFirst, TLeftSecond, TRightResult> rightFirstLeftSecondSelector,
            Func<TRightFirst, TRightSecond, TRightResult> rightSelector)
        {
            //// TODO TOPIC what is the best name for this?
            //// TODO TOPIC your ultimate use case is along the lines of first.zip(second).throwright()
            //// TODO do you want a convenience overload that makes the left side tuples?

            //// https://hackage.haskell.org/package/base-4.21.0.0/docs/Control-Monad.html
            //// https://hackage.haskell.org/package/base-4.21.0.0/docs/Data-Either.html
            //// https://almarefa.net/blog/how-to-combine-two-different-types-of-lists-in
            //// (look at the `flatten` example [here](https://learn-haskell.blog/06-errors_and_files/01-either.html)
            //// i believe that the linq `join` is some variant of list comprehension in haskell (see the example where `gcd i j == 1` [here](https://wiki.haskell.org/List_comprehension))

            return first
                .Apply(
                    leftFirst =>
                        second
                            .Apply(
                                leftSecond => Fx.Either.Either.Left(leftSelector(leftFirst, leftSecond)).Right<TRightResult>(),
                                rightSecond => Fx.Either.Either.Left<TLeftResult>().Right(leftFirstRightSecondSelector(leftFirst, rightSecond))),
                    rightFirst =>
                        second
                            .Apply(
                                leftSecond => Fx.Either.Either.Left<TLeftResult>().Right(rightFirstLeftSecondSelector(rightFirst, leftSecond)),
                                rightSecond => Fx.Either.Either.Left<TLeftResult>().Right(rightSelector(rightFirst, rightSecond))));
        }
    }

    public abstract class GraphQuery //// TODO graphrequest?
    {
        private GraphQuery()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">Throws any of the exceptions that the <see cref="AsyncVisitor{TResult, TContext}.DispatchAsync"/> overloads can throw</exception> //// TODO is this good?
        protected abstract Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context);

        public abstract class AsyncVisitor<TResult, TContext>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Throws any of the exceptions that the <see cref="DispatchAsync"/> overloads can throw</exception> //// TODO is this good?
            public async Task<TResult> VisitAsync(GraphQuery node, TContext context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return await node.AcceptAsync(this, context).ConfigureAwait(false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            public abstract Task<TResult> DispatchAsync(GraphQuery.Page node, TContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
            /// <exception cref="Exception">Can throw any exception as documented by the derived type</exception>
            internal abstract Task<TResult> DispatchAsync(GraphQuery.GetEvents node, TContext context);
        }

        public sealed class Page : GraphQuery
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="relativeUri"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeUri"/> is <see langword="null"/></exception>
            internal Page(RelativeUri relativeUri)
            {
                if (relativeUri == null)
                {
                    throw new ArgumentNullException(nameof(relativeUri));
                }

                this.RelativeUri = relativeUri;
            }

            internal RelativeUri RelativeUri { get; }

            /// <inheritdoc/>
            protected sealed override async Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return await visitor.DispatchAsync(this, context).ConfigureAwait(false);
            }
        }

        internal sealed class GetEvents : GraphQuery
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="relativeUri"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeUri"/> is <see langword="null"/></exception>
            public GetEvents(RelativeUri relativeUri)
            {
                if (relativeUri == null)
                {
                    throw new ArgumentNullException(nameof(relativeUri));
                }

                this.RelativeUri = relativeUri;
            }

            public RelativeUri RelativeUri { get; }

            /// <inheritdoc/>
            protected sealed override async Task<TResult> AcceptAsync<TResult, TContext>(AsyncVisitor<TResult, TContext> visitor, TContext context)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                return await visitor.DispatchAsync(this, context).ConfigureAwait(false);
            }
        }
    }

    internal sealed class ServiceRoot
    {
        public ServiceRoot(Inners.Scheme scheme, Inners.Host host, uint? port, IEnumerable<Inners.Segment> segments)
        {
            Scheme = scheme;
            Host = host;
            Port = port; //// TODO make this not nullable
            Segments = segments;
        }

        public Inners.Scheme Scheme { get; }
        public Inners.Host Host { get; }
        public uint? Port { get; }
        public IEnumerable<Inners.Segment> Segments { get; }

        public static class Inners
        {
            public abstract class Scheme
            {
                private Scheme()
                {
                }

                public sealed class Https : Scheme
                {
                    private Https()
                    {
                    }

                    public static Https Instance { get; } = new Https();
                }

                public sealed class Http : Scheme
                {
                    private Http()
                    {
                    }

                    public static Http Instance { get; } = new Http();
                }
            }

            public sealed class Host
            {
                public Host(string value)
                {
                    if (value.Contains("/") || value.Contains(":") || value.Contains("?") || value.Contains("#"))
                    {
                        throw new ArgumentException("TODO");
                    }

                    try
                    {
                        new Uri(value);
                    }
                    catch
                    {
                        throw;
                    }

                    Value = value;
                }

                public string Value { get; }
            }

            public sealed class Segment
            {
                public Segment(string value)
                {
                    if (value.Contains("/") || value.Contains("?") || value.Contains("#"))
                    {
                        throw new ArgumentException("TODO");
                    }

                    try
                    {
                        new Uri(value);
                    }
                    catch
                    {
                        throw;
                    }

                    Value = value;
                }

                public string Value { get; }
            }
        }
    }

    public interface IGraphCalendarEventsEvaluator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphQuery"/> is <see langword="null"/></exception>
        /// <exception cref="HttpRequestException">Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout, or the server responded with a payload that was not valid HTTP</exception> //// TODO this part about the invalid HTTP needs to be added to all of your xmldoc where you get an httprequestexception from httpclient
        /// <exception cref="GraphErrorDeserializationException">Thrown if an error occurred while deserializing the error response</exception>
        /// <exception cref="GraphSuccessDeserializationException">Thrown if an error occurred while deserializing success response</exception>
        /// <exception cref="UnauthorizedAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
        /// <exception cref="GraphProcessingException">Thrown if graph encountered an error processing the request</exception>
        Task<GraphCalendarEventsResponse> Evaluate(GraphQuery graphQuery);

        /// <summary>
        /// 
        /// </summary>
        OdataServiceRoot ServiceRoot { get; }

        //// TODO i think what you need to do is have this be `igraphcalendareventssource` and it has a `generate{blank}query` method that returns `igraphcalendareventcontext`s where `{blank}` is like `get` and such; then, the `igraphcalendareventcontext` is going mimic `iquerycontext`, `iqueryable`, etc. and have an `evaluate` method, that way there's no possible inconsistencies across instances of source, evaluator or whatever that you're concerneda bout in the comment below; it would also put all of the "translation" logic from "clr type of graph object to odata representation" in one place (i.e. both the payloads and the urls are in the context implementations)
        //// internal GraphQuery.GetEvents CreateQuery(); //// TODO but what ify ou get a call to `evalaute` with an instance that you didn't create?
    }

    public sealed class GraphCalendarEventsResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <param name="nextPage"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="events"/> or <paramref name="nextPage"/> is <see langword="null"/></exception>
        public GraphCalendarEventsResponse(
            IReadOnlyList<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>> events,
            OdataNextLink nextPage)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (nextPage == null)
            {
                throw new ArgumentNullException(nameof(nextPage));
            }

            this.Events = events;
            this.NextPage = nextPage;
        }

        public IReadOnlyList<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>> Events { get; }

        public OdataNextLink NextPage { get; } //// TODO this adds a direct dependency on odata; is that ok?
    }

    public sealed class GraphCalendarEventsContextTranslationException : Exception
    {
        public GraphCalendarEventsContextTranslationException(string message, string rawEventContents)
            : base(message)
        {
            this.RawEventContents = rawEventContents; ///// tODO make sure you are doing `this.` everywhere
        }

        public GraphCalendarEventsContextTranslationException(string message, Exception innerException, string rawEventContents)
            : base(message, innerException)
        {
            this.RawEventContents = rawEventContents;
        }

        public string RawEventContents { get; }
    }

    public sealed class GraphProcessingException : Exception
    {
        public GraphProcessingException(HttpStatusCode httpStatusCode, IEnumerable<HttpHeader> headers, GraphErrorResponse graphErrorResponse, string message)
            : base(message)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Headers = headers;
            this.GraphErrorResponse = graphErrorResponse;
        }

        public HttpStatusCode HttpStatusCode { get; }
        public IEnumerable<HttpHeader> Headers { get; }
        public GraphErrorResponse GraphErrorResponse { get; }
    }

    public sealed class GraphErrorResponse
    {
        public GraphErrorResponse(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("TODO cannot be empty", nameof(code));
            }

            this.Code = code;
        }

        public string Code { get; }

        //// TODO finish implementing this
    }

    public sealed class GraphErrorDeserializationException : Exception
    {
        public GraphErrorDeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class GraphSuccessDeserializationException : Exception
    {
        public GraphSuccessDeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class GraphCalendarEventsEvaluator : IGraphCalendarEventsEvaluator
    {
        private readonly EvaluateVisitor evaluateVisitor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphOdataContext"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphOdataContext"/> is <see langword="null"/></exception>
        public GraphCalendarEventsEvaluator(IGraphOdataStructuredContext graphOdataContext)
        {
            //// TODO to prevent needing to model all of the odata stuff, this constructor should be internal, and then there should be a public factory method that can give instances by creating its own instance of the *also* internal odata interface types; this factory method can be deprecated if needed once this constructor can be made public; it doesn't *have* to be deprecated, but it might make it easier to maintain as you expose new parameters on the constructors for the implementers of the odata interfaces
            if (graphOdataContext == null)
            {
                throw new ArgumentNullException(nameof(graphOdataContext));
            }

            this.evaluateVisitor = new EvaluateVisitor(graphOdataContext);
            this.ServiceRoot = graphOdataContext.ServiceRoot;
        }

        /// <inheritdoc/>
        public OdataServiceRoot ServiceRoot { get; }

        /// <inheritdoc/>
        public async Task<GraphCalendarEventsResponse> Evaluate(GraphQuery graphQuery)
        {
            if (graphQuery == null)
            {
                throw new ArgumentNullException(nameof(graphQuery));
            }

            try
            {
                return await this.evaluateVisitor.VisitAsync(graphQuery, default).ConfigureAwait(false);
            }
            catch (OdataErrorDeserializationException odataErrorDeserializationException)
            {
                throw new GraphErrorDeserializationException("TODO", odataErrorDeserializationException);
            }
            catch (OdataSuccessDeserializationException odataSuccessDeserializationException)
            {
                throw new GraphSuccessDeserializationException("TODO", odataSuccessDeserializationException);
            }
        }

        private sealed class EvaluateVisitor : GraphQuery.AsyncVisitor<GraphCalendarEventsResponse, Nothing>
        {
            private readonly IGraphOdataStructuredContext graphOdataContext;

            private readonly GetPageVisitor getPageVisitor;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="graphOdataContext"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphOdataContext"/> is <see langword="null"/></exception>
            public EvaluateVisitor(IGraphOdataStructuredContext graphOdataContext)
            {
                if (graphOdataContext == null)
                {
                    throw new ArgumentNullException(nameof(graphOdataContext));
                }

                this.graphOdataContext = graphOdataContext;
                this.getPageVisitor = GetPageVisitor.Instance;
            }

            /// <inheritdoc/>
            /// <exception cref="HttpRequestException">Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout, or the server responded with a payload that was not valid HTTP</exception> //// TODO this part about the invalid HTTP needs to be added to all of your xmldoc where you get an httprequestexception from httpclient
            /// <exception cref="OdataErrorDeserializationException">Thrown if an error occurred while deserializing the OData error response</exception>
            /// <exception cref="OdataSuccessDeserializationException">Thrown if an error occurred while deserializing the OData success response</exception>
            /// <exception cref="UnauthorizedAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
            /// <exception cref="GraphProcessingException">Thrown if graph encountered an error processing the request</exception>
            public sealed override async Task<GraphCalendarEventsResponse> DispatchAsync(GraphQuery.Page node, Nothing context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return await this.GetPage(node.RelativeUri).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            /// <exception cref="HttpRequestException">Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout, or the server responded with a payload that was not valid HTTP</exception> //// TODO this part about the invalid HTTP needs to be added to all of your xmldoc where you get an httprequestexception from httpclient
            /// <exception cref="OdataErrorDeserializationException">Thrown if an error occurred while deserializing the OData error response</exception>
            /// <exception cref="OdataSuccessDeserializationException">Thrown if an error occurred while deserializing the OData success response</exception>
            /// <exception cref="UnauthorizedAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
            /// <exception cref="GraphProcessingException">Thrown if graph encountered an error processing the request</exception>
            internal sealed override async Task<GraphCalendarEventsResponse> DispatchAsync(GraphQuery.GetEvents node, Nothing context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return await this.GetPage(node.RelativeUri).ConfigureAwait(false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="url"/> is <see langword="null"/></exception>
            /// <exception cref="HttpRequestException">Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout, or the server responded with a payload that was not valid HTTP</exception> //// TODO this part about the invalid HTTP needs to be added to all of your xmldoc where you get an httprequestexception from httpclient
            /// <exception cref="OdataErrorDeserializationException">Thrown if an error occurred while deserializing the OData error response</exception>
            /// <exception cref="OdataSuccessDeserializationException">Thrown if an error occurred while deserializing the OData success response</exception>
            /// <exception cref="UnauthorizedAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
            /// <exception cref="GraphProcessingException">Thrown if graph encountered an error processing the request</exception>
            private async Task<GraphCalendarEventsResponse> GetPage(RelativeUri url)
            {
                if (url == null)
                {
                    throw new ArgumentNullException(nameof(url));
                }

                var odataCollectionRequest = new OdataGetCollectionRequest(url, Enumerable.Empty<HttpHeader>());
                var odataCollectionResponse = await this.graphOdataContext.GetCollection(odataCollectionRequest).ConfigureAwait(false);

                return odataCollectionResponse
                    .ResponseContent
                    .Select(
                        left => this.getPageVisitor.Visit(left, default),
                        right => new GraphProcessingException(
                            odataCollectionResponse.HttpStatusCode,
                            odataCollectionResponse.Headers,
                            new GraphErrorResponse(right.Code),
                            $"An error occurred in graph while processing the request to '{url.OriginalString}'."))
                    .ThrowRight();
            }

            private sealed class GetPageVisitor : OdataCollectionResponse.Visitor<GraphCalendarEventsResponse, Nothing>
            {
                private readonly OdataCollectionValueVisitor odataCollectionValueVisitor;

                /// <summary>
                /// 
                /// </summary>
                private GetPageVisitor()
                {
                    this.odataCollectionValueVisitor = OdataCollectionValueVisitor.Instance;
                }

                /// <summary>
                /// 
                /// </summary>
                public static GetPageVisitor Instance { get; } = new GetPageVisitor();

                /// <inheritdoc/>
                public override GraphCalendarEventsResponse Dispatch(OdataCollectionResponse.Values node, Nothing context)
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }

                    var graphCalendarEvents = node
                        .Value
                        .Select(
                            odataCollectionValue => this.odataCollectionValueVisitor
                                .Visit(
                                    odataCollectionValue,
                                    default))
                        .ToList(); //// TODO do you want this to be lazy?

                    return new GraphCalendarEventsResponse(graphCalendarEvents, node.NextLink);
                }

                private sealed class OdataCollectionValueVisitor : OdataCollectionValue.Visitor<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, Nothing>
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    private OdataCollectionValueVisitor()
                    {
                    }

                    /// <summary>
                    /// 
                    /// </summary>
                    public static OdataCollectionValueVisitor Instance { get; } = new OdataCollectionValueVisitor();

                    /// <inheritdoc/>
                    internal sealed override IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException> Dispatch(OdataCollectionValue.Json node, Nothing context)
                    {
                        if (node == null)
                        {
                            throw new ArgumentNullException(nameof(node));
                        }

                        GraphCalendarEventBuilder? graphCalendarEvent;
                        try
                        {
                            graphCalendarEvent = JsonSerializer.Deserialize<GraphCalendarEventBuilder>(node.Node);
                        }
                        catch (JsonException jsonException) //// TODO always write out the exception name
                        {
                            return Either
                                .Left<GraphCalendarEvent>()
                                .Right(new GraphCalendarEventsContextTranslationException(
                                    "An error occurred while translating the OData collection element into a Graph calendar event",
                                    jsonException,
                                    node.Node.ToString()));
                        }

                        if (graphCalendarEvent == null)
                        {
                            // OData allows null element in collections, but we know that graph is not supposed to return null events
                            return Either
                                .Left<GraphCalendarEvent>()
                                .Right(new GraphCalendarEventsContextTranslationException(
                                    "An error occurred while translating the OData collection element into a Graph calendar event",
                                    node.Node.ToString()));
                        }

                        return graphCalendarEvent.Build(node.Node.ToString());
                    }
                }

                private sealed class GraphCalendarEventBuilder
                {
                    [JsonPropertyName("id")]
                    public string? Id { get; set; }

                    [JsonPropertyName("subject")]
                    public string? Subject { get; set; }

                    [JsonPropertyName("start")]
                    public TimeStructureBuilder? Start { get; set; }

                    [JsonPropertyName("body")]
                    public BodyStructureBuilder? Body { get; set; }

                    [JsonPropertyName("isCancelled")]
                    public bool? IsCancelled { get; set; }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="rawEventContents"></param>
                    /// <returns></returns>
                    public IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException> Build(string rawEventContents)
                    {
                        var invalidities = new List<string>();

                        if (this.Id == null)
                        {
                            invalidities.Add($"'{nameof(Id)}' cannot be null");
                        }

                        if (this.Subject == null)
                        {
                            invalidities.Add($"'{nameof(Subject)}' cannot be null");
                        }

                        if (this.Start == null)
                        {
                            invalidities.Add($"'{nameof(Start)}' cannot be null");
                        }

                        if (this.Body == null)
                        {
                            invalidities.Add($"'{nameof(Body)}' cannot be null");
                        }

                        if (this.IsCancelled == null)
                        {
                            invalidities.Add($"'{nameof(IsCancelled)}' cannot be null");
                        }

                        if (invalidities.Count > 0)
                        { 
                            return Either
                                .Left<GraphCalendarEvent>()
                                .Right(new GraphCalendarEventsContextTranslationException(
                                    $"An error occurred while translating the OData collection element into a Graph calendar event: {string.Join(", ", invalidities)}",
                                    rawEventContents));
                        }

                        //// TODO see if you can avoid the bangs in the rest of the method
                        var start = this.Start!.Build(rawEventContents);
                        var body = this.Body!.Build(rawEventContents);

                        return
                            start
                            .Zip<TimeStructure, GraphCalendarEventsContextTranslationException, BodyStructure, GraphCalendarEventsContextTranslationException, (TimeStructure, BodyStructure), GraphCalendarEventsContextTranslationException>(
                                body,
                                (left, right) => (left, right),
                                (startSuccess, bodyError) => bodyError, //// TODO incorporate the known start success information
                                (startError, bodySuccess) => startError,  //// TODO incorporate the known body success information
                                (startError, bodyError) => new GraphCalendarEventsContextTranslationException(
                                    $"{startError.Message}. {bodyError.Message}.", //// TODO the period here is why every exception message should use proper punctuation
                                    rawEventContents))
                            .Select(
                                left => new GraphCalendarEvent(
                                    this.Id!,
                                    this.Subject!,
                                    left.Item1,
                                    left.Item2,
                                    this.IsCancelled!.Value),
                                right => right);
                    }
                }

                public sealed class BodyStructureBuilder
                {
                    [JsonPropertyName("context")]
                    public string? Content { get; set; }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public IEither<BodyStructure, GraphCalendarEventsContextTranslationException> Build(string rawEventContents)
                    {
                        var invalidities = new List<string>();

                        if (this.Content == null)
                        {
                            invalidities.Add($"'{nameof(Content)}' cannot be null");
                        }

                        if (invalidities.Count > 0)
                        {
                            return Either
                                .Left<BodyStructure>()
                                .Right(new GraphCalendarEventsContextTranslationException(
                                    $"An error occurred while translating the OData collection element into the '{nameof(GraphCalendarEventBuilder.Body)}' portion of a Graph calendar event: {string.Join(", ", invalidities)}",
                                    rawEventContents));
                        }

                        return Either
                            .Left(new BodyStructure(
                                this.Content!)) //// TODO see if you can avoid the bang
                            .Right<GraphCalendarEventsContextTranslationException>();
                    }
                }

                public sealed class TimeStructureBuilder
                {
                    [JsonPropertyName("dateTime")]
                    public string? DateTime { get; set; }

                    [JsonPropertyName("timeZone")]
                    public string? TimeZone { get; set; }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public IEither<TimeStructure, GraphCalendarEventsContextTranslationException> Build(string rawEventContents)
                    {
                        var invalidities = new List<string>();

                        if (this.DateTime == null)
                        {
                            invalidities.Add($"'{nameof(DateTime)}' cannot be null");
                        }

                        if (this.TimeZone == null)
                        {
                            invalidities.Add($"'{nameof(TimeZone)}' cannot be null");
                        }

                        if (invalidities.Count > 0)
                        {
                            return Either
                                .Left<TimeStructure>()
                                .Right(new GraphCalendarEventsContextTranslationException(
                                    $"An error occurred while translating the OData collection element into the '{nameof(GraphCalendarEventBuilder.Start)}' portion of a Graph calendar event: {string.Join(", ", invalidities)}",
                                    rawEventContents));
                        }

                        //// TODO see if you can avoid the bangs in the rest of this methods
                        return Either
                            .Left(new TimeStructure(
                                this.DateTime!,
                                this.TimeZone!))
                            .Right<GraphCalendarEventsContextTranslationException>();
                    }
                }
            }
        }
    }

    public sealed class GraphCalendarEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <param name="start"></param>
        /// <param name="body"></param>
        /// <param name="isCancelled"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> or <paramref name="subject"/> or <paramref name="start"/> or <paramref name="body"/> is <see langword="null"/></exception>
        public GraphCalendarEvent(string id, string subject, TimeStructure start, BodyStructure body, bool isCancelled)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            this.Id = id;
            this.Subject = subject;
            this.Start = start;
            this.Body = body;
            this.IsCancelled = isCancelled;
        }

        public string Id { get; set; }

        public string Subject { get; set; }

        public TimeStructure Start { get; set; }

        public BodyStructure Body { get; set; }

        public bool IsCancelled { get; set; }
    }

    public sealed class BodyStructure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> is <see langword="null"/></exception>
        public BodyStructure(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.Content = content;
        }

        public string Content { get; set; }
    }

    public sealed class TimeStructure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZone"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dateTime"/> or <paramref name="timeZone"/> is <see langword="null"/></exception>
        public TimeStructure(string dateTime, string timeZone)
        {
            if (dateTime == null)
            {
                throw new ArgumentNullException(nameof(dateTime));
            }

            if (timeZone == null)
            {
                throw new ArgumentNullException(nameof(timeZone));
            }

            this.DateTime = dateTime;
            this.TimeZone = timeZone;
        }

        public string DateTime { get; set; }

        public string TimeZone { get; set; }
    }

    public sealed class GraphPagingException : Exception
    {
        public GraphPagingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class GraphCalendarEventsContextExtensions
    {
        private sealed class PageQueryResult : QueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>.Element
        {
            private readonly GraphCalendarEventsResponse graphCalendarEventsResponse;
            private readonly int index;
            private readonly OdataNextLinkVisitor odataNextLinkVisitor;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="graphCalendarEventsResponse"></param>
            /// <param name="index"></param>
            /// <param name="odataNextLinkVisitor"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEventsResponse"/> or <paramref name="odataNextLinkVisitor"/> is <see langword="null"/></exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative</exception>
            public PageQueryResult(GraphCalendarEventsResponse graphCalendarEventsResponse, int index, OdataNextLinkVisitor odataNextLinkVisitor)
                : base(graphCalendarEventsResponse.Events[index])
            {
                if (graphCalendarEventsResponse == null)
                {
                    throw new ArgumentNullException(nameof(graphCalendarEventsResponse));
                }

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"'{nameof(index)}' must not be a negative value. The provided value was '{index}'.");
                }

                if (odataNextLinkVisitor == null)
                {
                    throw new ArgumentNullException(nameof(odataNextLinkVisitor));
                }

                this.graphCalendarEventsResponse = graphCalendarEventsResponse;
                this.index = index;
                this.odataNextLinkVisitor = odataNextLinkVisitor;
            }

            /// <inheritdoc/>
            public override Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException> Next()
            {
                if (this.index + 1 < this.graphCalendarEventsResponse.Events.Count)
                {
                    return new PageQueryResult(this.graphCalendarEventsResponse, this.index + 1, this.odataNextLinkVisitor);
                }

                //// TODO figure out async `queryresult`s
                return this.odataNextLinkVisitor.VisitAsync(this.graphCalendarEventsResponse.NextPage, default).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCalendarEventsContext"></param>
        /// <param name="graphQuery">TODO this allows <paramref name="graphQuery"/> to be a paging query; do you want to protect against that for some reason?</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEventsContext"/> or <paramref name="graphQuery"/> is <see langword="null"</exception>
        public static async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> Page(
            this IGraphCalendarEventsEvaluator graphCalendarEventsContext,
            GraphQuery graphQuery)
        {
            if (graphCalendarEventsContext == null)
            {
                throw new ArgumentNullException(nameof(graphCalendarEventsContext));
            }

            if (graphQuery == null)
            {
                throw new ArgumentNullException(nameof(graphQuery));
            }
            
            return await graphCalendarEventsContext.Page(graphQuery, page => graphCalendarEventsContext).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCalendarEventsContext"></param>
        /// <param name="graphQuery">TODO this allows <paramref name="graphQuery"/> to be a paging query; do you want to protect against that for some reason?</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEventsContext"/> or <paramref name="graphQuery"/> or <paramref name="contextGenerator"/> is <see langword="null"/></exception>
        public static async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> Page(
            this IGraphCalendarEventsEvaluator graphCalendarEventsContext,
            GraphQuery graphQuery,
            Func<OdataNextLink.Absolute, IGraphCalendarEventsEvaluator> contextGenerator)
        {
            if (graphCalendarEventsContext == null)
            {
                throw new ArgumentNullException(nameof(graphCalendarEventsContext));
            }

            if (graphQuery == null)
            {
                throw new ArgumentNullException(nameof(graphQuery));
            }

            if (contextGenerator == null)
            {
                throw new ArgumentNullException(nameof(contextGenerator));
            }

            return await Page(
                graphCalendarEventsContext,
                graphQuery,
                new OdataNextLinkVisitor(graphCalendarEventsContext, contextGenerator)).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCalendarEventsContext"></param>
        /// <param name="graphQuery">TODO this allows <paramref name="graphQuery"/> to be a paging query; do you want to protect against that for some reason?</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEventsContext"/> or <paramref name="graphQuery"/> or <paramref name="odataNextLinkVisitor"/> is <see langword="null"</exception>
        private static async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> Page(
            this IGraphCalendarEventsEvaluator graphCalendarEventsContext,
            GraphQuery graphQuery,
            OdataNextLinkVisitor odataNextLinkVisitor)
        {
            if (graphCalendarEventsContext == null)
            {
                throw new ArgumentNullException(nameof(graphCalendarEventsContext));
            }

            if (graphQuery == null)
            {
                throw new ArgumentNullException(nameof(graphQuery));
            }

            if (odataNextLinkVisitor == null)
            {
                throw new ArgumentNullException(nameof(odataNextLinkVisitor));
            }

            GraphCalendarEventsResponse response;
            try
            {
                response = await graphCalendarEventsContext.Evaluate(graphQuery).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is HttpRequestException or GraphErrorDeserializationException or GraphSuccessDeserializationException or UnauthorizedAccessException or GraphProcessingException)
            {
                return new PageQueryResult2(
                    Either
                        .Left<IElement<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>>()
                        .Right(
                            Either
                                .Left(new PageError(new GraphPagingException("TODO", exception)))
                                .Right<IEmpty>())
                        .ToQueryResultNode()); //// TODO consolidating into one exception type makes it pretty difficult for a caller to be able to write error handling code...
            }

            if (response.Events.Count == 0)
            {
                //// TODO this means that we won't return until we find the first non-empty page; is that really expected?
                return await odataNextLinkVisitor.VisitAsync(response.NextPage, default).ConfigureAwait(false);
            }

            return new PageQueryResult(response, 0, odataNextLinkVisitor);
        }

        private sealed class PageError : IError<GraphPagingException>
        {
            public PageError(GraphPagingException value)
            {
                Value = value;
            }

            public GraphPagingException Value { get; }
        }

        /// <summary>
        /// TODO should you have a shared type for the query result concrete implementation?
        /// </summary>
        private sealed class PageQueryResult2 : IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>
        {
            public PageQueryResult2(IQueryResultNode<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException> nodes)
            {
                this.Nodes = nodes;
            }

            public IQueryResultNode<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException> Nodes { get; }
        }

        private sealed class OdataNextLinkVisitor : OdataNextLink.Visitor<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>, Nothing>
        {
            private readonly IGraphCalendarEventsEvaluator graphCalendarEventsContext;
            private readonly Func<OdataNextLink.Absolute, IGraphCalendarEventsEvaluator> contextGenerator;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="graphCalendarEventsContext"></param>
            /// <param name="contextGenerator"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphCalendarEventsContext"/> or <paramref name="contextGenerator"/> is <see langword="null"/></exception>
            public OdataNextLinkVisitor(
                IGraphCalendarEventsEvaluator graphCalendarEventsContext, 
                Func<OdataNextLink.Absolute, IGraphCalendarEventsEvaluator> contextGenerator)
            {
                if (graphCalendarEventsContext == null)
                {
                    throw new ArgumentNullException(nameof(graphCalendarEventsContext));
                }

                if (contextGenerator == null)
                {
                    throw new ArgumentNullException(nameof(contextGenerator));
                }

                this.graphCalendarEventsContext = graphCalendarEventsContext;
                this.contextGenerator = contextGenerator;
            }

            /// <inheritdoc/>
            protected internal override Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> AcceptAsync(OdataNextLink.Null node, Nothing context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return Task.FromResult<IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>>(EmptyQueryResult.Instance);
            }

            private sealed class EmptyQueryResult : IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>
            {
                private EmptyQueryResult()
                {
                    this.Nodes = Either
                    .Left<IElement<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>>()
                    .Right(
                        Either
                            .Left<IError<GraphPagingException>>()
                            .Right(Empty.Instance))
                    .ToQueryResultNode();
                }

                public static EmptyQueryResult Instance { get; } = new EmptyQueryResult();

                public IQueryResultNode<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException> Nodes { get; }
            }

            /// <summary>
            /// TODO should this be a shared type?
            /// </summary>
            private sealed class Empty : Fx.QueryContext.IEmpty
            {
                private Empty()
                {
                }

                public static Empty Instance { get; } = new Empty();
            }

            /// <inheritdoc/>
            protected internal override async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> AcceptAsync(OdataNextLink.Relative node, Nothing context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return await this
                    .graphCalendarEventsContext
                    .Page(
                        new GraphQuery.Page(
                                node.ToRelativeUri()), 
                        this)
                    .ConfigureAwait(false);
            }

            /// <inheritdoc/>
            protected internal override async Task<Fx.QueryContext.IQueryResult<IEither<GraphCalendarEvent, GraphCalendarEventsContextTranslationException>, GraphPagingException>> AcceptAsync(OdataNextLink.Absolute node, Nothing context)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                var nextContext = this.contextGenerator(node);
                return await nextContext
                    .Page(
                        new GraphQuery.Page(
                            nextContext
                                .ServiceRoot
                                .GetUri(
                                    node)),
                        this)
                    .ConfigureAwait(false);
            }
        }
    }
}
