namespace OddTrotter.AzureBlobClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public sealed class MemoryBlobClient : IAzureBlobClient
    {
        private readonly ConcurrentDictionary<string, byte[]> blobs;

        public MemoryBlobClient()
        {
            this.blobs = new ConcurrentDictionary<string, byte[]>();
        }

        /// <summary>
        /// The format of the error message used when a blob is not found
        /// </summary>
        public static string NotFoundErrorMessageFormat { get; } = "Could not find blob with name {0}";

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

            HttpResponseMessage? httpResponseMessage = null;
            try
            {
                httpResponseMessage = new HttpResponseMessage();
                if (!blobs.TryGetValue(blobName, out var data))
                {
                    httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
                    HttpContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(string.Format(NotFoundErrorMessageFormat, blobName));
                        httpResponseMessage.Content = stringContent;

                        return await Task.FromResult(httpResponseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                httpResponseMessage.StatusCode = HttpStatusCode.OK;
                HttpContent? byteArrayContent = null;
                try
                {
                    byteArrayContent = new ByteArrayContent(data);
                    httpResponseMessage.Content = byteArrayContent;

                    return await Task.FromResult(httpResponseMessage);
                }
                catch
                {
                    byteArrayContent?.Dispose();
                    throw;
                }
            }
            catch
            {
                httpResponseMessage?.Dispose();
                throw;
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

            var data = await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);
            var isUpdated = false;
            this.blobs.AddOrUpdate(
                blobName,
                data,
                (key, value) =>
                {
                    isUpdated = true;
                    return data;
                });

            return await Task.FromResult(new HttpResponseMessage(isUpdated ? HttpStatusCode.OK : HttpStatusCode.Created)).ConfigureAwait(false);
        }
    }
}
