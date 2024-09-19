﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.OdataPocRoot.Odata.Odata.RequestEvaluator
{
    using Fx.OdataPocRoot.Odata.UriExpressionVisitorImplementations;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(RelativeUri uri);
    }

    public sealed class HttpClientRequestEvaluator : IRequestEvaluator
    {
        private readonly IHttpClient httpClient;

        private readonly FilterToStringVisitor filterToStringVisitor;

        private readonly SelectToStringVisitor selectToStringVisitor;

        public HttpClientRequestEvaluator(IHttpClient httpClient)
            : this(httpClient, RequestEvaluatorSettings.Default)
        {
        }

        public HttpClientRequestEvaluator(IHttpClient httpClient, RequestEvaluatorSettings settings)
        {
            //// TODO make an interface for httpclient?
            //// TODO have a client factory instead? you're not disposing right now...
            this.httpClient = httpClient;

            this.filterToStringVisitor = settings.FilterToStringVisitor;
            this.selectToStringVisitor = settings.SelectToStringVisitor;
        }

        public async Task<OdataResponse.Instance> Evaluate(OdataRequest.GetInstance request)
        {
            //// TODO do you want to use a uribuilder for this?
            var queryOptions = new List<string>();

            //// TODO implement expand

            if (request.Select != null)
            {
                var stringBuilder = new StringBuilder();
                this.selectToStringVisitor.Visit(request.Select, stringBuilder);
                queryOptions.Add(stringBuilder.ToString());
            }

            var optionsString = string.Join("&", queryOptions);

            var requestUri =
                request.Uri.OriginalString.TrimEnd('/') +
                (string.IsNullOrEmpty(optionsString) ? string.Empty : $"?{optionsString}");

            HttpResponseMessage? httpResponseMessage = null;
            try
            {
                httpResponseMessage = await this.httpClient
                    .GetAsync(new Uri(requestUri, UriKind.Relative).ToRelativeUri())
                    .ConfigureAwait(false);

                httpResponseMessage.EnsureSuccessStatusCode(); //// TODO properly handle error cases

                var httpResponseContent = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                return new OdataResponse.Instance(httpResponseContent);
            }
            catch
            {
                httpResponseMessage?.Dispose();
                throw;
            }
        }

        public async Task<OdataResponse.Collection> Evaluate(OdataRequest.GetCollection request)
        {
            var queryOptions = new List<string>();

            //// TODO implement expand
            
            if (request.Filter != null)
            {
                var stringBuilder = new StringBuilder();
                this.filterToStringVisitor.Visit(request.Filter, stringBuilder);
                queryOptions.Add(stringBuilder.ToString());
            }

            if (request.Select != null)
            {
                var stringBuilder = new StringBuilder();
                this.selectToStringVisitor.Visit(request.Select, stringBuilder);
                queryOptions.Add(stringBuilder.ToString());
            }

            var optionsString = string.Join("&", queryOptions);

            var requestUri =
                request.Uri.OriginalString.TrimEnd('/') +
                (string.IsNullOrEmpty(optionsString) ? string.Empty : $"?{optionsString}");

            HttpResponseMessage? httpResponseMessage = null;
            try
            {
                httpResponseMessage = await this.httpClient
                    .GetAsync(new Uri(requestUri, UriKind.Relative).ToRelativeUri())
                    .ConfigureAwait(false);

                httpResponseMessage.EnsureSuccessStatusCode(); //// TODO properly handle error cases

                var httpResponseContent = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                return new OdataResponse.Collection(httpResponseContent);
            }
            catch
            {
                httpResponseMessage?.Dispose();
                throw;
            }
        }
    }
}