namespace Fx
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PlaygroundTests
    {
        [TestMethod]
        public async Task ReadingFromDeadNetworkStream()
        {
            using (var httpClient = new HttpClient())
            {
                using (var httpResponse = await httpClient.GetAsync("https://www.google.com").ConfigureAwait(false))
                {
                    using (var contentStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var buffer = new byte[1024];
                        int read;
                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                        {
                        }
                    }
                }
            }
        }
    }
}
