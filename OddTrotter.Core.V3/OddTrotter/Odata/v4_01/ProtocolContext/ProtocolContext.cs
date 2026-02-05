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

    using Protocol = OddTrotter.Odata.v4_01.ProtocolContext;
    using Reader = OddTrotter.Odata.v4_01.Reader;
    using Request = OddTrotter.Odata.v4_01.Reader.RequestReader;
    using Response = OddTrotter.Odata.v4_01.Reader.ResponseReader;

    internal sealed class ProtocolContext : IProtocolContext
    {
        private readonly Func<HttpRequestMessage, Request.IRequestReader> requestReaderFactory;
        private readonly Func<IRequestWriter> requestWriterFactory;

        internal ProtocolContext(
            Func<HttpRequestMessage, Request.IRequestReader> requestReaderFactory,
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
        /// <exception cref="Protocol.WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write?
        /// <exception cref="Protocol.ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="Protocol.ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        public async Task<OdataResponse> Send(OdataRequest request)
        {
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

                Response.IStatusCodeReader statusCodeReader;
                try
                {
                    statusCodeReader = await responseReader.Read().ConfigureAwait(false);
                }
                catch (IOException ioException)
                {
                    throw new Protocol.ReadException("TODO", ioException);
                }
                catch (Reader.ReadException readException)
                {
                    throw new Protocol.ProtocolException("TODO", readException);
                }

                Response.StatusCodeToken statusCodeToken;
                HttpStatusCode httpStatusCode;
                try
                {
                    (statusCodeToken, httpStatusCode) = await statusCodeReader.Read().ConfigureAwait(false);
                }
                catch (IOException ioException)
                {
                    throw new Protocol.ReadException("TODO", ioException);
                }
                catch (Reader.ReadException readException)
                {
                    throw new Protocol.ProtocolException("TODO", readException);
                }

                odataResponseBuilder.HttpStatusCode = httpStatusCode.Value;



                var bodyReader = await ProtocolContext.Read(headersReader, odataResponseBuilder).ConfigureAwait(false);

                odataResponseBuilder = await ProtocolContext.Read(bodyReader, odataResponseBuilder).ConfigureAwait(false);

                return odataResponseBuilder.Build();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bodyReader"></param>
        /// <param name="odataResponseBuilder"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="Protocol.ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="Protocol.ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        private static async Task<OdataResponseBuilder> Read(Response.IBodyReader bodyReader, OdataResponseBuilder odataResponseBuilder)
        {
            Response.BodyToken bodyToken;
            try
            {
                bodyToken = await bodyReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

            //// TODO you are here
            return await bodyToken.Apply(
                async property =>
                {
                    //// TODO you are here
                    var bodyReader = await ProtocolContext.Read(property.Reader, odataResponseBuilder).ConfigureAwait(false);
                    return await ProtocolContext.Read(bodyReader, odataResponseBuilder).ConfigureAwait(false);
                },
                async end => await Task.FromResult(odataResponseBuilder).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyReader"></param>
        /// <param name="odataResponseBuilder"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="Protocol.ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="Protocol.ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        private static async Task<Response.IBodyReader> Read(Response.IPropertyReader propertyReader, OdataResponseBuilder odataResponseBuilder)
        {
            Response.IPropertyNameReader propertyNameReader;
            try
            {
                propertyNameReader = await propertyReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

            Response.IPropertyValueReader propertyValueReader;
            PropertyName propertyName;
            try
            {
                (propertyValueReader, propertyName) = await propertyNameReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

            Response.PropertyValueToken propertyValueToken;
            try
            {
                propertyValueToken = await propertyValueReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

            return await propertyValueToken.Apply(
                async literal =>
                {
                    Response.LiteralToken literalToken;
                    try
                    {
                        literalToken = await literal.Reader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    //// TODO what layer do you find out if there are duplicate property names? do you need a new layer for this? the argument for a new layer is this: the existing `protocolcontext` simply takes the readers and makes them into CLR types that mimic a parse tree; is it really that layer's responsibility to ensure that things like duplicate property names are validated? well, the answer to that is "yes" because that's what we've defined as the job of "protocol", but is there something between "protocol" and "reader" that *doesn't* care? //// TODO call it "parsecontext"?
                    return await literalToken.Apply(
                        async @true =>
                        {
                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, new OdataPropertyValue.String("true")));
                            Response.IBodyReader bodyReader;
                            try
                            {
                                (bodyReader, _) = await @true.Reader.Read().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new Protocol.ReadException("TODO", ioException);
                            }
                            catch (Reader.ReadException readException)
                            {
                                throw new Protocol.ProtocolException("TODO", readException);
                            }

                            return bodyReader;
                        },
                        async @false =>
                        {
                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, new OdataPropertyValue.String("false")));
                            Response.IBodyReader bodyReader;
                            try
                            {
                                (bodyReader, _) = await @false.Reader.Read().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new Protocol.ReadException("TODO", ioException);
                            }
                            catch (Reader.ReadException readException)
                            {
                                throw new Protocol.ProtocolException("TODO", readException);
                            }

                            return bodyReader;
                        },
                        async numberReader =>
                        {
                            Response.IBodyReader bodyReader;
                            Number number;
                            try
                            {
                                (bodyReader, number) = await numberReader.Reader.Read().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new Protocol.ReadException("TODO", ioException);
                            }
                            catch (Reader.ReadException readException)
                            {
                                throw new Protocol.ProtocolException("TODO", readException);
                            }

                            odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, new OdataPropertyValue.String(number.Value)));
                            return bodyReader;
                        }).ConfigureAwait(false);
                },
                async @null =>
                {
                    odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, new OdataPropertyValue.String("null")));
                    Response.IBodyReader bodyReader;
                    try
                    {
                        (bodyReader, _) = await @null.Reader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    return bodyReader;
                },
                async @string =>
                {
                    Response.IBodyReader bodyReader;
                    StringToken stringToken;
                    try
                    {
                        (bodyReader, stringToken) = await @string.Reader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    odataResponseBuilder.Properties.Add(new OdataProperty(propertyName.Value, new OdataPropertyValue.String(stringToken.Value)));
                    return bodyReader;
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headersReader"></param>
        /// <param name="odataResponseBuilder"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="Protocol.ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="Protocol.ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        private static async Task<T> Read<T>(Response.IHeadersReader<T> headersReader, OdataResponseBuilder odataResponseBuilder)
        {
            Response.HeadersToken<T> headersToken;
            try
            {
                headersToken = await headersReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

            return await headersToken.Apply(
                async header =>
                {
                    Response.IHeaderKvpReader<T> kvpHeaderReader;
                    try
                    {
                        kvpHeaderReader = await header.Reader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    Response.IHeaderKeyReader<T> headerKeyReader;
                    try
                    {
                        headerKeyReader = await kvpHeaderReader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    Response.HeaderKeyToken<T> headerKeyToken;
                    HeaderKey headerKey;
                    try
                    {
                        (headerKeyToken, headerKey) = await headerKeyReader.Read().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new Protocol.ReadException("TODO", ioException);
                    }
                    catch (Reader.ReadException readException)
                    {
                        throw new Protocol.ProtocolException("TODO", readException);
                    }

                    return await headerKeyToken.Apply(
                        async headerValue => await ProtocolContext.Read(headerValue.Reader, headerKey, odataResponseBuilder).ConfigureAwait(false),
                        async headers => await ProtocolContext.Read(headers.Reader, odataResponseBuilder).ConfigureAwait(false))
                    .ConfigureAwait(false);
                },
                body => Task.FromResult(body.Reader))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerValueReader"></param>
        /// <param name="headerKey"></param>
        /// <param name="odataResponseBuilder"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="Protocol.ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="Protocol.ProtocolException">Thrown if the underlying response payload is not valid OData</exception>
        private static async Task<T> Read<T>(Response.IHeaderValueReader<T> headerValueReader, HeaderKey headerKey, OdataResponseBuilder odataResponseBuilder)
        {
            Response.HeaderValueToken<T> headerValueToken;
            HeaderValue headerValue;
            try
            {
                (headerValueToken, headerValue) = await headerValueReader.Read().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new Protocol.ReadException("TODO", ioException);
            }
            catch (Reader.ReadException readException)
            {
                throw new Protocol.ProtocolException("TODO", readException);
            }

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
                
                return new OdataResponse(
                    this.HttpStatusCode, 
                    this.Headers, 
                    this.Properties, 
                    System.Linq.Enumerable.Empty<ControlInformation>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestReader"></param>
        /// <param name="requestWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<Response.IResponseReader> Transfer(Request.IRequestReader requestReader, IRequestWriter requestWriter)
        {
            var verbReader = await requestReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IVerbWriter verbWriter;
            try
            {
                verbWriter = await requestWriter.Write().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }

            var (urlReader, httpVerb) = await verbReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IUrlWriter urlWriter;
            try
            {
                urlWriter = await verbWriter.Write(httpVerb).ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }

            var urlSchemeReader = await urlReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IUrlSchemeWriter urlSchemeWriter;
            try
            {
                urlSchemeWriter = await urlWriter.Write().ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }

            var (urlDomainReader, urlScheme) = await urlSchemeReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IUrlDomainWriter urlDomainWriter;
            try
            {
                urlDomainWriter = await urlSchemeWriter.Write(urlScheme).ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }

            var (urlPathReader, urlDomain) = await urlDomainReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IUrlPathWriter urlPathWriter;
            try
            {
                urlPathWriter = await urlDomainWriter.Write(urlDomain).ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }

            var (urlQueryReader, urlQueryWriter) = await ProtocolContext.Transfer(urlPathReader, urlPathWriter).ConfigureAwait(false);

            var (headersReader, headersWriter) = await ProtocolContext.Transfer(urlQueryReader, urlQueryWriter).ConfigureAwait(false);

            var (bodyReader, bodyWriter) = await ProtocolContext.Transfer(headersReader, headersWriter).ConfigureAwait(false);

            return await ProtocolContext.Transfer(bodyReader, bodyWriter).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bodyReader"></param>
        /// <param name="bodyWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<Response.IResponseReader> Transfer(Request.IBodyReader bodyReader, IBodyWriter bodyWriter)
        {
            var bodyToken = await bodyReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await bodyToken
                .Apply(
                    async end =>
                    {
                        Response.IResponseReader responseReader;
                        try
                        {
                            responseReader = await bodyWriter.Send().ConfigureAwait(false);
                        }
                        catch (IOException ioException)
                        {
                            throw new WriteException("TODO", ioException);
                        }

                        return responseReader;
                    })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headersReader"></param>
        /// <param name="headersWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(Request.IBodyReader BodyReader, IBodyWriter BodyWriter)> Transfer(
            Request.IHeadersReader headersReader,
            IHeadersWriter headersWriter)
        {
            var headersToken = await headersReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await headersToken.Apply(
                async header =>
                {
                    IHeaderWriter headerWriter;
                    try
                    {
                        headerWriter = await headersWriter.WriteHeader().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    var headerKvpReader = await header.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    IHeaderKvpWriter headerKvpWriter;
                    try
                    {
                        headerKvpWriter = await headerWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    var headerKeyReader = await headerKvpReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    var (headerKeyToken, headerKey) = await headerKeyReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    IHeaderKeyWriter headerKeyWriter;
                    try
                    {
                        headerKeyWriter = await headerKvpWriter.Write(headerKey).ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return await headerKeyToken.Apply(
                        async headerValue =>
                        {
                            return await ProtocolContext.Transfer(headerValue.Reader, headerKeyWriter).ConfigureAwait(false);
                        },
                        async headers =>
                        {
                            IHeadersWriter headersWriter;
                            try
                            {
                                headersWriter = await headerKeyWriter.Write().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new WriteException("TODO", ioException);
                            }

                            return await ProtocolContext.Transfer(headers.Reader, headersWriter).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                },
                async body =>
                {
                    IBodyWriter bodyWriter;
                    try
                    {
                        bodyWriter = await headersWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return (body.Reader, bodyWriter);
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerValueReader"></param>
        /// <param name="headerKeyWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(Request.IBodyReader BodyReader, IBodyWriter BodyWriter)> Transfer(
            Request.IHeaderValueReader headerValueReader,
            IHeaderKeyWriter headerKeyWriter)
        {
            var (headerValueToken, headerValue) = await headerValueReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            IHeaderValueWriter headerValueWriter;
            try
            {
                headerValueWriter = await headerKeyWriter.Write(headerValue).ConfigureAwait(false);
            }
            catch (IOException ioException)
            {
                throw new WriteException("TODO", ioException);
            }
            
            return await headerValueToken.Apply(
                async headerValueReader =>
                {
                    IHeaderKeyWriter headerKeyWriter;
                    try
                    {
                        headerKeyWriter = await headerValueWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return await ProtocolContext.Transfer(headerValueReader.Reader, headerKeyWriter).ConfigureAwait(false);
                },
                async headers =>
                {
                    IHeaderKeyWriter headerKeyWriter;
                    try
                    {
                        headerKeyWriter = await headerValueWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    IHeadersWriter headersWriter;
                    try
                    {
                        headersWriter = await headerKeyWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return await ProtocolContext.Transfer(headers.Reader, headersWriter).ConfigureAwait(false);
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlQueryReader"></param>
        /// <param name="urlQueryWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(Request.IHeadersReader HeadersReader, IHeadersWriter HeadersWriter)> Transfer(
            Request.IUrlQueryReader urlQueryReader,
            IUrlQueryWriter urlQueryWriter)
        {
            var urlQueryToken = await urlQueryReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await urlQueryToken.Apply(
                async kvp =>
                {
                    IUrlQueryKvpWriter urlQueryKvpWriter;
                    try
                    {
                        urlQueryKvpWriter = await urlQueryWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    var urlQueryNameReader = await kvp.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    var (urlQueryNameToken, urlQueryName) = await urlQueryNameReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    IUrlQueryNameWriter urlQueryNameWriter;
                    try
                    {
                        urlQueryNameWriter = await urlQueryKvpWriter.Write(urlQueryName).ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return await urlQueryNameToken.Apply(
                        async queryValue =>
                        {
                            var (newUrlQueryReader, urlQueryValue) = await queryValue.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control

                            IUrlQueryValueWriter urlQueryValueWriter;
                            try
                            {
                                urlQueryValueWriter = await urlQueryNameWriter.WriteValue().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new WriteException("TODO", ioException);
                            }

                            IUrlQueryWriter newUrlQueryWriter;
                            try
                            {
                                newUrlQueryWriter = await urlQueryValueWriter.Write(urlQueryValue).ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new WriteException("TODO", ioException);
                            }

                            return await ProtocolContext.Transfer(newUrlQueryReader, newUrlQueryWriter).ConfigureAwait(false);
                        },
                        async query =>
                        {
                            IUrlQueryWriter urlQueryWriter;
                            try
                            {
                                urlQueryWriter = await urlQueryNameWriter.Write().ConfigureAwait(false);
                            }
                            catch (IOException ioException)
                            {
                                throw new WriteException("TODO", ioException);
                            }

                            return await ProtocolContext.Transfer(query.Reader, urlQueryWriter).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                },
                async headers =>
                {
                    IHeadersWriter headersWriter;
                    try
                    {
                        headersWriter = await urlQueryWriter.WriteHeaders().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return (headers.Reader, headersWriter);
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlPathReader"></param>
        /// <param name="urlPathWriter"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred sending the payload to the service</exception>
        private static async Task<(Request.IUrlQueryReader UrlQueryReader, IUrlQueryWriter UrlQueryWriter)> Transfer(
            Request.IUrlPathReader urlPathReader, 
            IUrlPathWriter urlPathWriter)
        {
            var urlPathToken = await urlPathReader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
            return await urlPathToken.Apply(
                async pathSegment =>
                {
                    var (newUrlPathReader, urlPathSegment) = await pathSegment.Reader.Read().ConfigureAwait(false); // NOTE: shouldn't throw any exceptions because the data is an in-memory representation of the request that we have validated and control
                    IUrlPathSegmentWriter urlPathSegmentWriter;
                    try
                    {
                        urlPathSegmentWriter = await urlPathWriter.WriteSegment().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    IUrlPathWriter newUrlPathWriter;
                    try
                    {
                        newUrlPathWriter = await urlPathSegmentWriter.Write(urlPathSegment).ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return await ProtocolContext.Transfer(newUrlPathReader, newUrlPathWriter).ConfigureAwait(false);
                },
                async query =>
                {
                    IUrlQueryWriter urlQueryWriter;
                    try
                    {
                        urlQueryWriter = await urlPathWriter.Write().ConfigureAwait(false);
                    }
                    catch (IOException ioException)
                    {
                        throw new WriteException("TODO", ioException);
                    }

                    return (query.Reader, urlQueryWriter);
                }).ConfigureAwait(false);
        }
    }
}
