namespace OddTrotter.GraphClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
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
    [TestCategory(TestCategories.Integration)]
    [TestClass]
    public sealed class GraphClientIntegrationTests
    {
        [TestMethod]
        public void Test()
        {
            //// TODO:
            //// 1. you should have a script that creates a new MSA account and gets a free test tenant from that account; this will require some (minimal) user interaction from the person running the script
            //// 2. then you should have a script that uses that tenant to create a user and an application in that tenant
            //// 3. this user and application will be used to create the test-specific users/applications/other resources
            ////
            //// do the above once in-order to ensure that the steps will work
            //// implement the above in reverse order
            ////
            //// all of the permissions granted should be done so using just-in-time mechanisms
            //// jit mechanisms should be used to prevent the original super-user for maintaining their super-user permissions once the tests are done
        }

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
        /// Updates a user that does not exist
        /// </summary>
        [TestMethod]
        public async Task PatchNonexistentUser()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.PatchNonexistentUser, new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    using (var httpResponse = await graphClient.PatchAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri(), content).ConfigureAwait(false))
                    {
                        Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode, await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                }
            }
        }

        /// <summary>
        /// Updates a user when there is no network
        /// </summary>
        [TestMethod]
        public async Task PatchUserWithNoNetwork()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var graphClientSettings = new GraphClientSettings.Builder()
            {
                GraphRootUri = new Uri("https://0.0.0.0/v1.0"),
            }.Build();
            using (var graphClient = new GraphClient("sometoken", graphClientSettings))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<HttpRequestException>(() => graphClient.PatchAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Update a user with an invalid access token
        /// </summary>
        [TestMethod]
        public async Task PatchUserWithInvalidAccessToken()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.PatchAsync(new Uri("/users/00000000-0000-0000-000000000000", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Updates a service principal with an access token that doesn't have write permissions
        /// </summary>
        [TestMethod]
        public async Task PatchServicePrincipalWithNoPermissions()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.PatchServicePrincipalWithNoPermissions, new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.PatchAsync(new Uri("/servicePrincipals(appId='00000003-0000-0000-c000-000000000000')", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Creates a new application
        /// </summary>
        [TestMethod]
        public async Task PostApplication()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.PostApplication, new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "testing"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    using (var httpResponse = await graphClient.PostAsync(new Uri("/applications", UriKind.Relative).ToRelativeUri(), content).ConfigureAwait(false))
                    {
                        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode, await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a user when there is no network
        /// </summary>
        [TestMethod]
        public async Task PostUserWithNoNetwork()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            var graphClientSettings = new GraphClientSettings.Builder()
            {
                GraphRootUri = new Uri("https://0.0.0.0/v1.0"),
            }.Build();
            using (var graphClient = new GraphClient("sometoken", graphClientSettings))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<HttpRequestException>(() => graphClient.PostAsync(new Uri("/users", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Creates a user with an invalid access token
        /// </summary>
        [TestMethod]
        public async Task PostUserWithInvalidAccessToken()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient("sometoken", new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "test"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.PostAsync(new Uri("/users", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Creates an application with an access token that doesn't have write permissions
        /// </summary>
        [TestMethod]
        public async Task PostApplicationWithNoPermissions()
        {
            var testConfiguration = GetAndValidateTestConfiguration();
            using (var graphClient = new GraphClient(testConfiguration.PostApplicationWithNoPermissions, new GraphClientSettings.Builder().Build()))
            {
                using (var content = new StringContent(
"""
{
  "displayName": "testing"
}
""",
                    new MediaTypeHeaderValue("application/json")))
                {
                    await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => graphClient.PostAsync(new Uri("/applications", UriKind.Relative).ToRelativeUri(), content)).ConfigureAwait(false);
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

                if (testConfigurationBuilder.PatchNonexistentUser == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PatchNonexistentUser)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PatchServicePrincipalWithNoPermissions == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PatchServicePrincipalWithNoPermissions)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PostApplication == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PostApplication)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (testConfigurationBuilder.PostApplicationWithNoPermissions == null)
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained a 'null' value for '{nameof(testConfigurationBuilder.PostApplicationWithNoPermissions)}'. Please follow the instructions in the JSON file for what value is expected in this property.");
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

                if (string.Equals(testConfiguration.PatchNonexistentUser, testName ?? nameof(testConfiguration.PatchNonexistentUser), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PatchNonexistentUser)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PatchServicePrincipalWithNoPermissions, testName ?? nameof(testConfiguration.PatchServicePrincipalWithNoPermissions), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PatchServicePrincipalWithNoPermissions)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PostApplication, testName ?? nameof(testConfiguration.PostApplication), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PostApplication)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
                }

                if (string.Equals(testConfiguration.PostApplicationWithNoPermissions, testName ?? nameof(testConfiguration.PostApplicationWithNoPermissions), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"The tests in '{testConfigurationType.Name}' use an embedded resource to store a test configuration. The test configuration at '{resourceName}' contained its default value for the property '{nameof(testConfiguration.PostApplicationWithNoPermissions)}', which is not valid for the test. Please follow the instructions in the JSON file for what value is expected in this property.");
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
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="getNonexistentUserAbsoluteUri"/> or <paramref name="getNonexistentUserAbsoluteUri"/> or <paramref name="getUserWithNoPermissionsAbsoluteUri"/> or <paramref name="getNonexistentUserRelativeUri"/> or <paramref name="getUserWithNoNetworkRelativeUri"/> or <paramref name="getUserWithNoPermissionsRelativeUri"/> or <paramref name="patchNonexistentUser"/> or <paramref name="postApplication"/> or <paramref name="postApplicationWithNoPermissions"/> is <see langword="null"/></exception>
            public TestConfiguration(string getNonexistentUserAbsoluteUri, string getUserWithNoNetworkAbsoluteUri, string getUserWithNoPermissionsAbsoluteUri, string getNonexistentUserRelativeUri, string getUserWithNoNetworkRelativeUri, string getUserWithNoPermissionsRelativeUri, string patchNonexistentUser, string patchServicePrincipalWithNoPermissions, string postApplication, string postApplicationWithNoPermissions)
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

                if (patchNonexistentUser == null)
                {
                    throw new ArgumentNullException(nameof(patchNonexistentUser));
                }

                if (patchServicePrincipalWithNoPermissions == null)
                {
                    throw new ArgumentNullException(nameof(patchServicePrincipalWithNoPermissions));
                }

                if (postApplication == null)
                {
                    throw new ArgumentNullException(nameof(postApplication));
                }

                if (postApplicationWithNoPermissions == null)
                {
                    throw new ArgumentNullException(nameof(postApplicationWithNoPermissions));
                }

                this.GetNonexistentUserAbsoluteUri = getNonexistentUserAbsoluteUri;
                this.GetUserWithNoNetworkAbsoluteUri = getUserWithNoNetworkAbsoluteUri;
                this.GetUserWithNoPermissionsAbsoluteUri = getUserWithNoPermissionsAbsoluteUri;
                this.GetNonexistentUserRelativeUri = getNonexistentUserRelativeUri;
                this.GetUserWithNoNetworkRelativeUri = getUserWithNoNetworkRelativeUri;
                this.GetUserWithNoPermissionsRelativeUri = getUserWithNoPermissionsRelativeUri;
                this.PatchNonexistentUser = patchNonexistentUser;
                this.PatchServicePrincipalWithNoPermissions = patchServicePrincipalWithNoPermissions;
                this.PostApplication = postApplication;
                this.PostApplicationWithNoPermissions = postApplicationWithNoPermissions;
            }

            public string GetNonexistentUserAbsoluteUri { get; }

            public string GetUserWithNoNetworkAbsoluteUri { get; }

            public string GetUserWithNoPermissionsAbsoluteUri { get; }

            public string GetNonexistentUserRelativeUri { get; }

            public string GetUserWithNoNetworkRelativeUri { get; }

            public string GetUserWithNoPermissionsRelativeUri { get; }

            public string PatchNonexistentUser { get; }

            public string PatchServicePrincipalWithNoPermissions { get; }

            public string PostApplication { get; }

            public string PostApplicationWithNoPermissions { get; }

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
                
                [JsonPropertyName("PatchNonexistentUser")]
                public string? PatchNonexistentUser { get; set; }

                [JsonPropertyName("PatchServicePrincipalWithNoPermissions")]
                public string? PatchServicePrincipalWithNoPermissions { get; set; }

                [JsonPropertyName("PostApplication")]
                public string? PostApplication { get; set; }

                [JsonPropertyName("PostApplicationWithNoPermissions")]
                public string? PostApplicationWithNoPermissions { get; set; }

                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                /// <exception cref="ArgumentNullException">Thrown if <see cref="GetNonexistentUserAbsoluteUri"/> or <see cref="GetUserWithNoNetworkAbsoluteUri"/> or <see cref="GetUserWithNoPermissionsAbsoluteUri"/> or <see cref="GetNonexistentUserRelativeUri"/> or <see cref="GetUserWithNoNetworkRelativeUri"/> or <see cref="GetUserWithNoPermissionsRelativeUri"/> or <see cref="PatchNonexistentUser"/> or <see cref="PatchServicePrincipalWithNoPermissions"/> or <see cref="PostApplication"/> or <see cref="PostApplicationWithNoPermissions"/> is <see langword="null"/></exception>
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

                    if (this.PatchNonexistentUser == null)
                    {
                        throw new ArgumentNullException(nameof(this.PatchNonexistentUser));
                    }

                    if (this.PatchServicePrincipalWithNoPermissions == null)
                    {
                        throw new ArgumentNullException(nameof(this.PatchServicePrincipalWithNoPermissions));
                    }

                    if (this.PostApplication == null)
                    {
                        throw new ArgumentNullException(nameof(this.PostApplication));
                    }

                    if (this.PostApplicationWithNoPermissions == null)
                    {
                        throw new ArgumentNullException(nameof(this.PostApplicationWithNoPermissions));
                    }

                    return new TestConfiguration(this.GetNonexistentUserAbsoluteUri, this.GetUserWithNoNetworkAbsoluteUri, this.GetUserWithNoPermissionsAbsoluteUri, this.GetNonexistentUserRelativeUri, this.GetUserWithNoNetworkRelativeUri, this.GetUserWithNoPermissionsRelativeUri, this.PatchNonexistentUser, this.PatchServicePrincipalWithNoPermissions, this.PostApplication, this.PostApplicationWithNoPermissions);
                }
            }
        }
    }
}
