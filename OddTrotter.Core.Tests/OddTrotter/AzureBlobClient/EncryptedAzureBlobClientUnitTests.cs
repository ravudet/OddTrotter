namespace OddTrotter.AzureBlobClient
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OddTrotter.Encryptor;

    /// <summary>
    /// Unit tests for <see cref="EncryptedAzureBlobClient"/>
    /// </summary>
    [TestClass]
    public sealed class EncryptedAzureBlobClientUnitTests
    {
        /// <summary>
        /// Creates a <see cref="EncryptedAzureBlobClient"/> with a <see langword="null"/> delegated client
        /// </summary>
        [TestMethod]
        public void NullDelegateClient()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new EncryptedAzureBlobClient(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                new Encryptor()));
        }

        /// <summary>
        /// Creates a <see cref="EncryptedAzureBlobClient"/> with a <see langword="null"/> <see cref="Encryptor"/>
        /// </summary>
        [TestMethod]
        public void NullEncryptor()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new EncryptedAzureBlobClient(
                new MemoryBlobClient(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        /// <summary>
        /// Retrieves an azure blob with data too short to have been encrypted
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetBlobWithShortData()
        {
            var blobName = "someblob";
            var blobContent = new string('a', 1);
            var memoryBlobClient = new MemoryBlobClient();
            using (var httpContent = new StringContent(blobContent))
            {
                await memoryBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false);
            }

            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(memoryBlobClient, new Encryptor());

            await Assert.ThrowsExceptionAsync<InvalidBlobNameException>(() => encryptedAzureBlobClient.GetAsync(blobName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an azure blob with data that was not encrypted with the provided key
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetBlobWithWrongKey()
        {
            var blobName = "someblob";
            var blobContent = new string('a', 100);
            var memoryBlobClient = new MemoryBlobClient();
            using (var httpContent = new StringContent(blobContent))
            {
                await memoryBlobClient.PutAsync(blobName, httpContent).ConfigureAwait(false);
            }

            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(memoryBlobClient, new Encryptor());

            await Assert.ThrowsExceptionAsync<InvalidBlobNameException>(() => encryptedAzureBlobClient.GetAsync(blobName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an azure blob whose data is too long to encrypt
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutBlobWithLongData()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            using (var httpContent = new StringContent(new string('a', StringUtilities.MaxLength)))
            {
                await Assert.ThrowsExceptionAsync<InvalidBlobDataException>(() => encryptedAzureBlobClient.PutAsync("someblob", httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves an azure blob that doesn't exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNonexistentBlobWithErrorMessage()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            var blobName = "someblob";
            using (var httpResponse = await encryptedAzureBlobClient.GetAsync(blobName).ConfigureAwait(false))
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
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).GetNullBlobName();
        }

        /// <summary>
        /// Retrieves an azure blob with an empty name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetEmptyBlobName()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).GetEmptyBlobName();
        }

        /// <summary>
        /// Writes an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNullBlobName()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutNullBlobName();
        }

        /// <summary>
        /// Writes an azure blob with <see langword="null"/> content
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNullContent()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutNullContent();
        }

        /// <summary>
        /// Writes an azure blob with an empty name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutEmptyBlobName()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutEmptyBlobName();
        }

        /// <summary>
        /// Retrieves an azure blob that doesn't exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNonexistentBlob()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).GetNonexistentBlob();
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutAndGet()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutAndGet();
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it where all URL data contains extra slashes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutAndGetWithExtraSlashes()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutAndGetWithExtraSlashes();
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it where all URL data contains extra slashes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutExistingBlob()
        {
            var encryptedAzureBlobClient = new EncryptedAzureBlobClient(new MemoryBlobClient(), new Encryptor());
            await new AzureBlobClientTests(() => encryptedAzureBlobClient).PutExistingBlob();
        }
    }
}
