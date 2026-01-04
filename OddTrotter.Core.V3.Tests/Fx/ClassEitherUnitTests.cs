namespace Fx.Either
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //// TODO the "bare minimum" should be for that top-level "production" code; you will then do this recursively later for other files
    //// TODO implement the bare minimum needed for these tests; here, the bare minimum includes anything required for type inference
    //// TODO keep a list of all of the patterns that you need to complete (like the below thing about typeholders and implementing multiple interfaces)
    //// TODO there should be a typeholder property (or extension) for each interface implemented; so, for example, `realizable<T>` should have `typeholder<realizable<T>, t, itask<T>> aseither` *and* `typeholder<realizable<T>, t> ascontinuable`
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC; here, the bare minimum includes anything required for type inference; like, make everything look pretty, allo the way down
    //// TODO names should be like `either`, `valueeither`, and `frameeither`; establish this convention for other `ref struct`s as well //// TODO for `ref struct` maybe `scopedeither` works better than `frameeither`
    //// TODO `iawaitable` should be *only* what is needed for `await` to work
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO implement assert extensions so you can always use assert.that
    //// TODO for the current "bare minimum", make everything look really nice and complete; you don't need every overload and variation, but the ones that you do have should be complete
    //// TODO you need to implement "everything" (all of the extension variations and overloads; any renames that need to happen; implementing visitors and such; full code quality); but, you weren't very systematic the first time through with what you implemented, nor with what you tested; you should do that now
    //// TODO add `assert.that` to code quality
    //// TODO you have to use `is null` for null checks because `==` can be overridden; add this to code quality
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
            var continued = taskWrapper
                .ContinueWith(
                    _ => "hello",
                    _ =>
                    {
                        wrappingException = new InvalidOperationException("some message", _);
                        throw wrappingException;
                    },
                    _ => "hello");

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
            var either = Either.Left<Exception>().Right("asdf");
            var rightMapException = Assert
                .That
                .ThrowsException<RightMapException>(() =>
                    either.Apply(
                        left => left.ToString().Length,
                        right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = Either.Right<Exception>().Left("asdf");
            var leftMapException = Assert
                .That
                .ThrowsException<LeftMapException>(() => 
                    either.Apply(
                        left => int.Parse(left),
                        right => right.ToString().Length));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = Either.Left<Exception>().Right("asdf");
            var rightMapException = Assert
                .That
                .ThrowsException<RightMapException>(() =>
                    either.Select(
                        left => left,
                        right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            var either = Either.Right<Exception>().Left("asdf");
            var leftMapException = Assert
                .That
                .ThrowsException<LeftMapException>(() => 
                    either.Select(
                        left => int.Parse(left),
                        right => right));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = Either.Right<Exception>().Left(42);
            var result = await ClassEitherUnitTests.TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = Either.Left<int>().Right(exception);
            var result = await ClassEitherUnitTests.TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
        }

        private static async Task<string> TestMethod1Impl(IEither<int, Exception> either)
        {
            //// TODO it is not good that the `apply` overload being called here is "passing through" the awaitables from the map delegates; (i think) if exceptions were thrown, then this would actually result in incorrect behavior //// TODO look at `testmethod3impl` and you will see that this is not true; i'm not clear why
            return await either
                .Apply(
                    value => ToString(value),
                    exception => ToString(exception))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            var exception = new Exception("the message");
            var either = Either.Left<int>().Right(exception);

            var rightMapException =
                await Assert
                    .That
                    .ThrowsExceptionAsync<RightMapException>(
                        async () =>
                            await ClassEitherUnitTests
                                .TestMethod3Impl(either)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);

            Assert.That.AreEqual(exception, rightMapException.InnerException);
        }

        private static async Task<string> TestMethod3Impl(IEither<int, Exception> either)
        {
            return await either
                .Apply(
                    value => ToString(value),
                    exception => throw exception)
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            var either = Either.Right<Exception>().Left("42");
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = Either.Right<Exception>().Left("not a number");
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = Either.Left<string>().Right(exception);
            var result = await TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod2Impl(IEither<string, Exception> either)
        {
            //// TODO you are here
            var parsed = either.SelectAsync( ///// TODO don't call this "async"?
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
