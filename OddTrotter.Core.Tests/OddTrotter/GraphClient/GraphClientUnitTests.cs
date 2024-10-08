﻿namespace OddTrotter.GraphClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="GraphClient"/>
    /// </summary>
    [TestCategory(TestCategories.Unit)]
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
                new GraphClientSettings.Builder().Build()));
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a whitespace access token
        /// </summary>
        [TestMethod]
        public void WhitespaceAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() => new GraphClient(
                "   \t",
                new GraphClientSettings.Builder().Build()));
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
                ));
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> with a <see langword="null"> settings
        /// </summary>
        [TestMethod]
        public void InvalidAccessToken()
        {
            Assert.ThrowsException<InvalidAccessTokenException>(() => new GraphClient(
                Environment.NewLine + "sometoken",
                new GraphClientSettings.Builder().Build()));
        }

        /// <summary>
        /// Creates a new <see cref="GraphClient"/> using an invalid bearer token
        /// </summary>
        [TestMethod]
        public void InvalidBearerToken()
        {
            var bearerToken = "1/mZ1edKKACtPAb7zGlwSzvs72PvhAbGmB8K1ZrGxpcNM";
            var factory = (string token) => new GraphClient(token, new GraphClientSettings.Builder().Build());

            Assert.ThrowsException<InvalidAccessTokenException>(() => factory(bearerToken));

            bearerToken = $"Bearer {bearerToken}";
            factory(bearerToken);
        }

        /// <summary>
        /// Retrieves a <see langword="null"/> URI
        /// </summary>
        [TestMethod]
        public async Task GetNullRelativeUri()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.GetAsync(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (RelativeUri)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a <see langword="null"/> URI
        /// </summary>
        [TestMethod]
        public async Task GetNullAbsoluteUri()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.GetAsync(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (AbsoluteUri)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a <see langword="null"/> URI
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchNullUri()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            using (var content = new StringContent(string.Empty))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.PatchAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    content
                    )).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Updates with <see langword="null"/> content
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchNullContent()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.PatchAsync(
                new Uri("/someuri", UriKind.Relative).ToRelativeUri(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see langword="null"/> URI
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostNullUri()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            using (var content = new StringContent(string.Empty))
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.PostAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    content
                    )).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates with <see langword="null"/> content
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostNullContent()
        {
            var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => graphClient.PostAsync(
                new Uri("/someuri", UriKind.Relative).ToRelativeUri(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )).ConfigureAwait(false);
        }
    }
}
