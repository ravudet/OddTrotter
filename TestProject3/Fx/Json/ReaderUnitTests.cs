namespace Fx.Json
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Fx.Parsing.Reader;
    using System.Threading;
    using NuGet.Frameworks;

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
            ReadMemberToValue(member1, context, 4, out var value2);
            Assert.IsTrue(value2.TryMove(context, out var valueCategory2));
            Assert.IsTrue(valueCategory2.TryTrue(out var true1));
            Assert.IsTrue(true1.TryMove(context, out var subsequentMembers1, out _, out _));
            Assert.IsTrue(subsequentMembers1.TryMove(context, out var subsequentMembersCategory1));
            Assert.IsTrue(subsequentMembersCategory1.TrySome(out var member2));
            ReadMemberToValue(member2, context, 5, out var value3);
            Assert.IsTrue(value3.TryMove(context, out var valueCategory3));
            Assert.IsTrue(valueCategory3.TryFalse(out var false1));
            Assert.IsTrue(false1.TryMove(context, out var subsequentMembers2, out _, out _));
            Assert.IsTrue(subsequentMembers2.TryMove(context, out var subsequentMembersCategory2));
            Assert.IsTrue(subsequentMembersCategory2.TrySome(out var member3));

            //// TODO you are here
        }

        private static void ReadMemberToValue<TNextReader>(MemberReader<TNextReader>? memberReader, Context context, int memberNameLength, out ValueReader<TNextReader>? nextReader)
        {
            Assert.IsTrue(memberReader.TryMove(context, out var string1));
            ReadString(string1, context, memberNameLength, out var whitespace3);
            ReadWhitespace(whitespace3, context, 0, out var colon1);
            Assert.IsTrue(colon1.TryMove(context, out var whitespace4, out _));
            ReadWhitespace(whitespace4, context, 1, out nextReader);
        }

        private static void ReadString<TNextReader>(StringReader<TNextReader>? stringReader, Context context, int count, out TNextReader? nextReader)
        {
            Assert.IsTrue(stringReader.TryMove(context, out var stringDelimiter1));
            Assert.IsTrue(stringDelimiter1.TryMove(context, out var charsReader, out _));
            for (int i = 0; i < count; ++i)
            {
                Assert.IsTrue(charsReader.TryMove(context, out var charsCategory));
                Assert.IsTrue(charsCategory.TrySome(out var @char));
                Assert.IsTrue(@char.TryMove(context, out var charCategory));
                Assert.IsTrue(charCategory.TryUnescaped(out var unescapedChar));
                Assert.IsTrue(unescapedChar.TryMove(context, out charsReader, out _));
            }

            Assert.IsTrue(charsReader.TryMove(context, out var category));
            Assert.IsTrue(category.TryNone(out var stringDelimiter2));
            Assert.IsTrue(stringDelimiter2.TryMove(context, out nextReader, out _));
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
