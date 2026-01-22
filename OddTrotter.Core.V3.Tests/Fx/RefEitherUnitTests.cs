namespace Fx
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.Marshalling;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class RefEitherUnitTests
    {
        public required TestContext TestContext { get; set; }

        [TestMethod]
        public void ApplyRightMapException()
        {
            var either = RefEither.Left<Exception>().Right("asdf");
            var rightMapException = Assert
                .That
                .RefStruct(either)
                .ThrowsException<RightMapException>(
                    either => either.AsEither.Apply(
                        left => left.ToString().Length,
                        right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = RefEither.Right<Exception>().Left("asdf");
            var leftMapException = Assert
                .That
                .RefStruct(either)
                .ThrowsException<LeftMapException>(
                    either => either.AsEither.Apply(
                        left => int.Parse(left),
                        right => right.ToString().Length));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = RefEither.Left<Exception>().Right("asdf");
            var rightMapException = Assert
                .That
                .RefStruct(either)
                .ThrowsException<RightMapException>(
                    either => either.AsEither.Select(
                        left => left,
                        right => int.Parse(right)));

            Assert.That.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            var either = RefEither.Right<Exception>().Left("asdf");
            var leftMapException = Assert
                .That
                .RefStruct(either)
                .ThrowsException<LeftMapException>(
                    either => either.AsEither.Select(
                        left => int.Parse(left),
                        right => right));

            Assert.That.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = RefEither.Right<Exception>().Left(42);
            var result = await RefEitherUnitTests.TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = RefEither.Left<int>().Right(exception);
            var result = await RefEitherUnitTests.TestMethod1Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod1Impl(RefEither<int, Exception> either)
        {
            return either
                .AsEither
                .ApplyAsync(
                    value => ToString(value),
                    exception => ToString(exception));
        }

        [TestMethod]
        public async Task Bar()
        {
            var @string = await RefEitherUnitTests
                .Foo()
                .AsEither()
                .ApplyAsync(
                    value => Foo1(value),
                    exception => Foo2(exception))
                .ConfigureAwait(false);

            Assert.That.AreEqual("42", @string);
        }

        private static async Task<string> Foo2(Exception exception)
        {
            await Task.Delay(100).ConfigureAwait(false);
            return await ToString(exception).ConfigureAwait(false);
        }

        private static async Task<string> Foo1(int value)
        {
            await Task.Delay(100).ConfigureAwait(false);
            return await ToString(value).ConfigureAwait(false);
        }

        private static Realizable<RefEither<int, Exception>> Foo()
        {
            return Realizable.Realizable.FromFuture(//// TODO why do you need the namespace here?
                Task
                    .Delay(100)
                    .ToTaskWrapper()
                    .ContinueWith2(
                        _ => RefEither.Right<Exception>().Left(42),
                        _ => throw _,
                        _ => throw _));
        }

        [TestMethod]
        public async Task Bar2()
        {
            var @string = await RefEitherUnitTests
                .Foo5()
                .AsEither()
                .ApplyAsync(
                    value => Foo3(value),
                    exception => Foo4(exception))
                .ConfigureAwait(false);

            Assert.That.AreEqual("42", @string);
        }

        private static async Task<string> Foo4(Exception exception)
        {
            return await ToString(exception).ConfigureAwait(false);
        }

        private static async Task<string> Foo3(int value)
        {
            return await ToString(value).ConfigureAwait(false);
        }

        private static Realizable<RefEither<int, Exception>> Foo5()
        {
            return Realizable.Realizable.FromResult(RefEither.Right<Exception>().Left(42));
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            var either = RefEither.Right<Exception>().Left("42");
            var result = await RefEitherUnitTests.TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = RefEither.Right<Exception>().Left("not a number");
            var result = await RefEitherUnitTests.TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = RefEither.Left<string>().Right(exception);
            var result = await RefEitherUnitTests.TestMethod2Impl(either).ConfigureAwait(false);

            Assert.That.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod2Impl(RefEither<string, Exception> either)
        {
            var parsed = either
                .AsEither
                .SelectAsync(
                    value => Parse(value),
                    error => Task.FromResult(error).ToTaskWrapper());

            return parsed
                .AsEither()
                .Apply(
                    actualParsing => actualParsing.Apply(
                        actuallyParsed => actuallyParsed.ToString(),
                        parseError => parseError.ToString()),
                    readError => readError.ToString());
        }

        public static TaskWrapper<IEither<int, Exception>> Parse(string value)
        {
            return RefEitherUnitTests.ParseImpl(value).ToTaskWrapper();
        }

        private static async Task<IEither<int, Exception>> ParseImpl(string value)
        {
            return await Task.FromResult(RefEitherUnitTests.ParseInner(value)).ConfigureAwait(false);
        }

        private static IEither<int, Exception> ParseInner(string value)
        {
            int parsed;
            try
            {
                parsed = int.Parse(value);
            }
            catch (Exception exception)
            {
                return Either.Either.Left<int>().Right(exception); //// TODO why do you need the namespace?
            }

            return Either.Either.Right<Exception>().Left(parsed); //// TODO why do you need the namespace?
        }

        public static Realizable<string> ToString(int value)
        {
            return Realizable.Realizable.FromFuture(RefEitherUnitTests.ToStringImpl(value).ToTaskWrapper());
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static Realizable<string> ToString(Exception exception)
        {
            return Realizable.Realizable.FromFuture(RefEitherUnitTests.ToStringImpl(exception).ToTaskWrapper());
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }

        [TestMethod]
        public void RefLeft()
        {
            var value = 42;
            var either = RefEither.Right<Exception>().Left(new SomeRef(value));

            var result = either
                .AsEither
                .Apply(
                    left => left.Value,
                    right => right.ToString().Length);

            Assert.That.AreEqual(value, result);
        }

        private readonly ref struct SomeRef
        {
            public SomeRef(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TestMethod]
        public void RefRight()
        {
            var value = 42;
            var either = RefEither.Left<Exception>().Right(new SomeRef(value));

            var result = either
                .AsEither
                .Apply(
                    left => left.ToString().Length,
                    right => right.Value);

            Assert.That.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AsyncRefLeft()
        {
            var value = "42";
            var either = await RefEitherUnitTests.AsyncRefWork(value).ConfigureAwait(false);

            Assert.That.IsTrue(either.AsEither.Decompose(out var result, out var exception));
            Assert.That.AreEqual(42, result.Value);
            Assert.That.IsNull(exception);
        }

        private static ITask<RefEither<SomeRef, Exception>> AsyncRefWork(string value)
        {
            return RefEitherUnitTests
                .Parse(value)
                .ContinueWith2(
                    potentiallyParsed => potentiallyParsed
                        .Apply(
                            value => RefEither.Right<Exception>().Left(new SomeRef(value)),
                            exception => RefEither.Left<SomeRef>().Right(exception)),
                    _ => throw _,
                    _ => throw _);
        }

        [TestMethod]
        public async Task ReadFromFile()
        {
            var workingDirectory = Path.Combine(TestContext.TestRunDirectory, TestContext.TestName);
            var filePath = Path.Combine(workingDirectory, "somedata.txt");
            var value = 42;
            await RefEitherUnitTests.WriteToFile(filePath, value.ToString()).ConfigureAwait(false);

            var potentiallyParsed = await RefEitherUnitTests.ParseFromFile(filePath).ConfigureAwait(false);

            Assert.That.IsTrue(potentiallyParsed.AsEither.Decompose(out var result, out var exception));
            Assert.That.AreEqual(value, result);
            Assert.That.IsNull(exception);
        }

        private static ITask<RefEither<int, Exception>> ParseFromFile(string filePath)
        {
            //// TODO you are here
            //// TODO taskextensions
            //// TODO taskwrapper
            //// TODO realizableextensions

            return
                File.ReadAllTextAsync(filePath).ToFuture().ContinueWith2(
                text => ParseToRef(text),
                _ => throw _,
                _ => throw _);

            /*return new RefTask<string, RefEither<int, Exception>>(
                new TaskWrapper<string>(File.ReadAllTextAsync(filePath)),
                text => ParseToRef(text));*/
        }

        private static RefEither<int, Exception> ParseToRef(string text)
        {
            int value;
            try
            {
                value = int.Parse(text);
            }
            catch (Exception exception)
            {
                return new RefEither<int, Exception>(exception);
            }

            return new RefEither<int, Exception>(value);
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
