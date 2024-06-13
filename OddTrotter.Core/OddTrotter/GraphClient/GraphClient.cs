namespace OddTrotter.GraphClient
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public sealed class GraphClient : IGraphClient, IDisposable
    {
        private readonly string rootUrl;

        private readonly string accessToken;

        private readonly HttpClient httpClient;

        private bool disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <param name="accessToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="accessToken"/> or <paramref name="settings"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="accessToken"/> whitespace-only characters or is not a valid header value</exception>
        public GraphClient(string accessToken, GraphClientSettings settings)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException($"'{nameof(accessToken)}' cannot be null or exclusively whitespace characters");
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.rootUrl = settings.GraphRootUri.OriginalString.TrimEnd('/');
            this.accessToken = accessToken;

            this.disposed = false;
            this.httpClient = new HttpClient();
            try
            {
                this.httpClient.Timeout = settings.HttpClientTimeout;
                try
                {
                    this.httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
                }
                catch (FormatException e)
                {
                    throw new ArgumentException($"'{nameof(accessToken)}' had a value of '{accessToken}' which is not a valid 'Authorization' header value");
                }
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.httpClient.Dispose();

            this.disposed = true;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
        {
            if (relativeUri == null)
            {
                throw new ArgumentNullException(nameof(relativeUri));
            }

            var requestUrl = new Uri(this.rootUrl + '/' + relativeUri.OriginalString.Trim('/'), UriKind.Absolute).ToAbsoluteUri();
            return await GetAsync(requestUrl).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
        {
            if (absoluteUri == null)
            {
                throw new ArgumentNullException(nameof(absoluteUri));
            }

            HttpResponseMessage? httpResponse = null;
            try
            {
                try
                {
                    httpResponse = await this.httpClient.GetAsync(absoluteUri).ConfigureAwait(false);
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException($"An error occurred during the request to {absoluteUri.OriginalString}", e);
                }

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized || httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    var httpResponseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new InvalidAccessTokenException(absoluteUri.OriginalString, this.accessToken, httpResponseContent);
                }
            }
            catch
            {
                httpResponse?.Dispose();
                throw;
            }

            return httpResponse;
        }

        public async Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
        {
            //// TODO document exceptions
            return await this.httpClient.PatchAsync(this.rootUrl + '/' + relativeUri.ToString(), httpContent).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
        {
            //// TODO document exceptions
            return await this.httpClient.PostAsync(this.rootUrl + '/' + relativeUri.ToString(), httpContent).ConfigureAwait(false);
        }
    }
}