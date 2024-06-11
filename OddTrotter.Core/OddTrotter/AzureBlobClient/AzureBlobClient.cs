namespace OddTrotter.AzureBlobClient
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public sealed class AzureBlobClient : IAzureBlobClient
    {
        private readonly string containerUri;

        private readonly string sasToken;

        private readonly string apiVersion;

        private readonly string blobType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerUri"></param>
        /// <param name="sasToken"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="containerUri"/> or <paramref name="sasToken"/> or <paramref name="apiVersion"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="apiVersion"/> is empty</exception>
        /// <exception cref="InvalidSasTokenException">Thrown if <paramref name="sasToken"/> results in malformed blob URLs</exception>
        public AzureBlobClient(AbsoluteUri containerUri, string sasToken, string apiVersion)
            : this(containerUri, sasToken, apiVersion, new AzureBlobClientSettings.Builder().Build())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerUri"></param>
        /// <param name="sasToken"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="containerUri"/> or <paramref name="sasToken"/> or <paramref name="apiVersion"/> or <paramref name="settings"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="apiVersion"/> is empty</exception>
        /// <exception cref="InvalidSasTokenException">Thrown if <paramref name="sasToken"/> results in malformed blob URLs</exception>
        public AzureBlobClient(AbsoluteUri containerUri, string sasToken, string apiVersion, AzureBlobClientSettings settings)
        {
            if (containerUri == null)
            {
                throw new ArgumentNullException(nameof(containerUri));
            }

            if (sasToken == null)
            {
                throw new ArgumentNullException(nameof(sasToken));
            }

            if (apiVersion == null)
            {
                throw new ArgumentNullException(nameof(apiVersion));
            }

            if (string.IsNullOrEmpty(apiVersion))
            {
                throw new ArgumentException($"'{nameof(apiVersion)}' cannot be empty");
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.containerUri = containerUri.OriginalString.TrimEnd('/');
            this.sasToken = sasToken;
            try
            {
                GenerateBlobUrl("blobName");
            }
            catch (InvalidBlobNameException e)
            {
                throw new InvalidSasTokenException(sasToken, e);
            }

            this.apiVersion = apiVersion;
            this.blobType = settings.BlobType;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(string blobName)
        {
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"'{nameof(blobName)}' cannot be empty");
            }

            var blobUrl = GenerateBlobUrl(blobName);
            using (var httpClient = new HttpClient()) //// TODO a new client should not be created for every request
            {
                httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString());
                httpClient.DefaultRequestHeaders.Add("x-ms-version", this.apiVersion);

                HttpResponseMessage? httpResponseMessage = null;
                try
                {
                    httpResponseMessage = await httpClient.GetAsync(blobUrl).ConfigureAwait(false);
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new InvalidSasTokenException(this.sasToken, responseContent);
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SasTokenNoReadPrivilegesException(this.sasToken, blobUrl.OriginalString, responseContent);
                    }
                }
                catch
                {
                    httpResponseMessage?.Dispose();
                    throw;
                }

                return httpResponseMessage;
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
        {
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"'{nameof(blobName)}' cannot be empty");
            }

            if (httpContent == null)
            {
                throw new ArgumentNullException(nameof(httpContent));
            }

            var blobUrl = GenerateBlobUrl(blobName);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString());
                httpClient.DefaultRequestHeaders.Add("x-ms-version", this.apiVersion);
                httpClient.DefaultRequestHeaders.Add("x-ms-blob-type", this.blobType);

                HttpResponseMessage? httpResponseMessage = null;
                try
                {
                    httpResponseMessage = await httpClient.PutAsync(blobUrl, httpContent).ConfigureAwait(false);
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new InvalidSasTokenException(this.sasToken, responseContent);
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SasTokenNoWritePrivilegesException(this.sasToken, blobUrl.OriginalString, responseContent);
                    }
                }
                catch
                {
                    httpResponseMessage?.Dispose();
                    throw;
                }

                return httpResponseMessage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidBlobNameException">Thrown if <paramref name="blobName"/> results in an invalid URL</exception>
        private Uri GenerateBlobUrl(string blobName)
        {
            try
            {
                return new Uri($"{this.containerUri}/{blobName}?{this.sasToken}");
            }
            catch (UriFormatException e)
            {
                throw new InvalidBlobNameException(blobName, e);
            }
        }
    }
}
