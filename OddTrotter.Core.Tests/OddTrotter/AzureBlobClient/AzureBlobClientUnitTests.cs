namespace OddTrotter.AzureBlobClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="AzureBlobClient"/>
    /// </summary>
    [TestClass]
    public sealed class AzureBlobClientUnitTests
    {
        /// <summary>
        /// Creates a <see cref="AzureBlobClient"/> with a <see langword="null"/> container URI
        /// </summary>
        [TestMethod]
        public void NullContainerUri()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobClient(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                "sometoken", 
                "someversion"));
        }

        /// <summary>
        /// Creates a <see cref="AzureBlobClient"/> with a <see langword="null"/> SAS token
        /// </summary>
        [TestMethod]
        public void NullSasToken()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                "someversion"));
        }

        /// <summary>
        /// Creates a <see cref="AzureBlobClient"/> with a <see langword="null"/> API verseion
        /// </summary>
        [TestMethod]
        public void NullApiVersion()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        /// <summary>
        /// Creates a <see cref="AzureBlobClient"/> with an empty API version
        /// </summary>
        [TestMethod]
        public void EmptyApiVersion()
        {
            Assert.ThrowsException<ArgumentException>(() => new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                string.Empty
                ));
        }

        /// <summary>
        /// Creates a <see cref="AzureBlobClient"/> with a <see langword="null"/> <see cref="AzureBlobClientSettings"/>
        /// </summary>
        [TestMethod]
        public void NullSettings()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10",
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }
        
        /// <summary>
        /// Retrieves an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNullBlobName()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
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
        [TestMethod]
        public async Task GetEmptyBlobName()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => azureBlobClient.GetAsync(string.Empty)).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an azure blob with a <see langword="null"/> name
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNullBlobName()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
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
        [TestMethod]
        public async Task PutNullContent()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
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
        [TestMethod]
        public async Task PutEmptyBlobName()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://accountname.blob.core.windows.net/containername").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            using (var httpContent = new StringContent("somecontent"))
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => azureBlobClient.PutAsync(
                    string.Empty,
                    httpContent)).ConfigureAwait(false);
            }
        }
    }
}
