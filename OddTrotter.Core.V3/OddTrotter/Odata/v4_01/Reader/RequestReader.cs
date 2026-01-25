namespace OddTrotter.Odata.v4_01.Reader
{
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
            return new VerbReader(this.httpRequestMessage);
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
            httpVerb = new HttpVerb(this.httpRequestMessage.Method.Method); //// TODO not all methods are supported by odata
            return new UrlReader(this.httpRequestMessage);
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
            return new UrlSchemeReader(this.httpRequestMessage);
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
            var requestUri = this.httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            urlScheme = new UrlScheme(requestUri.Scheme);
            return new UrlDomainReader(this.httpRequestMessage);
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
            var requestUri = this.httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            urlDomain = new UrlDomain(requestUri.Host);
            return new UrlPathReader(this.httpRequestMessage);
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

            var requestUri = this.httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            if (requestUri.Segments.Length > this.segment)
            {
                return new UrlPathToken.Query(new UrlQueryReader(this.httpRequestMessage));
            }
            else
            {
                return new UrlPathToken.PathSegment(new UrlPathSegmentReader(this.httpRequestMessage, this.segment));
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
            var requestUri = this.httpRequestMessage.RequestUri;
            if (requestUri == null)
            {
                throw new OdataException("TODO can this actually be null?");
            }

            if (this.segment >= requestUri.Segments.Length)
            {
                throw new OdataException("TODO");
            }

            urlPathSegment = new UrlPathSegment(requestUri.Segments[this.segment]);
            return new UrlPathReader(this.httpRequestMessage, this.segment + 1);
        }
    }

    internal sealed class UrlQueryReader : IUrlQueryReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal UrlQueryReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public UrlQueryToken Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlQueryKvpReader : IUrlQueryKvpReader
    {
        public IUrlQueryNameReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlQueryNameReader : IUrlQueryNameReader
    {
        public UrlQueryNameToken Read(out UrlQueryName urlQueryName)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlQueryValueReader : IUrlQueryValueReader
    {
        public IUrlQueryReader Read(out UrlQueryValue urlQueryValue)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeadersReader : IHeadersReader
    {
        public IHeaderReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeaderReader : IHeaderReader
    {
        public IHeaderKvpReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeaderKvpReader : IHeaderKvpReader
    {
        public IHeaderKeyReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeaderKeyReader : IHeaderKeyReader
    {
        public HeaderKeyToken Read(out HeaderKey headerKey)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class HeaderValueReader : IHeaderValueReader
    {
        public IHeadersReader Read(out HeaderValue headerValue)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class BodyReader : IBodyReader
    {
        public BodyToken Read()
        {
            throw new System.NotImplementedException();
        }
    }
}
