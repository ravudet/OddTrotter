namespace Fx
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //// TODO implement tests using the ref struct either implementation
    //// TODO implement a test with ref structs values
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO implement any TODOs
    //// TODO implement the bare minimum needed for these tests; here, the bare minimum includes anything required for type inference
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC; here, the bare minimum includes anything required for type inference
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO implement assert extensions so you can always use assert.that
    //// TODO it seems like you have determined that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such

    [TestClass]
    public sealed class ClassEitherUnitTests
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

    public static class AssertExtensions
    {
        public static T ThrowsException<T>(this Assert assert, Action action)
            where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }

        public readonly ref struct ThrowsExceptionBuilder<TState>
            where TState : allows ref struct
        {
            private readonly Assert assert;
            private readonly TState state;

            public ThrowsExceptionBuilder(Assert assert, TState state)
            {
                this.assert = assert;
                this.state = state;
            }

            public TException Commit<TException>(Action<TState> action)
                where TException : Exception
            {
                try
                {
                    action(this.state);
                    return this.assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return this.assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }
        }

        public static ThrowsExceptionBuilder<TState> ThrowsException<TState>(this Assert assert, TState state)
            where TState : allows ref struct
        {
            return new ThrowsExceptionBuilder<TState>(assert, state);
        }
    }

    [TestClass]
    public sealed class RefEitherUnitTests
    {
        [TestMethod]
        public void ApplyRightMapException()
        {
            var either = new RefEither<Exception, string>("asdf");
            var rightMapException = Assert.That.ThrowsException(either).Commit<RightMapException>(
                either => either.TypeHolder.Apply(
                    left => left.ToString().Length,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = new RefEither<string, Exception>("asdf");
            var leftMapException = Assert.That.ThrowsException(either).Commit<LeftMapException>(
                either => either.TypeHolder.Apply(
                    left => int.Parse(left),
                    right => right.ToString().Length));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = new RefEither<Exception, string>("asdf");
            var rightMapException = Assert.That.ThrowsException(either).Commit<RightMapException>(
                either => either.TypeHolder.Select(
                    left => left,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            //// TODO use `Assert.That` everywhere

            var either = new RefEither<string, Exception>("asdf");
            var leftMapException = Assert.That.ThrowsException(either).Commit<LeftMapException>(
                either => either.TypeHolder.Select(
                    left => int.Parse(left),
                    right => right));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = new RefEither<int, Exception>(42);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = new RefEither<int, Exception>(exception);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod1Impl(RefEither<int, Exception> either)
        {
            return either.Apply<string, bool, Realizable<string>>(
                (int value, ref bool context) => ToString(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref Context);
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            //// TODO you are here
            var either = new RefEither<string, Exception>("42");
            var result = await TestMethod2Impl(either);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = new RefEither<string, Exception>("not a number");
            var result = await TestMethod2Impl(either);

            Assert.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = new RefEither<string, Exception>(exception);
            var result = await TestMethod2Impl(either);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod2Impl(RefEither<string, Exception> either)
        {
            var parsed = either.SelectAsync<RefEither<string, Exception>, string, Exception, TaskWrapper<IEither<int, Exception>>, IEither<int, Exception>, TaskWrapper<Exception>, Exception>(
                value => Parse(value),
                error => new TaskWrapper<Exception>(Task.FromResult(error)));

            //// TODO use `typeholder` here (or some other way to address the type inference issue)
            return parsed.Apply<RefEither<IEither<int, Exception>, Exception>, IEither<int, Exception>, Exception, string>(
                actualParsing => actualParsing.Apply(
                    actuallyParsed => actuallyParsed.ToString(),
                    parseError => parseError.ToString())!,
                readError => readError.ToString()!);
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

        public static Realizable<string> ToString(int value)
        {
            return new Realizable<string>(new TaskWrapper<string>(ToStringImpl(value)));
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static Realizable<string> ToString(Exception exception)
        {
            return new Realizable<string>(new TaskWrapper<string>(ToStringImpl(exception)));
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }

        private static bool Context = false;
    }
}
