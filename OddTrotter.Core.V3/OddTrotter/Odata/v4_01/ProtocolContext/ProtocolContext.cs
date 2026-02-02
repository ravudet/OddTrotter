namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Collections.Generic;
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

        public async Task<OdataResponse> Send(OdataRequest request)
        {

            //// TODO go through todos in this file
            

            //// TODO you need to know what exceptions are being thrown


            using (var httpRequestMessage = new HttpRequestMessage(new HttpMethod(request.HttpVerb), request.Url))
            {
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Name, header.Value);
                }

                var requestReader = this.requestReaderFactory(httpRequestMessage);
                var requestWriter = this.requestWriterFactory();

                var responseReader = await ProtocolContext.Transfer(requestReader, requestWriter).ConfigureAwait(false);

                var odataResponseBuilder = new OdataResponseBuilder();

                var statusCodeReader = responseReader.Read();

                var headersReader = statusCodeReader.Read(out var httpStatusCode);
                odataResponseBuilder.HttpStatusCode = httpStatusCode.Value;

                var bodyReader = await ProtocolContext.Read(headersReader, odataResponseBuilder).ConfigureAwait(false);

                odataResponseBuilder = ProtocolContext.Read(bodyReader, odataResponseBuilder);

                return odataResponseBuilder.Build();
            }
        }

        private static async Task<OdataResponseBuilder> Read(OddTrotter.Odata.v4_01.Reader.ResponseReader.IBodyReader bodyReader, OdataResponseBuilder odataResponseBuilder)
        {
            //// TODO because you can read and write to streams, should the `read` and `write` methods all be async?
            
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
            return await headersToken.Apply( //// TODO the apply methods need a `context` parameter so you can pass the builder; the builder likely should be a `ref struct` passed by `ref`
                async header =>
                {
                    var kvpHeaderReader = header.Reader.Read();
                    var headerKeyReader = kvpHeaderReader.Read();
                    var headerKeyToken = headerKeyReader.Read(out var headerKey);
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
            var headerValueToken = headerValueReader.Read(out var headerValue);
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

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Transfer(IRequestReader requestReader, IRequestWriter requestWriter)
        {
            //// TODO you could have ioexceptions while reading
            //// TODO you could have ioexceptions while writing
            //// TODO you could have invalid syntax when reading
            //// TODO you could have network issues when sending
            //// TODO can you have network issues when writing? or will those end up as ioexceptions? //// TODO you set up PlaygourndTests.ReadingFromDeadNetworkStream to demonstrate this

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

            var (urlQueryReader, urlQueryWriter) = ProtocolContext.Transfer(urlPathReader, urlPathWriter);

            var (headersReader, headersWriter) = ProtocolContext.Transfer(urlQueryReader, urlQueryWriter);

            var (bodyReader, bodyWriter) = ProtocolContext.Transfer(headersReader, headersWriter);

            return await ProtocolContext.Transfer(bodyReader, bodyWriter).ConfigureAwait(false);
        }

        private static async Task<OddTrotter.Odata.v4_01.Reader.ResponseReader.IResponseReader> Transfer(IBodyReader bodyReader, IBodyWriter bodyWriter)
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
                            return ProtocolContext.Transfer(headerValue.Reader, headerKeyWriter);
                        },
                        headers =>
                        {
                            return ProtocolContext.Transfer(headers.Reader, headerKeyWriter.Write());
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
                    return ProtocolContext.Transfer(headerValueReader.Reader, headerValueWriter.Write());
                },
                headers =>
                {
                    return ProtocolContext.Transfer(headers.Reader, headerValueWriter.Write().Write());
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

                            return ProtocolContext.Transfer(newUrlQueryReader, newUrlQueryWriter);
                        },
                        query =>
                        {
                            return ProtocolContext.Transfer(query.Reader, urlQueryNameWriter.Write());
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

                    return ProtocolContext.Transfer(newUrlPathReader, newUrlPathWriter);
                },
                query =>
                {
                    return (query.Reader, urlPathWriter.Write());
                });
        }
    }
}
