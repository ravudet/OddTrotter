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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is <see langword="null"/></exception>
        public RequestWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<IVerbWriter> Write()
        {
            return await Task.FromResult(new VerbWriter(httpClient)).ConfigureAwait(false);
        }
    }

    internal sealed class VerbWriter : IVerbWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is <see langword="null"/></exception>
        private readonly IHttpClient httpClient;

        public VerbWriter(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<IUrlWriter> Write(HttpVerb httpVerb)
        {
            //// TODO use the provided verb
            return await Task.FromResult(new UrlWriter(httpClient, HttpMethod.Get)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlWriter : IUrlWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> is <see langword="null"/></exception>
        public UrlWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        /// <inheritdoc/>
        public async Task<IUrlSchemeWriter> Write()
        {
            return await Task.FromResult(new UrlSchemeWriter(httpClient, httpMethod)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlSchemeWriter : IUrlSchemeWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> is <see langword="null"/></exception>
        public UrlSchemeWriter(IHttpClient httpClient, HttpMethod httpMethod)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
        }

        /// <inheritdoc/>
        public async Task<IUrlDomainWriter> Write(UrlScheme urlScheme)
        {
            return await Task.FromResult(new UrlDomainWriter(httpClient, httpMethod, urlScheme.Value)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlDomainWriter : IUrlDomainWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlDomainWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlPathWriter> Write(UrlDomain urlDomain)
        {
            return await Task.FromResult(new UrlPathWriter(httpClient, httpMethod, url + urlDomain.Value)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlPathWriter : IUrlPathWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlPathWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryWriter> Write()
        {
            return await Task.FromResult(new UrlQueryWriter(httpClient, httpMethod, url, true)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IUrlPathSegmentWriter> WriteSegment()
        {
            return await Task.FromResult(new UrlPathSegmentWriter(httpClient, httpMethod, url)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlPathSegmentWriter : IUrlPathSegmentWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlPathSegmentWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlPathWriter> Write(UrlPathSegment urlPathSegment)
        {
            return await Task.FromResult(new UrlPathWriter(httpClient, httpMethod, url + '/' + urlPathSegment.Value)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryWriter : IUrlQueryWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly bool first;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlQueryWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, bool first)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.first = first;
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryKvpWriter> Write()
        {
            return await Task.FromResult(new UrlQueryKvpWriter(httpClient, httpMethod, url + (first ? '?' : '&'))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IHeadersWriter> WriteHeaders()
        {
            return await Task.FromResult(new HeadersWriter(httpClient, httpMethod, url)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryKvpWriter : IUrlQueryKvpWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlQueryKvpWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryNameWriter> Write(UrlQueryName urlQueryName)
        {
            return await Task.FromResult(new UrlQueryNameWriter(httpClient, httpMethod, url + urlQueryName.Value)).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryNameWriter : IUrlQueryNameWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlQueryNameWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryWriter> Write()
        {
            return await Task.FromResult(new UrlQueryWriter(httpClient, httpMethod, url, false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryValueWriter> WriteValue()
        {
            return await Task.FromResult(new UrlQueryValueWriter(httpClient, httpMethod, url + '=')).ConfigureAwait(false);
        }
    }

    internal sealed class UrlQueryValueWriter : IUrlQueryValueWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public UrlQueryValueWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        /// <inheritdoc/>
        public async Task<IUrlQueryWriter> Write(UrlQueryValue urlQueryValue)
        {
            return await Task.FromResult(new UrlQueryWriter(httpClient, httpMethod, url + urlQueryValue.Value, false)).ConfigureAwait(false);
        }
    }

    internal sealed class HeadersWriter : IHeadersWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public HeadersWriter(IHttpClient httpClient, HttpMethod httpMethod, string url)
            : this(httpClient, httpMethod, url, Enumerable.Empty<Tuple<string, string>>())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        internal HeadersWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        /// <inheritdoc/>
        public async Task<IBodyWriter> Write()
        {
            return await Task.FromResult(new BodyWriter(httpClient, httpMethod, url, headers)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IHeaderWriter> WriteHeader()
        {
            return await Task.FromResult(new HeaderWriter(httpClient, httpMethod, url, headers)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderWriter : IHeaderWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public HeaderWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        /// <inheritdoc/>
        public async Task<IHeaderKvpWriter> Write()
        {
            return await Task.FromResult(new HeaderKvpWriter(httpClient, httpMethod, url, headers)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderKvpWriter : IHeaderKvpWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public HeaderKvpWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        /// <inheritdoc/>
        public async Task<IHeaderKeyWriter> Write(HeaderKey headerKey)
        {
            return await Task.FromResult(new HeaderKeyWriter(httpClient, httpMethod, url, headers, headerKey.Value, string.Empty, true)).ConfigureAwait(false);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> or <paramref name="headerKey"/> or <paramref name="header"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
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

        /// <inheritdoc/>
        public async Task<IHeadersWriter> Write()
        {
            return await Task.FromResult(new HeadersWriter(httpClient, httpMethod, url, headers.Append(Tuple.Create(headerKey, headerKey)))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IHeaderValueWriter> Write(HeaderValue headerValue)
        {
            return await Task.FromResult(new HeaderValueWriter(httpClient, httpMethod, url, headers, headerKey, header + (first ? string.Empty : ";") + headerValue.Value)).ConfigureAwait(false);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="headerKey"></param>
        /// <param name="header"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> or <paramref name="headerKey"/> or <paramref name="header"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public HeaderValueWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers, string headerKey, string header)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
            this.headerKey = headerKey;
            this.header = header;
        }

        /// <inheritdoc/>
        public async Task<IHeaderKeyWriter> Write()
        {
            return await Task.FromResult(new HeaderKeyWriter(httpClient, httpMethod, url, headers, headerKey, header, false)).ConfigureAwait(false);
        }
    }

    internal sealed class BodyWriter : IBodyWriter
    {
        private readonly IHttpClient httpClient;
        private readonly HttpMethod httpMethod;
        private readonly string url;
        private readonly IEnumerable<Tuple<string, string>> headers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="httpMethod"/> or <paramref name="url"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is not a valid URL</exception>
        public BodyWriter(IHttpClient httpClient, HttpMethod httpMethod, string url, IEnumerable<Tuple<string, string>> headers)
        {
            this.httpClient = httpClient;
            this.httpMethod = httpMethod;
            this.url = url;
            this.headers = headers;
        }

        /// <inheritdoc/>
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
