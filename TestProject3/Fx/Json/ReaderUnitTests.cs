namespace Fx.Json
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ReaderUnitTests
    {
        private const string data =
"""
{
    "true": true,
    "false": false,
    "number": 1234,
    "string": "asdf",
    "null": null,
    "object": {
        "true": true,
        "false": false,
        "number": 1234,
        "string": "asdf",
        "null": null
    },
    "emptyObject": {},
    "emptyArray": [],
    "array": [
        {
            "true": true,
            "false": false,
            "number": 1234,
            "string": "asdf",
            "null": null
        }
    ]
}
""";

        [TestMethod]
        public async Task FullRead()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
                FullRead(buffer);
            }
        }

        private static void FullRead(byte[] buffer)
        {
        }

        [TestMethod]
        public void StreamedRead()
        {
        }

        //// TODO go through all todos for `oddtrotter.calendarv1.tokenization`; also look at the unit tests for those same types
    }
}
