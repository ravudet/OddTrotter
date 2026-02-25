namespace OddTrotter.Graph.CalendarEventsContext //// TODO there's almost certainly a better namespace to put this class; i'm actually not clear if this class is even specific to graph (though one that renews tokens would be, or at least it would be specific to entra)
{
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;
    using OddTrotter.Odata.v4_01.Reader.RequestWriter;

    internal sealed class AuthorizedRequestWriter : IRequestWriter
    {
        private readonly IRequestWriter requestWriter;
        private readonly string accessToken;

        public AuthorizedRequestWriter(IRequestWriter requestWriter, string accessToken)
        {
            this.requestWriter = requestWriter;
            this.accessToken = accessToken;
        }

        public async Task<IVerbWriter> Write()
        {
            var verbWriter = await this.requestWriter.Write().ConfigureAwait(false);
            return new VerbWriter(verbWriter, this.accessToken);
        }

        private sealed class VerbWriter : IVerbWriter
        {
            private readonly IVerbWriter verbWriter;
            private readonly string accessToken;

            public VerbWriter(IVerbWriter verbWriter, string accessToken)
            {
                this.verbWriter = verbWriter;
                this.accessToken = accessToken;
            }

            public async Task<IUrlWriter> Write(HttpVerb httpVerb)
            {
                var urlWriter = await this.verbWriter.Write(httpVerb).ConfigureAwait(false);
                return new UrlWriter(urlWriter, this.accessToken);
            }

            private sealed class UrlWriter : IUrlWriter
            {
                private readonly IUrlWriter urlWriter;
                private readonly string accessToken;

                public UrlWriter(IUrlWriter urlWriter, string accessToken)
                {
                    this.urlWriter = urlWriter;
                    this.accessToken = accessToken;
                }

                public async Task<IUrlSchemeWriter> Write()
                {
                    var urlSchemeWriter = await this.urlWriter.Write().ConfigureAwait(false);
                    return new UrlSchemeWriter(urlSchemeWriter, this.accessToken);
                }

                private sealed class UrlSchemeWriter : IUrlSchemeWriter
                {
                    private readonly IUrlSchemeWriter urlSchemeWriter;
                    private readonly string accessToken;

                    public UrlSchemeWriter(IUrlSchemeWriter urlSchemeWriter, string accessToken)
                    {
                        this.urlSchemeWriter = urlSchemeWriter;
                        this.accessToken = accessToken;
                    }

                    public async Task<IUrlDomainWriter> Write(UrlScheme urlScheme)
                    {
                        var urlDomainWriter = await this.urlSchemeWriter.Write(urlScheme).ConfigureAwait(false);
                        return new UrlDomainWriter(urlDomainWriter, this.accessToken);
                    }

                    private sealed class UrlDomainWriter : IUrlDomainWriter
                    {
                        private readonly IUrlDomainWriter urlDomainWriter;
                        private readonly string accessToken;

                        public UrlDomainWriter(IUrlDomainWriter urlDomainWriter, string accessToken)
                        {
                            this.urlDomainWriter = urlDomainWriter;
                            this.accessToken = accessToken;
                        }

                        public async Task<IUrlPathWriter> Write(UrlDomain urlDomain)
                        {
                            var urlPathWriter = await this.urlDomainWriter.Write(urlDomain).ConfigureAwait(false);
                            return new UrlPathWriter(urlPathWriter, this.accessToken);
                        }

                        private sealed class UrlPathWriter : IUrlPathWriter
                        {
                            private readonly IUrlPathWriter urlPathWriter;
                            private readonly string accessToken;

                            public UrlPathWriter(IUrlPathWriter urlPathWriter, string accessToken)
                            {
                                this.urlPathWriter = urlPathWriter;
                                this.accessToken = accessToken;
                            }

                            public async Task<IUrlQueryWriter> Write()
                            {
                                var urlQueryWriter = await this.urlPathWriter.Write().ConfigureAwait(false);
                                return new UrlQueryWriter(urlQueryWriter, this.accessToken);
                            }

                            public async Task<IUrlPathSegmentWriter> WriteSegment()
                            {
                                var urlPathSegmentWriter = await this.urlPathWriter.WriteSegment().ConfigureAwait(false);
                                return new UrlPathSegmentWriter(urlPathSegmentWriter, this.accessToken);
                            }

                            private sealed class UrlQueryWriter : IUrlQueryWriter
                            {
                                private readonly IUrlQueryWriter urlQueryWriter;
                                private readonly string accessToken;

                                public UrlQueryWriter(IUrlQueryWriter urlQueryWriter, string accessToken)
                                {
                                    this.urlQueryWriter = urlQueryWriter;
                                    this.accessToken = accessToken;
                                }

                                public async Task<IUrlQueryKvpWriter> Write()
                                {
                                    var urlQueryKvpWriter = await this.urlQueryWriter.Write().ConfigureAwait(false);
                                    return new UrlQueryKvpWriter(urlQueryKvpWriter, this.accessToken);
                                }

                                public async Task<IHeadersWriter> WriteHeaders()
                                {
                                    var headersWriter = await this.urlQueryWriter.WriteHeaders().ConfigureAwait(false);
                                    var headerWriter = await headersWriter.WriteHeader().ConfigureAwait(false);
                                    var headerKvpWriter = await headerWriter.Write().ConfigureAwait(false);
                                    var headerKeyWriter = await headerKvpWriter.Write(new HeaderKey("Authorization")).ConfigureAwait(false);
                                    var headerValueWriter = await headerKeyWriter.Write(new HeaderValue(this.accessToken)).ConfigureAwait(false);
                                    var headerKeyWriter2 = await headerValueWriter.Write().ConfigureAwait(false);
                                    return await headerKeyWriter2.Write().ConfigureAwait(false);
                                }

                                private sealed class UrlQueryKvpWriter : IUrlQueryKvpWriter
                                {
                                    private readonly IUrlQueryKvpWriter urlQueryKvpWriter;
                                    private readonly string accessToken;

                                    public UrlQueryKvpWriter(IUrlQueryKvpWriter urlQueryKvpWriter, string accessToken)
                                    {
                                        this.urlQueryKvpWriter = urlQueryKvpWriter;
                                        this.accessToken = accessToken;
                                    }

                                    public async Task<IUrlQueryNameWriter> Write(UrlQueryName urlQueryName)
                                    {
                                        var urlQueryNameWriter = await this.urlQueryKvpWriter.Write(urlQueryName).ConfigureAwait(false);
                                        return new UrlQueryNameWriter(urlQueryNameWriter, accessToken);
                                    }

                                    private sealed class UrlQueryNameWriter : IUrlQueryNameWriter
                                    {
                                        private readonly IUrlQueryNameWriter urlQueryNameWriter;
                                        private readonly string accessToken;

                                        public UrlQueryNameWriter(IUrlQueryNameWriter urlQueryNameWriter, string accessToken)
                                        {
                                            this.urlQueryNameWriter = urlQueryNameWriter;
                                            this.accessToken = accessToken;
                                        }

                                        public async Task<IUrlQueryWriter> Write()
                                        {
                                            var urlQueryWriter = await this.urlQueryNameWriter.Write().ConfigureAwait(false);
                                            return new UrlQueryWriter(urlQueryWriter, accessToken);
                                        }

                                        public async Task<IUrlQueryValueWriter> WriteValue()
                                        {
                                            var urlQueryValueWriter = await this.urlQueryNameWriter.WriteValue().ConfigureAwait(false);
                                            return new UrlQueryValueWriter(urlQueryValueWriter, accessToken);
                                        }

                                        private sealed class UrlQueryValueWriter : IUrlQueryValueWriter
                                        {
                                            private readonly IUrlQueryValueWriter urlQueryValueWriter;
                                            private readonly string accessToken;

                                            public UrlQueryValueWriter(IUrlQueryValueWriter urlQueryValueWriter, string accessToken)
                                            {
                                                this.urlQueryValueWriter = urlQueryValueWriter;
                                                this.accessToken = accessToken;
                                            }

                                            public async Task<IUrlQueryWriter> Write(UrlQueryValue urlQueryValue)
                                            {
                                                var urlQueryWriter = await this.urlQueryValueWriter.Write(urlQueryValue).ConfigureAwait(false);
                                                return new UrlQueryWriter(urlQueryWriter, this.accessToken);
                                            }
                                        }
                                    }
                                }
                            }

                            private sealed class UrlPathSegmentWriter : IUrlPathSegmentWriter
                            {
                                private readonly IUrlPathSegmentWriter urlPathSegmentWriter;
                                private readonly string accessToken;

                                public UrlPathSegmentWriter(IUrlPathSegmentWriter urlPathSegmentWriter, string accessToken)
                                {
                                    this.urlPathSegmentWriter = urlPathSegmentWriter;
                                    this.accessToken = accessToken;
                                }

                                public async Task<IUrlPathWriter> Write(UrlPathSegment urlPathSegment)
                                {
                                    var urlPathWriter = await this.urlPathSegmentWriter.Write(urlPathSegment).ConfigureAwait(false);
                                    return new UrlPathWriter(urlPathWriter, this.accessToken);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
