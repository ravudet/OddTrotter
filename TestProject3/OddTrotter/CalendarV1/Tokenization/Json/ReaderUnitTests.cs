namespace OddTrotter.CalendarV1.Tokenization.Json
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ReaderUnitTests
    {
        [TestMethod]
        public void Broad()
        {
            var data =
"""

""";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var valueReader = Helpers.StartReading(stream, new ArrayResizer(stream.Length));
                valueReader.ReadToEnd();
            }
        }

        private sealed class ArrayResizer : IArrayResizer
        {
            private readonly long defaultSize;

            public ArrayResizer(long defaultSize)
            {
                this.defaultSize = defaultSize;
            }

            public byte[] Resize(byte[] previous)
            {
                if (previous.Length == 0)
                {
                    return new byte[this.defaultSize];
                }

                var next = new byte[previous.Length * 2];
                Array.Copy(previous, 0, next, 0, previous.Length);
                return next;
            }
        }
    }
}
