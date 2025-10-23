namespace OddTrotter.CalendarV1.Tokenization.ReaderImplementations
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Formats.Asn1;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices.JavaScript;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Fx.Either;

    using OddTrotter.CalendarV1.Tokenization.Readers;

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

                public int BytesConsumed { get; set; } //// TODO this being mutable is not ideal
            }

            private static void Read(ref ResponseContext responseContext, ref Utf8JsonReader jsonReader)
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

                if (!read)
                {
                    // we couldn't read either because we have consumed the whole buffer, or because the buffer is not large enough for the current JSON token

                    // let's start by assuming that we just consumed the whole buffer
                    var remainder = (int)(responseContext.BufferValidity - jsonReader.BytesConsumed); //// TODO off by 1?
                    Array.Copy(responseContext.Buffer, (int)jsonReader.BytesConsumed, responseContext.Buffer, 0, remainder);
                    var bytesRead = responseContext.ResponseContent.Read(responseContext.Buffer, remainder, responseContext.Buffer.Length - remainder);
                    responseContext = new ResponseContext(responseContext.ResponseContent, responseContext.Buffer, bytesRead);

                    jsonReader = new Utf8JsonReader(responseContext.Buffer.AsSpan(0, responseContext.BufferValidity));
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
                        bytesRead = responseContext.ResponseContent.Read(oldBuffer, responseContext.BufferValidity, oldBuffer.Length - responseContext.BufferValidity + 1); //// TODO make this async
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
                }
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
                    var bytesRead = responseContent.Read(buffer, 0, buffer.Length);

                    this.responseContext = new ResponseContext(responseContent, buffer, bytesRead);

                    var jsonReader = new Utf8JsonReader(buffer.AsSpan(0, this.responseContext.BufferValidity));
                    GetResponseHeadersReader.Read(ref this.responseContext, ref jsonReader);
                    while (jsonReader.TokenType == JsonTokenType.Comment)
                    {
                        GetResponseHeadersReader.Read(ref this.responseContext, ref jsonReader);
                    }

                    if (jsonReader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new Exception("tODO not a valid odata payload");
                    }

                    GetResponseHeadersReader.Read(ref this.responseContext, ref jsonReader);
                    while (jsonReader.TokenType == JsonTokenType.Comment)
                    {
                        GetResponseHeadersReader.Read(ref this.responseContext, ref jsonReader);
                    }

                    //// TODO this could actually also be the end of the object, e.g. a single-valued response that doesn't need a context where no properties were selected
                    if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new Exception("tODO not a valid odata payload");
                    }

                    this.responseContext.BytesConsumed = (int)jsonReader.BytesConsumed;
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
                            return;
                        }

                        var jsonReader = new Utf8JsonReader(this.consumedResponseContext.Buffer.AsSpan(this.consumedResponseContext.BytesConsumed, this.consumedResponseContext.BufferValidity - this.consumedResponseContext.BytesConsumed));
                        if (jsonReader.TokenType == JsonTokenType.EndObject)
                        {
                            // there was no content in the response, e.g. a single-valued response that doesn't need a context where no properties were selected

                        }

                        /*var propertyName = jsonReader.GetString();
                        if (string.Equals(propertyName, "@odata.context")) //// TODO are we case sensitive? if so, use reader.valuetextequals
                        {
                            jsonReader.Read();
                        }*/
                        throw new NotImplementedException();
                    }

                    public IOdataContextToken<IGetResponseBodyAfterOdataContextReader> TryMoveNext(out bool moved)
                    {
                        if (this.responseContext == null)
                        {
                            moved = false;
                            return default!;
                        }

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
                        public ValueTask Read()
                        {
                            throw new NotImplementedException();
                        }

                        public OdataContextUrl TryGetValue(out bool moved)
                        {
                            throw new NotImplementedException();
                        }

                        public IGetResponseBodyAfterOdataContextReader TryMoveNext(out bool moved)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public sealed class GetResponseBodyAfterOdataContextReader : IGetResponseBodyAfterOdataContextReader
                    {
                        public ValueTask Read()
                        {
                            throw new NotImplementedException();
                        }

                        public IGetResponseBodyAfterOdataContextToken TryMoveNext(out bool moved)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
        }
    }
}
