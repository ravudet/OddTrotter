namespace OddTrotter.AzureBlobClient
{
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="EncryptedAzureBlobClient"/>
    /// </summary>
    [TestClass]
    public sealed class MemoryBlobClientUnitTests
    {
        /// <summary>
        /// Retrieves an azure blob that doesn't exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNonexistentBlobWithErrorMessage()
        {
            var memoryBlobClient = new MemoryBlobClient();
            var blobName = "someblob";
            using (var httpResponse = await memoryBlobClient.GetAsync(blobName).ConfigureAwait(false))
            {
                Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
                var responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                Assert.AreEqual(string.Format(MemoryBlobClient.NotFoundErrorMessageFormat, blobName), responseContent);
            }
        }

        /// <summary>
        /// Retrieves an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNullBlobName()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).GetNullBlobName();
        }

        /// <summary>
        /// Retrieves an azure blob with an empty name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetEmptyBlobName()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).GetEmptyBlobName();
        }

        /// <summary>
        /// Writes an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNullBlobName()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutNullBlobName();
        }

        /// <summary>
        /// Writes an azure blob with <see langword="null"/> content
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNullContent()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutNullContent();
        }

        /// <summary>
        /// Writes an azure blob with an empty name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutEmptyBlobName()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutEmptyBlobName();
        }

        /// <summary>
        /// Retrieves an azure blob that doesn't exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNonexistentBlob()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).GetNonexistentBlob();
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutAndGet()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutAndGet();
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it where all URL data contains extra slashes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutAndGetWithExtraSlashes()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutAndGetWithExtraSlashes();
        }

        /// <summary>
        /// Writes an azure blob that already exists
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutExistingBlob()
        {
            var memoryBlobClient = new MemoryBlobClient();
            await new AzureBlobClientTests(() => memoryBlobClient).PutExistingBlob();
        }
    }
}
