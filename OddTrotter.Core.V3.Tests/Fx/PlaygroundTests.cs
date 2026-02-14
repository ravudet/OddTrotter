namespace Fx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.Odata.v4_01.StrongConventionContext;

    [TestClass]
    public sealed class PlaygroundTests
    {
        [TestMethod]
        public async Task ReadingFromDeadNetworkStream()
        {
            //// TODO to repro this, you need to turn off wifi, set a breakpoint for after `readasstreamasync`, run the test, let the breakpoint get hit, disconnect the network cable, and continue from the breakpoint

            using (var handler = new SocketsHttpHandler())
            {
                //// TODO by default (i.e. when no handler is provided), `httpclient` doesn't appear to have a timeout for when the underlying connection is no longer available (e.g. due to disconnected hardware), so we need to set a timeout; 5 seconds is probably too short for practical uses
                handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5);
                using (var httpClient = new HttpClient(handler, false))
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36 Edg/144.0.3719.104");

                    var url = "https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://chuangtzu.ftp.acc.umu.se/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://www.google.com";


                    //// TODO `httpcompletionoption` needs to be set so that only the headers are read; otherwise, we will sit at this line until the entire payload is read into memory
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
                                    //// TODO `readasync` throws `ioexception` (which i think is good that they actually chose to preserve the `stream` contract), but when the underlying connection has an issue, it has an inner `socketexception`, so we, knowing that the stream is actually coming from an httpclient, can use this inner exception to give our caller a better experience
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

        /*public static void Foo(Calendar<INetwork> calendarProvider)
        {
            calendarProvider.Events.Case
        }

        public class Calendar<T>
            where T : ICase
        {
            public StringValue<T> Id { get; }

            public IOdataCollection<T, CalendarEvent<T>> Events { get; }
        }

        public class CalendarEvent<T>
            where T : ICase
        {
            public StringValue<T> Id { get; }

            public BoolValue<T> IsCanceled { get; }

            public DateTimeValue<T> Start { get; }

            public IOdataCollection<T, CalendarEvent<T>> Instances { get; }
        }

        public class StringValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public class DateTimeValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public class BoolValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public interface IOdataCollection<TCase, out TElement>
            where TCase : ICase
        {
            TCase Case { get; }
        }

        public interface ISingleValue<T>
        {
            T Value { get; }
        }

        public interface ICase
        {
        }

        public interface INetwork : ICase
        {
        }

        public interface IMemory : ICase
        {
        }*/
    }
}
