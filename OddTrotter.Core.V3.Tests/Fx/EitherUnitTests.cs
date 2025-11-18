namespace Fx
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EitherUnitTests
    {
        [TestMethod]
        public void ApplyRightMapException()
        {
            var either = new Either<Exception, string>("asdf");
            var rightMapException = Assert.ThrowsException<RightMapException>(() =>
                either.Apply(
                    left => left.ToString().Length,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = new Either<string, Exception>("asdf");
            var leftMapException = Assert.ThrowsException<LeftMapException>(() => 
                either.Apply(
                    left => int.Parse(left),
                    right => right.ToString().Length));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = new Either<Exception, string>("asdf");
            var rightMapException = Assert.ThrowsException<RightMapException>(() =>
                either.Select(
                    left => left,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            var either = new Either<string, Exception>("asdf");
            var leftMapException = Assert.ThrowsException<LeftMapException>(() => 
                either.Select(
                    left => int.Parse(left),
                    right => right));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = new Either<int, Exception>(42);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = new Either<int, Exception>(exception);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static async Task<string> TestMethod1Impl(IEither<int, Exception> either)
        {
            bool context = false;
            return await either.Apply<string, bool, TaskWrapper<string>>(
                (int value, ref bool context) => ToString(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref context);
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            var either = new Either<string, Exception>("42");
            var result = await TestMethod2Impl(either);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = new Either<string, Exception>("not a number");
            var result = await TestMethod2Impl(either);

            Assert.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = new Either<string, Exception>(exception);
            var result = await TestMethod2Impl(either);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod2Impl(IEither<string, Exception> either)
        {
            var parsed = either.SelectAsync(
                value => Parse(value),
                error => new TaskWrapper<Exception>(Task.FromResult(error)));

            return parsed.Apply(
                actualParsing => actualParsing.Apply(
                    actuallyParsed => actuallyParsed.ToString(),
                    parseError => parseError.ToString()),
                readError => readError.ToString());
        }

        public static TaskWrapper<IEither<int, Exception>> Parse(string value)
        {
            return new TaskWrapper<IEither<int, Exception>>(ParseImpl(value));
        }

        private static async Task<IEither<int, Exception>> ParseImpl(string value)
        {
            return await Task.FromResult(ParseInner(value)).ConfigureAwait(false);
        }

        private static IEither<int, Exception> ParseInner(string value)
        {
            try
            {
                return new Either<int, Exception>(int.Parse(value));
            }
            catch (Exception exception)
            {
                return new Either<int, Exception>(exception);
            }
        }

        public static TaskWrapper<string> ToString(int value)
        {
            return new TaskWrapper<string>(ToStringImpl(value));
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static TaskWrapper<string> ToString(Exception exception)
        {
            return new TaskWrapper<string>(ToStringImpl(exception));
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }
    }
}
