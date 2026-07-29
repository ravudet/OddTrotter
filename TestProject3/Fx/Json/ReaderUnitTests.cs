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
    using OddTrotter.CalendarV1.Tokenization.Json;
    using System.Runtime.InteropServices;

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
                var buffer = new byte[20];
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
            var whitespace1 = await reader.Move(context);
            var value1 = await ReadWhitespace(whitespace1, context, 0);
            var valueCategory1 = await value1.Move(context);
            Assert.IsTrue(valueCategory1.TryObject(out var object1));
            var objectStart1 = await object1.Move(context);
            (var whitespace2, _) = await objectStart1.Move(context);
            var members1 = await ReadWhitespace(whitespace2, context, 6);
            var membersCategory1 = await members1.Move(context);
            Assert.IsTrue(membersCategory1.TrySome(out var member1));

            // true
            // false
            // 1234
            // asdf
            // null
            var subsequentMembers1 = await ReadTrueFalseNumberStringNull(member1, context, 1, 4);

            // object
            var value2 = await ReadSubsequentMemberToValue(subsequentMembers1, context, 6, 6);
            var valueCategory2 = await value2.Move(context);
            Assert.IsTrue(valueCategory2.TryObject(out var object2));
            var objectStart2 = await object2.Move(context);
            (var whitespace3, _) = await objectStart2.Move(context);
            var members2 = await ReadWhitespace(whitespace3, context, 10);
            var membersCategory2 = await members2.Move(context);
            Assert.IsTrue(membersCategory2.TrySome(out var member2));
            var subsequentMembers2 = await ReadTrueFalseNumberStringNull(member2, context, 2, 4);
            var subsequentMembersCategory2 = await subsequentMembers2.Move(context);
            Assert.IsTrue(subsequentMembersCategory2.TryNone(out var whitespace4));
            var objectEnd1 = await ReadWhitespace(whitespace4, context, 6);
            (var subsequentMembers3, _) = await objectEnd1.Move(context);

            // emptyobject
            var value3 = await ReadSubsequentMemberToValue(subsequentMembers3, context, 11, 6);
            var valueCategory3 = await value3.Move(context);
            Assert.IsTrue(valueCategory3.TryObject(out var object3));
            var objectStart3 = await object3.Move(context);
            (var whitespace5, _) = await objectStart3.Move(context);
            var members3 = await ReadWhitespace(whitespace5, context, 0);
            var membersCategory3 = await members3.Move(context);
            Assert.IsTrue(membersCategory3.TryNone(out var whitespace6));
            var objectEnd2 = await ReadWhitespace(whitespace6, context, 0);
            (var subsequentMembers4, _) = await objectEnd2.Move(context);

            // emptyarray
            var value4 = await ReadSubsequentMemberToValue(subsequentMembers4, context, 10, 6);
            var valueCategory4 = await value4.Move(context);
            Assert.IsTrue(valueCategory4.TryArray(out var array1));
            var arrayStart1 = await array1.Move(context);
            (var whitespace7, _) = await arrayStart1.Move(context);
            var arrayElements1 = await ReadWhitespace(whitespace7, context, 0);
            var arrayElementsCategory1 = await arrayElements1.Move(context);
            Assert.IsTrue(arrayElementsCategory1.TryNone(out var whitespace8));
            var arrayEnd1 = await ReadWhitespace(whitespace8, context, 0);
            (var subsequentMembers5, _) = await arrayEnd1.Move(context);

            //// TODO add the rest of the reading here
        }

        private static async ValueTask<TNextReader?> ReadWhitespace<TNextReader>(WhitespaceReader<TNextReader>? whitespaceReader, Context context, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var whitespaceCategory = await whitespaceReader.Move(context);
                Assert.IsTrue(whitespaceCategory.TrySome(out var whitespaceCharacter));
                (whitespaceReader, _) = await whitespaceCharacter.Move(context);
            }

            var category = await whitespaceReader.Move(context);
            Assert.IsTrue(category.TryNone(out var nextReader));

            return nextReader;
        }

        private static async ValueTask<SubsequentMembersReader<TNextReader>?> ReadTrueFalseNumberStringNull<TNextReader>(MemberReader<SubsequentMembersReader<TNextReader>>? memberReader, Context context, int tabCount, int tabLength)
        {
            var whitespaceLength = tabCount * tabLength + 2;

            // true
            var value2 = await ReadMemberToValue(memberReader, context, 4);
            var valueCategory2 = await value2.Move(context);
            Assert.IsTrue(valueCategory2.TryTrue(out var true1));
            (var subsequentMembers, _) = await true1.Move(context);

            // false
            var value3 = await ReadSubsequentMemberToValue(subsequentMembers, context, 5, whitespaceLength);
            var valueCategory3 = await value3.Move(context);
            Assert.IsTrue(valueCategory3.TryFalse(out var false1));
            (var subsequentMembers2, _) = await false1.Move(context);

            // 1234
            var value4 = await ReadSubsequentMemberToValue(subsequentMembers2, context, 6, whitespaceLength);
            var valueCategory4 = await value4.Move(context);
            Assert.IsTrue(valueCategory4.TryNumber(out var number1));
            var subsequentMembers3 = await ReadNumber(number1, context);

            // asdf
            var value5 = await ReadSubsequentMemberToValue(subsequentMembers3, context, 6, whitespaceLength);
            var valueCategory5 = await value5.Move(context);
            Assert.IsTrue(valueCategory5.TryString(out var string1));
            var subsequentMembers4 = await ReadString(string1, context, 4);

            // null
            var value6 = await ReadSubsequentMemberToValue(subsequentMembers4, context, 4, whitespaceLength);
            var valueCategory6 = await value6.Move(context);
            Assert.IsTrue(valueCategory6.TryNull(out var null1));
            (var nextReader, _) = await null1.Move(context);

            return nextReader;
        }

        private static async ValueTask<TNextReader?> ReadNumber<TNextReader>(NumberReader<TNextReader>? numberReader, Context context)
        {
            var sign = await numberReader.Move(context);
            (var @int, _) = await sign.Move(context);
            var intCategory = await @int.Move(context);
            Assert.IsTrue(intCategory.TryNonZero(out var leadingDigit));
            (var digits, _) = await leadingDigit.Move(context);
            for (int i = 0; i < 4; ++i)
            {
                var category = await digits.Move(context);
                Assert.IsTrue(category.TrySome(out var digit));
                (digits, _) = await digit.Move(context);
            }

            var digitsCategory = await digits.Move(context);
            Assert.IsTrue(digitsCategory.TryNone(out var frac));
            var fracCategory = await frac.Move(context);
            Assert.IsTrue(fracCategory.TryAbsent(out var exp));
            var expCategory = await exp.Move(context);
            Assert.IsTrue(expCategory.TryAbsent(out var nextReader));

            return nextReader;
        }

        private static async ValueTask<ValueReader<SubsequentMembersReader<TNextReader>>?> ReadSubsequentMemberToValue<TNextReader>(SubsequentMembersReader<TNextReader>? subsequentMembersReader, Context context, int memberNameLength, int tabLength)
        {
            var subsequentMembersCategory = await subsequentMembersReader.Move(context);
            Assert.IsTrue(subsequentMembersCategory.TrySome(out var comma));
            (var whitespace1, _) = await comma.Move(context);
            var member = await ReadWhitespace(whitespace1, context, tabLength);
            var valueReader = await ReadMemberToValue(member, context, memberNameLength);

            return valueReader;
        }

        private static async ValueTask<ValueReader<TNextReader>?> ReadMemberToValue<TNextReader>(MemberReader<TNextReader>? memberReader, Context context, int memberNameLength)
        {
            var string1 = await memberReader.Move(context);
            var whitespace3 = await ReadString(string1, context, memberNameLength);
            var colon1 = await ReadWhitespace(whitespace3, context, 0);
            (var whitespace4, _) = await colon1.Move(context);
            var nextReader = await ReadWhitespace(whitespace4, context, 1);

            return nextReader;
        }

        private static async ValueTask<TNextReader?> ReadString<TNextReader>(StringReader<TNextReader>? stringReader, Context context, int count)
        {
            var stringDelimiter1 = await stringReader.Move(context);
            (var charsReader, _) = await stringDelimiter1.Move(context);
            for (int i = 0; i < count; ++i)
            {
                var charsCategory = await charsReader.Move(context);
                Assert.IsTrue(charsCategory.TrySome(out var @char));
                var charCategory = await @char.Move(context);
                Assert.IsTrue(charCategory.TryUnescaped(out var unescapedChar));
                (charsReader, _) = await unescapedChar.Move(context);
            }

            var category = await charsReader.Move(context);
            Assert.IsTrue(category.TryNone(out var stringDelimiter2));
            (var nextReader, _) = await stringDelimiter2.Move(context);

            return nextReader;
        }

        //// TODO write the streamed read test
        //// TODO go through all todos for `oddtrotter.calendarv1.tokenization`; also look at the unit tests for those same types

    }
}
