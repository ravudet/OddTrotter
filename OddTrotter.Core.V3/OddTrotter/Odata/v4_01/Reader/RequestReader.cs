namespace OddTrotter.Odata.v4_01.Reader
{
    using System.Net.Http;

    internal sealed class RequestReader
    {
        private readonly HttpRequestMessage httpRequestMessage;

        internal RequestReader(HttpRequestMessage httpRequestMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
        }

        public VerbReader Read()
        {
            return new VerbReader();
        }
    }

    internal sealed class VerbReader
    {
    }

    internal sealed class UrlReader
    {
    }

    internal sealed class UrlSchemeReader
    {
    }

    internal sealed class UrlDomainReader
    {
    }

    internal sealed class UrlPathReader
    {
    }

    internal sealed class UrlPathSegmentReader
    {
    }

    internal sealed class UrlQueryReader
    {
    }

    internal sealed class UrlQueryKvpReader
    {
    }

    internal sealed class UrlQueryNameReader
    {
    }

    internal sealed class UrlQueryValueReader
    {
    }

    internal sealed class HeadersReader
    {
    }

    internal sealed class BodyReader
    {
    }
}
