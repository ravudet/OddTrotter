namespace OddTrotter.Odata.v4_01.Reader
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Calendar;

    internal sealed class RequestWriter
    {
        private readonly IHttpClient httpClient;

        public RequestWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public VerbWriter Write()
        {
            return new VerbWriter(this.httpClient);
        }
    }

    internal sealed class VerbWriter
    {
        private readonly IHttpClient httpClient;

        public VerbWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public UrlWriter Write(HttpVerb httpVerb)
        {
            //// TODO use the provided verb
            return new UrlWriter(this.httpClient, HttpMethod.Get);
        }
    }

    internal sealed class UrlWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        public UrlWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        public UrlSchemeWriter Write()
        {
            return new UrlSchemeWriter(this.httpClient, this.httpMethod);
        }
    }

    internal sealed class UrlSchemeWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        public UrlSchemeWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        public UrlDomainWriter Write(UrlScheme urlScheme)
        {
            return new UrlDomainWriter(this.httpClient, this.httpMethod, urlScheme.Value);
        }
    }

    internal sealed class UrlDomainWriter
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

        public UrlPathWriter Write(UrlDomain urlDomain)
        {
            return new UrlPathWriter(this.httpClient, this.httpMethod, this.url + urlDomain.Value);
        }
    }

    internal sealed class UrlPathWriter
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

        public UrlQueryWriter Write()
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url, true);
        }

        public UrlPathSegmentWriter WriteSegment()
        {
            return new UrlPathSegmentWriter(this.httpClient, this.httpMethod, this.url);
        }
    }

    internal sealed class UrlPathSegmentWriter
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

        public UrlPathWriter Write(UrlPathSegment urlPathSegment)
        {
            return new UrlPathWriter(this.httpClient, this.httpMethod, this.url + '/' + urlPathSegment.Value);
        }
    }

    internal sealed class UrlQueryWriter
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

        public Task<ResponseReader> Send()
        {
            throw new Exception("TODO this shouldn't send but instead should begin writing headers");
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

        public UrlQueryKvpWriter WriteKvp()
        {
            return new UrlQueryKvpWriter(this.httpClient, this.httpMethod, this.url + (this.first ? '?' : '&'));
        }
    }

    internal sealed class UrlQueryKvpWriter
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

        public UrlQueryNameWriter WriteName(UrlQueryName urlQueryName)
        {
            return new UrlQueryNameWriter(this.httpClient, this.httpMethod, this.url + urlQueryName.Value);
        }
    }

    internal sealed class UrlQueryNameWriter
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

        public UrlQueryWriter Write()
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url, false);
        }

        public UrlQueryValueWriter WriteValue()
        {
            return new UrlQueryValueWriter(this.httpClient, this.httpMethod, this.url + '=');
        }
    }

    internal sealed class UrlQueryValueWriter
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

        public UrlQueryWriter Write(UrlQueryValue urlQueryValue)
        {
            return new UrlQueryWriter(this.httpClient, this.httpMethod, this.url + urlQueryValue.Value, false);
        }
    }
}
