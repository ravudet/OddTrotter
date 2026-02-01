namespace Fx
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PlaygroundTests
    {
        [TestMethod]
        public async Task ReadingFromDeadNetworkStream()
        {
            using (var handler = new SocketsHttpHandler())
            {
                handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5);
                using (var httpClient = new HttpClient(handler, false))
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36 Edg/144.0.3719.104");

                    var url = "https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://chuangtzu.ftp.acc.umu.se/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://www.google.com";


                    using (var httpResponse = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        using (var contentStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            var buffer = new byte[1024];
                            int read = -1;

                            while (read != 0)
                            {
                                try
                                {
                                    read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                                }
                                catch (IOException ioException)
                                {
                                    if (ioException.InnerException is SocketException socketException)
                                    {
                                        throw new HttpRequestException("TODO", socketException);
                                    }

                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
