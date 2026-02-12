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

        [TestMethod]
        public void IntermediateStruct()
        {
            var builder = new TimeStructureBuilder();
            var state = new DeserializationState(ref builder);
            IntermediateStructHelper(ref state);

            Assert.AreEqual("asdf", state.Builder.DateTime);

            var timeStructure = state.Builder.Build();

            Assert.AreEqual("asdf", timeStructure.DateTime);
        }

        private static void IntermediateStructHelper(ref DeserializationState state)
        {
            state.Builder.DateTime = "asdf";

            state.Builder.Another.Value = 1234;

            var builder = state.Builder;

            builder.Another.Value = 1234;

            state.Builder.TimeZone = "qwer";
        }

        private ref struct DeserializationState
        {
            private readonly ref TimeStructureBuilder timeStructureBuilder;

            public DeserializationState(ref TimeStructureBuilder timeStructureBuilder)
            {
                this.Errors = new List<DeserializationException>();
                this.timeStructureBuilder = ref timeStructureBuilder; //// TODO the trick here is that `timestructurebuilder` is *not* a `ref struct`; can you use this to do your linked list thing?
            }

            public ref TimeStructureBuilder Builder
            {
                get
                {
                    return ref this.timeStructureBuilder;
                }
            }

            public List<DeserializationException> Errors { get; set; }
        }

        private struct TimeStructureBuilder
        {
            public string? DateTime { get; set; }

            public string? TimeZone { get; set; }

            public Another Another;

            public OddTrotter.Graph.CalendarEventsContext.TimeStructure Build()
            {
                ArgumentNullException.ThrowIfNull(this.DateTime);
                ArgumentNullException.ThrowIfNull(this.TimeZone);

                return new OddTrotter.Graph.CalendarEventsContext.TimeStructure(this.DateTime, this.TimeZone);
            }
        }

        private struct Another
        {
            public int? Value { get; set; }
        }
    }
}
