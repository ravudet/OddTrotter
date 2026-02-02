namespace OddTrotter.Odata.v4_01.Reader.RequestReader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fx.Either;

    internal sealed class RequestReader : IRequestReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal RequestReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<IVerbReader> Read()
        {
            return await Task.FromResult(new VerbReader(httpRequestMessage)).ConfigureAwait(false);
        }
    }

    internal sealed class VerbReader : IVerbReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal VerbReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<(IUrlReader UrlReader, HttpVerb HttpVerb)> Read() //// TODO i think the read methods should return a tuple?
        {
            var httpVerb = new HttpVerb(httpRequestMessage.Method.Method); //// TODO not all methods are supported by odata
            var urlReader = new UrlReader(httpRequestMessage);
            return await Task.FromResult((urlReader, httpVerb)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlReader : IUrlReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<IUrlSchemeReader> Read()
        {
            return await Task.FromResult(new UrlSchemeReader(httpRequestMessage)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlSchemeReader : IUrlSchemeReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlSchemeReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<(IUrlDomainReader UrlDomainReader, UrlScheme UrlScheme)> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            var urlScheme = new UrlScheme(requestUri.Scheme);
            var urlDomainReader = new UrlDomainReader(httpRequestMessage);

            return await Task.FromResult((urlDomainReader, urlScheme)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlDomainReader : IUrlDomainReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlDomainReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<(IUrlPathReader UrlPathReader, UrlDomain UrlDomain)> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            var urlDomain = new UrlDomain(requestUri.Host);
            var urlPathReader = new UrlPathReader(httpRequestMessage);

            return await Task.FromResult((urlPathReader, urlDomain)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlPathReader : IUrlPathReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int segment;

        internal UrlPathReader(HttpRequestMessage httpRequestMessage)
            : this(httpRequestMessage, 0)
        {
        }

        internal UrlPathReader(HttpRequestMessage httpRequestMessage, int segment)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.segment = segment;
        }

        public async Task<UrlPathToken> Read()
        {
            //// TODO we should probably confirm that there is no fragment (and anything else that odata doesn't leverage)

            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            if (requestUri.Segments.Length > segment)
            {
                return await Task.FromResult(new UrlPathToken.Query(new UrlQueryReader(httpRequestMessage))).ConfigureAwait(false);
            }
            else
            {
                return await Task.FromResult(new UrlPathToken.PathSegment(new UrlPathSegmentReader(httpRequestMessage, segment))).ConfigureAwait(false);
            }
        }
    }

    internal sealed class UrlPathSegmentReader : IUrlPathSegmentReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int segment;

        internal UrlPathSegmentReader(HttpRequestMessage httpRequestMessage, int segment)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.segment = segment;
        }

        public async Task<(IUrlPathReader UrlPathReader, UrlPathSegment UrlPathSegment)> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            if (segment >= requestUri.Segments.Length)
            {
                throw new ReadException("TODO");
            }

            var urlPathSegment = new UrlPathSegment(requestUri.Segments[segment]);
            var urlPathReader = new UrlPathReader(httpRequestMessage, segment + 1);

            return await Task.FromResult((urlPathReader, urlPathSegment)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryReader : IUrlQueryReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int index;

        internal UrlQueryReader(HttpRequestMessage httpRequestMessage)
            : this(httpRequestMessage, 1)
        {
        }

        internal UrlQueryReader(HttpRequestMessage httpRequestMessage, int index)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.index = index;
        }

        public async Task<UrlQueryToken> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            if (string.IsNullOrEmpty(requestUri.Query) || string.IsNullOrEmpty(requestUri.Query.Substring(index)))
            {
                return await Task.FromResult(new UrlQueryToken.Headers(new HeadersReader(httpRequestMessage))).ConfigureAwait(false);
            }
            else
            {
                return await Task.FromResult(new UrlQueryToken.Kvp(new UrlQueryKvpReader(httpRequestMessage, index))).ConfigureAwait(false);
            }
        }
    }

    internal sealed class UrlQueryKvpReader : IUrlQueryKvpReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int index;

        internal UrlQueryKvpReader(HttpRequestMessage httpRequestMessage, int index)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.index = index;
        }

        public async Task<IUrlQueryNameReader> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            return await Task.FromResult(new UrlQueryNameReader(httpRequestMessage, index)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryNameReader : IUrlQueryNameReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int index;

        internal UrlQueryNameReader(HttpRequestMessage httpRequestMessage, int index)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.index = index;
        }

        public async Task<(UrlQueryNameToken UrlQueryNameToken, UrlQueryName UrlQueryName)> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            var kvpDelimiterIndex = requestUri.Query.IndexOf('&');
            var valueDelimiterIndex = requestUri.Query.IndexOf('=');

            UrlQueryName urlQueryName;
            UrlQueryNameToken urlQueryNameToken;
            if (kvpDelimiterIndex == -1 && valueDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(string.Empty); //// TODO is this a legal URL?
                urlQueryNameToken = new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, requestUri.Query.Length));
            }
            else if (kvpDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, valueDelimiterIndex));
                urlQueryNameToken = new UrlQueryNameToken.QueryValue(new UrlQueryValueReader(httpRequestMessage, valueDelimiterIndex + 1));
            }
            else if (valueDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, kvpDelimiterIndex));
                urlQueryNameToken = new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex + 1));
            }
            else if (kvpDelimiterIndex < valueDelimiterIndex)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, kvpDelimiterIndex));
                urlQueryNameToken = urlQueryNameToken = new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex + 1));
            }
            else
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, valueDelimiterIndex));
                urlQueryNameToken = new UrlQueryNameToken.QueryValue(new UrlQueryValueReader(httpRequestMessage, valueDelimiterIndex + 1));
            }

            return await Task.FromResult((urlQueryNameToken, urlQueryName)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryValueReader : IUrlQueryValueReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly int index;

        internal UrlQueryValueReader(HttpRequestMessage httpRequestMessage, int index)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.index = index;
        }

        public async Task<(IUrlQueryReader UrlQueryReader, UrlQueryValue UrlQueryValue)> Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new ReadException("TODO can this actually be null?");
            }

            var kvpDelimiterIndex = requestUri.Query.IndexOf('&');
            if (kvpDelimiterIndex == -1)
            {
                kvpDelimiterIndex = requestUri.Query.Length;
            }

            var urlQueryValue = new UrlQueryValue(requestUri.Query.Substring(index, kvpDelimiterIndex));
            var urlQueryReader = new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex);

            return await Task.FromResult((urlQueryReader, urlQueryValue)).ConfigureAwait(false);
        }
    }

    internal sealed class HeadersReader : IHeadersReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator;

        internal HeadersReader(HttpRequestMessage httpRequestMessage)
            : this(httpRequestMessage, httpRequestMessage.Headers.GetEnumerator())
        {
        }

        internal HeadersReader(HttpRequestMessage httpRequestMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.enumerator = enumerator;
        }

        public async Task<HeadersToken> Read()
        {
            if (!enumerator.MoveNext())
            {
                return new HeadersToken.Body(new BodyReader(httpRequestMessage));
            }

            return await Task.FromResult(new HeadersToken.Header(new HeaderReader(httpRequestMessage, enumerator))).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderReader : IHeaderReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator;

        internal HeaderReader(HttpRequestMessage httpRequestMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.enumerator = enumerator;
        }

        public async Task<IHeaderKvpReader> Read()
        {
            return await Task.FromResult(new HeaderKvpReader(httpRequestMessage, enumerator)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderKvpReader : IHeaderKvpReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator;

        internal HeaderKvpReader(HttpRequestMessage httpRequestMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.enumerator = enumerator;
        }

        public async Task<IHeaderKeyReader> Read()
        {
            return await Task.FromResult(new HeaderKeyReader(httpRequestMessage, enumerator)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderKeyReader : IHeaderKeyReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator;

        internal HeaderKeyReader(HttpRequestMessage httpRequestMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.enumerator = enumerator;
        }

        public async Task<(HeaderKeyToken HeaderKeyToken, HeaderKey HeaderKey)> Read()
        {
            var headerKey = new HeaderKey(enumerator.Current.Key);

            var valuesEnumerator = enumerator.Current.Value.GetEnumerator();
            if (!valuesEnumerator.MoveNext())
            {
                var headers = new HeaderKeyToken.Headers(new HeadersReader(httpRequestMessage, enumerator));

                return await Task.FromResult((headers, headerKey)).ConfigureAwait(false);
            }

            var headerValue = new HeaderKeyToken.HeaderValue(new HeaderValueReader(httpRequestMessage, enumerator, valuesEnumerator));

            return await Task.FromResult((headerValue, headerKey)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderValueReader : IHeaderValueReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator;
        private readonly IEnumerator<string> valuesEnumerator;

        internal HeaderValueReader(HttpRequestMessage httpRequestMessage, IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator, IEnumerator<string> valuesEnumerator)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.enumerator = enumerator;
            this.valuesEnumerator = valuesEnumerator;
        }

        public async Task<(HeaderValueToken HeaderValueToken, HeaderValue HeaderValue)> Read()
        {
            var headerValue = new HeaderValue(valuesEnumerator.Current);
            if (valuesEnumerator.MoveNext())
            {
                var token = new HeaderValueToken.HeaderValue(new HeaderValueReader(httpRequestMessage, enumerator, valuesEnumerator));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
            }
            else
            {
                var token = new HeaderValueToken.Headers(new HeadersReader(httpRequestMessage, enumerator));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
            }
        }
    }

    internal sealed class BodyReader : IBodyReader
    {
        private readonly HttpRequestMessage httpRequestMessage;
        
        internal BodyReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public async Task<BodyToken> Read()
        {
            //// TODO implement actually reading the body
            return await Task.FromResult(BodyToken.End.Instance).ConfigureAwait(false);
        }
    }
}
