namespace OddTrotter.Odata.v4_01.Reader.RequestWriter
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal interface IRequestWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IVerbWriter> Write();
    }

    internal interface IVerbWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpVerb"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlWriter> Write(HttpVerb httpVerb); //// TODO this should probably be dependent on the verb (e.g. get requests don't have a body)
    }

    internal interface IUrlWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlSchemeWriter> Write();
    }

    internal interface IUrlSchemeWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="urlScheme"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlDomainWriter> Write(UrlScheme urlScheme);
    }

    internal interface IUrlDomainWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="urlDomain"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlPathWriter> Write(UrlDomain urlDomain);
    }

    internal interface IUrlPathWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryWriter> Write();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpVerb"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlPathSegmentWriter> WriteSegment();
    }

    internal interface IUrlPathSegmentWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="urlPathSegment"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlPathWriter> Write(UrlPathSegment urlPathSegment);
    }

    internal interface IUrlQueryWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeadersWriter> WriteHeaders();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryKvpWriter> Write();
    }

    internal interface IUrlQueryKvpWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="urlQueryName"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryNameWriter> Write(UrlQueryName urlQueryName);
    }

    internal interface IUrlQueryNameWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryWriter> Write();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryValueWriter> WriteValue();
    }

    internal interface IUrlQueryValueWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="urlQueryValue"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IUrlQueryWriter> Write(UrlQueryValue urlQueryValue);
    }

    internal interface IHeadersWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IBodyWriter> Write();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeaderWriter> WriteHeader();
    }

    internal interface IHeaderWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeaderKvpWriter> Write();
    }

    internal interface IHeaderKvpWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerKey"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeaderKeyWriter> Write(HeaderKey headerKey);
    }

    internal interface IHeaderKeyWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeadersWriter> Write();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerValue"/> is <see langword="null"/></exception>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeaderValueWriter> Write(HeaderValue headerValue);
    }

    internal interface IHeaderValueWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<IHeaderKeyWriter> Write();
    }

    internal interface IBodyWriter
    {
        //// TODO add the methods to write the body

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred writing to the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while sending the payload to the service</exception>
        Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Send();
    }
}
