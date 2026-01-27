using System;

namespace OddTrotter.Odata.v4_01.Reader.RequestReader
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

    internal interface IUrlDomainReader //// TODO the domain actually isn't in the HTTP request, it's in the ip request
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
