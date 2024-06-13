namespace OddTrotter.AzureBlobClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Integration tests for <see cref="AzureBlobClient"/>
    /// </summary>
    /// <remarks>
    /// Note that these tests require:
    /// 1. A network connection with access to blob.core.windows.net
    /// 2. The use of the "oddtrotterabcit" storage account
    /// </remarks>
    [TestClass]
    public sealed class AzureBlobClientIntegrationTests
    {
        /// <summary>
        /// Retrieves an azure blob using an invalid SAS token
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetInvalidSasToken()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            await Assert.ThrowsExceptionAsync<InvalidSasTokenException>(() => azureBlobClient.GetAsync("someblob")).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an azure blob using an expired SAS token
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetExpiredSasToken()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                testConfiguration.GetExpiredSasToken,
                "2020-02-10");
            await Assert.ThrowsExceptionAsync<SasTokenNoReadPrivilegesException>(() => azureBlobClient.GetAsync("someblob")).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an azure blob using a SAS token that has no read permissions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNoReadPermissions()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                testConfiguration.GetNoReadPermissions,
                "2020-02-10");
            await Assert.ThrowsExceptionAsync<SasTokenNoReadPrivilegesException>(() => azureBlobClient.GetAsync("someblob")).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an azure blob when a network error occurs
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNoNetwork()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://0.0.0.0/oddtrotter").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => azureBlobClient.GetAsync("someblob")).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes an azure blob using an invalid SAS token
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutInvalidSasToken()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            using (var httpContent = new StringContent("some content"))
            {
                await Assert.ThrowsExceptionAsync<InvalidSasTokenException>(() => azureBlobClient.PutAsync("someblob", httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob using an expired SAS token
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutExpiredSasToken()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                testConfiguration.PutExpiredSasToken,
                "2020-02-10");
            using (var httpContent = new StringContent("some content"))
            {
                await Assert.ThrowsExceptionAsync<SasTokenNoWritePrivilegesException>(() => azureBlobClient.PutAsync("someblob", httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob using a SAS token that has no write permissions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNoWritePermissions()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                testConfiguration.PutNoWritePermissions,
                "2020-02-10");
            using (var httpContent = new StringContent("some content"))
            {
                await Assert.ThrowsExceptionAsync<SasTokenNoWritePrivilegesException>(() => azureBlobClient.PutAsync("someblob", httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob when a network error occurs
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNoNetwork()
        {
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://0.0.0.0/oddtrotter").ToAbsoluteUri(),
                "sometoken",
                "2020-02-10");
            using (var httpContent = new StringContent("some content"))
            {
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => azureBlobClient.PutAsync("someblob", httpContent)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes an azure blob and then retrieves it
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutAndGet()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter").ToAbsoluteUri(),
                testConfiguration.PutAndGet,
                "2020-02-10");
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
        [TestMethod]
        public async Task PutAndGetWithExtraSlashes()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var azureBlobClient = new AzureBlobClient(
                new Uri("https://oddtrotterabcit.blob.core.windows.net/oddtrotter/").ToAbsoluteUri(),
                testConfiguration.PutAndGet,
                "2020-02-10");
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
            var testConfigurationType = typeof(AzureBlobClientIntegrationTests);

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
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The embedded resource was expected at '{resourceName}' but no embedded resource was found with that name. The repository has a JSON file at this location that contains the overall structure of the configuraiton and instructions on how to modify the configuration. Please ensure that this file exists before running these tests.", e);
                }
                catch (NotImplementedException e)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The embedded resource was found at '{resourceName}' but the contents were larger than '{Int64.MaxValue}'. This indicates a misconfiguration of the JSON file. Please read the instructions in the JSON file at this location and modify it accordingly.", e);
                }

                if (resourceStream == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The embedded resource was expected at '{resourceName}' but no embedded resource was found with that name. The repository has a JSON file at this location that contains the overall structure of the configuraiton and instructions on how to modify the configuration. Please ensure that this file exists before running these tests.");
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
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' was not valid JSON.", e);
                }
                catch (NotSupportedException e)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a JSON object that did not have a corresponding JsonConverter", e);
                }

                if (testConfigurationBuilder == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained only the 'null' value");
                }

                if (testConfigurationBuilder.GetNoReadPermissions == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetNoReadPermissions)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.GetExpiredSasToken == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.GetExpiredSasToken)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PutExpiredSasToken == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PutExpiredSasToken)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PutNoWritePermissions == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PutNoWritePermissions)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PutAndGet == null)
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PutAndGet)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                var testConfiguration = testConfigurationBuilder.Build();

                if (string.Equals(testConfiguration.GetNoReadPermissions, testName ?? nameof(testConfiguration.GetNoReadPermissions), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetNoReadPermissions)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.GetExpiredSasToken, testName ?? nameof(testConfiguration.GetExpiredSasToken), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.GetExpiredSasToken)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PutExpiredSasToken, testName ?? nameof(testConfiguration.GetExpiredSasToken), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PutExpiredSasToken)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PutNoWritePermissions, testName ?? nameof(testConfiguration.GetExpiredSasToken), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PutNoWritePermissions)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PutAndGet, testName ?? nameof(testConfiguration.GetExpiredSasToken), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{nameof(AzureBlobClientIntegrationTests)}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PutAndGet)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
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
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="getNoReadPermissions"/> or <paramref name="getExpiredSasToken"/> or <paramref name="putExpiredSasToken"/> or <paramref name="putNoWritePermissions"/> or <paramref name="putAndGet"/> or <paramref name="putAndGetWithExtraSlashes"/> is <see langword="null"/></exception>
            public TestConfiguration(string getNoReadPermissions, string getExpiredSasToken, string putExpiredSasToken, string putNoWritePermissions, string putAndGet, string putAndGetWithExtraSlashes)
            {
                if (getNoReadPermissions == null)
                {
                    throw new ArgumentNullException(nameof(getNoReadPermissions));
                }

                if (getExpiredSasToken == null)
                {
                    throw new ArgumentNullException(nameof(getExpiredSasToken));
                }

                if (putExpiredSasToken == null)
                {
                    throw new ArgumentNullException(nameof(putExpiredSasToken));
                }

                if (putNoWritePermissions == null)
                {
                    throw new ArgumentNullException(nameof(putNoWritePermissions));
                }

                if (putAndGet == null)
                {
                    throw new ArgumentNullException(nameof(putAndGet));
                }

                if (putAndGetWithExtraSlashes == null)
                {
                    throw new ArgumentNullException(nameof(putAndGetWithExtraSlashes));
                }

                this.GetNoReadPermissions = getNoReadPermissions;
                this.GetExpiredSasToken = getExpiredSasToken;
                this.PutExpiredSasToken = putExpiredSasToken;
                this.PutNoWritePermissions = putNoWritePermissions;
                this.PutAndGet = putAndGet;
                this.PutAndGetWithExtraSlashes = putAndGetWithExtraSlashes;
            }

            public string GetNoReadPermissions { get; }

            public string GetExpiredSasToken { get; }

            public string PutExpiredSasToken { get; }

            public string PutNoWritePermissions { get; }

            public string PutAndGet { get; }

            public string PutAndGetWithExtraSlashes { get; }

            public sealed class Builder
            {
                [JsonPropertyName("GetNoReadPermissions")]
                public string? GetNoReadPermissions { get; set; }

                [JsonPropertyName("GetExpiredSasToken")]
                public string? GetExpiredSasToken { get; set; }

                [JsonPropertyName("PutExpiredSasToken")]
                public string? PutExpiredSasToken { get; set; }

                [JsonPropertyName("PutNoWritePermissions")]
                public string? PutNoWritePermissions { get; set; }

                [JsonPropertyName("PutAndGet")]
                public string? PutAndGet { get; set; }

                [JsonPropertyName("PutAndGetWithExtraSlashes")]
                public string? PutAndGetWithExtraSlashes { get; set; }

                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <see cref="GetNoReadPermissions"/> or <see cref="GetExpiredSasToken"/> or <see cref="PutExpiredSasToken"/> or <see cref="PutNoWritePermissions"/> or <see cref="PutAndGet"/> or <see cref="PutAndGetWithExtraSlashes"/> is <see langword="null"/></exception>
                public TestConfiguration Build()
                {
                    if (this.GetNoReadPermissions == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetNoReadPermissions));
                    }

                    if (this.GetExpiredSasToken == null)
                    {
                        throw new ArgumentNullException(nameof(this.GetExpiredSasToken));
                    }

                    if (this.PutExpiredSasToken == null)
                    {
                        throw new ArgumentNullException(nameof(this.PutExpiredSasToken));
                    }

                    if (this.PutNoWritePermissions == null)
                    {
                        throw new ArgumentNullException(nameof(this.PutNoWritePermissions));
                    }

                    if (this.PutAndGet == null)
                    {
                        throw new ArgumentNullException(nameof(this.PutAndGet));
                    }

                    if (this.PutAndGetWithExtraSlashes == null)
                    {
                        throw new ArgumentNullException(nameof(this.PutAndGetWithExtraSlashes));
                    }

                    return new TestConfiguration(this.GetNoReadPermissions, this.GetExpiredSasToken, this.PutExpiredSasToken, this.PutNoWritePermissions, this.PutAndGet, this.PutAndGetWithExtraSlashes);
                }
            }
        }
    }
}
