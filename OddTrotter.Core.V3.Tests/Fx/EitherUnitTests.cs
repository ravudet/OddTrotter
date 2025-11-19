namespace Fx
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //// TODO the test `applyrightmapexception` succeeds on its own, but fails when run with other tests... //// TODO this is because the task hasn't completed yet, so the check to in taskwrapper.awaiter.getresult for the exception is null; first of all, you need `getresult` to ensure that the task is completed before continuing; second, you should probably have no use task.run actually, and instead have an adapter method to uses task.fromresult, task.fromfault, task.fromcanceled
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such

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
