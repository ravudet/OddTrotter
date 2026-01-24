using System.Threading.Tasks;

namespace OddTrotter.Odata.v4_01.Reader
{
    internal interface IRequestWriter
    {
        IVerbWriter Write();
    }

    internal interface IVerbWriter
    {
        IUrlWriter Write(HttpVerb httpVerb);
    }

    internal sealed class HttpVerb
    {
        internal HttpVerb(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal interface IUrlWriter
    {
        IUrlSchemeWriter Write();
    }

    internal interface IUrlSchemeWriter
    {
        IUrlDomainWriter Write(UrlScheme urlScheme);
    }

    internal sealed class UrlScheme
    {
        internal UrlScheme(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal interface IUrlDomainWriter
    {
        IUrlPathWriter Write(UrlDomain urlDomain);
    }

    internal sealed class UrlDomain
    {
        internal UrlDomain(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal interface IUrlPathWriter
    {
        IUrlQueryWriter Write();

        IUrlPathSegmentWriter WriteSegment();
    }

    internal interface IUrlPathSegmentWriter
    {
        IUrlPathWriter Write(UrlPathSegment urlPathSegment);
    }

    internal sealed class UrlPathSegment
    {
        internal UrlPathSegment(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal interface IUrlQueryWriter
    {
        Task<IResponseReader> Send();

        IUrlQueryKvpWriter Write();
    }

    internal interface IUrlQueryKvpWriter
    {
        IUrlQueryNameWriter Write(UrlQueryName urlQueryName);
    }

    internal interface IUrlQueryNameWriter
    {
        IUrlQueryWriter Write();

        IUrlQueryValueWriter WriteValue();
    }

    internal sealed class UrlQueryName
    {
        internal UrlQueryName(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    internal interface IUrlQueryValueWriter
    {
        IUrlQueryWriter Write(UrlQueryValue urlQueryValue);
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
