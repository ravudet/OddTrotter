namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;

    internal sealed class HttpClientConventionContext : IConventionContext //// TODO call this one weak and the other one strong? (as in weak typing and strong typing?)
    {
        private readonly Func<HttpRequestMessage, IRequestReader> requestReaderFactory;
        private readonly Func<IRequestWriter> requestWriterFactory;

        internal HttpClientConventionContext(
            Func<HttpRequestMessage, IRequestReader> requestReaderFactory,
            Func<IRequestWriter> requestWriterFactory)
        {
            this.requestReaderFactory = requestReaderFactory;
            this.requestWriterFactory = requestWriterFactory;
        }

        public async Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            //// TODO protocol should mean syntax and convention should mean semantics (i.e. it was syntactically an odata response (protocol), but semantically, it was supposed to be a collection, and it wasn't (convention))


            //// TODO do you want a level below this that has an odatarequest and an odataresponse?

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.Url))
            {
                //// TODO headers




                var requestReader = this.requestReaderFactory(httpRequestMessage);
                var requestWriter = this.requestWriterFactory();

                var responseReader = await HttpClientConventionContext.Transfer(requestReader, requestWriter).ConfigureAwait(false);
            }
        }

        private static Task<IResponseReader> Transfer(IRequestReader requestReader, IRequestWriter requestWriter)
        {
            var verbReader = requestReader.Read();
            var verbWriter = requestWriter.Write();

            var urlReader = verbReader.Read(out var httpVerb);
            var urlWriter = verbWriter.Write(httpVerb);

            var urlSchemeReader = urlReader.Read();
            var urlSchemeWriter = urlWriter.Write();

            var urlDomainReader = urlSchemeReader.Read(out var urlScheme);
            var urlDomainWriter = urlSchemeWriter.Write(urlScheme);

            var urlPathReader = urlDomainReader.Read(out var urlDomain);
            var urlPathWriter = urlDomainWriter.Write(urlDomain);

            var (urlQueryReader, urlQueryWriter) = Transfer(urlPathReader, urlPathWriter);

            var (headersReader, headersWriter) = Transfer(urlQueryReader, urlQueryWriter);


        }

        private static (IUrlQueryReader UrlQueryReader, IUrlQueryWriter UrlQueryWriter) Transfer(
            IUrlPathReader urlPathReader, 
            IUrlPathWriter urlPathWriter)
        {
            var urlPathToken = urlPathReader.Read();
            return urlPathToken.Apply(
                pathSegment =>
                {
                    var newUrlPathReader = pathSegment.Reader.Read(out var urlPathSegment);
                    var urlPathSegmentWriter = urlPathWriter.WriteSegment();
                    var newUrlPathWriter = urlPathSegmentWriter.Write(urlPathSegment);

                    return HttpClientConventionContext.Transfer(newUrlPathReader, newUrlPathWriter);
                },
                query =>
                {
                    return (query.Reader, urlPathWriter.Write());
                });
        }

        private static (IHeadersReader HeadersReader, IHeadersWriter HeadersWriter) Transfer(
            IUrlQueryReader urlQueryReader,
            IUrlQueryWriter urlQueryWriter)
        {
            var urlQueryToken = urlQueryReader.Read();
            return urlQueryToken.Apply(
                kvp =>
                {
                    var urlQueryKvpWriter = urlQueryWriter.Write();

                    var urlQueryNameReader = kvp.Reader.Read();
                    var urlQueryNameToken = urlQueryNameReader.Read(out var urlQueryName);
                    var urlQueryNameWriter = urlQueryKvpWriter.Write(urlQueryName);

                    return urlQueryNameToken.Apply(
                        queryValue =>
                        {
                            var newUrlQueryReader = queryValue.Reader.Read(out var urlQueryValue);

                            var urlQueryValueWriter = urlQueryNameWriter.WriteValue();
                            var newUrlQueryWriter = urlQueryValueWriter.Write(urlQueryValue);

                            return HttpClientConventionContext.Transfer(newUrlQueryReader, newUrlQueryWriter);
                        },
                        query =>
                        {
                            return HttpClientConventionContext.Transfer(query.Reader, urlQueryNameWriter.Write());
                        });
                },
                headers =>
                {
                    return (headers.Reader, urlQueryWriter.WriteHeaders());
                })
        }
    }
}
