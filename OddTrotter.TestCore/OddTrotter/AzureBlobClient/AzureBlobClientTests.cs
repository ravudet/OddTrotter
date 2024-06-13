namespace OddTrotter.AzureBlobClient
{
    using System.Threading.Tasks;
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Net;

    /// <summary>
    /// Tests for internal consistency of <see cref="IAzureBlobClient"/> implementations
    /// </summary>
    public sealed class AzureBlobClientTests
    {
        private readonly Func<IAzureBlobClient> azureBlobClientFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="azureBlobClientFactory"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="azureBlobClientFactory"/> is <see langword="null"/></exception>
        public AzureBlobClientTests(Func<IAzureBlobClient> azureBlobClientFactory)
        {
            if (azureBlobClientFactory == null)
            {
                throw new ArgumentNullException(nameof(azureBlobClientFactory));
            }

            this.azureBlobClientFactory = azureBlobClientFactory;
        }

        /// <summary>
        /// Retrieves an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        public async Task GetNullBlobName()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => azureBlobClient.GetAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }
        
        /// <summary>
         /// Retrieves an azure blob with an empty name
         /// </summary>
         /// <returns></returns>
        public async Task GetEmptyBlobName()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => azureBlobClient.GetAsync(string.Empty)).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        public async Task PutNullBlobName()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            using (var httpContent = new StringContent("somecontent"))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => azureBlobClient.PutAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob with <see langword="null"/> content
        /// </summary>
        /// <returns></returns>
        public async Task PutNullContent()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => azureBlobClient.PutAsync(
                "someblob",
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an azure blob with an empty name
        /// </summary>
        /// <returns></returns>
        public async Task PutEmptyBlobName()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            using (var httpContent = new StringContent("somecontent"))
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => azureBlobClient.PutAsync(
                    string.Empty,
                    httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it
        /// </summary>
        /// <returns></returns>
        public async Task PutAndGet()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            var currentTime = DateTime.UtcNow.Ticks.ToString(); // adding the current time to names and content to avoid test runs from conflicting with each other
            var content = "some content" + currentTime;
            var blobName = "someblob" + currentTime;

            using (var httpContent = new StringContent(content))
            {
                using (var putResponse = await azureBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.Created, putResponse.StatusCode);
                }

                using (var getResponse = await azureBlobClient.GetAsync(blobName).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
                    var getResponseContent = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Assert.AreEqual(content, getResponseContent);
                }
            }
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it where all URL data contains extra slashes
        /// </summary>
        /// <returns></returns>
        public async Task PutAndGetWithExtraSlashes()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            var currentTime = DateTime.UtcNow.Ticks.ToString(); // adding the current time to names and content to avoid test runs from conflicting with each other
            var content = "/some content" + currentTime + "/";
            var blobName = "someblob" + currentTime;

            using (var httpContent = new StringContent(content))
            {
                using (var putResponse = await azureBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.Created, putResponse.StatusCode);
                }

                using (var getResponse = await azureBlobClient.GetAsync(blobName).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
                    var getResponseContent = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Assert.AreEqual(content, getResponseContent);
                }
            }
        }

        /// <summary>
        /// Retrieves an azure blob that doesn't exist
        /// </summary>
        /// <returns></returns>
        public async Task GetNonexistentBlob()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            var blobName = nameof(GetNonexistentBlob) + DateTime.UtcNow.ToString();
            using (var httpResponse = await azureBlobClient.GetAsync(blobName).ConfigureAwait(false))
            {
                Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
            }
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it
        /// </summary>
        /// <returns></returns>
        public async Task PutExistingBlob()
        {
            var azureBlobClient = this.azureBlobClientFactory();
            var currentTime = DateTime.UtcNow.Ticks.ToString(); // adding the current time to names and content to avoid test runs from conflicting with each other
            var content = "some content" + currentTime;
            var blobName = "someblob" + currentTime;

            using (var httpContent = new StringContent(content))
            {
                using (var putResponse = await azureBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.Created, putResponse.StatusCode);
                }
            }

            using (var httpContent = new StringContent(content))
            {
                using (var putResponse = await azureBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.Created, putResponse.StatusCode);
                }
            }
        }
    }
}
