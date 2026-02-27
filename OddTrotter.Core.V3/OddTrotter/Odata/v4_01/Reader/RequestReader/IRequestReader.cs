namespace OddTrotter.Odata.v4_01.Reader.RequestReader
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal interface IRequestReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IVerbReader> Read();
    }

    internal interface IVerbReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IUrlReader UrlReader, HttpVerb HttpVerb)> Read();
    }

    internal interface IUrlReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IUrlSchemeReader> Read();
    }

    internal interface IUrlSchemeReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IUrlDomainReader UrlDomainReader, UrlScheme UrlScheme)> Read();
    }

    internal interface IUrlDomainReader //// TODO the domain actually isn't in the HTTP request, it's in the ip request
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IUrlPathReader UrlPathReader, UrlDomain UrlDomain)> Read();
    }

    internal interface IUrlPathReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<UrlPathToken> Read();
    }

    internal abstract class UrlPathToken
    {
        private UrlPathToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="pathSegmentMap"></param>
        /// <param name="queryMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pathSegmentMap"/> or <paramref name="queryMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<PathSegment, TResult> pathSegmentMap,
            Func<Query, TResult> queryMap)
        {
            if (this is PathSegment pathSegment)
            {
                return pathSegmentMap(pathSegment);
            }
            else if (this is Query query)
            {
                return queryMap(query);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        public sealed class PathSegment : UrlPathToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal PathSegment(IUrlPathSegmentReader reader)
            {
                Reader = reader;
            }

            internal IUrlPathSegmentReader Reader { get; }
        }

        public sealed class Query : UrlPathToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Query(IUrlQueryReader reader)
            {
                Reader = reader;
            }

            internal IUrlQueryReader Reader { get; }
        }
    }

    internal interface IUrlPathSegmentReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IUrlPathReader UrlPathReader, UrlPathSegment UrlPathSegment)> Read();
    }

    internal interface IUrlQueryReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<UrlQueryToken> Read();
    }

    internal abstract class UrlQueryToken
    {
        private UrlQueryToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="kvpMap"></param>
        /// <param name="headersMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="kvpMap"/> or <paramref name="headersMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<Kvp, TResult> kvpMap,
            Func<Headers, TResult> headersMap)
        {
            if (this is Kvp kvp)
            {
                return kvpMap(kvp);
            }
            else if (this is Headers headers)
            {
                return headersMap(headers);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class Kvp : UrlQueryToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Kvp(IUrlQueryKvpReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryKvpReader Reader { get; }
        }

        internal sealed class Headers : UrlQueryToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IUrlQueryKvpReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IUrlQueryNameReader> Read();
    }

    internal interface IUrlQueryNameReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(UrlQueryNameToken UrlQueryNameToken, UrlQueryName UrlQueryName)> Read();
    }

    internal abstract class UrlQueryNameToken
    {
        private UrlQueryNameToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryValueMap"></param>
        /// <param name="queryMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryValueMap"/> or <paramref name="queryMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<QueryValue, TResult> queryValueMap,
            Func<Query, TResult> queryMap)
        {
            if (this is QueryValue queryValue)
            {
                return queryValueMap(queryValue);
            }
            else if (this is Query query)
            {
                return queryMap(query);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class QueryValue : UrlQueryNameToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal QueryValue(IUrlQueryValueReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryValueReader Reader { get; }
        }

        internal sealed class Query : UrlQueryNameToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Query(IUrlQueryReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryReader Reader { get; }
        }
    }

    internal interface IUrlQueryValueReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IUrlQueryReader UrlQueryReader, UrlQueryValue UrlQueryValue)> Read();
    }

    internal interface IHeadersReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<HeadersToken> Read();
    }

    internal abstract class HeadersToken
    {
        private HeadersToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerMap"></param>
        /// <param name="bodyMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerMap"/> or <paramref name="bodyMap"/> is <see langword="null"/></exception>`
        internal TResult Apply<TResult>(
            Func<Header, TResult> headerMap,
            Func<Body, TResult> bodyMap)
        {
            if (this is Header header)
            {
                return headerMap(header);
            }
            else if (this is Body body)
            {
                return bodyMap(body);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class Header : HeadersToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Header(IHeaderReader reader)
            {
                Reader = reader;
            }

            public IHeaderReader Reader { get; }
        }

        internal sealed class Body : HeadersToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Body(IBodyReader reader)
            {
                Reader = reader;
            }

            public IBodyReader Reader { get; }
        }
    }

    internal interface IHeaderReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKvpReader> Read();
    }

    internal interface IHeaderKvpReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKeyReader> Read();
    }

    internal interface IHeaderKeyReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderKeyToken HeaderKeyToken, HeaderKey HeaderKey)> Read();
    }

    internal abstract class HeaderKeyToken
    {
        private HeaderKeyToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerValueMap"></param>
        /// <param name="headersMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerValueMap"/> or <paramref name="headersMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<HeaderValue, TResult> headerValueMap,
            Func<Headers, TResult> headersMap)
        {
            if (this is HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is Headers headers)
            {
                return headersMap(headers);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class HeaderValue : HeaderKeyToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderKeyToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IHeaderValueReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderValueToken HeaderValueToken, HeaderValue HeaderValue)> Read();
    }

    internal abstract class HeaderValueToken
    {
        private HeaderValueToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerValueMap"></param>
        /// <param name="headersMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerValueMap"/> or <paramref name="headersMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<HeaderValue, TResult> headerValueMap,
            Func<Headers, TResult> headersMap)
        {
            if (this is HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is Headers headers)
            {
                return headersMap(headers);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class HeaderValue : HeaderValueToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderValueToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IBodyReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<BodyToken> Read();
    }

    internal abstract class BodyToken
    {
        private BodyToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="endMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<End, TResult> endMap)
        {
            if (this is End end)
            {
                return endMap(end);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class End : BodyToken
        {
            private End()
            {
            }

            public static End Instance { get; } = new End();
        }
    }
}
