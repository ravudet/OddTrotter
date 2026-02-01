namespace OddTrotter.Odata.v4_01.Reader.RequestWriter
{
    using System.Threading.Tasks;

    internal interface IRequestWriter
    {
        Task<IVerbWriter> Write();
    }

    internal interface IVerbWriter
    {
        Task<IUrlWriter> Write(HttpVerb httpVerb); //// TODO this should probably be dependent on the verb (e.g. get requests don't have a body)
    }

    internal interface IUrlWriter
    {
        Task<IUrlSchemeWriter> Write();
    }

    internal interface IUrlSchemeWriter
    {
        Task<IUrlDomainWriter> Write(UrlScheme urlScheme);
    }

    internal interface IUrlDomainWriter
    {
        Task<IUrlPathWriter> Write(UrlDomain urlDomain);
    }

    internal interface IUrlPathWriter
    {
        Task<IUrlQueryWriter> Write();

        Task<IUrlPathSegmentWriter> WriteSegment();
    }

    internal interface IUrlPathSegmentWriter
    {
        Task<IUrlPathWriter> Write(UrlPathSegment urlPathSegment);
    }

    internal interface IUrlQueryWriter
    {
        Task<IHeadersWriter> WriteHeaders();

        Task<IUrlQueryKvpWriter> Write();
    }

    internal interface IUrlQueryKvpWriter
    {
        Task<IUrlQueryNameWriter> Write(UrlQueryName urlQueryName);
    }

    internal interface IUrlQueryNameWriter
    {
        Task<IUrlQueryWriter> Write();

        Task<IUrlQueryValueWriter> WriteValue();
    }

    internal interface IUrlQueryValueWriter
    {
        Task<IUrlQueryWriter> Write(UrlQueryValue urlQueryValue);
    }

    internal interface IHeadersWriter
    {
        Task<IBodyWriter> Write();

        Task<IHeaderWriter> WriteHeader();
    }

    internal interface IHeaderWriter
    {
        Task<IHeaderKvpWriter> Write();
    }

    internal interface IHeaderKvpWriter
    {
        Task<IHeaderKeyWriter> Write(HeaderKey headerKey);
    }

    internal interface IHeaderKeyWriter
    {
        Task<IHeadersWriter> Write();

        Task<IHeaderValueWriter> Write(HeaderValue headerValue);
    }

    internal interface IHeaderValueWriter
    {
        Task<IHeaderKeyWriter> Write();
    }

    internal interface IBodyWriter
    {
        //// TODO add the methods to write the body

        Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Send();
    }
}
