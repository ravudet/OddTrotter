namespace OddTrotter.Odata.v4_01.Reader.RequestWriter
{
    using System;
    using System.Collections.Generic;
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
            return new VerbWriter(httpClient);
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
            return new UrlWriter(httpClient, HttpMethod.Get);
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
            return new UrlSchemeWriter(httpClient, httpMethod);
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
            return new UrlDomainWriter(httpClient, httpMethod, urlScheme.Value);
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
            return new UrlPathWriter(httpClient, httpMethod, url + urlDomain.Value);
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
            return new UrlQueryWriter(httpClient, httpMethod, url, true);
        }

        public IUrlPathSegmentWriter WriteSegment()
        {
            return new UrlPathSegmentWriter(httpClient, httpMethod, url);
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
            return new UrlPathWriter(httpClient, httpMethod, url + '/' + urlPathSegment.Value);
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
            return new UrlQueryKvpWriter(httpClient, httpMethod, url + (first ? '?' : '&'));
        }

        public IHeadersWriter WriteHeaders()
        {
            return new HeadersWriter(httpClient, httpMethod, url);
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
            return new UrlQueryNameWriter(httpClient, httpMethod, url + urlQueryName.Value);
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
            return new UrlQueryWriter(httpClient, httpMethod, url, false);
        }

        public IUrlQueryValueWriter WriteValue()
        {
            return new UrlQueryValueWriter(httpClient, httpMethod, url + '=');
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
            return new UrlQueryWriter(httpClient, httpMethod, url + urlQueryValue.Value, false);
        }
    }

    internal sealed class HeadersWriter : IHeadersWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        public HeadersWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
            : this(httpClient, httpMethod, url, Enumerable.Empty<Tuple<string, string>>())
        {
        }

        internal HeadersWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        public IBodyWriter Write()
        {
            return new BodyWriter(httpClient, httpMethod, url, headers);
        }

        public IHeaderWriter WriteHeader()
        {
            return new HeaderWriter(httpClient, httpMethod, url, headers);
        }
    }

    internal sealed class HeaderWriter : IHeaderWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        public HeaderWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        public IHeaderKvpWriter Write()
        {
            return new HeaderKvpWriter(httpClient, httpMethod, url, headers);
        }
    }

    internal sealed class HeaderKvpWriter : IHeaderKvpWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        public HeaderKvpWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        public IHeaderKeyWriter Write(HeaderKey headerKey)
        {
            return new HeaderKeyWriter(httpClient, httpMethod, url, headers, headerKey.Value, string.Empty, true);
        }
    }

    internal sealed class HeaderKeyWriter : IHeaderKeyWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;
        private readonly string headerKey;
        private readonly string header;
        private readonly bool first;

        public HeaderKeyWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers, string headerKey, string header, bool first)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
            this.headerKey = headerKey;
            this.header = header;
            this.first = first;
        }

        public IHeadersWriter Write()
        {
            return new HeadersWriter(httpClient, httpMethod, url, headers.Append(Tuple.Create(headerKey, headerKey)));
        }

        public IHeaderValueWriter Write(HeaderValue headerValue)
        {
            return new HeaderValueWriter(httpClient, httpMethod, url, headers, headerKey, header + (first ? string.Empty : ";") + headerValue.Value);
        }
    }

    internal sealed class HeaderValueWriter : IHeaderValueWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;
        private readonly string headerKey;
        private readonly string header;

        public HeaderValueWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers, string headerKey, string header)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
            this.headerKey = headerKey;
            this.header = header;
        }

        public IHeaderKeyWriter Write()
        {
            return new HeaderKeyWriter(httpClient, httpMethod, url, headers, headerKey, header, false);
        }
    }

    internal sealed class BodyWriter : IBodyWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;


        public BodyWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        public async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Send()
        {
            if (httpMethod == HttpMethod.Get)
            {
                var response = await httpClient.GetAsync(new AbsoluteUri(new Uri(url, UriKind.Absolute)), headers.Select(header => new HttpHeader(header.Item1, header.Item2)));
                return new OddTrotter.Odata.v4_01.Reader.ResponseReader.ResponseReader(response);
            }
            else
            {
                throw new NotSupportedException("TODO how do you want to handle feature gaps?");
            }
        }
    }
}
