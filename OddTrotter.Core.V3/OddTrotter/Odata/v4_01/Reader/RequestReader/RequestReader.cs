namespace OddTrotter.Odata.v4_01.Reader.RequestReader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using Fx.Either;

    internal sealed class RequestReader : IRequestReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal RequestReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IVerbReader Read()
        {
            return new VerbReader(httpRequestMessage);
        }
    }

    internal sealed class VerbReader : IVerbReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal VerbReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IUrlReader Read(out HttpVerb httpVerb)
        {
            httpVerb = new HttpVerb(httpRequestMessage.Method.Method); //// TODO not all methods are supported by odata
            return new UrlReader(httpRequestMessage);
        }
    }

    internal sealed class UrlReader : IUrlReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IUrlSchemeReader Read()
        {
            return new UrlSchemeReader(httpRequestMessage);
        }
    }

    internal sealed class UrlSchemeReader : IUrlSchemeReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlSchemeReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IUrlDomainReader Read(out UrlScheme urlScheme)
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            urlScheme = new UrlScheme(requestUri.Scheme);
            return new UrlDomainReader(httpRequestMessage);
        }
    }

    internal sealed class UrlDomainReader : IUrlDomainReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlDomainReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IUrlPathReader Read(out UrlDomain urlDomain)
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            urlDomain = new UrlDomain(requestUri.Host);
            return new UrlPathReader(httpRequestMessage);
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

        public UrlPathToken Read()
        {
            //// TODO we should probably confirm that there is no fragment (and anything else that odata doesn't leverage)

            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            if (requestUri.Segments.Length > segment)
            {
                return new UrlPathToken.Query(new UrlQueryReader(httpRequestMessage));
            }
            else
            {
                return new UrlPathToken.PathSegment(new UrlPathSegmentReader(httpRequestMessage, segment));
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

        public IUrlPathReader Read(out UrlPathSegment urlPathSegment)
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            if (segment >= requestUri.Segments.Length)
            {
                throw new OdataException("TODO");
            }

            urlPathSegment = new UrlPathSegment(requestUri.Segments[segment]);
            return new UrlPathReader(httpRequestMessage, segment + 1);
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

        public UrlQueryToken Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            if (string.IsNullOrEmpty(requestUri.Query) || string.IsNullOrEmpty(requestUri.Query.Substring(index)))
            {
                return new UrlQueryToken.Headers(new HeadersReader(httpRequestMessage));
            }
            else
            {
                return new UrlQueryToken.Kvp(new UrlQueryKvpReader(httpRequestMessage, index));
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

        public IUrlQueryNameReader Read()
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            return new UrlQueryNameReader(httpRequestMessage, index);
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

        public UrlQueryNameToken Read(out UrlQueryName urlQueryName)
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            var kvpDelimiterIndex = requestUri.Query.IndexOf('&');
            var valueDelimiterIndex = requestUri.Query.IndexOf('=');

            if (kvpDelimiterIndex == -1 && valueDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(string.Empty); //// TODO is this a legal URL?
                return new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, requestUri.Query.Length));
            }

            if (kvpDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, valueDelimiterIndex));
                return new UrlQueryNameToken.QueryValue(new UrlQueryValueReader(httpRequestMessage, valueDelimiterIndex + 1));
            }

            if (valueDelimiterIndex == -1)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, kvpDelimiterIndex));
                return new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex + 1));
            }

            if (kvpDelimiterIndex < valueDelimiterIndex)
            {
                urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, kvpDelimiterIndex));
                return new UrlQueryNameToken.Query(new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex + 1));
            }

            urlQueryName = new UrlQueryName(requestUri.Query.Substring(index, valueDelimiterIndex));
            return new UrlQueryNameToken.QueryValue(new UrlQueryValueReader(httpRequestMessage, valueDelimiterIndex + 1));
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

        public IUrlQueryReader Read(out UrlQueryValue urlQueryValue)
        {
            var requestUri = httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            var kvpDelimiterIndex = requestUri.Query.IndexOf('&');
            if (kvpDelimiterIndex == -1)
            {
                kvpDelimiterIndex = requestUri.Query.Length;
            }

            urlQueryValue = new UrlQueryValue(requestUri.Query.Substring(index, kvpDelimiterIndex));
            return new UrlQueryReader(httpRequestMessage, kvpDelimiterIndex);
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

        public HeadersToken Read()
        {
            if (!enumerator.MoveNext())
            {
                return new HeadersToken.Body(new BodyReader(httpRequestMessage));
            }

            return new HeadersToken.Header(new HeaderReader(httpRequestMessage, enumerator));
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

        public IHeaderKvpReader Read()
        {
            return new HeaderKvpReader(httpRequestMessage, enumerator);
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

        public IHeaderKeyReader Read()
        {
            return new HeaderKeyReader(httpRequestMessage, enumerator);
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

        public HeaderKeyToken Read(out HeaderKey headerKey)
        {
            headerKey = new HeaderKey(enumerator.Current.Key);

            var valuesEnumerator = enumerator.Current.Value.GetEnumerator();
            if (!valuesEnumerator.MoveNext())
            {
                return new HeaderKeyToken.Headers(new HeadersReader(httpRequestMessage, enumerator));
            }

            return new HeaderKeyToken.HeaderValue(new HeaderValueReader(httpRequestMessage, enumerator, valuesEnumerator));
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

        public HeaderValueToken Read(out HeaderValue headerValue)
        {
            headerValue = new HeaderValue(valuesEnumerator.Current);
            if (valuesEnumerator.MoveNext())
            {
                return new HeaderValueToken.HeaderValue(new HeaderValueReader(httpRequestMessage, enumerator, valuesEnumerator));
            }
            else
            {
                return new HeaderValueToken.Headers(new HeadersReader(httpRequestMessage, enumerator));
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

        public BodyToken Read()
        {
            //// TODO implement actually reading the body
            return BodyToken.End.Instance;
        }
    }
}
