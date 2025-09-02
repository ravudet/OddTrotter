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

        public GetResponseReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public ValueTask Read()
        {
            return ValueTask.CompletedTask;
        }

        public IGetResponseHeadersReader TryMoveNext(out bool moved)
        {
            moved = true;
            //// TODO disposable
            return new GetResponseHeadersReader(httpResponseMessage, httpResponseMessage.Headers.GetEnumerator());
        }

        private sealed class GetResponseHeadersReader : IGetResponseHeadersReader
        {
            private readonly HttpResponseMessage httpResponseMessage;

            private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;

            public GetResponseHeadersReader(HttpResponseMessage httpResponseMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator)
            {
                this.httpResponseMessage = httpResponseMessage;
                this.headersEnumerator = headersEnumerator;
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
                    return new GetResponseHeadersToken.GetResponseHeader(new GetResponseHeaderReader(this.httpResponseMessage, this.headersEnumerator));
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
                    throw new NotImplementedException();
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

                public GetResponseHeaderReader(HttpResponseMessage httpResponseMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator)
                {
                    this.httpResponseMessage = httpResponseMessage;
                    this.headersEnumerator = headersEnumerator;
                }

                public ValueTask Read()
                {
                    return ValueTask.CompletedTask;
                }

                public IGetResponseHeaderToken TryMoveNext(out bool moved)
                {
                    moved = true;
                    return new GetResponseHeaderToken.CustomHeader(new CustomHeaderReader(this.httpResponseMessage, this.headersEnumerator));
                }

                private abstract class GetResponseHeaderToken : IGetResponseHeaderToken
                {
                    private GetResponseHeaderToken()
                    {
                    }

                    public TResult Apply<TResult>(Func<ICustomHeaderReader<IGetResponseHeadersReader>, TResult> customHeaderReader)
                    {
                        throw new NotImplementedException();
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

                private sealed class CustomHeaderReader : ICustomHeaderReader<GetResponseHeadersReader>
                {
                    private readonly HttpResponseMessage httpResponseMessage;
                    private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;

                    public CustomHeaderReader(HttpResponseMessage httpResponseMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator)
                    {
                        this.httpResponseMessage = httpResponseMessage;
                        this.headersEnumerator = headersEnumerator;
                    }

                    public ValueTask Read()
                    {
                        return ValueTask.CompletedTask;
                    }

                    public ICustomHeaderFieldNameReader<GetResponseHeadersReader> TryMoveNext(out bool moved)
                    {
                        moved = true;
                        return new CustomHeaderFieldNameReader(this.httpResponseMessage, this.headersEnumerator);
                    }

                    private sealed class CustomHeaderFieldNameReader : ICustomHeaderFieldNameReader<GetResponseHeadersReader>
                    {
                        private readonly HttpResponseMessage httpResponseMessage;
                        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator;

                        public CustomHeaderFieldNameReader(HttpResponseMessage httpResponseMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> headersEnumerator)
                        {
                            this.httpResponseMessage = httpResponseMessage;
                            this.headersEnumerator = headersEnumerator;
                        }


                        public ValueTask Read()
                        {
                            return ValueTask.CompletedTask;
                        }

                        public CustomHeaderFieldName TryGetValue(out bool moved)
                        {
                            //// TODO you are here
                            //// TODO but you should really do the disposable thing before continuing to implement
                            throw new NotImplementedException();
                        }

                        public ICustomHeaderFieldValueReader<GetResponseHeadersReader> TryMoveNext(out bool moved)
                        {
                            throw new NotImplementedException();
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
