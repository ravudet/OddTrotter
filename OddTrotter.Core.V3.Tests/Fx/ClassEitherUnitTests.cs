namespace Fx
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //// TODO the "bare minimum" should be for that top-level "production" code; you will then do this recursively later for other files
    //// TODO implement the bare minimum needed for these tests; here, the bare minimum includes anything required for type inference
    //// TODO there should be a typeholder property (or extension) for each interface implemented; so, for example, `realizable<T>` should have `typeholder<realizable<T>, t, itask<T>> aseither` *and* `typeholder<realizable<T>, t> ascontinuable`
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC; here, the bare minimum includes anything required for type inference; like, make everything look pretty, allo the way down
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO implement assert extensions so you can always use assert.that
    //// TODO for the current "bare minimum", make everything look really nice and complete; you don't need every overload and variation, but the ones that you do have should be complete
    //// TODO you need to implement "everything" (all of the extension variations and overloads; any renames that need to happen; implementing visitors and such; full code quality); but, you weren't very systematic the first time through with what you implemented, nor with what you tested; you should do that now
    //// TODO you could have a `class` implementation of `ieither` that takes delegates for left and right (where those delegates can return `ref struct`s); is this worth doing?
    //// TODO it seems like you have determined that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such

    [TestClass]
    public sealed class ClassEitherUnitTests
    {
        public required TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TaskWrapperContinueWithSourceContinuationThrows()
        {
            var exceptionToThrow = new InvalidOperationException("blah");

            var continued = ClassEitherUnitTests
                .Parse("42")
                .ContinueWith(
                    either => throw exceptionToThrow,
                    _ => "hello",
                    _ => "hello");

            var thrownException = 
                await Assert
                    .That
                    .RefStructAwaitable(continued.ToAwaitable().AsAwaitable())
                    .Throws<InvalidOperationException>()
                .ConfigureAwait(false);
            Assert.That.AreEqual(exceptionToThrow, thrownException);
        }

        [TestMethod]
        public async Task TaskWrapperContinueWithExceptionContinuationThrows()
        {
            var originalException = new Exception("the original");
            var taskWrapper = Task.FromException<string>(originalException).ToTaskWrapper();

            InvalidOperationException? wrappingException = null;
            var continued = taskWrapper.ContinueWith(
                _ => "hello",
                _ =>
                {
                    wrappingException = new InvalidOperationException("some message", _);
                    throw wrappingException;
                },
                _ => "hello");

            //// TODO you are here
            //// TODO the test is failing becauase of something in `taskwrapper` losing track of one of the continuations
            var thrownException =
                await Assert
                    .That
                    .RefStructAwaitable(continued.ToAwaitable().AsAwaitable())
                    .Throws<InvalidOperationException>()
                .ConfigureAwait(false);
            Assert.IsNotNull(wrappingException);
            Assert.That.AreEqual(wrappingException, thrownException);
            Assert.That.AreEqual(originalException, thrownException.InnerException);
        }

        [TestMethod]
        public void ApplyRightMapException()
        {
            var either = new Either<Exception, string>.Right("asdf");
            var rightMapException = Assert.ThrowsException<RightMapException>(() =>
                either.Apply(
                    left => left.ToString().Length,
                    right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = new Either<string, Exception>.Left("asdf");
            var leftMapException = Assert.ThrowsException<LeftMapException>(() => 
                either.Apply(
                    left => int.Parse(left),
                    right => right.ToString().Length));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = new Either<Exception, string>.Right("asdf");
            var rightMapException = Assert.ThrowsException<RightMapException>(() =>
                either.Select(
                    left => left,
                    right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            var either = new Either<string, Exception>.Left("asdf");
            var leftMapException = Assert.ThrowsException<LeftMapException>(() => 
                either.Select(
                    left => int.Parse(left),
                    right => right));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = new Either<int, Exception>.Left(42);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = new Either<int, Exception>.Right(exception);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
        }

        private static async Task<string> TestMethod1Impl(IEither<int, Exception> either)
        {
            return await either
                .Apply(
                    value => ToString(value),
                    exception => ToString(exception))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            var either = new Either<string, Exception>.Left("42");
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = new Either<string, Exception>.Left("not a number");
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = new Either<string, Exception>.Right(exception);
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
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
                return new Either<int, Exception>.Left(int.Parse(value));
            }
            catch (Exception exception)
            {
                return new Either<int, Exception>.Right(exception);
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

        [TestMethod]
        public async Task ReadFromFile()
        {
            var workingDirectory = Path.Combine(TestContext.TestRunDirectory, TestContext.TestName);
            var filePath = Path.Combine(workingDirectory, "somedata.txt");
            await WriteToFile(filePath, "42").ConfigureAwait(false);

            var potentiallyParsed = await ParseFromFile(filePath).ConfigureAwait(false);
            Assert.That.IsTrue(potentiallyParsed.Decompose(out var result, out _));
            Assert.That.AreEqual(42, result);
        }

        private static async Task<IEither<int, Exception>> ParseFromFile(string filePath)
        {
            var text = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            return await ParseImpl(text).ConfigureAwait(false);
        }

        private static async Task WriteToFile(string filePath, string contents)
        {
            var parentDirectory = Path.GetDirectoryName(filePath);
            if (parentDirectory == null)
            {
                throw new Exception("TODO");
            }

            while (true)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, contents).ConfigureAwait(false);
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(parentDirectory);
                }
            }
        }
    }
}
