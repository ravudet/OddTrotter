namespace Fx.Either
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //// TODO the "bare minimum" should be for that top-level "production" code; you will then do this recursively later for other files

    //// TODO the stuff in the `oddtrotter` folder is still being explored; once you've productized the stuff in `fx` and `system`, you should completely rewrite the stuff in `oddtrotter`

    //// TODO taskextensions
    //// TODO taskwrapper
    //// TODO realizableextensions
    //// TODO keep a list of all of the patterns that you need to complete (like the below thing about typeholders and implementing multiple interfaces) (mixins) (what are all of the extension method variants? (async maps, ref struct, async either, etc.) (document on concrete types when `innerexception` is set)
    //// TODO there should be a typeholder property (or extension) for each interface implemented; so, for example, `realizable<T>` should have `typeholder<realizable<T>, t, itask<T>> aseither` *and* `typeholder<realizable<T>, t> ascontinuable` //// TODO these should be properties so that consumers can create extensions with the same name without conflicting






















    //// TODO in strong convention context, are you sure you want to use ieither instead of teither : ieither, allows ref struct?
    //// TODO i don't know if i like `calendareventdeserializer.extensions.try`; get this right as part of the "bare minimum" before moving forward
    //// TODO move any extensions or helpers to their appropriate "production" places
    //// TODO you implemented the odata interfaces, in service of the next TODO item
        //// TODO i think i like everything below, but i'm going to mull it over throughout the day, so instead i'm working on an "authorized" convention context first
        //// TODO strong convention context (it actually starts at *weak* convention context) is going to differentiate well-known odata error responses (like 501 and 503); graph calendar events context could already surface these as exceptions; however, the "paging" thing is still tripping me up; basically, it shouldn't be a paging *exception*, it should be a discriminated union, because we *know* what the inner exceptions will be; so it should be paging error, and it should be a union of all of the exceptions that `evaluate` can throw; *but* this doesn't solve the `iquerycontext` issue; this is because, ultimately what we have is a list of exceptions to get 1 page; the "paging error/exception/whatever" is just a union of all of those potential issues for the subsequent pages; and so the *graph* context has enough knowledge to know the list of exceptions for the first page; but when we abstract this into an `iquerycontext`, we have lost the knowledge for that first page to properly document it, even though subsequent pages it is "self-documented" by the generic parameter of "paging exception/error/whatever"; you didn't run into this before because you simply considered an initial error to be an empty response collection + the "paging exception" of the initial error; but now you've decided that it is "good" to let the caller differentiate if the got some pages *without* errors, they just happened to be empty, or if they got no pages at all because the first call failed
        //// TODO maybe you could have a `querycontextexception<TError>` and `iquerycontext<TClientValue, TDataStoreValue, TError>` could document that it throws `querycontextexception<TError>`?
        //// TODO in graph.calendareventscontext, is there a difference between graph giving an error response and graph giving a "malformed" respose (e.g. a collection when a single value was expected, something that's not json, something that's missing properties, etc.)? //// TODO i mean, yes, if you want to differentiate invalid token errors
        //// TODO now you need to do the UI level
    //// TODO MAKE SURE TO GO ALL THE WAY TO THE UI LEVEL!
    //// TODO actually do a mock test in `contexttests`
    //// TODO remove anything under oddtrotter in v3 that is dead code
    //// TODO do you actually like the way that all of the code looks in the v3 oddtrotter stuff? that's how you will know that the next item is done
    //// TODO remember, "bare minimum" is actually production code

    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC; here, the bare minimum includes anything required for type inference; like, make everything look pretty, allo the way down
    //// TODO names should be like `either`, `valueeither`, and `frameeither`; establish this convention for other `ref struct`s as well //// TODO for `ref struct` maybe `scopedeither` works better than `frameeither` //// TODO `unboxableeither`? `boxableeither`?
    //// TODO `iawaitable` should be *only* what is needed for `await` to work
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO go through calendarv2 in oddtrotter.core to see if there's any ideas to pull from there
    //// TODO get feedback on names and style at this point, using oddtrotter POC as the demonstration; "style" here is asking about anything, but particularly the newlines for the lambdas (if you have `a => a.b().c()` is that `a => a\n.b()\n.c()` or is it `a =>\na\n.b()\n.c()`? does it change when there is an `await`?) and the fluent ".{method}" new line conventions, and where to put `await` and `configureawait` and all of that stuff
    //// TODO implement assert extensions so you can always use assert.that
    //// TODO for the current "bare minimum", make everything look really nice and complete; you don't need every overload and variation, but the ones that you do have should be complete
    //// TODO add `assert.that` to code quality
    //// TODO add `await="true"` exception documentation to code quality
    //// TODO you need to implement "everything" (all of the extension variations and overloads; any renames that need to happen; implementing visitors and such; full code quality); but, you weren't very systematic the first time through with what you implemented, nor with what you tested; you should do that now //// TODO this should probably be a "bottom up" kind of thing to make sure that all of the documented nuances at lower levels get surfaced at the highest levels
    ////    TODO for all of your tests, because of the nature of `realizable`, you will need to have a test where the values are realized and a test where the values are not realized
    //// TODO you have to use `is null` for null checks because `==` can be overridden; add this to code quality //// TODO but i think you already use object.referenceequal for this though?
    //// TODO you could have a `class` implementation of `ieither` that takes delegates for left and right (where those delegates can return `ref struct`s); is this worth doing?
    //// TODO it seems like you have determined that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
    //// TODO add to code quality that if an exception has additional properties, the `tostring` method should be overloaded to include those properties? (NOTE: .NET doesn't do this)
    //// TODO add covariance and contravariance to code quality
    //// TODO don't forget to implement all of the mixins for all of the fundamental ieither implementations; make sure that all wrappers for an either implement a monad so that the wrappers don't have to implement all of the mixins just to get at the underlying either's mixins

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
