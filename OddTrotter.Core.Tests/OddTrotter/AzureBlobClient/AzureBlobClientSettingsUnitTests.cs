namespace OddTrotter.AzureBlobClient
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="AzureBlobClientSettings"/>
    /// </summary>
    [TestClass]
    public sealed class AzureBlobClientSettingsUnitTests
    {
        /// <summary>
        /// Creates default <see cref="AzureBlobClientSettings"/>
        /// </summary>
        [TestMethod]
        public void DefaultSettings()
        {
            var builder = new AzureBlobClientSettings.Builder();
            var settings = builder.Build();
            Assert.AreEqual(builder.BlobType, settings.BlobType);
        }

        /// <summary>
        /// Creates <see cref="AzureBlobClientSettings"/> with a <see langword="null"/> blob type
        /// </summary>
        [TestMethod]
        public void NullBlobType()
        {
            var builder = new AzureBlobClientSettings.Builder()
            {
                BlobType =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            Assert.ThrowsException<ArgumentNullException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="AzureBlobClientSettings"/> with an empty blob type
        /// </summary>
        [TestMethod]
        public void EmptyBlobType()
        {
            var builder = new AzureBlobClientSettings.Builder()
            {
                BlobType = string.Empty,
            };

            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="AzureBlobClientSettings"/> with a whitespace blob type
        /// </summary>
        [TestMethod]
        public void WhitespaceBlobType()
        {
            var builder = new AzureBlobClientSettings.Builder()
            {
                BlobType = "   \t",
            };

            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="AzureBlobClientSettings"/> with a blob type that results in an invalid header value
        /// </summary>
        [TestMethod]
        public void InvalidBlobType()
        {
            var builder = new AzureBlobClientSettings.Builder()
            {
                BlobType = Environment.NewLine + "asdf",
            };

            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }
    }
}
