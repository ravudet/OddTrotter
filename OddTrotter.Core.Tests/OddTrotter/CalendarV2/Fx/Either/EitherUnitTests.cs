/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    using Fx.Try;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EitherFactoryUnitTests
    {
        [TestMethod]
        public void CreateLeft()
        {
            var value = 42;
            var either = Either.Left(value).Right<string>();

            either.Apply(
                (left, context) =>
                {
                    Assert.AreEqual(value, left);
                    return new Nothing();
                },
                (right, context) =>
                {
                    Assert.Fail("a left was expected");
                    return new Nothing();
                },
                new Nothing());
        }

        [TestMethod]
        public void CreateRight()
        {
            var value = "this is a value";
            var either = Either.Left<int>().Right(value);

            either.Apply(
                (left, context) =>
                {
                    Assert.Fail("a right was expected");
                    return new Nothing();
                },
                (right, context) =>
                {
                    Assert.AreEqual(value, right);
                    return new Nothing();
                },
                new Nothing());
        }

        [TestMethod]
        public void DefaultFullConstructor()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new Either.Full<string>());
        }

        [TestMethod]
        public void DefaultFull()
        {
            Assert.ThrowsException<InvalidOperationException>(() => default(Either.Full<string>).Right<int>());
        }

        [TestMethod]
        public void CreateNullDisciminator()
        {
            var value = "Asfd";

            Assert.ThrowsException<ArgumentNullException>(() => Either.Create(value,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , int.Parse, val => string.Concat(val, val)));
        }

        [TestMethod]
        public void CreateNullLeftFactory()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(() => Either.Create(value, val => val.Length % 2 == 0,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<string, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , val => string.Concat(val, val)));
        }

        [TestMethod]
        public void CreateNullRightFactory()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(() => Either.Create(value, val => val.Length % 2 == 0, int.Parse,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<string, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void CreateWithDiscriminatorLeft()
        {
            var value = "42";

            var either = Either.Create(value, val => val.Length % 2 == 0, int.Parse, val => string.Concat(val, val));

            Assert.IsTrue(either.TryGetLeft(out var left));
            Assert.AreEqual(42, left);
            Assert.IsFalse(either.TryGetRight(out var right));
        }

        [TestMethod]
        public void CreateWithDiscriminatorRight()
        {
            var value = "423";

            var either = Either.Create(value, val => val.Length % 2 == 0, int.Parse, val => string.Concat(val, val));

            Assert.IsFalse(either.TryGetLeft(out var left));
            Assert.IsTrue(either.TryGetRight(out var right));
            Assert.AreEqual("423423", right);
        }

        [TestMethod]
        public void TryCreateNullDisciminator()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(
                () => 
                    Either
                        .TryCreate(
                            value,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            (Try<string, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            (value, parsed) => value, 
                            (value) => value));
        }

        [TestMethod]
        public void TryCreateNullLeftFactory()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    Either
                        .TryCreate(
                            value,
                            IntTryParse,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            (Func<string, int, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            value => value));
        }

        private static Try<string, int> IntTryParse { get; } = int.TryParse;

        [TestMethod]
        public void TryCreateNullRightFactory()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(
                () => 
                    Either
                        .TryCreate(
                            value,
                            )
        }
    }
}
