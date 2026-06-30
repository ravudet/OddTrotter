namespace Fx.Json
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Fx.Parsing.Reader;
    using System.Threading;

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
                var context = await Context.FromStream(stream, buffer).ConfigureAwait(false);
                FullRead(context);
            }
        }

        private static void FullRead(Context context)
        {
            var reader = context.Json();
            Assert.IsTrue(reader.TryMove(context, out var whitespace1));
            Assert.IsTrue(whitespace1.TryMove(context, out var whitespaceCategory1));
            Assert.IsTrue(whitespaceCategory1.TryNone(out var value1));
            Assert.IsTrue(value1.TryMove(context, out var valueCategory1));
            Assert.IsTrue(valueCategory1.TryObject(out var object1));
            Assert.IsTrue(object1.TryMove(context, out var objectStart1));
            Assert.IsTrue(objectStart1.TryMove(context, out var whitespace2, out _));
            ReadWhitespace(whitespace2, context, 6, out var members1);
            Assert.IsTrue(members1.TryMove(context, out var membersCategory1));
            Assert.IsTrue(membersCategory1.TrySome(out var member1));
            Assert.IsTrue(member1.TryMove(context, out var string1));
            Assert.IsTrue(string1.TryMove(context, out var stringDelimiter1));
            Assert.IsTrue(stringDelimiter1.try)

            //// TODO you are here
        }

        private static void ReadWhitespace<TNextReader>(WhitespaceReader<TNextReader>? whitespaceReader, Context context, int count, out TNextReader? nextReader)
        {
            for (int i = 0; i < count; ++i)
            {
                Assert.IsTrue(whitespaceReader.TryMove(context, out var whitespaceCategory));
                Assert.IsTrue(whitespaceCategory.TrySome(out var whitespaceCharacter));
                Assert.IsTrue(whitespaceCharacter.TryMove(context, out whitespaceReader, out _));
            }

            Assert.IsTrue(whitespaceReader.TryMove(context, out var category));
            Assert.IsTrue(category.TryNone(out nextReader));
        }

        [TestMethod]
        public void StreamedRead()
        {
        }

        //// TODO go through all todos for `oddtrotter.calendarv1.tokenization`; also look at the unit tests for those same types
    }
}
