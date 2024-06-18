namespace OddTrotter.TodoList
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using OddTrotter.AzureBlobClient;
    using OddTrotter.GraphClient;

    /// <summary>
    /// Unit tests for <see cref="TodoListService"/>
    /// </summary>
    [TestClass]
    public sealed class TodoListServiceUnitTests
    {
        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> memory cache
        /// </summary>
        [TestMethod]
        public void NullMemoryCache()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                new MockGraphClient(),
                new MemoryBlobClient()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> graph client
        /// </summary>
        [TestMethod]
        public void NullGraphClient()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
                    memoryCache,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    new MemoryBlobClient()));
            }
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> azure blob client
        /// </summary>
        [TestMethod]
        public void NullAzureBlobClient()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
                    memoryCache,
                    new MockGraphClient(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    ));
            }
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> memory cache
        /// </summary>
        [TestMethod]
        public void NullMemoryCacheWithSettings()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                new MockGraphClient(),
                new MemoryBlobClient(),
                new TodoListServiceSettings.Builder().Build()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> graph client
        /// </summary>
        [TestMethod]
        public void NullGraphClientWithSettings()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
                    memoryCache,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    new MemoryBlobClient(),
                new TodoListServiceSettings.Builder().Build()));
            }
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> azure blob client
        /// </summary>
        [TestMethod]
        public void NullAzureBlobClientWithSettings()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
                    memoryCache,
                    new MockGraphClient(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    new TodoListServiceSettings.Builder().Build()));
            }
        }

        /// <summary>
        /// Creates a <see cref="TodoListService"/> with a <see langword="null"/> azure blob client
        /// </summary>
        [TestMethod]
        public void NullSettings()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                Assert.ThrowsException<ArgumentNullException>(() => new TodoListService(
                    memoryCache,
                    new MockGraphClient(),
                    new MemoryBlobClient(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    ));
            }
        }

        private sealed class MockGraphClient : IGraphClient
        {
            public Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the azure blob client sees the todo list blob name as invalid
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidBlobName()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MockRetrieveTodoListInvalidBlobNameBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<InvalidBlobNameException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListInvalidBlobNameBlobClient : IAzureBlobClient
        {
            public Task<HttpResponseMessage> GetAsync(string blobName)
            {
                throw new InvalidBlobNameException("blob name", new Exception());
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the azure blob client has a network issue
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListBlobHttpRequestException()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MockRetrieveTodoListBlobHttpRequestExceptionBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListBlobHttpRequestExceptionBlobClient : IAzureBlobClient
        {
            public Task<HttpResponseMessage> GetAsync(string blobName)
            {
                throw new HttpRequestException();
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the azure blob client is using an invalid SAS token
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidSasToken()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MockRetrieveTodoListInvalidSasTokenBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<InvalidSasTokenException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListInvalidSasTokenBlobClient : IAzureBlobClient
        {
            public Task<HttpResponseMessage> GetAsync(string blobName)
            {
                throw new InvalidSasTokenException("the token", "a message");
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the azure blob client is using a SAS token that has no read privileges
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListSasTokenNoRead()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MockRetrieveTodoListSasTokenNoReadBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<SasTokenNoReadPrivilegesException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListSasTokenNoReadBlobClient : IAzureBlobClient
        {
            public Task<HttpResponseMessage> GetAsync(string blobName)
            {
                throw new SasTokenNoReadPrivilegesException("the token", "the url", "a message");
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list
        /// </summary>
        [TestMethod]
        public async Task RetrieveTodoList()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual("a todo list item", todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "testing",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_3",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the azure blob client fails when retrieving the todo list blob
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListBlobRetrievalFailure()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MockRetrieveTodoListBlobRetrievalFailureBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var azureStorageException = await Assert.ThrowsExceptionAsync<AzureStorageException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
                Assert.AreEqual(nameof(RetrieveTodoListBlobRetrievalFailure), azureStorageException.Message, true);
            }
        }

        private sealed class MockRetrieveTodoListBlobRetrievalFailureBlobClient : IAzureBlobClient
        {
            public async Task<HttpResponseMessage> GetAsync(string blobName)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListBlobRetrievalFailure));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the todo list blob is malformed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedBlob()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                using (var stringContent = new StringContent(nameof(RetrieveTodoListMalformedBlob)))
                {
                    await azureBlobClient.PutAsync("todoListData", stringContent).ConfigureAwait(false);
                }

                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var malformedBlobDataException = await Assert.ThrowsExceptionAsync<MalformedBlobDataException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
                Assert.AreEqual(nameof(RetrieveTodoListMalformedBlob), malformedBlobDataException.Message, true);
            }
        }

        /// <summary>
        /// Retrieves the todo list when the todo list blob is null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullBlob()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullBlobGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                using (var stringContent = new StringContent("null"))
                {
                    await azureBlobClient.PutAsync("todoListData", stringContent).ConfigureAwait(false);
                }

                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual("a todo list item", todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullBlobGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "testing",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_3",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when it's been retrieved previously
        /// </summary>
        [TestMethod]
        public async Task RetrieveTodoListWithExistingBlobData()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListWithExistingBlobDataGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
a todo list item
"""
                    , todoListResult.TodoList, true);
            }

            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListWithExistingBlobDataGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                using (var stringContent = new StringContent(
"""
{
  "lastRecordedEventTimeStamp": "2024-05-30"
}
"""
                    ))
                {
                    await azureBlobClient.PutAsync("todoListData", stringContent).ConfigureAwait(false);
                }

                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
"""
                    , todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListWithExistingBlobDataGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-06-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_3",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client is using an invalid access token on the first page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidAccessTokenOnFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListInvalidAccessTokenOnFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListInvalidAccessTokenOnFirstPageGraphClient : IGraphClient
        {
            public Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                throw new InvalidAccessTokenException("the url", "the access token", "a message");
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client encounters a network error on the first page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListGraphNetworkErrorOnFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphNetworkErrorOnFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("/me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListGraphNetworkErrorOnFirstPageGraphClient : IGraphClient
        {
            public Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                throw new HttpRequestException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client encounters a failure on the first page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListGraphFailureOnFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphFailureOnFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("/me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListGraphFailureOnFirstPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListGraphFailureOnFirstPage));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the first page of calendar events is malformed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMalformedFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("/me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListMalformedFirstPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListMalformedFirstPage));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the first page of calendar events is null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("/me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullFirstPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent("null");
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the first page of calendar events has a null collection
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullCollectionFirstPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullCollectionFirstPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("/me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullCollectionFirstPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(
"""
{
  "value": null
}
"""
                        );
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph returns an invalid next link for the second page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidNextLink()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListInvalidNextLinkGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.AreEqual("invalid", todoListResult.BrokenNextLink, true);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    , 
                    todoListResult.TodoList, 
                    true);
            }
        }

        private sealed class MockRetrieveTodoListInvalidNextLinkGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "invalid"
}
""";
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client is using an invalid access token on the second page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidAccessTokenOnSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListInvalidAccessTokenOnSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<InvalidAccessTokenException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListInvalidAccessTokenOnSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format = 
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "testing",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new InvalidAccessTokenException("the url", "the access token", "a message");
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client encounters a network error on the second page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListGraphNetworkErrorOnSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphNetworkErrorOnSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("https://localhost//me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListGraphNetworkErrorOnSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new HttpRequestException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the graph client encounters a failure on the second page of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListGraphFailureOnSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphFailureOnSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("https://localhost//me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListGraphFailureOnSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListGraphFailureOnFirstPage));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the second page of calendar events is malformed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMalformedSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("https://localhost//me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListMalformedSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListMalformedSecondPage));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the second page of calendar events is null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("https://localhost//me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListNullSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent("null");
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the second page of calendar events has a null collection
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullCollectionSecondPage()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullCollectionSecondPageGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNotNull(todoListResult.BrokenNextLink);
                Assert.IsTrue(todoListResult.BrokenNextLink.StartsWith("https://localhost//me/calendar/events", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(todoListResult.BrokenNextLink.Contains("$skip", StringComparison.OrdinalIgnoreCase));
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListNullCollectionSecondPageGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(
"""
{
  "value": null
}
"""
                        );
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list across multiple pages of calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMultiplePages()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMultiplePagesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(
"""
some data
some more data
some data
some more data
"""
                    ,
                    todoListResult.TodoList,
                    true);
            }
        }

        private sealed class MockRetrieveTodoListMultiplePagesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var format =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ],
    "@odata.nextLink": "{{0}}"
}
""".Replace("{", "{{").Replace("}", "}}").Replace("{{{{", "{").Replace("}}}}", "}");
                var content = string.Format(
                    format,
                    $"https://localhost/{relativeUri.OriginalString}&$skip=1");
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some more data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(content);
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when a network error occurs while retrieving the first instance of a series event
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNetworkErrorInFirstInstanceOfSeries()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNetworkErrorInFirstInstanceOfSeriesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.AreEqual("/me/calendar/events/some_id_2", todoListResult.BrokenNextLink, true);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNetworkErrorInFirstInstanceOfSeriesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    throw new HttpRequestException();
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when a graph error occurs while retrieving the first instance of a series event
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListGraphErrorInFirstInstanceOfSeries()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListGraphErrorInFirstInstanceOfSeriesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.AreEqual("/me/calendar/events/some_id_2", todoListResult.BrokenNextLink, true);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListGraphErrorInFirstInstanceOfSeriesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list while the first instance of a series event is malformed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedFirstInstanceOfSeries()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMalformedFirstInstanceOfSeriesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.AreEqual("/me/calendar/events/some_id_2", todoListResult.BrokenNextLink, true);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListMalformedFirstInstanceOfSeriesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content = nameof(RetrieveTodoListMalformedFirstInstanceOfSeries);
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list while the first instance of a series event is null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullFirstInstanceOfSeries()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullFirstInstanceOfSeriesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.AreEqual("/me/calendar/events/some_id_2", todoListResult.BrokenNextLink, true);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullFirstInstanceOfSeriesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content = "null";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where a series event doesn't have any future instances
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListSeriesWithoutFutureInstances()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListSeriesWithoutFutureInstancesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListSeriesWithoutFutureInstancesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where a series event has future instances
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListSeriesWithFutureInstances()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListSeriesWithFutureInstancesGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual("a todo list item", todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListSeriesWithFutureInstancesGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_2",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
                else if (path.StartsWith("/me/calendar/events/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
                {
                    var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id_3",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                    StringContent? stringContent = null;
                    try
                    {
                        stringContent = new StringContent(content);
                        HttpResponseMessage? responseMessage = null;
                        try
                        {
                            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = stringContent;
                            return await Task.FromResult(responseMessage).ConfigureAwait(false);
                        }
                        catch
                        {
                            responseMessage?.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        stringContent?.Dispose();
                        throw;
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Retrieves the todo list where an event has a null subject
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullSubject()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullSubjectGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullSubjectGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": null,
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }
 
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where an event has a null start
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullStart()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullStartGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.AreEqual(1, todoListResult.EventsWithoutStarts.Count());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullStartGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": null
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where an event has a null start date time
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullStartDateTime()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullStartDateTimeGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.AreEqual(1, todoListResult.EventsWithoutStarts.Count());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullStartDateTimeGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": null,
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where an event has a malformed start date time
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedStartDateTime()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMalformedStartDateTimeGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.AreEqual(1, todoListResult.EventsWithStartParseFailures.Count());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListMalformedStartDateTimeGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>a todo list item</p></body></html>"
            },
            "start": {
                "dateTime": "not a timestamp",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where an event has a malformed start date time
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNullContent()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNullContentGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.AreEqual(1, todoListResult.EventsWithoutBodies.Count());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListNullContentGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": null,
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list where an event has malformed content
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListMalformedContent()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListMalformedContentGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.AreEqual(1, todoListResult.EventsWithBodyParseFailures.Count());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);
            }
        }

        private sealed class MockRetrieveTodoListMalformedContentGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "content"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when there are no new calendar events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNoNewEvents()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNoNewEventsGraphClient();
                var azureBlobClient = new MemoryBlobClient();
                using (var stringContent = new StringContent(
"""
{
  "lastRecordedEventTimeStamp": "2024-04-17"
}
"""
                    ))
                {
                    await azureBlobClient.PutAsync("todoListData", stringContent).ConfigureAwait(false);
                }

                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var todoListResult = await todoListService.RetrieveTodoList().ConfigureAwait(false);

                Assert.IsNull(todoListResult.BrokenNextLink);
                Assert.IsFalse(todoListResult.EventsWithoutStarts.Any());
                Assert.IsFalse(todoListResult.EventsWithStartParseFailures.Any());
                Assert.IsFalse(todoListResult.EventsWithoutBodies.Any());
                Assert.IsFalse(todoListResult.EventsWithBodyParseFailures.Any());
                Assert.AreEqual(string.Empty, todoListResult.TodoList, true);

                using (var httpResponse = await azureBlobClient.GetAsync("todoListData").ConfigureAwait(false))
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Assert.AreEqual(true, responseContent?.Contains("2024-04-17"));
                }
            }
        }

        private sealed class MockRetrieveTodoListNoNewEventsGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when there is a network error writing the blob data back
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNetworkErrorWritingToBlob()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNetworkErrorWritingToBlobGraphClient();
                var azureBlobClient = new MockRetrieveTodoListNetworkErrorWritingToBlobBlobClient(new MemoryBlobClient());
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListNetworkErrorWritingToBlobBlobClient : IAzureBlobClient
        {
            private readonly IAzureBlobClient delegateClient;

            public MockRetrieveTodoListNetworkErrorWritingToBlobBlobClient(IAzureBlobClient delegateClient)
            {
                this.delegateClient = delegateClient;
            }

            public async Task<HttpResponseMessage> GetAsync(string blobName)
            {
                return await this.delegateClient.GetAsync(blobName).ConfigureAwait(false);
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new HttpRequestException();
            }
        }

        private sealed class MockRetrieveTodoListNetworkErrorWritingToBlobGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the SAS token used does not have write permissions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListNoBlobWritePermissions()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListNoBlobWritePermissionsGraphClient();
                var azureBlobClient = new MockRetrieveTodoListNoBlobWritePermissionsBlobClient(new MemoryBlobClient());
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<SasTokenNoWritePrivilegesException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListNoBlobWritePermissionsBlobClient : IAzureBlobClient
        {
            private readonly IAzureBlobClient delegateClient;

            public MockRetrieveTodoListNoBlobWritePermissionsBlobClient(IAzureBlobClient delegateClient)
            {
                this.delegateClient = delegateClient;
            }

            public async Task<HttpResponseMessage> GetAsync(string blobName)
            {
                return await this.delegateClient.GetAsync(blobName).ConfigureAwait(false);
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new SasTokenNoWritePrivilegesException("the token", "the url", "a message");
            }
        }

        private sealed class MockRetrieveTodoListNoBlobWritePermissionsGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when the blob data written is invalid
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListInvalidBlobDataOnWrite()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListInvalidBlobDataOnWriteGraphClient();
                var azureBlobClient = new MockRetrieveTodoListInvalidBlobDataOnWriteBlobClient(new MemoryBlobClient());
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                await Assert.ThrowsExceptionAsync<InvalidBlobDataException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
            }
        }

        private sealed class MockRetrieveTodoListInvalidBlobDataOnWriteBlobClient : IAzureBlobClient
        {
            private readonly IAzureBlobClient delegateClient;

            public MockRetrieveTodoListInvalidBlobDataOnWriteBlobClient(IAzureBlobClient delegateClient)
            {
                this.delegateClient = delegateClient;
            }

            public async Task<HttpResponseMessage> GetAsync(string blobName)
            {
                return await this.delegateClient.GetAsync(blobName).ConfigureAwait(false);
            }

            public Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                throw new InvalidBlobDataException("the blob name", new Exception());
            }
        }

        private sealed class MockRetrieveTodoListInvalidBlobDataOnWriteGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieves the todo list when an error occurs writing the blob data
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RetrieveTodoListBlobWriteError()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var graphClient = new MockRetrieveTodoListBlobWriteErrorGraphClient();
                var azureBlobClient = new MockRetrieveTodoListBlobWriteErrorBlobClient(new MemoryBlobClient());
                var todoListService = new TodoListService(memoryCache, graphClient, azureBlobClient);
                var azureStorageException = await Assert.ThrowsExceptionAsync<AzureStorageException>(() => todoListService.RetrieveTodoList()).ConfigureAwait(false);
                Assert.AreEqual(nameof(RetrieveTodoListBlobWriteError), azureStorageException.Message);
            }
        }

        private sealed class MockRetrieveTodoListBlobWriteErrorBlobClient : IAzureBlobClient
        {
            private readonly IAzureBlobClient delegateClient;

            public MockRetrieveTodoListBlobWriteErrorBlobClient(IAzureBlobClient delegateClient)
            {
                this.delegateClient = delegateClient;
            }

            public async Task<HttpResponseMessage> GetAsync(string blobName)
            {
                return await this.delegateClient.GetAsync(blobName).ConfigureAwait(false);
            }

            public async Task<HttpResponseMessage> PutAsync(string blobName, HttpContent httpContent)
            {
                StringContent? stringContent = null;
                try
                {
                    stringContent = new StringContent(nameof(RetrieveTodoListBlobWriteError));
                    HttpResponseMessage? responseMessage = null;
                    try
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                        responseMessage.Content = stringContent;
                        return await Task.FromResult(responseMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        responseMessage?.Dispose();
                        throw;
                    }
                }
                catch
                {
                    stringContent?.Dispose();
                    throw;
                }
            }
        }

        private sealed class MockRetrieveTodoListBlobWriteErrorGraphClient : IGraphClient
        {
            public async Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri)
            {
                var path = GetPath(relativeUri);
                if (string.Equals(path, "/me/calendar/events", StringComparison.OrdinalIgnoreCase))
                {
                    var queryParameters = GetQuery(relativeUri)
                        .Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(option => option.Split('=', StringSplitOptions.RemoveEmptyEntries));
                    if (TryFindBy(queryParameters, option => option[0], "$filter", StringComparer.OrdinalIgnoreCase, out var filter))
                    {
                        if (string.Join('=', filter).Contains("type eq 'singleInstance'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": [
        {
            "id": "some_id",
            "subject": "todo list",
            "body": {
                "contentType": "html",
                "content": "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><p>some data</p></body></html>"
            },
            "start": {
                "dateTime": "2024-05-17T23:30:00.0000000",
                "timeZone": "UTC"
            }
        }
    ]
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                        else if (string.Join('=', filter).Contains("type eq 'seriesMaster'", StringComparison.OrdinalIgnoreCase))
                        {
                            var content =
"""
{
    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('some_user')/calendar/events",
    "value": []
}
""";
                            StringContent? stringContent = null;
                            try
                            {
                                stringContent = new StringContent(content);
                                HttpResponseMessage? responseMessage = null;
                                try
                                {
                                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    responseMessage.Content = stringContent;
                                    return await Task.FromResult(responseMessage).ConfigureAwait(false);
                                }
                                catch
                                {
                                    responseMessage?.Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                stringContent?.Dispose();
                                throw;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }

            public Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent)
            {
                throw new NotImplementedException();
            }
        }

        private static string GetQuery(RelativeUri relativeUri)
        {
            //// TODO
            var absoluteUri = new Uri(new Uri("https://localhost/"), relativeUri);
            var query = absoluteUri.GetComponents(UriComponents.Query, UriFormat.SafeUnescaped);
            return query;
        }

        private static string GetPath(RelativeUri relativeUri)
        {
            //// TODO
            var absoluteUri = new Uri(new Uri("https://localhost/"), relativeUri);
            var path = absoluteUri.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.SafeUnescaped);
            return path;
        }

        private static bool TryFindBy<TSource, TSelected>(IEnumerable<TSource> source, Func<TSource, TSelected> selector, TSelected comparison, IEqualityComparer<TSelected> comparer, out TSource found)
        {
            //// TODO
            foreach (var element in source)
            {
                var selected = selector(element);
                if (comparer.Equals(selected, comparison))
                {
                    found = element;
                    return true;
                }
            }

            found = default;
            return false;
        }
    }
}
