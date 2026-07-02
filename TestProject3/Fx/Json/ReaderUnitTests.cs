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
                var stopwatch = new System.Diagnostics.Stopwatch();
                var iterations = 10000;
                for (int i = 0; i < iterations; ++i)
                {
                    stopwatch.Start();
                    FullRead(context);
                    stopwatch.Stop();
                    context.ValidBytes = (uint)stream.Length;
                    context.CurrentByteIndex = 0;
                }

                Console.WriteLine(stopwatch.ElapsedTicks);
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

            // true
            // false
            // 1234
            // asdf
            // null
            ReadTrueFalseNumberStringNull(member1, context, 1, 4, out var subsequentMembers1);

            // object
            ReadSubsequentMemberToValue(subsequentMembers1, context, 6, 6, out var value2);
            Assert.IsTrue(value2.TryMove(context, out var valueCategory2));
            Assert.IsTrue(valueCategory2.TryObject(out var object2));
            Assert.IsTrue(object2.TryMove(context, out var objectStart2));
            Assert.IsTrue(objectStart2.TryMove(context, out var whitespace3, out _));
            ReadWhitespace(whitespace3, context, 10, out var members2);
            Assert.IsTrue(members2.TryMove(context, out var membersCategory2));
            Assert.IsTrue(membersCategory2.TrySome(out var member2));
            ReadTrueFalseNumberStringNull(member2, context, 2, 4, out var subsequentMembers2);
            Assert.IsTrue(subsequentMembers2.TryMove(context, out var subsequentMembersCategory2));
            Assert.IsTrue(subsequentMembersCategory2.TryNone(out var whitespace4));
            ReadWhitespace(whitespace4, context, 6, out var objectEnd1);
            Assert.IsTrue(objectEnd1.TryMove(context, out var subsequentMembers3, out _));

            // emptyobject
            ReadSubsequentMemberToValue(subsequentMembers3, context, 11, 6, out var value3);
            Assert.IsTrue(value3.TryMove(context, out var valueCategory3));
            Assert.IsTrue(valueCategory3.TryObject(out var object3));
            Assert.IsTrue(object3.TryMove(context, out var objectStart3));
            Assert.IsTrue(objectStart3.TryMove(context, out var whitespace5, out _));
            ReadWhitespace(whitespace5, context, 0, out var members3);
            Assert.IsTrue(members3.TryMove(context, out var membersCategory3));
            Assert.IsTrue(membersCategory3.TryNone(out var whitespace6));
            ReadWhitespace(whitespace6, context, 0, out var objectEnd2);
            Assert.IsTrue(objectEnd2.TryMove(context, out var subsequentMembers4, out _));

            // emptyarray
            ReadSubsequentMemberToValue(subsequentMembers4, context, 10, 6, out var value4);
            Assert.IsTrue(value4.TryMove(context, out var valueCategory4));
            Assert.IsTrue(valueCategory4.TryArray(out var array1));
            Assert.IsTrue(array1.TryMove(context, out var arrayStart1));
            Assert.IsTrue(arrayStart1.TryMove(context, out var whitespace7, out _));
            ReadWhitespace(whitespace7, context, 0, out var arrayElements1);
            Assert.IsTrue(arrayElements1.TryMove(context, out var arrayElementsCategory1));
            Assert.IsTrue(arrayElementsCategory1.TryNone(out var whitespace8));
            ReadWhitespace(whitespace8, context, 0, out var arrayEnd1);
            Assert.IsTrue(arrayEnd1.TryMove(context, out var subsequentMembers5, out _));

            // array
            ReadSubsequentMemberToValue(subsequentMembers5, context, 5, 6, out var value5);
            Assert.IsTrue(value5.TryMove(context, out var valueCategory5));
            Assert.IsTrue(valueCategory5.TryArray(out var array2));
            Assert.IsTrue(array2.TryMove(context, out var arrayStart2));
            Assert.IsTrue(arrayStart2.TryMove(context, out var whitespace9, out _));
            ReadWhitespace(whitespace9, context, 10, out var arrayElements2);
            Assert.IsTrue(arrayElements2.TryMove(context, out var arrayElementsCategory2));
            Assert.IsTrue(arrayElementsCategory2.TrySome(out var value6));
            Assert.IsTrue(value6.TryMove(context, out var valueCategory6));
            Assert.IsTrue(valueCategory6.TryObject(out var object4));
            Assert.IsTrue(object4.TryMove(context, out var objectStart4));
            Assert.IsTrue(objectStart4.TryMove(context, out var whitespace10, out _));
            ReadWhitespace(whitespace10, context, 14, out var members4);
            Assert.IsTrue(members4.TryMove(context, out var membersCategory4));
            Assert.IsTrue(membersCategory4.TrySome(out var member4));
            ReadTrueFalseNumberStringNull(member4, context, 3, 4, out var subsequentMembers6);
            Assert.IsTrue(subsequentMembers6.TryMove(context, out var subsequentMembersCategory6));
            Assert.IsTrue(subsequentMembersCategory6.TryNone(out var whitespace11));
            ReadWhitespace(whitespace11, context, 10, out var objectEnd3);
            Assert.IsTrue(objectEnd3.TryMove(context, out var subsequentArrayElements1, out _));
            Assert.IsTrue(subsequentArrayElements1.TryMove(context, out var subsequentArrayElementsCategory1));
            Assert.IsTrue(subsequentArrayElementsCategory1.TryNone(out var whitespace12));
            ReadWhitespace(whitespace12, context, 6, out var arrayEnd2);
            Assert.IsTrue(arrayEnd2.TryMove(context, out var subsequentMembers7, out _));
            Assert.IsTrue(subsequentMembers7.TryMove(context, out var subsequentMembersCategory7));
            Assert.IsTrue(subsequentMembersCategory7.TryNone(out var whitespace13));
            ReadWhitespace(whitespace13, context, 2, out var objectEnd4);
            Assert.IsTrue(objectEnd4.TryMove(context, out var whitespace14, out _));

            Assert.IsFalse(whitespace14.TryMove(context, out _));
            Assert.AreEqual(context.Stream.Length, context.CurrentByteIndex);
            context.ValidBytes = 0; // simulate a read of the exhausted stream
            ReadWhitespace(whitespace14, context, 0, out var nothing);
            Assert.AreEqual(new Nothing(), nothing);

            Assert.AreEqual(context.Stream.Length, context.CurrentByteIndex);
        }

        private static void ReadTrueFalseNumberStringNull<TNextReader>(MemberReader<SubsequentMembersReader<TNextReader>>? memberReader, Context context, int tabCount, int tabLength, out SubsequentMembersReader<TNextReader>? nextReader)
        {
            var whitespaceLength = tabCount * tabLength + 2;

            // true
            ReadMemberToValue(memberReader, context, 4, out var value2);
            Assert.IsTrue(value2.TryMove(context, out var valueCategory2));
            Assert.IsTrue(valueCategory2.TryTrue(out var true1));
            Assert.IsTrue(true1.TryMove(context, out var subsequentMembers, out _, out _));

            // false
            ReadSubsequentMemberToValue(subsequentMembers, context, 5, whitespaceLength, out var value3);
            Assert.IsTrue(value3.TryMove(context, out var valueCategory3));
            Assert.IsTrue(valueCategory3.TryFalse(out var false1));
            Assert.IsTrue(false1.TryMove(context, out var subsequentMembers2, out _, out _));

            // 1234
            ReadSubsequentMemberToValue(subsequentMembers2, context, 6, whitespaceLength, out var value4);
            Assert.IsTrue(value4.TryMove(context, out var valueCategory4));
            Assert.IsTrue(valueCategory4.TryNumber(out var number1));
            ReadNumber(number1, context, out var subsequentMembers3);

            // asdf
            ReadSubsequentMemberToValue(subsequentMembers3, context, 6, whitespaceLength, out var value5);
            Assert.IsTrue(value5.TryMove(context, out var valueCategory5));
            Assert.IsTrue(valueCategory5.TryString(out var string1));
            ReadString(string1, context, 4, out var subsequentMembers4);

            // null
            ReadSubsequentMemberToValue(subsequentMembers4, context, 4, whitespaceLength, out var value6);
            Assert.IsTrue(value6.TryMove(context, out var valueCategory6));
            Assert.IsTrue(valueCategory6.TryNull(out var null1));
            Assert.IsTrue(null1.TryMove(context, out nextReader, out _, out _));
        }

        private static void ReadNumber<TNextReader>(NumberReader<TNextReader>? numberReader, Context context, out TNextReader? nextReader)
        {
            Assert.IsTrue(numberReader.TryMove(context, out var sign));
            Assert.IsTrue(sign.TryMove(context, out var @int, out _));
            Assert.IsTrue(@int.TryMove(context, out var intCategory));
            Assert.IsTrue(intCategory.TryNonZero(out var leadingDigit));
            Assert.IsTrue(leadingDigit.TryMove(context, out var digits, out _));
            for (int i = 0; i < 4; ++i)
            {
                Assert.IsTrue(digits.TryMove(context, out var category));
                Assert.IsTrue(category.TrySome(out var digit));
                Assert.IsTrue(digit.TryMove(context, out digits, out _));
            }

            Assert.IsTrue(digits.TryMove(context, out var digitsCategory));
            Assert.IsTrue(digitsCategory.TryNone(out var frac));
            Assert.IsTrue(frac.TryMove(context, out var fracCategory));
            Assert.IsTrue(fracCategory.TryAbsent(out var exp));
            Assert.IsTrue(exp.TryMove(context, out var expCategory));
            Assert.IsTrue(expCategory.TryAbsent(out nextReader));
        }

        private static void ReadSubsequentMemberToValue<TNextReader>(SubsequentMembersReader<TNextReader>? subsequentMembersReader, Context context, int memberNameLength, int tabLength, out ValueReader<SubsequentMembersReader<TNextReader>>? valueReader)
        {
            Assert.IsTrue(subsequentMembersReader.TryMove(context, out var subsequentMembersCategory));
            Assert.IsTrue(subsequentMembersCategory.TrySome(out var comma));
            Assert.IsTrue(comma.TryMove(context, out var whitespace1, out _));
            ReadWhitespace(whitespace1, context, tabLength, out var member);
            ReadMemberToValue(member, context, memberNameLength, out valueReader);
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
        public async Task StreamedRead()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ReaderUnitTests.data)))
            {
                var buffer = new byte[stream.Length];
                var context = await Context.FromStream(stream, buffer).ConfigureAwait(false);
                var stopwatch = new System.Diagnostics.Stopwatch();
                var iterations = 10000;
                for (int i = 0; i < iterations; ++i)
                {
                    stopwatch.Start();
                    await StreamedRead(context).ConfigureAwait(false);
                    stopwatch.Stop();

                    stream.Position = 0;
                    context = await Context.FromStream(stream, buffer).ConfigureAwait(false);
                }

                Console.WriteLine(stopwatch.ElapsedTicks);
            }
        }

        private static async Task StreamedRead(Context context)
        {
            var reader = context.Json();
            var whitespace = await reader.Move(context);
            whitespace.TryMove(context, out _);
        }

        //// TODO write the streamed read test
        //// TODO go through all todos for `oddtrotter.calendarv1.tokenization`; also look at the unit tests for those same types
    }
}
