namespace OddTrotter.GraphClient
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="GraphClient"/>
    /// </summary>
    [TestClass]
    public sealed class GraphClientUnitTests
    {
        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a <see langword="null"> access token
        /// </summary>
        [TestMethod]
        public void NullAccessToken()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphClient(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                new GraphClientSettings.Builder().Build()).Dispose());
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a whitespace access token
        /// </summary>
        [TestMethod]
        public void WhitespaceAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() => new GraphClient(
                "   \t",
                new GraphClientSettings.Builder().Build()).Dispose());
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a <see langword="null"> settings
        /// </summary>
        [TestMethod]
        public void NullSettings()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphClient(
                "sometoken",
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ).Dispose());
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a <see langword="null"> settings
        /// </summary>
        [TestMethod]
        public void InvalidAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() => new GraphClient(
                Environment.NewLine + "sometoken",
                new GraphClientSettings.Builder().Build()).Dispose());
        }

        /// <summary>
        /// Retrieves a <see langword="null"/> URI
        /// </summary>
        [TestMethod]
        public async Task GetNullRelativeUri()
        {
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.GetAsync(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    (RelativeUri)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    )).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a <see langword="null"/> URI
        /// </summary>
        [TestMethod]
        public async Task GetNullAbsoluteUri()
        {
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.GetAsync(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    (AbsoluteUri)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    )).ConfigureAwait(false);
            }
        }
    }
}
