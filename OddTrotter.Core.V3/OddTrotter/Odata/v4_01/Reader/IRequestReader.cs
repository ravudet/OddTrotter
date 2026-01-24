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
    }

    internal interface IUrlSchemeReader
    {
    }

    internal interface IUrlDomainReader
    {
    }

    internal interface IUrlPathReader
    {
    }

    internal interface IUrlPathSegmentReader
    {
    }

    internal interface IUrlQueryReader
    {
    }

    internal interface IUrlQueryKvpReader
    {
    }

    internal interface IUrlQueryNameReader
    {
    }

    internal interface IUrlQueryValueReader
    {
    }

    internal interface IHeadersReader
    {
    }

    internal interface IBodyReader
    {
    }
}
