/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Linq
{
    using System;
    using System.Threading.Tasks;
    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class FirstOrDefaultUnitTests
    {
        [TestMethod]
        public void FirstOrDefaultNullEither()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new FirstOrDefault<string, int>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void ApplyLeft()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            var result = firstOrDefault.Apply(
                (left, context) => left[0],
                (right, context) => right.ToString()[0], 
                new Nothing());

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public void ApplyRight()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Right(42));

            var result = firstOrDefault.Apply(
                (left, context) => left[0],
                (right, context) => right.ToString()[0], new Nothing());

            Assert.AreEqual('4', result);
        }

        [TestMethod]
        public void ApplyLeftException()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => firstOrDefault.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException,
                    (right, context) => throw invalidCastException,
                    default));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);
        }

        [TestMethod]
        public void ApplyRightException()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Right(42));

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => firstOrDefault.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException,
                    (right, context) => throw invalidCastException,
                    default));

            Assert.AreEqual(invalidCastException, rightMapException.InnerException);
        }

        [TestMethod]
        public void ApplyNullLeftMap()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            Assert.ThrowsException<ArgumentNullException>(() => firstOrDefault.Apply<Nothing, Nothing>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , (right, context) => default, default));
        }

        [TestMethod]
        public void ApplyNullRightMap()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            Assert.ThrowsException<ArgumentNullException>(
                () => 
                    firstOrDefault.Apply<Nothing, Nothing>(
                        (left, context) => default,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        , default));
        }

        [TestMethod]
        public async Task ApplyAsyncLeft()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            var result = await firstOrDefault
                .ApplyAsync(
                    async (left, context) => await Task.FromResult(left[0]).ConfigureAwait(false),
                    async (right, context) => await Task.FromResult(right.ToString()[0]).ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual('a', result);
        }

        /*[TestMethod]
        public void ApplyRight()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Right(42));

            var result = firstOrDefault.Apply(
                (left, context) => left[0],
                (right, context) => right.ToString()[0], new Nothing());

            Assert.AreEqual('4', result);
        }

        [TestMethod]
        public void ApplyLeftException()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => firstOrDefault.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException,
                    (right, context) => throw invalidCastException,
                    default));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);
        }

        [TestMethod]
        public void ApplyRightException()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Right(42));

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => firstOrDefault.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException,
                    (right, context) => throw invalidCastException,
                    default));

            Assert.AreEqual(invalidCastException, rightMapException.InnerException);
        }

        [TestMethod]
        public void ApplyNullLeftMap()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            Assert.ThrowsException<ArgumentNullException>(() => firstOrDefault.Apply<Nothing, Nothing>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , (right, context) => default, default));
        }

        [TestMethod]
        public void ApplyNullRightMap()
        {
            IEither<string, int> firstOrDefault = new FirstOrDefault<string, int>(new Either<string, int>.Left("asdf"));

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    firstOrDefault.Apply<Nothing, Nothing>(
                        (left, context) => default,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        , default));
        }*/
    }
}
