namespace Fx.Either
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;









    //// patterns:
    ////
    //// use a visitor for discriminated unions //// TODO now that you are really just exposing an `apply` method in these cases, i think you can just have `apply` be `abstract` to get the same effect
    //// use the `either` factories instead of the constructor
    //// there should be a typeholder property (or extension) for each interface implemented on ref structs; so, for example, `realizable<T>` should have `typeholder<realizable<T>, t, itask<T>> aseither` *and* `typeholder<realizable<T>, t> ascontinuable` //// TODO these should be properties so that consumers can create extensions with the same name without conflicting
    //// factory class for generic types
    //// when exceptions are wrapped by a concrete type to adapt to an implemented interface, the concrete type should document the exception that was set to innerexception
    //// names should be like `either`, `valueeither`, and `scopedeither`; establish this convention for other `ref struct`s as well
    ////
    //// every `ieither` extension should have a mixin
    //// either extensions where the mapping returns an awaitable (or continuable or something "async") should be suffixed with "async" //// TODO double check that this rule is actually necessary
    //// tests for either extensions need to include all combinations of source and result types being ref struct or not, including left and right being different from each other
    //// either extensions have `allows ref struct` on as many generics as possible
    //// TODO still not 100% convinced on this; try it out first: either extensions that take in `typeholder` should return `typeholder`
    //// don't have nested "map" exceptions when an implementation leverages another extensions; e.g. `selectmany` implements a right map that calls `apply`; the right map should not throw a `rightmapexception` itself because this would result in the caller receiving a `rightmapexception` with an `innerexception` of a `rightmapexception` //// TODO you could add a `debug.assert` to the map exceptions to uphold this pattern
    //// methods that convert to a `typeholder` belong in their own separate extensions classes (because they are adapting a concrete input type to a typeholder)
    //// each "overload" (a change in the number or meaning of parameters) should have the "variations" that is the cross product of these dimensions:
    ////    `either` parameter: teither, typeholder<teither>, ieither, realizable<teither>, realizable<typeholder<teither>>, realizable<ieither>, realizable<either>, itask<ieither>, task<either>, task<ieither> //// TODO note that the reason for some of this "duplication" (like `task<either>` + `task<ieither>` and `realizable<either>` + `realizable<ieither>` is because of the lack of covariance for the concrete types (`task` and `realizable` in these cases), which prevents the chaining of the fluent API when the caller has custom methods that return the concrete types
    ////    "map" parameters: tcontinuable, icontinuable, realizable, task, itask, tresult, action /*TODO even though you had a use-case for this, i'm not 100% convinced on it*/
    ////    "context" parameter: absent, ref, value








    //// TODO implement assert extensions so you can always use assert.that; this should go in `fx.test` or something
    //// TODO add `assert.that` to code quality
    //// TODO add "good lambda parameter names" to code quality
    //// TODO add `await="true"` exception documentation to code quality
    //// TODO you have to use `is null` for null checks because `==` can be overridden; add this to code quality //// TODO but i think you already use object.referenceequal for this though?
    //// TODO add to code quality that if an exception has additional properties, the `tostring` method should be overloaded to include those properties? (NOTE: .NET doesn't do this)
    //// TODO add covariance and contravariance to code quality



    //// these should go in fx.core:
    ////    do a complete cleanup of the `system` folder
    ////        TODO `iawaitable` should be *only* what is needed for `await` to work
    ////    do a complete cleanup of the `fx` folder
    ////        TODO for all of your tests, because of the nature of `realizable`, you will need to have a test where the values are realized and a test where the values are not realized
    ////        TODO don't forget to implement all of the mixins for all of the fundamental ieither implementations; make sure that all wrappers for an either implement a monad so that the wrappers don't have to implement all of the mixins just to get at the underlying either's mixins
    ////        TODO it seems like you have determined that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
    ////    you have a list of patterns that you used for the `fx` folder; which of those do you want to add to code quality? anything that you don't add to code quality should be documented in the namespaces that follow those patterns
    ////    you need to move the query context stuff from the oddtrotter folder into fx.core
    ////        move any extensions or helpers into their appropriate "production" places
    ////        you should explore the `ref struct` stuff for that before doing it, though, similar to how you did `ieither`
    ////        go through calendarv2 in oddtrotter.core to see if there's any ideas to pull from there
    ////        go through calendarv1 in oddtrotter.core to see if there's any ideas to pull from there
    ////    pull anything else that's useful out of calendarv1 and calendarv2 in oddtrotter.core










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
                    .ThrowsException<InvalidOperationException>()
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
                        .ThrowsException<InvalidOperationException>()
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
            return await either
                .ApplyAsync(
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
        public async Task TestMethod4()
        {
            var exception = new Exception("the message");
            var either = Either.Left<int>().Right(exception);

            var rightMapException =
                await Assert
                    .That
                    .ThrowsExceptionAsync<RightGenerationException>(
                        async () =>
                            await ClassEitherUnitTests
                                .TestMethod4Impl(either)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);

            Assert.That.AreEqual(exception, rightMapException.InnerException);
        }

        private static async Task<string> TestMethod4Impl(IEither<int, Exception> either)
        {
            return await either
                .ApplyAsync(
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
            var parsed = either
                .SelectAsync(
                    value => Parse(value),
                    error => Task.FromResult(error).ToTaskWrapper());

            return parsed
                .Apply(
                    actualParsing => actualParsing
                        .Apply(
                            actuallyParsed => actuallyParsed.ToString(),
                            parseError => parseError.ToString()),
                    readError => readError.ToString());
        }

        public static TaskWrapper<IEither<int, Exception>> Parse(string value)
        {
            return ParseImpl(value).ToTaskWrapper();
        }

        private static async Task<IEither<int, Exception>> ParseImpl(string value)
        {
            return await Task.FromResult(ParseInner(value)).ConfigureAwait(false);
        }

        private static IEither<int, Exception> ParseInner(string value)
        {
            try
            {
                return Either.Right<Exception>().Left(int.Parse(value));
            }
            catch (Exception exception)
            {
                return Either.Left<int>().Right(exception);
            }
        }

        public static TaskWrapper<string> ToString(int value)
        {
            return ToStringImpl(value).ToTaskWrapper();
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static TaskWrapper<string> ToString(Exception exception)
        {
            return ToStringImpl(exception).ToTaskWrapper();
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
