namespace OddTrotter.GraphClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Integration tests for <see cref="GraphClient"/>
    /// </summary>
    /// <remarks>
    /// Note that these tests require:
    /// 1. A network connection with access to https://graph.microsoft.com/v1.0
    /// </remarks>
    [TestClass]
    public sealed class GraphClientIntegrationTests
    {
        /// <summary>
        /// Retrieves a user that does not exist
        /// </summary>
        [TestMethod]
        public async Task GetNonexistentUserAbsoluteUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.GetNonexistentUserAbsoluteUri, new GraphClientSettings.Builder().Build()))
            {
                using (var httpResponse = await graphClient.GetAsync(new Uri("https://graph.microsoft.com/v1.0/users/00000000-0000-0000-000000000000", UriKind.Absolute).ToAbsoluteUri()).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode, await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
        }

        /// <summary>
        /// Retrieves a user when there is no network
        /// </summary>
        [TestMethod]
        public async Task GetUserWithNoNetworkAbsoluteUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.GetUserWithNoNetworkAbsoluteUri, new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => graphClient.GetAsync(new Uri("https://0.0.0.0/v1.0/users/00000000-0000-0000-000000000000", UriKind.Absolute).ToAbsoluteUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a user with an invalid access token
        /// </summary>
        [TestMethod]
        public async Task GetUserWithInvalidAccessTokenAbsoluteUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.GetAsync(new Uri("https://graph.microsoft.com/v1.0/users/00000000-0000-0000-000000000000", UriKind.Absolute).ToAbsoluteUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a user with an access token that doesn't have read permissions
        /// </summary>
        [TestMethod]
        public async Task GetUserWithNoPermissionsAbsoluteUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.GetUserWithNoPermissionsAbsoluteUri, new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.GetAsync(new Uri("https://graph.microsoft.com/v1.0/servicePrincipals", UriKind.Absolute).ToAbsoluteUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a user that does not exist
        /// </summary>
        [TestMethod]
        public async Task GetNonexistentUserRelativeUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.GetNonexistentUserRelativeUri, new GraphClientSettings.Builder().Build()))
            {
                using (var httpResponse = await graphClient.GetAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri()).ConfigureAwait(false))
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode, await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
        }

        /// <summary>
        /// Retrieves a user when there is no network
        /// </summary>
        [TestMethod]
        public async Task GetUserWithNoNetworkRelativeUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var graphClientSettings = new GraphClientSettings.Builder()
            {
                GraphRootUri = new Uri("https://0.0.0.0/v1.0"),
            }.Build();
            using (var graphClient = new GraphClient(testConfiguration.GetUserWithNoNetworkRelativeUri, graphClientSettings))
            {
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => graphClient.GetAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a user with an invalid access token
        /// </summary>
        [TestMethod]
        public async Task GetUserWithInvalidAccessTokenRelativeUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.GetAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a user with an access token that doesn't have read permissions
        /// </summary>
        [TestMethod]
        public async Task GetUserWithNoPermissionsRelativeUri()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.GetUserWithNoPermissionsRelativeUri, new GraphClientSettings.Builder().Build()))
            {
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.GetAsync(new Uri("/servicePrincipals", UriKind.Relative).ToRelativeUri())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Thrown if the test configuraiton embedded resource could not be found or it contained an invalid configuration</exception>
        private static TestConfiguration GetAndValidateTestConfiguration([CallerMemberName] string memberName = "")
        {
            return GetTestConfiguration(memberName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Thrown if the test configuraiton embedded resource could not be found or it contained an invalid configuration</exception>
        private static TestConfiguration GetTestConfiguration(string? testName = null)
        {
            var testConfigurationType = typeof(GraphClientIntegrationTests);

            var resourceName = testConfigurationType.Namespace + "." + testConfigurationType.Name + ".json";
            Stream? resourceStream = null;
            try
            {
                try
                {
                    resourceStream = testConfigurationType.Assembly.GetManifestResourceStream(resourceName);
                }
                catch (FileNotFoundException e)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The embedded resource was expected at '{resourceName}' but no embedded resource was found with that name. The repository has a JSON file at this location that contains the overall structure of the configuraiton and instructions on how to modify the configuration. Please ensure that this file exists before running these tests.", e);
                }
                catch (NotImplementedException e)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The embedded resource was found at '{resourceName}' but the contents were larger than '{Int64.MaxValue}'. This indicates a misconfiguration of the JSON file. Please read the instructions in the JSON file at this location and modify it accordingly.", e);
                }

                if (resourceStream == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The embedded resource was expected at '{resourceName}' but no embedded resource was found with that name. The repository has a JSON file at this location that contains the overall structure of the configuraiton and instructions on how to modify the configuration. Please ensure that this file exists before running these tests.");
                }

                TestConfiguration.Builder? testConfigurationBuilder;
                try
                {
                    testConfigurationBuilder = JsonSerializer.Deserialize<TestConfiguration.Builder>(
                        resourceStream,
                        new JsonSerializerOptions()
                        {
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true,
                        });
                }
                catch (JsonException e)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' was not valid JSON.", e);
                }
                catch (NotSupportedException e)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a JSON object that did not have a corresponding JsonConverter", e);
                }

                if (testConfigurationBuilder == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained only the 'null' value");
                }

                if (testConfigurationBuilder.GetNonexistentUserAbsoluteUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetNonexistentUserAbsoluteUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetUserWithNoNetworkAbsoluteUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetUserWithNoNetworkAbsoluteUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetUserWithNoPermissionsAbsoluteUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetUserWithNoPermissionsAbsoluteUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetNonexistentUserRelativeUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetNonexistentUserRelativeUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetUserWithNoNetworkRelativeUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetUserWithNoNetworkRelativeUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetUserWithNoPermissionsRelativeUri == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetUserWithNoPermissionsRelativeUri)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                var testConfiguration = testConfigurationBuilder.Build();

                if (string.Equals(testConfiguration.GetNonexistentUserAbsoluteUri, testName ?? nameof(testConfiguration.GetNonexistentUserAbsoluteUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetNonexistentUserAbsoluteUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetUserWithNoNetworkAbsoluteUri, testName ?? nameof(testConfiguration.GetUserWithNoNetworkAbsoluteUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetUserWithNoNetworkAbsoluteUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetUserWithNoPermissionsAbsoluteUri, testName ?? nameof(testConfiguration.GetUserWithNoPermissionsAbsoluteUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetUserWithNoPermissionsAbsoluteUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetNonexistentUserRelativeUri, testName ?? nameof(testConfiguration.GetNonexistentUserRelativeUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetNonexistentUserRelativeUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetUserWithNoNetworkRelativeUri, testName ?? nameof(testConfiguration.GetUserWithNoNetworkRelativeUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetUserWithNoNetworkRelativeUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetUserWithNoPermissionsRelativeUri, testName ?? nameof(testConfiguration.GetUserWithNoPermissionsRelativeUri), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetUserWithNoPermissionsRelativeUri)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                return testConfiguration;
            }
            finally
            {
                resourceStream?.Dispose();
            }
        }

        private sealed class TestConfiguration
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="azureBlobContainerExpiredSasToken"></param>
            /// <param name="getExpiredSasToken"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="getNonexistentUserAbsoluteUri"/> or <paramref name="getNonexistentUserAbsoluteUri"/> or <paramref name="getUserWithNoPermissionsAbsoluteUri"/> or <paramref name="getNonexistentUserRelativeUri"/> or <paramref name="getUserWithNoNetworkRelativeUri"/> or <paramref name="getUserWithNoPermissionsRelativeUri"/> is <see langword="null"/></exception>
            public TestConfiguration(string getNonexistentUserAbsoluteUri, string getUserWithNoNetworkAbsoluteUri, string getUserWithNoPermissionsAbsoluteUri, string getNonexistentUserRelativeUri, string getUserWithNoNetworkRelativeUri, string getUserWithNoPermissionsRelativeUri)
            {
                if (getNonexistentUserAbsoluteUri == null)
                {
                    throw new ArgumentNullException(nameof(getNonexistentUserAbsoluteUri));
                }

                if (getUserWithNoNetworkAbsoluteUri == null)
                {
                    throw new ArgumentNullException(nameof(getUserWithNoNetworkAbsoluteUri));
                }

                if (getUserWithNoPermissionsAbsoluteUri == null)
                {
                    throw new ArgumentNullException(nameof(getUserWithNoPermissionsAbsoluteUri));
                }

                if (getNonexistentUserRelativeUri == null)
                {
                    throw new ArgumentNullException(nameof(getNonexistentUserRelativeUri));
                }

                if (getUserWithNoNetworkRelativeUri == null)
                {
                    throw new ArgumentNullException(nameof(getUserWithNoNetworkRelativeUri));
                }

                if (getUserWithNoPermissionsRelativeUri == null)
                {
                    throw new ArgumentNullException(nameof(getUserWithNoPermissionsRelativeUri));
                }

                this.GetNonexistentUserAbsoluteUri = getNonexistentUserAbsoluteUri;
                this.GetUserWithNoNetworkAbsoluteUri = getUserWithNoNetworkAbsoluteUri;
                this.GetUserWithNoPermissionsAbsoluteUri = getUserWithNoPermissionsAbsoluteUri;
                this.GetNonexistentUserRelativeUri = getNonexistentUserRelativeUri;
                this.GetUserWithNoNetworkRelativeUri = getUserWithNoNetworkRelativeUri;
                this.GetUserWithNoPermissionsRelativeUri = getUserWithNoPermissionsRelativeUri;
            }

            public string GetNonexistentUserAbsoluteUri { get; }

            public string GetUserWithNoNetworkAbsoluteUri { get; }

            public string GetUserWithNoPermissionsAbsoluteUri { get; }

            public string GetNonexistentUserRelativeUri { get; }

            public string GetUserWithNoNetworkRelativeUri { get; }

            public string GetUserWithNoPermissionsRelativeUri { get; }

            public sealed class Builder
            {
                [JsonPropertyName("GetNonexistentUserAbsoluteUri")]
                public string? GetNonexistentUserAbsoluteUri { get; set; }

                [JsonPropertyName("GetUserWithNoNetworkAbsoluteUri")]
                public string? GetUserWithNoNetworkAbsoluteUri { get; set; }

                [JsonPropertyName("GetUserWithNoPermissionsAbsoluteUri")]
                public string? GetUserWithNoPermissionsAbsoluteUri { get; set; }

                [JsonPropertyName("GetNonexistentUserRelativeUri")]
                public string? GetNonexistentUserRelativeUri { get; set; }

                [JsonPropertyName("GetUserWithNoNetworkRelativeUri")]
                public string? GetUserWithNoNetworkRelativeUri { get; set; }

                [JsonPropertyName("GetUserWithNoPermissionsRelativeUri")]
                public string? GetUserWithNoPermissionsRelativeUri { get; set; }

                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <see cref="GetNonexistentUserAbsoluteUri"/> or <see cref="GetUserWithNoNetworkAbsoluteUri"/> or <see cref="GetUserWithNoPermissionsAbsoluteUri"/> or <see cref="GetNonexistentUserRelativeUri"/> or <see cref="GetUserWithNoNetworkRelativeUri"/> or <see cref="GetUserWithNoPermissionsRelativeUri"/> is <see langword="null"/></exception>
                public TestConfiguration Build()
                {
                    if (this.GetNonexistentUserAbsoluteUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetNonexistentUserAbsoluteUri));
                    }

                    if (this.GetUserWithNoNetworkAbsoluteUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetUserWithNoNetworkAbsoluteUri));
                    }

                    if (this.GetUserWithNoPermissionsAbsoluteUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetUserWithNoPermissionsAbsoluteUri));
                    }

                    if (this.GetNonexistentUserRelativeUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetNonexistentUserRelativeUri));
                    }

                    if (this.GetUserWithNoNetworkRelativeUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetUserWithNoNetworkRelativeUri));
                    }

                    if (this.GetUserWithNoPermissionsRelativeUri == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetUserWithNoPermissionsRelativeUri));
                    }

                    return new TestConfiguration(this.GetNonexistentUserAbsoluteUri, this.GetUserWithNoNetworkAbsoluteUri, this.GetUserWithNoPermissionsAbsoluteUri, this.GetNonexistentUserRelativeUri, this.GetUserWithNoNetworkRelativeUri, this.GetUserWithNoPermissionsRelativeUri);
                }
            }
        }
    }
}
