namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;

    internal sealed class HttpClientConventionContext : IProtocolContext //// TODO call this one weak and the other one strong? (as in weak typing and strong typing?)
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

        public async Task<OdataResponse> GetCollection(OdataRequest request)
        {

            //// TODO then create the below breakdown and then create the responsereader interfaces and then implement the responsereader and then finish this implementation



            //// TODO protocol should mean syntax and convention should mean semantics (i.e. it was syntactically an odata response (protocol), but semantically, it was supposed to be a collection, and it wasn't (convention)) //// TODO i think this is actually delineated by when you apply the edm model to the validity of the request




            //// TODO i think you should have iprotocolcontext which takes odatarequest and returns odataresponse, basically using the readers to create a parse tree
            //// TODO then you should have weakconvention and strongconvention; weak should check things like "we were give a get collection request but received a single-valued response" and strong should use a generic type parameter
            //// TODO make the note that protocol is about syntax and convention is about semantics
            //// TODO you still have the question of when you should apply somehting like an actual instace of iedmmodel or whatever



            //// TODO do you want a level below this that has an odatarequest and an odataresponse?

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.Url))
            {
                var requestReader = this.requestReaderFactory(httpRequestMessage);
                var requestWriter = this.requestWriterFactory();

                var responseReader = await HttpClientConventionContext.Transfer(requestReader, requestWriter).ConfigureAwait(false);
            }
        }

        private static async Task<IResponseReader> Transfer(IRequestReader requestReader, IRequestWriter requestWriter)
        {
            //// TODO you could have ioexceptions while reading
            //// TODO you could have ioexceptions while writing
            //// TODO you could have invalid syntax when reading
            //// TODO you could have network issues when sending

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

            var (urlQueryReader, urlQueryWriter) = HttpClientConventionContext.Transfer(urlPathReader, urlPathWriter);

            var (headersReader, headersWriter) = HttpClientConventionContext.Transfer(urlQueryReader, urlQueryWriter);

            var (bodyReader, bodyWriter) = HttpClientConventionContext.Transfer(headersReader, headersWriter);

            return await HttpClientConventionContext.Transfer(bodyReader, bodyWriter).ConfigureAwait(false);
        }

        private static async Task<IResponseReader> Transfer(IBodyReader bodyReader, IBodyWriter bodyWriter)
        {
            var bodyToken = bodyReader.Read();
            return await bodyToken
                .Apply(
                    async end => await bodyWriter.Send().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private static (IBodyReader BodyReader, IBodyWriter BodyWriter) Transfer(
            IHeadersReader headersReader,
            IHeadersWriter headersWriter)
        {
            var headersToken = headersReader.Read();
            return headersToken.Apply(
                header =>
                {
                    var headerWriter = headersWriter.WriteHeader();

                    var headerKvpReader = header.Reader.Read();
                    var headerKvpWriter = headerWriter.Write();

                    var headerKeyReader = headerKvpReader.Read();
                    var headerKeyToken = headerKeyReader.Read(out var headerKey);
                    var headerKeyWriter = headerKvpWriter.Write(headerKey);

                    return headerKeyToken.Apply(
                        headerValue =>
                        {
                            return HttpClientConventionContext.Transfer(headerValue.Reader, headerKeyWriter);
                        },
                        headers =>
                        {
                            return HttpClientConventionContext.Transfer(headers.Reader, headerKeyWriter.Write());
                        });
                },
                body =>
                {
                    return (body.Reader, headersWriter.Write());
                });
        }

        private static (IBodyReader BodyReader, IBodyWriter BodyWriter) Transfer(
            IHeaderValueReader headerValueReader,
            IHeaderKeyWriter headerKeyWriter)
        {
            var headerValueToken = headerValueReader.Read(out var headerValue);
            var headerValueWriter = headerKeyWriter.Write(headerValue);

            return headerValueToken.Apply(
                headerValueReader =>
                {
                    return HttpClientConventionContext.Transfer(headerValueReader.Reader, headerValueWriter.Write());
                },
                headers =>
                {
                    return HttpClientConventionContext.Transfer(headers.Reader, headerValueWriter.Write().Write());
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
                });
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
    }
}
