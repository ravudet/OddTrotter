using System.Threading.Tasks;

namespace OddTrotter.Odata.v4_01.Reader.RequestWriter.RequestWriter
{
    internal interface IRequestWriter
    {
        IVerbWriter Write();
    }

    internal interface IVerbWriter
    {
        IUrlWriter Write(HttpVerb httpVerb); //// TODO this should probably be dependent on the verb (e.g. get requests don't have a body)
    }

    internal interface IUrlWriter
    {
        IUrlSchemeWriter Write();
    }

    internal interface IUrlSchemeWriter
    {
        IUrlDomainWriter Write(UrlScheme urlScheme);
    }

    internal interface IUrlDomainWriter
    {
        IUrlPathWriter Write(UrlDomain urlDomain);
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

    internal interface IUrlQueryWriter
    {
        IHeadersWriter WriteHeaders();

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

    internal interface IUrlQueryValueWriter
    {
        IUrlQueryWriter Write(UrlQueryValue urlQueryValue);
    }

    internal interface IHeadersWriter
    {
        IBodyWriter Write();

        IHeaderWriter WriteHeader();
    }

    internal interface IHeaderWriter
    {
        IHeaderKvpWriter Write();
    }

    internal interface IHeaderKvpWriter
    {
        IHeaderKeyWriter Write(HeaderKey headerKey);
    }

    internal interface IHeaderKeyWriter
    {
        IHeadersWriter Write();

        IHeaderValueWriter Write(HeaderValue headerValue);
    }

    internal interface IHeaderValueWriter
    {
        IHeaderKeyWriter Write();
    }

    internal interface IBodyWriter
    {
        //// TODO add the methods to write the body

        Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Send();
    }
}
