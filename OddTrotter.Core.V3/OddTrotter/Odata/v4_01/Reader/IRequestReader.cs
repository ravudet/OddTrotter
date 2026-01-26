using System;

namespace OddTrotter.Odata.v4_01.Reader
{
    internal interface IRequestReader
    {
        IVerbReader Read();
    }

    internal interface IVerbReader
    {
        IUrlReader Read(out HttpVerb httpVerb);
    }

    internal interface IUrlReader
    {
        IUrlSchemeReader Read();
    }

    internal interface IUrlSchemeReader
    {
        IUrlDomainReader Read(out UrlScheme urlScheme);
    }

    internal interface IUrlDomainReader
    {
        IUrlPathReader Read(out UrlDomain urlDomain);
    }

    internal interface IUrlPathReader
    {
        UrlPathToken Read();
    }

    internal abstract class UrlPathToken
    {
        private UrlPathToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<UrlPathToken.PathSegment, TResult> pathSegmentMap,
            Func<UrlPathToken.Query, TResult> queryMap)
        {
            if (this is UrlPathToken.PathSegment pathSegment)
            {
                return pathSegmentMap(pathSegment);
            }
            else if (this is UrlPathToken.Query query)
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
            internal PathSegment(IUrlPathSegmentReader reader)
            {
                Reader = reader;
            }

            internal IUrlPathSegmentReader Reader { get; }
        }

        public sealed class Query : UrlPathToken
        {
            internal Query(IUrlQueryReader reader)
            {
                Reader = reader;
            }

            internal IUrlQueryReader Reader { get; }
        }
    }

    internal interface IUrlPathSegmentReader
    {
        IUrlPathReader Read(out UrlPathSegment urlPathSegment);
    }

    internal interface IUrlQueryReader
    {
        UrlQueryToken Read();
    }

    internal abstract class UrlQueryToken
    {
        private UrlQueryToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<UrlQueryToken.Kvp, TResult> kvpMap,
            Func<UrlQueryToken.Headers, TResult> headersMap)
        {
            if (this is UrlQueryToken.Kvp kvp)
            {
                return kvpMap(kvp);
            }
            else if (this is UrlQueryToken.Headers headers)
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
            internal Kvp(IUrlQueryKvpReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryKvpReader Reader { get; }
        }

        internal sealed class Headers : UrlQueryToken
        {
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IUrlQueryKvpReader
    {
        IUrlQueryNameReader Read();
    }

    internal interface IUrlQueryNameReader
    {
        UrlQueryNameToken Read(out UrlQueryName urlQueryName);
    }

    internal abstract class UrlQueryNameToken
    {
        private UrlQueryNameToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<UrlQueryNameToken.QueryValue, TResult> queryValueMap,
            Func<UrlQueryNameToken.Query, TResult> queryMap)
        {
            if (this is UrlQueryNameToken.QueryValue queryValue)
            {
                return queryValueMap(queryValue);
            }
            else if (this is UrlQueryNameToken.Query query)
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
            internal QueryValue(IUrlQueryValueReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryValueReader Reader { get; }
        }

        internal sealed class Query : UrlQueryNameToken
        {
            internal Query(IUrlQueryReader reader)
            {
                Reader = reader;
            }

            public IUrlQueryReader Reader { get; }
        }
    }

    internal interface IUrlQueryValueReader
    {
        IUrlQueryReader Read(out UrlQueryValue urlQueryValue);
    }

    internal interface IHeadersReader
    {
        HeadersToken Read();
    }

    internal abstract class HeadersToken
    {
        private HeadersToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<HeadersToken.Header, TResult> headerMap,
            Func<HeadersToken.Body, TResult> bodyMap)
        {
            if (this is HeadersToken.Header header)
            {
                return headerMap(header);
            }
            else if (this is HeadersToken.Body body)
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
            internal Header(IHeaderReader reader)
            {
                Reader = reader;
            }

            public IHeaderReader Reader { get; }
        }

        internal sealed class Body : HeadersToken
        {
            internal Body(IBodyReader reader)
            {
                Reader = reader;
            }

            public IBodyReader Reader { get; }
        }
    }

    internal interface IHeaderReader
    {
        IHeaderKvpReader Read();
    }

    internal interface IHeaderKvpReader
    {
        IHeaderKeyReader Read();
    }

    internal interface IHeaderKeyReader
    {
        HeaderKeyToken Read(out HeaderKey headerKey);
    }

    internal abstract class HeaderKeyToken
    {
        private HeaderKeyToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<HeaderKeyToken.HeaderValue, TResult> headerValueMap,
            Func<HeaderKeyToken.Headers, TResult> headersMap)
        {
            if (this is HeaderKeyToken.HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is HeaderKeyToken.Headers headers)
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
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderKeyToken
        {
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IHeaderValueReader
    {
        HeaderValueToken Read(out HeaderValue headerValue);
    }

    internal abstract class HeaderValueToken
    {
        private HeaderValueToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<HeaderValueToken.HeaderValue, TResult> headerValueMap,
            Func<HeaderValueToken.Headers, TResult> headersMap)
        {
            if (this is HeaderValueToken.HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is HeaderValueToken.Headers headers)
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
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderValueToken
        {
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IBodyReader
    {
        BodyToken Read();
    }

    internal abstract class BodyToken
    {
        private BodyToken()
        {
        }

        internal TResult Apply<TResult>(
            Func<BodyToken.End, TResult> endMap)
        {
            if (this is BodyToken.End end)
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
