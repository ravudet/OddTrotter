namespace OddTrotter.GraphClient
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="GraphClientSettings"/>
    /// </summary>
    [TestClass]
    public sealed class GraphClientSettingsUnitTests
    {
        /// <summary>
        /// Creates default <see cref="GraphClientSettings"/>
        /// </summary>
        [TestMethod]
        public void DefaultSettings()
        {
            var builder = new GraphClientSettings.Builder()
            {
            };
            var settings = builder.Build();
            Assert.AreEqual(builder.GraphRootUri, settings.GraphRootUri);
            Assert.AreEqual(builder.HttpClientTimeout, settings.HttpClientTimeout);
        }

        /// <summary>
        /// Creates <see cref="GraphClientSettings"/> with a <see langword="null"/> root URI
        /// </summary>
        [TestMethod]
        public void NullGraphRootUri()
        {
            var builder = new GraphClientSettings.Builder()
            {
                GraphRootUri =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };
            Assert.ThrowsException<ArgumentNullException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="GraphClientSettings"/> with a relative root URI
        /// </summary>
        [TestMethod]
        public void RelativeGraphRootUri()
        {
            var builder = new GraphClientSettings.Builder()
            {
                GraphRootUri = new Uri("/some/uri", UriKind.Relative),
            };
            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="GraphClientSettings"/> with a negative timeout
        /// </summary>
        [TestMethod]
        public void NegativeHttpClientTimeout()
        {
            var builder = new GraphClientSettings.Builder()
            {
                HttpClientTimeout = TimeSpan.FromDays(-1),
            };
            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }
    }
}
