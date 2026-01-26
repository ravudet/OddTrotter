namespace OddTrotter.Odata.v4_01.Reader
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Calendar;

    internal sealed class RequestWriter : IRequestWriter
    {
        private readonly IHttpClient httpClient;

        public RequestWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public IVerbWriter Write()
        {
            return new VerbWriter(this.httpClient);
        }
    }

    internal sealed class VerbWriter : IVerbWriter
    {
        private readonly IHttpClient httpClient;

        public VerbWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public IUrlWriter Write(HttpVerb httpVerb)
        {
            //// TODO use the provided verb
            return new UrlWriter(this.httpClient, HttpMethod.Get);
        }
    }

    internal sealed class UrlWriter : IUrlWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        public UrlWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        public IUrlSchemeWriter Write()
        {
            return new UrlSchemeWriter(this.httpClient, this.httpMethod);
        }
    }

    internal sealed class UrlSchemeWriter : IUrlSchemeWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        public UrlSchemeWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        public IUrlDomainWriter Write(UrlScheme urlScheme)
        {
            return new UrlDomainWriter(this.httpClient, this.httpMethod, urlScheme.Value);
        }
    }

    internal sealed class UrlDomainWriter : IUrlDomainWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlDomainWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlPathWriter Write(UrlDomain urlDomain)
        {
            return new UrlPathWriter(this.httpClient, this.httpMethod, this.url + urlDomain.Value);
        }
    }

    internal sealed class UrlPathWriter : IUrlPathWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlPathWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlQueryWriter Write()
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url, true);
        }

        public IUrlPathSegmentWriter WriteSegment()
        {
            return new UrlPathSegmentWriter(this.httpClient, this.httpMethod, this.url);
        }
    }

    internal sealed class UrlPathSegmentWriter : IUrlPathSegmentWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlPathSegmentWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlPathWriter Write(UrlPathSegment urlPathSegment)
        {
            return new UrlPathWriter(this.httpClient, this.httpMethod, this.url + '/' + urlPathSegment.Value);
        }
    }

    internal sealed class UrlQueryWriter : IUrlQueryWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly bool first;

        public UrlQueryWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, bool first)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.first = first;
        }

        public IUrlQueryKvpWriter Write()
        {
            return new UrlQueryKvpWriter(this.httpClient, this.httpMethod, this.url + (this.first ? '?' : '&'));
        }

        public IHeadersWriter WriteHeaders()
        {
            return new HeadersWriter(this.httpClient, this.httpMethod, this.url);
        }
    }

    internal sealed class UrlQueryKvpWriter : IUrlQueryKvpWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryKvpWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlQueryNameWriter Write(UrlQueryName urlQueryName)
        {
            return new UrlQueryNameWriter(this.httpClient, this.httpMethod, this.url + urlQueryName.Value);
        }
    }

    internal sealed class UrlQueryNameWriter : IUrlQueryNameWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryNameWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlQueryWriter Write()
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url, false);
        }

        public IUrlQueryValueWriter WriteValue()
        {
            return new UrlQueryValueWriter(this.httpClient, this.httpMethod, this.url + '=');
        }
    }

    internal sealed class UrlQueryValueWriter : IUrlQueryValueWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryValueWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IUrlQueryWriter Write(UrlQueryValue urlQueryValue)
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url + urlQueryValue.Value, false);
        }
    }

    internal sealed class HeadersWriter : IHeadersWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public HeadersWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public IBodyWriter Write()
        {
            throw new NotImplementedException();
        }

        public IHeaderWriter WriteHeader()
        {
            throw new NotImplementedException();
        }
    }

    /*if (this.httpMethod == HttpMethod.Get)
            {
                //// TODO throws network exceptions
                var response = await this.httpClient.GetAsync(new AbsoluteUri(new Uri(this.url, UriKind.Absolute)), Enumerable.Empty<HttpHeader>());
                return new ResponseReader(response);
            }
            else
            {
                throw new NotSupportedException("TODO how do you want to handle feature gaps?");
            }*/
}
