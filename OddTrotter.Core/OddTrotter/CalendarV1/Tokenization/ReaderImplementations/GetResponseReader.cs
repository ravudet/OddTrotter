namespace OddTrotter.CalendarV1.Tokenization.ReaderImplementations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fx.Either;

    using OddTrotter.CalendarV1.Tokenization.Readers;

    public sealed class GetResponseReader : IGetResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        private readonly IDispositionManager dispositionManager;

        public GetResponseReader(HttpResponseMessage httpResponseMessage, IDispositionManager dispositionManager)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.dispositionManager = dispositionManager;
        }

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
        }

        public IGetResponseHeadersReader TryMoveNext(out bool moved)
        {
            moved = true;

            var headersEnumerator = this.dispositionManager.Register(() => httpResponseMessage.Headers.GetEnumerator());
            return new GetResponseHeadersReader(this.httpResponseMessage, headersEnumerator, this.dispositionManager);
        }

        private sealed class GetResponseHeadersReader : IGetResponseHeadersReader
        {
            private readonly HttpResponseMessage httpResponseMessage;

            private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
            private readonly IDispositionManager dispositionManager;

            public GetResponseHeadersReader(
                HttpResponseMessage httpResponseMessage, 
                IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                IDispositionManager dispositionManager)
            {
                this.httpResponseMessage = httpResponseMessage;
                this.headersEnumerator = headersEnumerator;
                this.dispositionManager = dispositionManager;
            }

            public ValueTask Read()
            {
                return ValueTask.CompletedTask;
            }

            public IGetResponseHeadersToken TryMoveNext(out bool moved)
            {
                moved = true;
                if (this.headersEnumerator.MoveNext())
                {
                    return new GetResponseHeadersToken.GetResponseHeader(new GetResponseHeaderReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager));
                }
                else
                {

                }
            }

            private abstract class GetResponseHeadersToken : IGetResponseHeadersToken
            {
                private GetResponseHeadersToken()
                {
                }

                public TResult Apply<TResult>(Func<IGetResponseHeaderReader, TResult> getResponseHeaderReader, Func<IGetResponseBodyReader, TResult> getResponseBodyReader)
                {
                    return new DelegateVisitor<TResult, Nothing>(getResponseHeaderReader, getResponseBodyReader).Visit(this, new Nothing());
                }

                private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
                {
                    private readonly Func<IGetResponseHeaderReader, TResult> getResponseHeaderReader;
                    private readonly Func<IGetResponseBodyReader, TResult> getResponseBodyReader;

                    public DelegateVisitor(Func<IGetResponseHeaderReader, TResult> getResponseHeaderReader, Func<IGetResponseBodyReader, TResult> getResponseBodyReader)
                    {
                        this.getResponseHeaderReader = getResponseHeaderReader;
                        this.getResponseBodyReader = getResponseBodyReader;
                    }

                    protected internal override TResult Accept(GetResponseHeader node, TContext context)
                    {
                        return this.getResponseHeaderReader(node.GetResponseHeaderReader);
                    }

                    protected internal override TResult Accept(GetResponseBody node, TContext context)
                    {
                        return this.getResponseBodyReader(node.GetResponseBodyReader);
                    }
                }

                protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

                public abstract class Visitor<TResult, TContext>
                {
                    /// <summary>
                    /// placeholder
                    /// </summary>
                    /// <param name="node"></param>
                    /// <param name="context"></param>
                    /// <returns></returns>
                    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                    /// <exception cref="LeftMapException">
                    /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Left"/> node
                    /// </exception>
                    /// <exception cref="RightMapException">
                    /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Right"/> node
                    /// </exception>
                    public TResult Visit(GetResponseHeadersToken node, TContext context)
                    {
                        ArgumentNullException.ThrowIfNull(node);

                        return node.Dispatch(this, context);
                    }

                    /// <summary>
                    /// placeholder
                    /// </summary>
                    /// <param name="node"></param>
                    /// <param name="context"></param>
                    /// <returns></returns>
                    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                    /// <exception cref="LeftMapException">
                    /// Thrown if an error occurred while processing <paramref name="node"/>
                    /// </exception>
                    protected internal abstract TResult Accept(GetResponseHeader node, TContext context);

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="node"></param>
                    /// <param name="context"></param>
                    /// <returns></returns>
                    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                    /// <exception cref="RightMapException">
                    /// Thrown if an error occurred while processing <paramref name="node"/>
                    /// </exception>
                    protected internal abstract TResult Accept(GetResponseBody node, TContext context);
                }

                public sealed class GetResponseHeader : GetResponseHeadersToken
                {
                    public GetResponseHeader(IGetResponseHeaderReader getResponseHeaderReader)
                    {
                        GetResponseHeaderReader = getResponseHeaderReader;
                    }

                    public IGetResponseHeaderReader GetResponseHeaderReader { get; }

                    protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                    {
                        return visitor.Accept(this, context);
                    }
                }

                public sealed class GetResponseBody : GetResponseHeadersToken
                {
                    public GetResponseBody(IGetResponseBodyReader getResponseBodyReader)
                    {
                        GetResponseBodyReader = getResponseBodyReader;
                    }

                    public IGetResponseBodyReader GetResponseBodyReader { get; }

                    protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                    {
                        return visitor.Accept(this, context);
                    }
                }
            }

            private sealed class GetResponseHeaderReader : IGetResponseHeaderReader
            {
                private readonly HttpResponseMessage httpResponseMessage;
                private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                private readonly IDispositionManager dispositionManager;

                public GetResponseHeaderReader(
                    HttpResponseMessage httpResponseMessage, 
                    IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                    IDispositionManager dispositionManager)
                {
                    this.httpResponseMessage = httpResponseMessage;
                    this.headersEnumerator = headersEnumerator;
                    this.dispositionManager = dispositionManager;
                }

                public ValueTask Read()
                {
                    return ValueTask.CompletedTask;
                }

                public IGetResponseHeaderToken TryMoveNext(out bool moved)
                {
                    moved = true;
                    return new GetResponseHeaderToken.CustomHeader(new CustomHeaderReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager));
                }

                private abstract class GetResponseHeaderToken : IGetResponseHeaderToken
                {
                    private GetResponseHeaderToken()
                    {
                    }

                    public TResult Apply<TResult>(Func<ICustomHeaderReader<IGetResponseHeadersReader>, TResult> customHeaderReader)
                    {
                        return new DelegateVisitor<TResult, Nothing>(customHeaderReader).Visit(this, new Nothing());
                    }

                    private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
                    {
                        private readonly Func<ICustomHeaderReader<IGetResponseHeadersReader>, TResult> customHeaderReader;

                        public DelegateVisitor(Func<ICustomHeaderReader<IGetResponseHeadersReader>, TResult> customHeaderReader)
                        {
                            this.customHeaderReader = customHeaderReader;
                        }

                        protected internal override TResult Accept(CustomHeader node, TContext context)
                        {
                            return this.customHeaderReader(node.CustomHeaderReader);
                        }
                    }

                    protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

                    public abstract class Visitor<TResult, TContext>
                    {
                        /// <summary>
                        /// placeholder
                        /// </summary>
                        /// <param name="node"></param>
                        /// <param name="context"></param>
                        /// <returns></returns>
                        /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                        /// <exception cref="LeftMapException">
                        /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Left"/> node
                        /// </exception>
                        /// <exception cref="RightMapException">
                        /// Thrown if an error occurred while processing an <see cref="Either{TLeft, TRight}.Right"/> node
                        /// </exception>
                        public TResult Visit(GetResponseHeaderToken node, TContext context)
                        {
                            ArgumentNullException.ThrowIfNull(node);

                            return node.Dispatch(this, context);
                        }

                        /// <summary>
                        /// placeholder
                        /// </summary>
                        /// <param name="node"></param>
                        /// <param name="context"></param>
                        /// <returns></returns>
                        /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <see langword="null"/></exception>
                        /// <exception cref="LeftMapException">
                        /// Thrown if an error occurred while processing <paramref name="node"/>
                        /// </exception>
                        protected internal abstract TResult Accept(CustomHeader node, TContext context);
                    }

                    public sealed class CustomHeader : GetResponseHeaderToken
                    {
                        public CustomHeader(ICustomHeaderReader<IGetResponseHeadersReader> customHeaderReader)
                        {
                            CustomHeaderReader = customHeaderReader;
                        }

                        public ICustomHeaderReader<IGetResponseHeadersReader> CustomHeaderReader { get; }

                        protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                        {
                            return visitor.Accept(this, context);
                        }
                    }
                }

                private sealed class CustomHeaderReader : ICustomHeaderReader<IGetResponseHeadersReader>
                {
                    private readonly HttpResponseMessage httpResponseMessage;
                    private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                    private readonly IDispositionManager dispositionManager;

                    public CustomHeaderReader(
                        HttpResponseMessage httpResponseMessage,
                        IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                        IDispositionManager dispositionManager)
                    {
                        this.httpResponseMessage = httpResponseMessage;
                        this.headersEnumerator = headersEnumerator;
                        this.dispositionManager = dispositionManager;
                    }

                    public ValueTask Read()
                    {
                        return ValueTask.CompletedTask;
                    }

                    public ICustomHeaderFieldNameReader<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                    {
                        moved = true;
                        return new CustomHeaderFieldNameReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager);
                    }

                    private sealed class CustomHeaderFieldNameReader : ICustomHeaderFieldNameReader<IGetResponseHeadersReader>
                    {
                        private readonly HttpResponseMessage httpResponseMessage;
                        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                        private readonly IDispositionManager dispositionManager;

                        public CustomHeaderFieldNameReader(
                            HttpResponseMessage httpResponseMessage, 
                            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                            IDispositionManager dispositionManager)
                        {
                            this.httpResponseMessage = httpResponseMessage;
                            this.headersEnumerator = headersEnumerator;
                            this.dispositionManager = dispositionManager;
                        }

                        public ValueTask Read()
                        {
                            return ValueTask.CompletedTask;
                        }

                        public CustomHeaderFieldName TryGetValue(out bool moved)
                        {
                            moved = true;
                            return new CustomHeaderFieldName(headersEnumerator.Current.Key);
                        }

                        public ICustomHeaderFieldValueReader<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                        {
                            moved = true;
                            return new CustomHeaderFieldValueReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager);
                        }

                        private sealed class CustomHeaderFieldValueReader : ICustomHeaderFieldValueReader<IGetResponseHeadersReader>
                        {
                            private readonly HttpResponseMessage httpResponseMessage;
                            private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                            private readonly IDispositionManager dispositionManager;

                            public CustomHeaderFieldValueReader(
                                HttpResponseMessage httpResponseMessage, 
                                IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                                IDispositionManager dispositionManager)
                            {
                                this.httpResponseMessage = httpResponseMessage;
                                this.headersEnumerator = headersEnumerator;
                                this.dispositionManager = dispositionManager;
                            }

                            public ValueTask Read()
                            {
                                return ValueTask.CompletedTask;
                            }

                            public ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                            {
                                var headerValuesEnumerator = this.dispositionManager.Register(() => this.headersEnumerator.Current.Value.GetEnumerator());

                                moved = true;
                                return new CustomHeaderFieldValueElementReader(
                                    this.httpResponseMessage,
                                    this.headersEnumerator,
                                    headerValuesEnumerator,
                                    this.dispositionManager);
                            }

                            private sealed class CustomHeaderFieldValueElementReader : ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;
                                private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                                private readonly IEnumerator<string> headerValuesEnumerator;
                                private readonly IDispositionManager dispositionManager;

                                public CustomHeaderFieldValueElementReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                                    IEnumerator<string> headerValuesEnumerator,
                                    IDispositionManager dispositionManager)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.headersEnumerator = headersEnumerator;
                                    this.headerValuesEnumerator = headerValuesEnumerator;
                                    this.dispositionManager = dispositionManager;
                                }

                                public ValueTask Read()
                                {
                                    return ValueTask.CompletedTask;
                                }

                                public ICustomHeaderFieldValueElementToken<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                                {
                                    //// TODO you are here
                                    //// implement this method
                                    
                                    if (!this.headerValuesEnumerator.MoveNext())
                                    {
                                        this.dispositionManager.Unregister(this.headerValuesEnumerator);

                                        moved = true;
                                        return new CustomHeaderFieldValueElementToken.GetResponseHeaders(new GetResponseHeadersReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager));
                                    }

                                    if (char.IsWhiteSpace(this.headerValuesEnumerator.Current[0]))
                                    {
                                        // lws
                                    }
                                    else
                                    {
                                        // content
                                    }
                                }

                                private abstract class CustomHeaderFieldValueElementToken : ICustomHeaderFieldValueElementToken<IGetResponseHeadersReader>
                                {
                                    private CustomHeaderFieldValueElementToken()
                                    {
                                    }

                                    public TResult Apply<TResult>(
                                        Func<ICustomHeaderFieldContentReader<IGetResponseHeadersReader>, TResult> customHeaderFieldContentReader,
                                        Func<ICustomHeaderLwsReader<IGetResponseHeadersReader>, TResult> customHeaderLwsReader, 
                                        Func<IGetResponseHeadersReader, TResult> nextReader)
                                    {
                                        return new DelegateVisitor<TResult, Nothing>(
                                            customHeaderFieldContentReader, 
                                            customHeaderLwsReader, 
                                            nextReader).Visit(this, new Nothing());
                                    }

                                    private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
                                    {
                                        private readonly Func<ICustomHeaderFieldContentReader<IGetResponseHeadersReader>, TResult> customHeaderFieldContentReader;
                                        private readonly Func<ICustomHeaderLwsReader<IGetResponseHeadersReader>, TResult> customHeaderLwsReader;
                                        private readonly Func<IGetResponseHeadersReader, TResult> nextReader;

                                        public DelegateVisitor(
                                            Func<ICustomHeaderFieldContentReader<IGetResponseHeadersReader>, TResult> customHeaderFieldContentReader, 
                                            Func<ICustomHeaderLwsReader<IGetResponseHeadersReader>, TResult> customHeaderLwsReader,
                                            Func<IGetResponseHeadersReader, TResult> nextReader)
                                        {
                                            this.customHeaderFieldContentReader = customHeaderFieldContentReader;
                                            this.customHeaderLwsReader = customHeaderLwsReader;
                                            this.nextReader = nextReader;
                                        }

                                        protected internal override TResult Accept(CustomHeaderFieldContent node, TContext context)
                                        {
                                            return this.customHeaderFieldContentReader(node.CustomHeaderFieldContentReader);
                                        }

                                        protected internal override TResult Accept(CustomHeaderLws node, TContext context)
                                        {
                                            return this.customHeaderLwsReader(node.CustomHeaderLwsReader);
                                        }

                                        protected internal override TResult Accept(GetResponseHeaders node, TContext context)
                                        {
                                            return this.nextReader(node.GetResponseHeadersReader);
                                        }
                                    }

                                    protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

                                    public abstract class Visitor<TResult, TContext>
                                    {
                                        public TResult Visit(CustomHeaderFieldValueElementToken node, TContext context)
                                        {
                                            ArgumentNullException.ThrowIfNull(node);

                                            return node.Dispatch(this, context);
                                        }

                                        protected internal abstract TResult Accept(CustomHeaderFieldContent node, TContext context);
                                        protected internal abstract TResult Accept(CustomHeaderLws node, TContext context);
                                        protected internal abstract TResult Accept(GetResponseHeaders node, TContext context);
                                    }

                                    public sealed class CustomHeaderFieldContent : CustomHeaderFieldValueElementToken
                                    {
                                        public CustomHeaderFieldContent(ICustomHeaderFieldContentReader<IGetResponseHeadersReader> customHeaderFieldContentReader)
                                        {
                                            CustomHeaderFieldContentReader = customHeaderFieldContentReader;
                                        }

                                        public ICustomHeaderFieldContentReader<IGetResponseHeadersReader> CustomHeaderFieldContentReader { get; }

                                        protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                                        {
                                            return visitor.Accept(this, context);
                                        }
                                    }

                                    public sealed class CustomHeaderLws : CustomHeaderFieldValueElementToken
                                    {
                                        public CustomHeaderLws(ICustomHeaderLwsReader<IGetResponseHeadersReader> customHeaderLwsReader)
                                        {
                                            CustomHeaderLwsReader = customHeaderLwsReader;
                                        }

                                        public ICustomHeaderLwsReader<IGetResponseHeadersReader> CustomHeaderLwsReader { get; }

                                        protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                                        {
                                            return visitor.Accept(this, context);
                                        }
                                    }

                                    public sealed class GetResponseHeaders : CustomHeaderFieldValueElementToken
                                    {
                                        public GetResponseHeaders(IGetResponseHeadersReader getResponseHeadersReader)
                                        {
                                            GetResponseHeadersReader = getResponseHeadersReader;
                                        }

                                        public IGetResponseHeadersReader GetResponseHeadersReader { get; }

                                        protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                                        {
                                            return visitor.Accept(this, context);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private sealed class GetResponseBodyReader : IGetResponseBodyReader
            {
                public ValueTask Read()
                {
                    throw new NotImplementedException();
                }

                public IOdataContextReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
