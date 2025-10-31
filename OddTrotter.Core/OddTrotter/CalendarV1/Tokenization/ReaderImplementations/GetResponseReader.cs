namespace OddTrotter.CalendarV1.Tokenization.ReaderImplementations
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Formats.Asn1;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices.JavaScript;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Fx.Either;

    using OddTrotter.CalendarV1.Tokenization.Readers;

    using Stash;


    //// TODO when do you give the caller the http status code?


    public sealed class GetResponseReader : IGetResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        private readonly IDispositionManager dispositionManager;

        public GetResponseReader(
            HttpResponseMessage httpResponseMessage, 
            IDispositionManager dispositionManager)
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
                    return new GetResponseHeadersToken.GetResponseBody(new GetResponseBodyReader(this.httpResponseMessage, this.dispositionManager));
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

                            public ICustomerHeaderFieldValueToken<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                            {
                                var headerValuesEnumerator = this.dispositionManager.Register(() => this.headersEnumerator.Current.Value.GetEnumerator());

                                if (headerValuesEnumerator.MoveNext())
                                {
                                    moved = true;
                                    return new CustomHeaderFieldValueToken.CustomHeaderFieldValueElement(new CustomHeaderFieldValueElementReader(this.httpResponseMessage, this.headersEnumerator, headerValuesEnumerator, 0, this.dispositionManager));
                                }
                                else
                                {
                                    this.dispositionManager.Unregister(headerValuesEnumerator);

                                    moved = true;
                                    return new CustomHeaderFieldValueToken.GetResponseHeaders(new GetResponseHeadersReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager));
                                }
                            }

                            private abstract class CustomHeaderFieldValueToken : ICustomerHeaderFieldValueToken<IGetResponseHeadersReader>
                            {
                                private CustomHeaderFieldValueToken()
                                {
                                }

                                public TResult Apply<TResult>(Func<ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader>, TResult> customerHeaderFieldValueElementReader, Func<IGetResponseHeadersReader, TResult> nextReader)
                                {
                                    return new DelegateVisitor<TResult, Nothing>(customerHeaderFieldValueElementReader, nextReader).Visit(this, new Nothing());
                                }

                                private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
                                {
                                    private readonly Func<ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader>, TResult> customerHeaderFieldValueElementReader;
                                    private readonly Func<IGetResponseHeadersReader, TResult> nextReader;

                                    public DelegateVisitor(Func<ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader>, TResult> customerHeaderFieldValueElementReader, Func<IGetResponseHeadersReader, TResult> nextReader)
                                    {
                                        this.customerHeaderFieldValueElementReader = customerHeaderFieldValueElementReader;
                                        this.nextReader = nextReader;
                                    }

                                    protected internal override TResult Accept(CustomHeaderFieldValueElement node, TContext context)
                                    {
                                        return this.customerHeaderFieldValueElementReader(node.CustomHeaderFieldValueElementReader);
                                    }

                                    protected internal override TResult Accept(GetResponseHeaders node, TContext context)
                                    {
                                        return this.nextReader(node.GetResponseHeadersReader);
                                    }
                                }

                                protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

                                public abstract class Visitor<TResult, TContext>
                                {
                                    public TResult Visit(CustomHeaderFieldValueToken node, TContext context)
                                    {
                                        ArgumentNullException.ThrowIfNull(node);

                                        return node.Dispatch(this, context);
                                    }

                                    protected internal abstract TResult Accept(CustomHeaderFieldValueElement node, TContext context);
                                    protected internal abstract TResult Accept(GetResponseHeaders node, TContext context);
                                }

                                public sealed class CustomHeaderFieldValueElement : CustomHeaderFieldValueToken
                                {
                                    public CustomHeaderFieldValueElement(ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader> customHeaderFieldValueElementReader)
                                    {
                                        CustomHeaderFieldValueElementReader = customHeaderFieldValueElementReader;
                                    }

                                    public ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader> CustomHeaderFieldValueElementReader { get; }

                                    protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                                    {
                                        return visitor.Accept(this, context);
                                    }
                                }

                                public sealed class GetResponseHeaders: CustomHeaderFieldValueToken
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
                            
                            private sealed class CustomHeaderFieldValueElementReader : ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;
                                private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                                private readonly IEnumerator<string> headerValuesEnumerator;
                                private readonly int currentHeaderValueIndex;
                                private readonly IDispositionManager dispositionManager;

                                public CustomHeaderFieldValueElementReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                                    IEnumerator<string> headerValuesEnumerator,
                                    int currentHeaderValueIndex,
                                    IDispositionManager dispositionManager)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.headersEnumerator = headersEnumerator;
                                    this.headerValuesEnumerator = headerValuesEnumerator;
                                    this.currentHeaderValueIndex = currentHeaderValueIndex;
                                    this.dispositionManager = dispositionManager;
                                }

                                public ValueTask Read()
                                {
                                    return ValueTask.CompletedTask;
                                }

                                public ICustomHeaderFieldValueElementToken<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                                {
                                    if (this.currentHeaderValueIndex >= this.headerValuesEnumerator.Current.Length)
                                    {
                                        this.dispositionManager.Unregister(this.headerValuesEnumerator);

                                        moved = true;
                                        return new CustomHeaderFieldValueElementToken.GetResponseHeaders(new GetResponseHeadersReader(this.httpResponseMessage, this.headersEnumerator, this.dispositionManager));
                                    }

                                    if (char.IsWhiteSpace(this.headerValuesEnumerator.Current[this.currentHeaderValueIndex]))
                                    {
                                        moved = true;
                                        return new CustomHeaderFieldValueElementToken.CustomHeaderLws(new CustomHeaderLwsReader(this.httpResponseMessage, this.headersEnumerator, this.headerValuesEnumerator, this.currentHeaderValueIndex, this.dispositionManager));
                                    }
                                    else
                                    {
                                        moved = true;
                                        return new CustomHeaderFieldValueElementToken.CustomHeaderFieldContent(new CustomHeaderFieldContentReader(this.httpResponseMessage, this.headersEnumerator, this.headerValuesEnumerator, this.currentHeaderValueIndex, this.dispositionManager));
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

                                private sealed class CustomHeaderLwsReader : ICustomHeaderLwsReader<IGetResponseHeadersReader>
                                {
                                    private readonly HttpResponseMessage httpResponseMessage;
                                    private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                                    private readonly IEnumerator<string> headerValuesEnumerator;
                                    private readonly int currentHeaderValueIndex;
                                    private readonly IDispositionManager dispositionManager;

                                    public CustomHeaderLwsReader(
                                        HttpResponseMessage httpResponseMessage,
                                        IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                                        IEnumerator<string> headerValuesEnumerator,
                                        int currentHeaderValueIndex,
                                        IDispositionManager dispositionManager)
                                    {
                                        this.httpResponseMessage = httpResponseMessage;
                                        this.headersEnumerator = headersEnumerator;
                                        this.headerValuesEnumerator = headerValuesEnumerator;
                                        this.currentHeaderValueIndex = currentHeaderValueIndex;
                                        this.dispositionManager = dispositionManager;
                                    }

                                    public ValueTask Read()
                                    {
                                        return ValueTask.CompletedTask;
                                    }

                                    public CustomHeaderLws TryGetValue(out bool moved)
                                    {
                                        return this.TryGetValue(out moved, out _);
                                    }

                                    public ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                                    {
                                        this.TryGetValue(out moved, out var nonWhitespaceIndex);
                                        if (!moved)
                                        {
                                            return default!;
                                        }

                                        moved = true;
                                        return new CustomHeaderFieldValueElementReader(this.httpResponseMessage, this.headersEnumerator, this.headerValuesEnumerator, nonWhitespaceIndex, this.dispositionManager);
                                    }

                                    private CustomHeaderLws TryGetValue(out bool moved, out int nonWhitespaceIndex)
                                    {
                                        for (nonWhitespaceIndex = this.currentHeaderValueIndex; nonWhitespaceIndex < this.headerValuesEnumerator.Current.Length && char.IsWhiteSpace(this.headerValuesEnumerator.Current[nonWhitespaceIndex]); ++nonWhitespaceIndex)
                                        {
                                        }

                                        moved = true;
                                        return new CustomHeaderLws(this.headerValuesEnumerator.Current.Substring(this.currentHeaderValueIndex, nonWhitespaceIndex - this.currentHeaderValueIndex));
                                    }
                                }

                                private sealed class CustomHeaderFieldContentReader : ICustomHeaderFieldContentReader<IGetResponseHeadersReader>
                                {
                                    private readonly HttpResponseMessage httpResponseMessage;
                                    private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;
                                    private readonly IEnumerator<string> headerValuesEnumerator;
                                    private readonly int currentHeaderValueIndex;
                                    private readonly IDispositionManager dispositionManager;

                                    public CustomHeaderFieldContentReader(
                                        HttpResponseMessage httpResponseMessage,
                                        IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator,
                                        IEnumerator<string> headerValuesEnumerator,
                                        int currentHeaderValueIndex,
                                        IDispositionManager dispositionManager)
                                    {
                                        this.httpResponseMessage = httpResponseMessage;
                                        this.headersEnumerator = headersEnumerator;
                                        this.headerValuesEnumerator = headerValuesEnumerator;
                                        this.currentHeaderValueIndex = currentHeaderValueIndex;
                                        this.dispositionManager = dispositionManager;
                                    }

                                    public ValueTask Read()
                                    {
                                        return ValueTask.CompletedTask;
                                    }

                                    public CustomHeaderFieldContent TryGetValue(out bool moved)
                                    {
                                        return this.TryGetValue(out moved, out _);
                                    }

                                    public ICustomHeaderFieldValueElementReader<IGetResponseHeadersReader> TryMoveNext(out bool moved)
                                    {
                                        this.TryGetValue(out moved, out var whitespaceIndex);
                                        if (!moved)
                                        {
                                            return default!;
                                        }

                                        moved = true;
                                        return new CustomHeaderFieldValueElementReader(this.httpResponseMessage, this.headersEnumerator, this.headerValuesEnumerator, whitespaceIndex, this.dispositionManager);
                                    }

                                    private CustomHeaderFieldContent TryGetValue(out bool moved, out int whitespaceIndex)
                                    {
                                        for (whitespaceIndex = this.currentHeaderValueIndex; whitespaceIndex < this.headerValuesEnumerator.Current.Length && !char.IsWhiteSpace(this.headerValuesEnumerator.Current[whitespaceIndex]); ++whitespaceIndex)
                                        {
                                        }

                                        moved = true;
                                        return new CustomHeaderFieldContent(this.headerValuesEnumerator.Current.Substring(this.currentHeaderValueIndex, whitespaceIndex - this.currentHeaderValueIndex));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private sealed class ResponseContext
            {
                public ResponseContext(Stream responseContent, byte[] buffer, int bufferValidity)
                {
                    ResponseContent = responseContent;
                    Buffer = buffer;
                    BufferValidity = bufferValidity;

                    this.BytesConsumed = 0;
                }

                public Stream ResponseContent { get; }

                public byte[] Buffer { get; }

                public int BufferValidity { get; }

                public long BytesConsumed { get; set; } //// TODO this being mutable is not ideal
            }

            private static Utf8JsonReader ToUtf8JsonReader(ResponseContext responseContext)
            {
                return new Utf8JsonReader(responseContext.Buffer.AsSpan((int)responseContext.BytesConsumed, responseContext.BufferValidity - (int)responseContext.BytesConsumed));
            }

            /// <summary>
            /// TODO better docs
            /// if it returns <see langword="true"/>, keep using <paramref name="jsonReader"/>, otherwise create a new one from the return <see cref="ResponseContext"/>
            /// </summary>
            /// <param name="responseContext"></param>
            /// <param name="jsonReader"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            private static Task<(bool, ResponseContext)> ConsumeNextToken(ResponseContext responseContext, ref Utf8JsonReader jsonReader)
            {
                bool read;
                try
                {
                    read = jsonReader.Read();
                }
                catch (JsonException jsonException)
                {
                    throw new Exception("TODO", jsonException);
                }

                if (read)
                {
                    responseContext.BytesConsumed = jsonReader.BytesConsumed;
                    return Task.FromResult((true, default(ResponseContext)!));
                }

                return ReadMore(responseContext, (int)jsonReader.BytesConsumed);
            }

            private static async Task<(bool, ResponseContext)> ReadMore(ResponseContext responseContext, int bytesConsumed) //// TODO it would have been cool for this to return `task<responsecontext>`, but that would mean in the caller you need to be able to convert `(bool, task<responsecontext>)` to `task<(bool, responsecontext)>` and you were too lazy to figure that out in the first go, so instead this method always returns `(false, {something})`
            {
                // we couldn't read either because we have consumed the whole buffer, or because the buffer is not large enough for the current JSON token

                // let's start by assuming that we just consumed the whole buffer
                var remainder = responseContext.BufferValidity - bytesConsumed; //// TODO off by 1?
                Array.Copy(responseContext.Buffer, bytesConsumed, responseContext.Buffer, 0, remainder);
                var bytesRead = await responseContext.ResponseContent.ReadAsync(responseContext.Buffer, remainder, responseContext.Buffer.Length - remainder).ConfigureAwait(false);
                responseContext = new ResponseContext(responseContext.ResponseContent, responseContext.Buffer, bytesRead);

                bool read;
                var jsonReader = new Utf8JsonReader(responseContext.Buffer.AsSpan(0, responseContext.BufferValidity));
                try
                {
                    read = jsonReader.Read();
                }
                catch (JsonException jsonException)
                {
                    throw new Exception("TODO", jsonException);
                }

                // if we still can't read, we need to increase the size of the buffer until we *can* read (or we run out of memory)
                while (!read)
                {
                    var oldBuffer = responseContext.Buffer;
                    Array.Resize(ref oldBuffer, oldBuffer.Length * 2); //// TODO make the resizing configurable
                    bytesRead = await responseContext.ResponseContent.ReadAsync(oldBuffer, responseContext.BufferValidity, oldBuffer.Length - responseContext.BufferValidity + 1).ConfigureAwait(false);
                    responseContext = new ResponseContext(responseContext.ResponseContent, oldBuffer, responseContext.BufferValidity + bytesRead);
                    jsonReader = new Utf8JsonReader(responseContext.Buffer.AsSpan(0, responseContext.BufferValidity));
                    try
                    {
                        read = jsonReader.Read();
                    }
                    catch (JsonException jsonException)
                    {
                        throw new Exception("TODO", jsonException);
                    }
                }

                return (false, responseContext);
            }

            private sealed class GetResponseBodyReader : IGetResponseBodyReader
            {
                private readonly HttpResponseMessage httpResponseMessage;

                private readonly IDispositionManager dispositionManager;

                private ResponseContext? responseContext;

                public GetResponseBodyReader(
                    HttpResponseMessage httpResponseMessage,
                    IDispositionManager dispositionManager)
                {
                    this.httpResponseMessage = httpResponseMessage;
                    this.dispositionManager = dispositionManager;
                }

                //[MemberNotNull(nameof(this.responseContext))]
                public async ValueTask Read()
                {
                    if (this.responseContext != null)
                    {
                        return;
                    }

                    var responseContent = await this
                        .dispositionManager
                        .RegisterAsync(async () =>
                             await this.httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        .ConfigureAwait(false);
                    if (responseContent == null)
                    {
                        throw new Exception("TODO create a custom exception type and document this");
                    }

                    // TODO https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader

                    var buffer = new byte[500]; //// TODO configurable size
                    var bytesRead = await responseContent.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    this.responseContext = new ResponseContext(responseContent, buffer, bytesRead);

                    var jsonReader = new Utf8JsonReader(buffer.AsSpan(0, this.responseContext.BufferValidity));
                    var (read, newResponseContext) = await GetResponseHeadersReader.ConsumeNextToken(this.responseContext, ref jsonReader).ConfigureAwait(false);
                    if (!read)
                    {
                        this.responseContext = newResponseContext;
                        jsonReader = ToUtf8JsonReader(this.responseContext);
                    }

                    while (jsonReader.TokenType == JsonTokenType.Comment)
                    {
                        (read, newResponseContext) = await GetResponseHeadersReader.ConsumeNextToken(this.responseContext, ref jsonReader).ConfigureAwait(false);
                        if (!read)
                        {
                            this.responseContext = newResponseContext;
                            jsonReader = ToUtf8JsonReader(this.responseContext);
                        }
                    }

                    if (jsonReader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new Exception("tODO not a valid odata payload");
                    }

                    (read, newResponseContext) = await GetResponseHeadersReader.ConsumeNextToken(this.responseContext, ref jsonReader).ConfigureAwait(false);
                    if (!read)
                    {
                        this.responseContext = newResponseContext;
                        jsonReader = ToUtf8JsonReader(this.responseContext);
                    }
                    while (jsonReader.TokenType == JsonTokenType.Comment)
                    {
                        (read, newResponseContext) = await GetResponseHeadersReader.ConsumeNextToken(this.responseContext, ref jsonReader).ConfigureAwait(false);
                        if (!read)
                        {
                            this.responseContext = newResponseContext;
                            jsonReader = ToUtf8JsonReader(this.responseContext);
                        }
                    }

                    if (jsonReader.TokenType != JsonTokenType.PropertyName && jsonReader.TokenType != JsonTokenType.EndObject)
                    {
                        //// this could actually also be the end of the object, e.g. a single-valued response that doesn't need a context where no properties were selected
                        throw new Exception("tODO not a valid odata payload");
                    }

                    this.responseContext.BytesConsumed = jsonReader.BytesConsumed;
                }

                public IOdataContextReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                {
                    if (this.responseContext == null)
                    {
                        moved = false;
                        return default!;
                    }

                    moved = true;
                    return new OdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                }

                private sealed class OdataContextReader : IOdataContextReader<IGetResponseBodyAfterOdataContextReader>
                {
                    private readonly HttpResponseMessage httpResponseMessage;

                    private readonly IDispositionManager dispositionManager;

                    private readonly ResponseContext consumedResponseContext;

                    private ResponseContext? responseContext;

                    public OdataContextReader(
                        HttpResponseMessage httpResponseMessage,
                        IDispositionManager dispositionManager,
                        ResponseContext responseContext)
                    {
                        this.httpResponseMessage = httpResponseMessage;
                        this.dispositionManager = dispositionManager;
                        this.consumedResponseContext = responseContext;
                    }

                    public ValueTask Read()
                    {
                        if (this.responseContext != null)
                        {
                            return ValueTask.CompletedTask;
                        }

                        var jsonReader = new Utf8JsonReader(this.consumedResponseContext.Buffer.AsSpan((int)this.consumedResponseContext.BytesConsumed, this.consumedResponseContext.BufferValidity - (int)this.consumedResponseContext.BytesConsumed));
                        if (jsonReader.TokenType == JsonTokenType.EndObject)
                        {
                            // there was no content in the response, e.g. a single-valued response that doesn't need a context where no properties were selected
                            this.responseContext = this.consumedResponseContext;
                            return ValueTask.CompletedTask;
                        }

                        if (jsonReader.TokenType == JsonTokenType.PropertyName)
                        {
                            //// TODO are we case sensitive? if so, use reader.valuetextequals
                            var propertyName = jsonReader.GetString();
                            if (!string.Equals(propertyName, "@odata.context", StringComparison.OrdinalIgnoreCase))
                            {
                                this.responseContext = this.consumedResponseContext; //// TODO this is going to result in the property reader reading the property name a second time, since we aren't passing the value that we just read
                                return ValueTask.CompletedTask;
                            }
                            else
                            {
                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                this.responseContext.BytesConsumed = jsonReader.BytesConsumed;
                                return ValueTask.CompletedTask;
                            }
                        }

                        throw new Exception("TODO not valid payload; actually, internally inconsistent state across your implementation");
                    }

                    public IOdataContextToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                    {
                        if (this.responseContext == null)
                        {
                            moved = false;
                            return default!;
                        }

                        if (this.responseContext == this.consumedResponseContext)
                        {
                            moved = true;
                            return new OdataContextToken.GetResponseBodyAfterOdataContext(new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                        }

                        moved = true;
                        return new OdataContextToken.OdataContextUrl(new OdataContextUrlReader(this.httpResponseMessage, this.dispositionManager, this.responseContext));
                    }

                    private abstract class OdataContextToken : IOdataContextToken<IGetResponseBodyAfterOdataContextReader>
                    {
                        private OdataContextToken()
                        {
                        }

                        public TResult Apply<TResult>(Func<IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader>, TResult> odataContextUrlReader, Func<IGetResponseBodyAfterOdataContextReader, TResult> nextReader)
                        {
                            return new DelegateVisitor<TResult, Nothing>(odataContextUrlReader, nextReader).Visit(this, new Nothing());
                        }

                        private sealed class DelegateVisitor<TResult, TContext> : Visitor<TResult, TContext>
                        {
                            private readonly Func<IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader>, TResult> odataContextUrlReader;
                            private readonly Func<IGetResponseBodyAfterOdataContextReader, TResult> nextReader;

                            public DelegateVisitor(Func<IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader>, TResult> odataContextUrlReader, Func<IGetResponseBodyAfterOdataContextReader, TResult> nextReader)
                            {
                                this.odataContextUrlReader = odataContextUrlReader;
                                this.nextReader = nextReader;
                            }

                            protected internal override TResult Accept(OdataContextUrl node, TContext context)
                            {
                                return this.odataContextUrlReader(node.OdataContextUrlReader);
                            }

                            protected internal override TResult Accept(GetResponseBodyAfterOdataContext node, TContext context)
                            {
                                return this.nextReader(node.GetResponseBodyAfterOdataContextReader);
                            }
                        }

                        protected abstract TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

                        public abstract class Visitor<TResult, TContext>
                        {
                            public TResult Visit(OdataContextToken node, TContext context)
                            {
                                ArgumentNullException.ThrowIfNull(node);

                                return node.Dispatch(this, context);
                            }

                            protected internal abstract TResult Accept(OdataContextUrl node, TContext context);

                            protected internal abstract TResult Accept(GetResponseBodyAfterOdataContext node, TContext context);
                        }

                        public sealed class OdataContextUrl : OdataContextToken
                        {
                            public OdataContextUrl(IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader> odataContextUrlReader)
                            {
                                OdataContextUrlReader = odataContextUrlReader;
                            }

                            public IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader> OdataContextUrlReader { get; }

                            protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                            {
                                return visitor.Accept(this, context);
                            }
                        }

                        public sealed class GetResponseBodyAfterOdataContext : OdataContextToken
                        {
                            public GetResponseBodyAfterOdataContext(IGetResponseBodyAfterOdataContextReader getResponseBodyAfterOdataContextReader)
                            {
                                GetResponseBodyAfterOdataContextReader = getResponseBodyAfterOdataContextReader;
                            }

                            public IGetResponseBodyAfterOdataContextReader GetResponseBodyAfterOdataContextReader { get; }

                            protected override TResult Dispatch<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context)
                            {
                                return visitor.Accept(this, context);
                            }
                        }
                    }

                    public sealed class OdataContextUrlReader : IOdataContextUrlReader<IGetResponseBodyAfterOdataContextReader>
                    {
                        private readonly HttpResponseMessage httpResponseMessage; //// TODO i think at some point you don't need the response message anymore

                        private readonly IDispositionManager dispositionManager;

                        private readonly ResponseContext consumedResponseContext;

                        private ResponseContext? responseContext;

                        public OdataContextUrlReader(
                            HttpResponseMessage httpResponseMessage,
                            IDispositionManager dispositionManager,
                            ResponseContext responseContext)
                        {
                            this.httpResponseMessage = httpResponseMessage;
                            this.dispositionManager = dispositionManager;
                            this.consumedResponseContext = responseContext;
                        }

                        public async ValueTask Read()
                        {
                            if (this.responseContext != null)
                            {
                                return;
                            }

                            var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                            var (read, newResponseContext) = await GetResponseHeadersReader.ConsumeNextToken(this.consumedResponseContext, ref jsonReader).ConfigureAwait(false);
                            if (!read)
                            {
                                this.responseContext = newResponseContext;
                                jsonReader = ToUtf8JsonReader(this.responseContext);
                            }
                            else
                            {
                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                            }

                            this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                            //// TODO what do you want the behavior to be if we throw, but htey call read again? nothing will change, but as written, the initial null check will result in the second call not throwing
                            if (jsonReader.TokenType != JsonTokenType.String)
                            {
                                throw new Exception("TODO invalid odata payload");
                            }
                        }

                        public OdataContextUrl TryGetValue(out bool moved)
                        {
                            if (this.responseContext == null)
                            {
                                moved = false;
                                return default!;
                            }

                            var jsonReader = ToUtf8JsonReader(this.responseContext);

                            if (jsonReader.TokenType != JsonTokenType.String)
                            {
                                throw new Exception("TODO this would a bug in the internal consistency of the reader");
                            }

                            var propertyValue = jsonReader.GetString();
                            if (propertyValue == null)
                            {
                                throw new Exception("TODO invalid odata payload, i think; look at the documentation");
                            }

                            moved = true;
                            return new OdataContextUrl(propertyValue);
                        }

                        public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                        {
                            if (this.responseContext == null)
                            {
                                moved = false;
                                return default!;
                            }

                            this.TryGetValue(out moved);
                            if (!moved)
                            {
                                return default!;
                            }

                            moved = true;
                            return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                        }
                    }

                    public sealed class GetResponseBodyAfterOdataContextReader : IGetResponseBodyAfterOdataContextReader
                    {
                        private readonly HttpResponseMessage httpResponseMessage; //// TODO i think at some point you don't need the response message anymore

                        private readonly IDispositionManager dispositionManager;

                        private readonly ResponseContext consumedResponseContext;

                        private ResponseContext? responseContext;

                        public GetResponseBodyAfterOdataContextReader(
                            HttpResponseMessage httpResponseMessage,
                            IDispositionManager dispositionManager,
                            ResponseContext responseContext)
                        {
                            this.httpResponseMessage = httpResponseMessage;
                            this.dispositionManager = dispositionManager;
                            this.consumedResponseContext = responseContext;
                        }

                        public async ValueTask Read()
                        {
                            if (this.responseContext != null)
                            {
                                return;
                            }

                            var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                            var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader).ConfigureAwait(false);
                            if (!read)
                            {
                                this.responseContext = newResponseContext;
                                jsonReader = ToUtf8JsonReader(this.responseContext);
                            }
                            else
                            {
                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                            }

                            this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                            //// TODO you need to skip all of the comments (you forgot to do this in the previous reader too)
                            if (jsonReader.TokenType != JsonTokenType.PropertyName && jsonReader.TokenType != JsonTokenType.EndObject)
                            {
                                throw new Exception("TODO invalid odata payload");
                            }
                        }

                        public IGetResponseBodyAfterOdataContextToken TryMoveNext(out bool moved)
                        {
                            if (this.responseContext == null)
                            {
                                moved = false;
                                return default!;
                            }

                            var jsonReader = ToUtf8JsonReader(this.responseContext);
                            if (jsonReader.TokenType == JsonTokenType.EndObject)
                            {
                                moved = true;
                                return GetResponseBodyAfterOdataContextToken.Terminal.Instance;
                            }

                            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                            {
                                throw new Exception("TODO this would be a bug in the internal consistency");
                            }

                            var propertyName = jsonReader.GetString();
                            if (propertyName == null)
                            {
                                throw new Exception("tODO invalid odata payload"); //// TODO really, trymovenext shouldn't throw for invald payloads
                            }

                            //// TODO you recently added several TODOs because you weren't able to access the documentation; go through all of those

                            if (propertyName.StartsWith('@'))
                            {
                                //// TODO not sure if root annotations can actually start with "@"; i don't think they can, and the below code is written under that assumption

                                if (propertyName.Contains('.'))
                                {
                                    moved = true;
                                    return new GetResponseBodyAfterOdataContextToken.ResponseRootAnnotation(new ResponseRootAnnotationReader(this.httpResponseMessage, this.dispositionManager, this.responseContext));
                                }
                                else
                                {
                                    moved = true;
                                    return new GetResponseBodyAfterOdataContextToken.ResponseRootControlInformation(new ResponseRootControlInformationReader(this.httpResponseMessage, this.dispositionManager, this.responseContext));
                                }
                            }

                            moved = true;
                            return new GetResponseBodyAfterOdataContextToken.Property(new PropertyReader(this.httpResponseMessage, this.dispositionManager, this.responseContext));
                        }

                        private abstract class GetResponseBodyAfterOdataContextToken : IGetResponseBodyAfterOdataContextToken
                        {
                            private GetResponseBodyAfterOdataContextToken()
                            {
                            }

                            public TResult Apply<TResult>(Func<IResponseRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> responseRootControlInformationReader, Func<IResponseRootAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> responseRootAnnotationReader, Func<IPropertyReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyReader, Func<Nothing, TResult> terminal)
                            {
                                throw new NotImplementedException("TODO");
                            }

                            public sealed class ResponseRootControlInformation : GetResponseBodyAfterOdataContextToken
                            {
                                public ResponseRootControlInformation(IResponseRootControlInformationReader<IGetResponseBodyAfterOdataContextReader> responseRootControlInformationReader)
                                {
                                    ResponseRootControlInformationReader = responseRootControlInformationReader;
                                }

                                public IResponseRootControlInformationReader<IGetResponseBodyAfterOdataContextReader> ResponseRootControlInformationReader { get; }
                            }

                            public sealed class ResponseRootAnnotation : GetResponseBodyAfterOdataContextToken
                            {
                                public ResponseRootAnnotation(IResponseRootAnnotationReader<IGetResponseBodyAfterOdataContextReader> responseRootAnnotationReader)
                                {
                                    ResponseRootAnnotationReader = responseRootAnnotationReader;
                                }

                                public IResponseRootAnnotationReader<IGetResponseBodyAfterOdataContextReader> ResponseRootAnnotationReader { get; }
                            }

                            public sealed class Property : GetResponseBodyAfterOdataContextToken
                            {
                                public Property(IPropertyReader<IGetResponseBodyAfterOdataContextReader> propertyReader)
                                {
                                    PropertyReader = propertyReader;
                                }

                                public IPropertyReader<IGetResponseBodyAfterOdataContextReader> PropertyReader { get; }
                            }

                            public sealed class Terminal : GetResponseBodyAfterOdataContextToken
                            {
                                private Terminal()
                                {
                                }

                                public static Terminal Instance { get; } = new Terminal();
                            }
                        }

                        private sealed class ResponseRootControlInformationReader : IResponseRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>
                        {
                            private readonly HttpResponseMessage httpResponseMessage;

                            private readonly IDispositionManager dispositionManager;

                            private readonly ResponseContext consumedResponseContext;

                            public ResponseRootControlInformationReader(
                                HttpResponseMessage httpResponseMessage,
                                IDispositionManager dispositionManager,
                                ResponseContext responseContext)
                            {
                                this.httpResponseMessage = httpResponseMessage;
                                this.dispositionManager = dispositionManager;
                                this.consumedResponseContext = responseContext;
                            }

                            public ValueTask Read()
                            {
                                return ValueTask.CompletedTask;
                            }

                            public IResponseRootControlInformationToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                            {
                                var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                                {
                                    throw new Exception("TODO bug in internal consistency; is this really the correct place to check this?");
                                }

                                var propertyName = jsonReader.GetString();
                                if (
                                    string.Equals(propertyName, "@odata.nextLink", StringComparison.Ordinal) || 
                                    string.Equals(propertyName, "@nextLink"))
                                {
                                    moved = true;
                                    return new ResponseRootControlInformationToken.ResponseRootNextLink(new ResponseRootNextLinkReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                                }

                                moved = true;
                                return new ResponseRootControlInformationToken.ResponseRootUnknownControlInformation(new ResponseRootUnknownControlInformationReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                            }

                            private abstract class ResponseRootControlInformationToken : IResponseRootControlInformationToken<IGetResponseBodyAfterOdataContextReader>
                            {
                                private ResponseRootControlInformationToken()
                                {
                                }

                                public TResult Apply<TResult>(Func<IResponseRootNextLinkReader<IGetResponseBodyAfterOdataContextReader>, TResult> nextLinkReader, Func<IResponseRootUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> unknownControlInformationReader)
                                {
                                    throw new NotImplementedException();
                                }

                                public sealed class ResponseRootNextLink : ResponseRootControlInformationToken
                                {
                                    public ResponseRootNextLink(IResponseRootNextLinkReader<IGetResponseBodyAfterOdataContextReader> responseRootNextLinkReader)
                                    {
                                        ResponseRootNextLinkReader = responseRootNextLinkReader;
                                    }

                                    public IResponseRootNextLinkReader<IGetResponseBodyAfterOdataContextReader> ResponseRootNextLinkReader { get; }
                                }

                                public sealed class ResponseRootUnknownControlInformation : ResponseRootControlInformationToken
                                {
                                    public ResponseRootUnknownControlInformation(IResponseRootUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader> responseRootUnknownControlInformationReader)
                                    {
                                        ResponseRootUnknownControlInformationReader = responseRootUnknownControlInformationReader;
                                    }

                                    public IResponseRootUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader> ResponseRootUnknownControlInformationReader { get; } //// TODO can you just always name these properties `Reader` as a convention?
                                }
                            }

                            private sealed class ResponseRootNextLinkReader : IResponseRootNextLinkReader<IGetResponseBodyAfterOdataContextReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;

                                private readonly IDispositionManager dispositionManager;

                                private readonly ResponseContext consumedResponseContext;

                                private ResponseContext? responseContext;

                                public ResponseRootNextLinkReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IDispositionManager dispositionManager,
                                    ResponseContext responseContext)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.dispositionManager = dispositionManager;
                                    this.consumedResponseContext = responseContext;
                                }

                                public async ValueTask Read()
                                {
                                    if (this.responseContext != null)
                                    {
                                        return;
                                    }

                                    var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                    var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader);
                                    if (!read)
                                    {
                                        this.responseContext = newResponseContext;
                                        jsonReader = ToUtf8JsonReader(this.responseContext);
                                    }
                                    else
                                    {
                                        this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                    }

                                    this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                                    if (jsonReader.TokenType != JsonTokenType.String)
                                    {
                                        throw new Exception("TODO not a valid odata payload");
                                    }

                                    var propertyValue = jsonReader.GetString();
                                    if (propertyValue == null)
                                    {
                                        throw new Exception("TODO not a valid odata payload");
                                    }
                                }

                                public NextLink TryGetValue(out bool moved)
                                {
                                    if (this.responseContext == null)
                                    {
                                        moved = false;
                                        return default!;
                                    }

                                    //// TODO what you've implemented up to this point actually requires `read` to *always* be called before the `try` methods; the try methods should only return `false` if `jsonreader.read` returns false

                                    var jsonReader = ToUtf8JsonReader(this.responseContext);
                                    var propertyValue = jsonReader.GetString();
                                    if (propertyValue == null)
                                    {
                                        throw new Exception("TODO internal consistency");
                                    }

                                    moved = true;
                                    return new NextLink(propertyValue);
                                }

                                public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                                {
                                    if (this.responseContext == null)
                                    {
                                        moved = false;
                                        return default!;
                                    }

                                    this.TryGetValue(out moved);
                                    if (!moved)
                                    {
                                        return default!;
                                    }

                                    moved = true;
                                    return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                                }
                            }

                            private sealed class ResponseRootUnknownControlInformationReader : IResponseRootUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;

                                private readonly IDispositionManager dispositionManager;

                                private readonly ResponseContext consumedResponseContext;

                                public ResponseRootUnknownControlInformationReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IDispositionManager dispositionManager,
                                    ResponseContext responseContext)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.dispositionManager = dispositionManager;
                                    this.consumedResponseContext = responseContext;
                                }

                                public ValueTask Read()
                                {
                                    return ValueTask.CompletedTask;
                                }

                                public IResponseRootUnknownControlInformationNameReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                {
                                    moved = true;
                                    return new ResponseRootUnknownControlInformationNameReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext);
                                }

                                private sealed class ResponseRootUnknownControlInformationNameReader : IResponseRootUnknownControlInformationNameReader<IGetResponseBodyAfterOdataContextReader>
                                {
                                    private readonly HttpResponseMessage httpResponseMessage;

                                    private readonly IDispositionManager dispositionManager;

                                    private readonly ResponseContext consumedResponseContext;

                                    public ResponseRootUnknownControlInformationNameReader(
                                        HttpResponseMessage httpResponseMessage,
                                        IDispositionManager dispositionManager,
                                        ResponseContext responseContext)
                                    {
                                        this.httpResponseMessage = httpResponseMessage;
                                        this.dispositionManager = dispositionManager;
                                        this.consumedResponseContext = responseContext;
                                    }

                                    public ValueTask Read()
                                    {
                                        return ValueTask.CompletedTask;
                                    }

                                    public ControlInformationName TryGetValue(out bool moved)
                                    {
                                        var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                        var propertyName = jsonReader.GetString();
                                        if (propertyName == null)
                                        {
                                            throw new Exception("TODO internal consistency");
                                        }

                                        moved = true;
                                        return new ControlInformationName(propertyName);
                                    }

                                    public IResponseRootUnknownControlInformationValueReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                    {
                                        this.TryGetValue(out moved);
                                        if (!moved)
                                        {
                                            return default!;
                                        }

                                        moved = true;
                                        return new ResponseRootUnknownControlInformationValueReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext);
                                    }

                                    private sealed class ResponseRootUnknownControlInformationValueReader : IResponseRootUnknownControlInformationValueReader<IGetResponseBodyAfterOdataContextReader>
                                    {
                                        private readonly HttpResponseMessage httpResponseMessage;

                                        private readonly IDispositionManager dispositionManager;

                                        private readonly ResponseContext consumedResponseContext;

                                        private ResponseContext? responseContext;

                                        public ResponseRootUnknownControlInformationValueReader(
                                            HttpResponseMessage httpResponseMessage,
                                            IDispositionManager dispositionManager,
                                            ResponseContext responseContext)
                                        {
                                            this.httpResponseMessage = httpResponseMessage;
                                            this.dispositionManager = dispositionManager;
                                            this.consumedResponseContext = responseContext;
                                        }

                                        public async ValueTask Read()
                                        {
                                            if (this.responseContext != null)
                                            {
                                                return;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                            var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader);
                                            if (!read)
                                            {
                                                this.responseContext = newResponseContext;
                                                jsonReader = ToUtf8JsonReader(this.responseContext);
                                            }
                                            else
                                            {
                                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                            }

                                            this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                                            // `odata.count` is a non-string, and `odata.id` can be `null`
                                            // while annotations can definitely use objects as the value, there are currently no control informations which do this; the closest is the `collectionAnnotations` control information, which is a collection; regardless, it seems possible, or even likely, that at some point an object could be returned; further, we are not supposed to fail for invalid annotations or control informations, so we need to handle this case anyway

                                            if (jsonReader.TokenType != JsonTokenType.False &&
                                                jsonReader.TokenType != JsonTokenType.Null &&
                                                jsonReader.TokenType != JsonTokenType.Number &&
                                                jsonReader.TokenType != JsonTokenType.StartArray &&
                                                jsonReader.TokenType != JsonTokenType.StartObject &&
                                                jsonReader.TokenType != JsonTokenType.String &&
                                                jsonReader.TokenType != JsonTokenType.True)
                                            {
                                                throw new Exception("TODO invalid odata payload");
                                            }
                                        }

                                        public ControlInformationValue TryGetValue(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.responseContext);
                                            switch (jsonReader.TokenType)
                                            {
                                                case JsonTokenType.False:
                                                    moved = true;
                                                    return new ControlInformationValue.Boolean(jsonReader.GetBoolean());
                                                case JsonTokenType.Null:
                                                    moved = true;
                                                    return ControlInformationValue.Null.Instance;
                                                case JsonTokenType.Number:
                                                    moved = true;
                                                    if (jsonReader.TryGetInt64(out var @long))
                                                    {
                                                        return new ControlInformationValue.Signed(@long);
                                                    }
                                                    else if (jsonReader.TryGetUInt64(out var @ulong))
                                                    {
                                                        return new ControlInformationValue.Unsigned(@ulong);
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("TODO not valid JSON, i think");
                                                    }
                                                case JsonTokenType.StartArray:
                                                    throw new Exception("TODO not supported TODO according to the standard, you actually ahve to implement skipping this value...");
                                                case JsonTokenType.StartObject:
                                                    throw new Exception("TODO not supported TODO according to the standard, you actually ahve to implement skipping this value...");
                                                case JsonTokenType.String:
                                                    moved = true;
                                                    var @string = jsonReader.GetString();
                                                    if (@string == null)
                                                    {
                                                        throw new Exception("TODO not valid JSON, we already checked for null");
                                                    }

                                                    return new ControlInformationValue.String(@string);
                                                case JsonTokenType.True:
                                                    moved = true;
                                                    return new ControlInformationValue.Boolean(jsonReader.GetBoolean());
                                                default:
                                                    throw new Exception("TODO internal consistency");
                                            }
                                        }

                                        public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            this.TryGetValue(out moved);
                                            if (!moved)
                                            {
                                                return default!;
                                            }

                                            moved = true;
                                            return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                                        }
                                    }
                                }
                            }
                        }

                        public sealed class ResponseRootAnnotationReader : IResponseRootAnnotationReader<IGetResponseBodyAfterOdataContextReader>
                        {
                            private readonly HttpResponseMessage httpResponseMessage;

                            private readonly IDispositionManager dispositionManager;

                            private readonly ResponseContext consumedResponseContext;

                            public ResponseRootAnnotationReader(
                                HttpResponseMessage httpResponseMessage,
                                IDispositionManager dispositionManager,
                                ResponseContext responseContext)
                            {
                                this.httpResponseMessage = httpResponseMessage;
                                this.dispositionManager = dispositionManager;
                                this.consumedResponseContext = responseContext;
                            }

                            public ValueTask Read()
                            {
                                return ValueTask.CompletedTask;
                            }

                            public IResponseRootAnnotationNameReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                            {
                                moved = true;
                                return new ResponseRootAnnotationNameReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext);
                            }

                            private sealed class ResponseRootAnnotationNameReader : IResponseRootAnnotationNameReader<IGetResponseBodyAfterOdataContextReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;

                                private readonly IDispositionManager dispositionManager;

                                private readonly ResponseContext consumedResponseContext;

                                public ResponseRootAnnotationNameReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IDispositionManager dispositionManager,
                                    ResponseContext responseContext)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.dispositionManager = dispositionManager;
                                    this.consumedResponseContext = responseContext;
                                }

                                public ValueTask Read()
                                {
                                    return ValueTask.CompletedTask;
                                }

                                public AnnotationName TryGetValue(out bool moved)
                                {
                                    var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                    var propertyName = jsonReader.GetString();
                                    if (propertyName == null)
                                    {
                                        throw new Exception("TODO internal consistency");
                                    }

                                    moved = true;
                                    return new AnnotationName(propertyName);
                                }

                                public IResponseRootAnnotationValueReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                {
                                    this.TryGetValue(out moved);
                                    if (!moved)
                                    {
                                        return default!;
                                    }

                                    moved = true;
                                    return new ResponseRootAnnotationValueReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext);
                                }

                                private sealed class ResponseRootAnnotationValueReader : IResponseRootAnnotationValueReader<IGetResponseBodyAfterOdataContextReader>
                                {
                                    private readonly HttpResponseMessage httpResponseMessage;

                                    private readonly IDispositionManager dispositionManager;

                                    private readonly ResponseContext consumedResponseContext;

                                    private ResponseContext? responseContext;

                                    public ResponseRootAnnotationValueReader(
                                        HttpResponseMessage httpResponseMessage,
                                        IDispositionManager dispositionManager,
                                        ResponseContext responseContext)
                                    {
                                        this.httpResponseMessage = httpResponseMessage;
                                        this.dispositionManager = dispositionManager;
                                        this.consumedResponseContext = responseContext;
                                    }

                                    public async ValueTask Read()
                                    {
                                        if (this.responseContext != null)
                                        {
                                            return;
                                        }

                                        var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                        var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader);
                                        if (!read)
                                        {
                                            this.responseContext = newResponseContext;
                                            jsonReader = ToUtf8JsonReader(this.responseContext);
                                        }
                                        else
                                        {
                                            this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                        }

                                        this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                                        if (jsonReader.TokenType != JsonTokenType.False &&
                                            jsonReader.TokenType != JsonTokenType.Null &&
                                            jsonReader.TokenType != JsonTokenType.Number &&
                                            jsonReader.TokenType != JsonTokenType.StartArray &&
                                            jsonReader.TokenType != JsonTokenType.StartObject &&
                                            jsonReader.TokenType != JsonTokenType.String &&
                                            jsonReader.TokenType != JsonTokenType.True)
                                        {
                                            throw new Exception("TODO invalid odata payload");
                                        }
                                    }

                                    public AnnotationValue TryGetValue(out bool moved)
                                    {
                                        if (this.responseContext == null)
                                        {
                                            moved = false;
                                            return default!;
                                        }

                                        var jsonReader = ToUtf8JsonReader(this.responseContext);
                                        switch (jsonReader.TokenType)
                                        {
                                            case JsonTokenType.False:
                                                moved = true;
                                                return new AnnotationValue.Boolean(jsonReader.GetBoolean());
                                            case JsonTokenType.Null:
                                                moved = true;
                                                return AnnotationValue.Null.Instance;
                                            case JsonTokenType.Number:
                                                moved = true;
                                                if (jsonReader.TryGetInt64(out var @long))
                                                {
                                                    return new AnnotationValue.Signed(@long);
                                                }
                                                else if (jsonReader.TryGetUInt64(out var @ulong))
                                                {
                                                    return new AnnotationValue.Unsigned(@ulong);
                                                }
                                                else
                                                {
                                                    throw new Exception("TODO not valid JSON, i think");
                                                }
                                            case JsonTokenType.StartArray:
                                                throw new Exception("TODO not supported TODO according to the standard, you actually ahve to implement skipping this value...");
                                            case JsonTokenType.StartObject:
                                                throw new Exception("TODO not supported TODO according to the standard, you actually ahve to implement skipping this value...");
                                            case JsonTokenType.String:
                                                moved = true;
                                                var @string = jsonReader.GetString();
                                                if (@string == null)
                                                {
                                                    throw new Exception("TODO not valid JSON, we already checked for null");
                                                }

                                                return new AnnotationValue.String(@string);
                                            case JsonTokenType.True:
                                                moved = true;
                                                return new AnnotationValue.Boolean(jsonReader.GetBoolean());
                                            default:
                                                throw new Exception("TODO internal consistency");
                                        }
                                    }

                                    public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                                    {
                                        if (this.responseContext == null)
                                        {
                                            moved = false;
                                            return default!;
                                        }

                                        this.TryGetValue(out moved);
                                        if (!moved)
                                        {
                                            return default!;
                                        }

                                        moved = true;
                                        return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                                    }
                                }
                            }
                        }

                        public sealed class PropertyReader : IPropertyReader<IGetResponseBodyAfterOdataContextReader>
                        {
                            private readonly HttpResponseMessage httpResponseMessage;

                            private readonly IDispositionManager dispositionManager;

                            private readonly ResponseContext consumedResponseContext;

                            public PropertyReader(
                                HttpResponseMessage httpResponseMessage,
                                IDispositionManager dispositionManager,
                                ResponseContext responseContext)
                            {
                                this.httpResponseMessage = httpResponseMessage;
                                this.dispositionManager = dispositionManager;
                                this.consumedResponseContext = responseContext;
                            }

                            public ValueTask Read()
                            {
                                return ValueTask.CompletedTask;
                            }

                            public IPropertyNameReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                            {
                                moved = true;
                                return new PropertyNameReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext);
                            }

                            private sealed class PropertyNameReader : IPropertyNameReader<IGetResponseBodyAfterOdataContextReader>
                            {
                                private readonly HttpResponseMessage httpResponseMessage;

                                private readonly IDispositionManager dispositionManager;

                                private readonly ResponseContext consumedResponseContext;

                                public PropertyNameReader(
                                    HttpResponseMessage httpResponseMessage,
                                    IDispositionManager dispositionManager,
                                    ResponseContext responseContext)
                                {
                                    this.httpResponseMessage = httpResponseMessage;
                                    this.dispositionManager = dispositionManager;
                                    this.consumedResponseContext = responseContext;
                                }

                                public ValueTask Read()
                                {
                                    return ValueTask.CompletedTask;
                                }

                                public PropertyName TryGetValue(out bool moved)
                                {
                                    var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                    var propertyName = jsonReader.GetString();
                                    if (propertyName == null)
                                    {
                                        throw new Exception("TODO internal consistency");
                                    }

                                    var atIndex = propertyName.IndexOf('@');
                                    if (atIndex < 0)
                                    {
                                        moved = true;
                                        return new PropertyName(propertyName);
                                    }

                                    propertyName = propertyName.Substring(0, atIndex);

                                    moved = true;
                                    return new PropertyName(propertyName);
                                }

                                public IPropertyNameToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                {
                                    this.TryGetValue(out moved);
                                    if (!moved)
                                    {
                                        return default!;
                                    }

                                    var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                    var propertyName = jsonReader.GetString();
                                    if (propertyName == null)
                                    {
                                        throw new Exception("TODO internal consistency");
                                    }

                                    var atIndex = propertyName.IndexOf('@');
                                    if (atIndex < 0)
                                    {
                                        moved = true;
                                        return new PropertyNameToken.Value(new PropertyValueReader());
                                    }

                                    var dotIndex = propertyName.IndexOf('.', atIndex);
                                    if (dotIndex < 0)
                                    {
                                        moved = true;
                                        return new PropertyNameToken.Annotation(new PropertyAnnotationReader());
                                    }

                                    moved = true;
                                    return new PropertyNameToken.ControlInformation(new PropertyControlInformationReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                                }

                                private abstract class PropertyNameToken : IPropertyNameToken<IGetResponseBodyAfterOdataContextReader>
                                {
                                    private PropertyNameToken()
                                    {
                                    }

                                    public TResult Apply<TResult>(Func<IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyControlInformationReader, Func<IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyAnnotationReader, Func<IPropertyValueReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyValueReader)
                                    {
                                        throw new NotImplementedException();
                                    }

                                    public sealed class ControlInformation : PropertyNameToken
                                    {
                                        public ControlInformation(IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader> reader)
                                        {
                                            Reader = reader;
                                        }

                                        public IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                    }

                                    public sealed class Annotation : PropertyNameToken
                                    {
                                        public Annotation(IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader> reader)
                                        {
                                            Reader = reader;
                                        }

                                        public IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                    }

                                    public sealed class Value : PropertyNameToken
                                    {
                                        public Value(IPropertyValueReader<IGetResponseBodyAfterOdataContextReader> reader)
                                        {
                                            Reader = reader;
                                        }

                                        public IPropertyValueReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                    }
                                }

                                public sealed class PropertyControlInformationReader : IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader>
                                {
                                    private readonly HttpResponseMessage httpResponseMessage;

                                    private readonly IDispositionManager dispositionManager;

                                    private readonly ResponseContext consumedResponseContext;

                                    public PropertyControlInformationReader(
                                        HttpResponseMessage httpResponseMessage,
                                        IDispositionManager dispositionManager,
                                        ResponseContext responseContext)
                                    {
                                        this.httpResponseMessage = httpResponseMessage;
                                        this.dispositionManager = dispositionManager;
                                        this.consumedResponseContext = responseContext;
                                    }

                                    public ValueTask Read()
                                    {
                                        return ValueTask.CompletedTask;
                                    }

                                    public IPropertyControlInformationToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                    {
                                        var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                        var propertyName = jsonReader.GetString();
                                        if (propertyName == null)
                                        {
                                            throw new Exception("TODO internal consistency");
                                        }

                                        var atIndex = propertyName.IndexOf('@');
                                        var controlInformationName = propertyName.Substring(atIndex + 1);
                                        if (
                                            string.Equals(controlInformationName, "odata.navigationLink", StringComparison.Ordinal) ||
                                            string.Equals(controlInformationName, "navigationLink", StringComparison.Ordinal))
                                        {
                                            moved = true;
                                            return new PropertyControlInformationToken.NavigationLink(new PropertyNavigationLinkReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                                        }

                                        if (
                                            string.Equals(controlInformationName, "odata.associationLink", StringComparison.Ordinal) ||
                                            string.Equals(controlInformationName, "associationLink", StringComparison.Ordinal))
                                        {
                                            moved = true;
                                            return new PropertyControlInformationToken.AssociationLink(new PropertyAssociationLinkReader(this.httpResponseMessage, this.dispositionManager, this.consumedResponseContext));
                                        }

                                        moved = true;
                                        return new PropertyControlInformationToken.Unknown(new PropertyUnknownControlInformationReader());
                                    }

                                    private abstract class PropertyControlInformationToken : IPropertyControlInformationToken<IGetResponseBodyAfterOdataContextReader>
                                    {
                                        private PropertyControlInformationToken()
                                        {
                                        }

                                        public TResult Apply<TResult>(Func<IPropertyAssociationLinkReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyAssociationLinkReader, Func<IPropertyNavigationLinkReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyNavigationLinkReader, Func<IPropertyUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyUnknownControlInformationReader)
                                        {
                                            throw new NotImplementedException();
                                        }

                                        public sealed class AssociationLink : PropertyControlInformationToken
                                        {
                                            public AssociationLink(IPropertyAssociationLinkReader<IGetResponseBodyAfterOdataContextReader> reader)
                                            {
                                                Reader = reader;
                                            }

                                            public IPropertyAssociationLinkReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                        }

                                        public sealed class NavigationLink : PropertyControlInformationToken
                                        {
                                            public NavigationLink(IPropertyNavigationLinkReader<IGetResponseBodyAfterOdataContextReader> reader)
                                            {
                                                Reader = reader;
                                            }

                                            public IPropertyNavigationLinkReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                        }

                                        public sealed class Unknown : PropertyControlInformationToken
                                        {
                                            public Unknown(IPropertyUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader> reader)
                                            {
                                                Reader = reader;
                                            }

                                            public IPropertyUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader> Reader { get; }
                                        }
                                    }

                                    private sealed class PropertyAssociationLinkReader : IPropertyAssociationLinkReader<IGetResponseBodyAfterOdataContextReader>
                                    {
                                        private readonly HttpResponseMessage httpResponseMessage;

                                        private readonly IDispositionManager dispositionManager;

                                        private readonly ResponseContext consumedResponseContext;

                                        private ResponseContext? responseContext;

                                        public PropertyAssociationLinkReader(
                                            HttpResponseMessage httpResponseMessage,
                                            IDispositionManager dispositionManager,
                                            ResponseContext responseContext)
                                        {
                                            this.httpResponseMessage = httpResponseMessage;
                                            this.dispositionManager = dispositionManager;
                                            this.consumedResponseContext = responseContext;
                                        }

                                        public async ValueTask Read()
                                        {
                                            if (this.responseContext != null)
                                            {
                                                return;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                            var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader); //// TODO you need to add configureawait everywhere
                                            if (!read)
                                            {
                                                this.responseContext = newResponseContext;
                                                jsonReader = ToUtf8JsonReader(this.responseContext);
                                            }
                                            else
                                            {
                                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                            }

                                            this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                                            if (jsonReader.TokenType != JsonTokenType.String)
                                            {
                                                throw new Exception("TODO invalid odata payload");
                                            }

                                            var propertyValue = jsonReader.GetString();
                                            if (propertyValue == null)
                                            {
                                                throw new Exception("TODO invalid odata payload");
                                            }
                                        }

                                        public AssociationLink TryGetValue(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.responseContext);
                                            var propertyValue = jsonReader.GetString();
                                            if (propertyValue == null)
                                            {
                                                throw new Exception("TODO internal consistency");
                                            }

                                            moved = true;
                                            return new AssociationLink(propertyValue);
                                        }

                                        public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            this.TryGetValue(out moved);
                                            if (!moved)
                                            {
                                                return default!;
                                            }

                                            moved = true;
                                            return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                                        }
                                    }

                                    public sealed class PropertyNavigationLinkReader : IPropertyNavigationLinkReader<IGetResponseBodyAfterOdataContextReader>
                                    {
                                        private readonly HttpResponseMessage httpResponseMessage;

                                        private readonly IDispositionManager dispositionManager;

                                        private readonly ResponseContext consumedResponseContext;

                                        private ResponseContext? responseContext;

                                        public PropertyNavigationLinkReader(
                                            HttpResponseMessage httpResponseMessage,
                                            IDispositionManager dispositionManager,
                                            ResponseContext responseContext)
                                        {
                                            this.httpResponseMessage = httpResponseMessage;
                                            this.dispositionManager = dispositionManager;
                                            this.consumedResponseContext = responseContext;
                                        }

                                        public async ValueTask Read()
                                        {
                                            if (this.responseContext != null)
                                            {
                                                return;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.consumedResponseContext);
                                            var (read, newResponseContext) = await ConsumeNextToken(this.consumedResponseContext, ref jsonReader);
                                            if (!read)
                                            {
                                                this.responseContext = newResponseContext;
                                                jsonReader = ToUtf8JsonReader(this.responseContext);
                                            }
                                            else
                                            {
                                                this.responseContext = new ResponseContext(this.consumedResponseContext.ResponseContent, this.consumedResponseContext.Buffer, this.consumedResponseContext.BufferValidity);
                                            }

                                            this.responseContext.BytesConsumed = jsonReader.BytesConsumed;

                                            if (jsonReader.TokenType != JsonTokenType.String)
                                            {
                                                throw new Exception("TODO invalid odata payload");
                                            }

                                            var propertyValue = jsonReader.GetString();
                                            if (propertyValue == null)
                                            {
                                                throw new Exception("TODO invalid odata payload");
                                            }
                                        }

                                        public NavigationLink TryGetValue(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            var jsonReader = ToUtf8JsonReader(this.responseContext);
                                            var propertyValue = jsonReader.GetString();
                                            if (propertyValue == null)
                                            {
                                                throw new Exception("TODO internal consistency");
                                            }

                                            moved = true;
                                            return new NavigationLink(propertyValue);
                                        }

                                        public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                                        {
                                            if (this.responseContext == null)
                                            {
                                                moved = false;
                                                return default!;
                                            }

                                            this.TryGetValue(out moved);
                                            if (!moved)
                                            {
                                                return default!;
                                            }

                                            moved = true;
                                            return new GetResponseBodyAfterOdataContextReader(this.httpResponseMessage, this.dispositionManager, this.responseContext);
                                        }
                                    }

                                    public sealed class PropertyUnknownControlInformationReader : IPropertyUnknownControlInformationReader<IGetResponseBodyAfterOdataContextReader>
                                    {
                                        public ValueTask Read()
                                        {
                                            throw new NotImplementedException();
                                        }

                                        public IPropertyUnknownControlInformationNameReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                        {
                                            throw new NotImplementedException();
                                        }
                                    }
                                }

                                public sealed class PropertyAnnotationReader : IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader>
                                {
                                    public ValueTask Read()
                                    {
                                        throw new NotImplementedException();
                                    }

                                    public IPropertyAnnotationNameReader<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }

                                public sealed class PropertyValueReader : IPropertyValueReader<IGetResponseBodyAfterOdataContextReader>
                                {
                                    public ValueTask Read()
                                    {
                                        throw new NotImplementedException();
                                    }

                                    public IPropertyNameToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
