namespace OddTrotter.Odata.v4_01.Reader
{
    using System;
    using System.Net.Http;

    internal sealed class RequestWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;

        public RequestWriter(Func<HttpMethod, string, T> factory)
        {
            this.factory = factory;
        }

        public GetRequestWriter<T> WriteGet()
        {
            return new GetRequestWriter<T>(this.factory);
        }
    }

    internal sealed class GetRequestWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;

        public GetRequestWriter(Func<HttpMethod, string, T> factory)
        {
            this.factory = factory;
        }

        public UrlWriter<T> Write()
        {
            return new UrlWriter<T>(this.factory, HttpMethod.Get);
        }
    }

    internal sealed class UrlWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;

        public UrlWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
        }

        public UrlSchemeWriter<T> Write()
        {
            return new UrlSchemeWriter<T>(this.factory, this.httpMethod);
        }
    }

    internal sealed class UrlSchemeWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;

        public UrlSchemeWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
        }

        public UrlDomainWriter<T> Write(UrlScheme urlScheme)
        {
            return new UrlDomainWriter<T>(this.factory, this.httpMethod, urlScheme.Value);
        }
    }

    internal sealed class UrlScheme
    {
        internal UrlScheme(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal sealed class UrlDomainWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlDomainWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlPathWriter<T> Write(UrlDomain urlDomain)
        {
            return new UrlPathWriter<T>(this.factory, this.httpMethod, this.url + urlDomain.Value);
        }
    }

    internal sealed class UrlDomain
    {
        internal UrlDomain(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal sealed class UrlPathWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlPathWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlQueryWriter<T> Write()
        {
            return new UrlQueryWriter<T>(this.factory, this.httpMethod, this.url);
        }

        public UrlPathSegmentWriter<T> WriteSegment()
        {
            return new UrlPathSegmentWriter<T>(this.factory, this.httpMethod, this.url);
        }
    }

    internal sealed class UrlPathSegmentWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlPathSegmentWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlPathWriter<T> Write(UrlPathSegment urlPathSegment)
        {
            return new UrlPathWriter<T>(this.factory, this.httpMethod, this.url + '/' + urlPathSegment.Value);
        }
    }

    internal sealed class UrlPathSegment
    {
        internal UrlPathSegment(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal sealed class UrlQueryWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public T Write()
        {
            return this.factory(this.httpMethod, this.url);
        }

        public UrlQueryKvpWriter<T> WriteKvp()
        {
            return new UrlQueryKvpWriter<T>(this.factory, this.httpMethod, this.url + '?');
        }
    }

    internal sealed class UrlQueryKvpWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryKvpWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlQueryNameWriter<T> WriteName(UrlQueryName urlQueryName)
        {
            return new UrlQueryNameWriter<T>(this.factory, this.httpMethod, this.url + urlQueryName.Value);
        }
    }

    internal sealed class UrlQueryNameWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryNameWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlQueryWriter<T> Write()
        {
            return new UrlQueryWriter<T>(this.factory, this.httpMethod, this.url);
        }

        public UrlQueryValueWriter<T> WriteValue()
        {
            return new UrlQueryValueWriter<T>(this.factory, this.httpMethod, this.url + '=');
        }
    }

    internal sealed class UrlQueryName
    {
        internal UrlQueryName(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal sealed class UrlQueryValueWriter<T>
    {
        private readonly Func<HttpMethod, string, T> factory;
        private readonly HttpMethod httpMethod;
        private readonly string url;

        public UrlQueryValueWriter(Func<HttpMethod, string, T> factory, HttpMethod httpMethod, string url)
        {
            this.factory = factory;
            this.httpMethod = httpMethod;
            this.url = url;
        }

        public UrlQueryWriter<T> Write(UrlQueryValue urlQueryValue)
        {
            return new UrlQueryWriter<T>(this.factory, this.httpMethod, this.url + urlQueryValue.Value);
        }
    }

    internal sealed class UrlQueryValue
    {
        internal UrlQueryValue(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }
}
