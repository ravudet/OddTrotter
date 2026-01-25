namespace OddTrotter.Odata.v4_01.Reader
{
    using System.Net.Http;

    internal sealed class RequestReader : IRequestReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal RequestReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public IVerbReader Read()
        {
            return new VerbReader();
        }
    }

    internal sealed class VerbReader : IVerbReader
    {
        public IUrlReader Read(out HttpVerb httpVerb)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlReader : IUrlReader
    {
        public IUrlSchemeReader Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlSchemeReader : IUrlSchemeReader
    {
        public IUrlDomainReader Read(out UrlScheme urlScheme)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlDomainReader : IUrlDomainReader
    {
        public IUrlPathReader Read(out UrlDomain urlDomain)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlPathReader : IUrlPathReader
    {
        public UrlPathToken Read()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlPathSegmentReader : IUrlPathSegmentReader
    {
        public IUrlPathReader Read(out UrlPathSegment urlPathSegment)
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class UrlQueryReader : IUrlQueryReader
    {
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
