﻿namespace OddTrotter.GraphClient
{
    using System;
    using System.Linq;

    using Microsoft.Extensions.Configuration;

    public static class GraphClientFactory
    {
        private const string graphAccessTokenFormFieldName = "graphAccessToken";

        private const string graphRootUrlConfigurationPropertyName = "GraphRootUrl";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpRequestData"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpRequestData"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
        /// <exception cref="GraphUriException">
        /// Thrown if the graph root URL specified in <paramref name="configuration"/> is not a valid URL or it is not an absolute URL
        /// </exception>
        /// <exception cref="MissingFormDataException">Thrown if the form data in <paramref name="httpRequestData"/> contained missing fields</exception>
        /// <exception cref="InvalidFormDataException">Thrown if the form data in <paramref name="httpRequestData"/> contained fields with invalid data</exception>
        public static GraphClient Create(HttpRequestData httpRequestData, IConfiguration configuration)
        {
            if (httpRequestData == null)
            {
                throw new ArgumentNullException(nameof(httpRequestData));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var graphClientSettingsBuilder = new GraphClientSettings.Builder();
            GetGraphRootUrl(graphClientSettingsBuilder, configuration);
            var accessToken = GetAccessToken(httpRequestData);
            
            return new GraphClient(accessToken, graphClientSettingsBuilder.Build());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingsBuilder"></param>
        /// <param name="configuration"></param>
        /// <exception cref="GraphUriException">
        /// Thrown if the graph root URL specified in <paramref name="configuration"/> is not a valid URL or it is not an absolute URL
        /// </exception>
        private static void GetGraphRootUrl(GraphClientSettings.Builder graphClientSettingsBuilder, IConfiguration configuration)
        {
            var graphRootUrl = configuration[graphRootUrlConfigurationPropertyName];
            if (graphRootUrl != null)
            {
                var graphUriException = new GraphUriException(graphRootUrl);
                try
                {
                    graphClientSettingsBuilder.GraphRootUri = new Uri(graphRootUrl);
                }
                catch (UriFormatException)
                {
                    throw graphUriException;
                }

                if (!graphClientSettingsBuilder.GraphRootUri.IsAbsoluteUri)
                {
                    throw graphUriException;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpRequestData"></param>
        /// <returns></returns>
        /// <exception cref="MissingFormDataException">Thrown if the form data in <paramref name="httpRequestData"/> contained missing fields</exception>
        /// <exception cref="InvalidFormDataException">Thrown if the form data in <paramref name="httpRequestData"/> contained fields with invalid data</exception>
        private static string GetAccessToken(HttpRequestData httpRequestData)
        {
            if (!httpRequestData.Form.TryGetValue(graphAccessTokenFormFieldName, out var accessTokens))
            {
                throw new MissingFormDataException(new[] { graphAccessTokenFormFieldName });
            }

            if (accessTokens.Count == 0)
            {
                throw new MissingFormDataException(new[] { graphAccessTokenFormFieldName });
            }

            string? accessToken;
            try
            {
                accessToken = accessTokens.Single();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidFormDataException(graphAccessTokenFormFieldName);
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new MissingFormDataException(new[] { graphAccessTokenFormFieldName });
            }

            return accessToken;
        }
    }
}
