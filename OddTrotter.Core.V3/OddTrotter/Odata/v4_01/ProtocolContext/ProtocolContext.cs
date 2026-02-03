namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Calendar;
    using OddTrotter.Odata.v4_01.Reader;
    using OddTrotter.Odata.v4_01.Reader.RequestReader;
    using OddTrotter.Odata.v4_01.Reader.RequestWriter;

    internal sealed class ProtocolContext : IProtocolContext
    {
        private readonly Func<HttpRequestMessage, IRequestReader> requestReaderFactory;
        private readonly Func<IRequestWriter> requestWriterFactory;

        internal ProtocolContext(
            Func<HttpRequestMessage, IRequestReader> requestReaderFactory,
            Func<IRequestWriter> requestWriterFactory)
        {
            this.requestReaderFactory = requestReaderFactory;
            this.requestWriterFactory = requestWriterFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<OdataResponse> Send(OdataRequest request)
        {



            //// TODO then go through this file to make sure you've got the exceptiosn handled, and document the `iprotocolcontext` interface to surface reasonable exceptions



            using (var httpRequestMessage = new HttpRequestMessage(new HttpMethod(request.HttpVerb), request.Url))
            {
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Name, header.Value);
                }

                var requestReader = this.requestReaderFactory(httpRequestMessage); //// TODO i think we know that there won't be `ioexception`s coming from the reader because we are controlling the underlying payload (the `httprequestmessage` variable in this method) and so we know that it doesn't have any IO issues
                var requestWriter = this.requestWriterFactory();

                //// TODO you are here
                var responseReader = await ProtocolContext.Transfer(requestReader, requestWriter).ConfigureAwait(false);

                var odataResponseBuilder = new OdataResponseBuilder();

                var statusCodeReader = await responseReader.Read().ConfigureAwait(false);

                var (headersReader, httpStatusCode) = await statusCodeReader.Read().ConfigureAwait(false);
                odataResponseBuilder.HttpStatusCode = httpStatusCode.Value;

                var bodyReader = await ProtocolContext.Read(headersReader, odataResponseBuilder).ConfigureAwait(false);

                odataResponseBuilder = await ProtocolContext.Read(bodyReader, odataResponseBuilder).ConfigureAwait(false);

                return odataResponseBuilder.Build();
            }
        }

        private static async Task<OdataResponseBuilder> Read(OddTrotter.Odata.v4_01.Reader.ResponseReader.IBodyReader bodyReader, OdataResponseBuilder odataResponseBuilder)
        {
            var bodyToken = await bodyReader.Read().ConfigureAwait(false);

            return await bodyToken.Apply(
                async property =>
                {
                    var bodyReader = await ProtocolContext.Read(property.Reader, odataResponseBuilder).ConfigureAwait(false);
                    return await ProtocolContext.Read(bodyReader, odataResponseBuilder).ConfigureAwait(false);
                },
                async end => await Task.FromResult(odataResponseBuilder).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IBodyReader> Read(OddTrotter.Odata.v4_01.Reader.ResponseReader.IPropertyReader propertyReader, OdataResponseBuilder odataResponseBuilder)
        {
            var propertyNameReader = await propertyReader.Read().ConfigureAwait(false);

            var (propertyValueReader, propertyName) = await propertyNameReader.Read().ConfigureAwait(false);

            var propertyValueToken = await propertyValueReader.Read().ConfigureAwait(false);
            return await propertyValueToken.Apply(
                async literal =>
                {
                    var literalToken = await literal.Reader.Read().ConfigureAwait(false);
                    //// TODO what layer do you find out if there are duplicate property names? do you need a new layer for this? the argument for a new layer is this: the existing `protocolcontext` simply takes the readers and makes them into CLR types that mimic a parse tree; is it really that layer's responsibility to ensure that things like duplicate property names are validated? well, the answer to that is "yes" because that's what we've defined as the job of "protocol", but is there something between "protocol" and "reader" that *doesn't* care? //// TODO call it "parsecontext"?
                    return await literalToken.Apply(
                        async @true =>
                        {
                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, "true"));
                            return (await @true.Reader.Read().ConfigureAwait(false)).BodyReader;
                        },
                        async @false =>
                        {
                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, "false"));
                            return (await @false.Reader.Read().ConfigureAwait(false)).BodyReader;
                        },
                        async numberReader =>
                        {
                            var (bodyReader, number) = await numberReader.Reader.Read().ConfigureAwait(false);
                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, number.Value));
                            return bodyReader;
                        }).ConfigureAwait(false);
                },
                async @null =>
                {
                    odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, "null"));
                    return (await @null.Reader.Read().ConfigureAwait(false)).BodyReader;
                },
                async @string =>
                {
                    var (bodyReader, stringToken) = await @string.Reader.Read().ConfigureAwait(false);
                    odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, stringToken.Value));
                    return bodyReader;
                }).ConfigureAwait(false);
        }

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IBodyReader> Read(OddTrotter.Odata.v4_01.Reader.ResponseReader.IHeadersReader headersReader, OdataResponseBuilder odataResponseBuilder)
        {
            var headersToken = await headersReader.Read().ConfigureAwait(false);
            return await headersToken.Apply(
                async header =>
                {
                    var kvpHeaderReader = await header.Reader.Read().ConfigureAwait(false);
                    var headerKeyReader = await kvpHeaderReader.Read().ConfigureAwait(false);
                    var (headerKeyToken, headerKey) = await headerKeyReader.Read().ConfigureAwait(false);
                    return await headerKeyToken.Apply(
                        async headerValue => await ProtocolContext.Read(headerValue.Reader, headerKey, odataResponseBuilder).ConfigureAwait(false),
                        async headers => await ProtocolContext.Read(headers.Reader, odataResponseBuilder).ConfigureAwait(false))
                    .ConfigureAwait(false);
                },
                body => Task.FromResult(body.Reader))
                .ConfigureAwait(false);
        }

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IBodyReader> Read(OddTrotter.Odata.v4_01.Reader.ResponseReader.IHeaderValueReader headerValueReader, HeaderKey headerKey, OdataResponseBuilder odataResponseBuilder)
        {
            var (headerValueToken, headerValue) = await headerValueReader.Read().ConfigureAwait(false);
            odataResponseBuilder.Headers.Add(new HttpHeader(headerKey.Value, headerValue.Value));
            return await headerValueToken.Apply(
                async headerValue => await ProtocolContext.Read(headerValue.Reader, headerKey, odataResponseBuilder).ConfigureAwait(false),
                async headers => await ProtocolContext.Read(headers.Reader, odataResponseBuilder).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private sealed class OdataResponseBuilder
        {
            public string? HttpStatusCode { get; set; }

            public List<HttpHeader> Headers { get; set; } = new List<HttpHeader>();

            public List<OdataProperty> Properties { get; set; } = new List<OdataProperty>();

            public OdataResponse Build()
            {
                ArgumentNullException.ThrowIfNull(this.HttpStatusCode, nameof(this.HttpStatusCode));
                //// TODO other null checks
                
                return new OdataResponse(this.HttpStatusCode, this.Headers, this.Properties);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestReader"></param>
        /// <param name="requestWriter"></param>
        /// <returns></returns>
        /// <exception cref="IOException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Transfer(IRequestReader requestReader, IRequestWriter requestWriter)
        {
            var verbReader = await requestReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            var verbWriter = await requestWriter.Write().ConfigureAwait(false);

            var (urlReader, httpVerb) = await verbReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            var urlWriter = await verbWriter.Write(httpVerb).ConfigureAwait(false);

            var urlSchemeReader = await urlReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            var urlSchemeWriter = await urlWriter.Write().ConfigureAwait(false);

            var (urlDomainReader, urlScheme) = await urlSchemeReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            var urlDomainWriter = await urlSchemeWriter.Write(urlScheme).ConfigureAwait(false);

            var (urlPathReader, urlDomain) = await urlDomainReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            var urlPathWriter = await urlDomainWriter.Write(urlDomain).ConfigureAwait(false);

            var (urlQueryReader, urlQueryWriter) = await ProtocolContext.Transfer(urlPathReader, urlPathWriter).ConfigureAwait(false);

            //// TODO you are here
            var (headersReader, headersWriter) = await ProtocolContext.Transfer(urlQueryReader, urlQueryWriter).ConfigureAwait(false);

            var (bodyReader, bodyWriter) = await ProtocolContext.Transfer(headersReader, headersWriter).ConfigureAwait(false);

            return await ProtocolContext.Transfer(bodyReader, bodyWriter).ConfigureAwait(false);
        }

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Transfer(IBodyReader bodyReader, IBodyWriter bodyWriter)
        {
            var bodyToken = await bodyReader.Read().ConfigureAwait(false);
            return await bodyToken
                .Apply(
                    async end => await bodyWriter.Send().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private static async Task<(IBodyReader BodyReader, IBodyWriter BodyWriter)> Transfer(
            IHeadersReader headersReader,
            IHeadersWriter headersWriter)
        {
            var headersToken = await headersReader.Read().ConfigureAwait(false);
            return await headersToken.Apply(
                async header =>
                {
                    var headerWriter = await headersWriter.WriteHeader().ConfigureAwait(false);

                    var headerKvpReader = await header.Reader.Read().ConfigureAwait(false);
                    var headerKvpWriter = await headerWriter.Write().ConfigureAwait(false);

                    var headerKeyReader = await headerKvpReader.Read().ConfigureAwait(false);
                    var (headerKeyToken, headerKey) = await headerKeyReader.Read().ConfigureAwait(false);
                    var headerKeyWriter = await headerKvpWriter.Write(headerKey).ConfigureAwait(false);

                    return await headerKeyToken.Apply(
                        async headerValue =>
                        {
                            return await ProtocolContext.Transfer(headerValue.Reader, headerKeyWriter).ConfigureAwait(false);
                        },
                        async headers =>
                        {
                            return await ProtocolContext.Transfer(headers.Reader, await headerKeyWriter.Write().ConfigureAwait(false)).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                },
                async body =>
                {
                    return (body.Reader, await headersWriter.Write().ConfigureAwait(false));
                }).ConfigureAwait(false);
        }

        private static async Task<(IBodyReader BodyReader, IBodyWriter BodyWriter)> Transfer(
            IHeaderValueReader headerValueReader,
            IHeaderKeyWriter headerKeyWriter)
        {
            var (headerValueToken, headerValue) = await headerValueReader.Read().ConfigureAwait(false);
            var headerValueWriter = await headerKeyWriter.Write(headerValue).ConfigureAwait(false);

            return await headerValueToken.Apply(
                async headerValueReader =>
                {
                    return await ProtocolContext.Transfer(headerValueReader.Reader, await headerValueWriter.Write().ConfigureAwait(false)).ConfigureAwait(false);
                },
                async headers =>
                {
                    return await ProtocolContext.Transfer(headers.Reader, await (await headerValueWriter.Write().ConfigureAwait(false)).Write().ConfigureAwait(false)).ConfigureAwait(false);
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlQueryReader"></param>
        /// <param name="urlQueryWriter"></param>
        /// <returns></returns>
        /// <exception cref="IOException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(IHeadersReader HeadersReader, IHeadersWriter HeadersWriter)> Transfer(
            IUrlQueryReader urlQueryReader,
            IUrlQueryWriter urlQueryWriter)
        {
            var urlQueryToken = await urlQueryReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await urlQueryToken.Apply(
                async kvp =>
                {
                    var urlQueryKvpWriter = await urlQueryWriter.Write().ConfigureAwait(false);

                    var urlQueryNameReader = await kvp.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    var (urlQueryNameToken, urlQueryName) = await urlQueryNameReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    var urlQueryNameWriter = await urlQueryKvpWriter.Write(urlQueryName).ConfigureAwait(false);

                    return await urlQueryNameToken.Apply(
                        async queryValue =>
                        {
                            var (newUrlQueryReader, urlQueryValue) = await queryValue.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control

                            var urlQueryValueWriter = await urlQueryNameWriter.WriteValue().ConfigureAwait(false);
                            var newUrlQueryWriter = await urlQueryValueWriter.Write(urlQueryValue).ConfigureAwait(false);

                            return await ProtocolContext.Transfer(newUrlQueryReader, newUrlQueryWriter).ConfigureAwait(false);
                        },
                        async query =>
                        {
                            return await ProtocolContext.Transfer(query.Reader, await urlQueryNameWriter.Write().ConfigureAwait(false)).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                },
                async headers =>
                {
                    return (headers.Reader, await urlQueryWriter.WriteHeaders().ConfigureAwait(false));
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlPathReader"></param>
        /// <param name="urlPathWriter"></param>
        /// <returns></returns>
        /// <exception cref="IOException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(IUrlQueryReader UrlQueryReader, IUrlQueryWriter UrlQueryWriter)> Transfer(
            IUrlPathReader urlPathReader, 
            IUrlPathWriter urlPathWriter)
        {
            var urlPathToken = await urlPathReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await urlPathToken.Apply(
                async pathSegment =>
                {
                    var (newUrlPathReader, urlPathSegment) = await pathSegment.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    var urlPathSegmentWriter = await urlPathWriter.WriteSegment().ConfigureAwait(false);
                    var newUrlPathWriter = await urlPathSegmentWriter.Write(urlPathSegment).ConfigureAwait(false);

                    return await ProtocolContext.Transfer(newUrlPathReader, newUrlPathWriter).ConfigureAwait(false);
                },
                async query =>
                {
                    return (query.Reader, await urlPathWriter.Write().ConfigureAwait(false));
                }).ConfigureAwait(false);
        }
    }
}
