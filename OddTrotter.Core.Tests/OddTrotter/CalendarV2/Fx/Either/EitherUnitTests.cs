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
        public void ToEitherPredicateNullPredicate()
        {
            var value = "asdf";

            Assert.ThrowsException<ArgumentNullException>(() => value.ToEither(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void ToEitherPredicateBranchTaken()
        {
            var value = "asdf";

            var either = value.ToEither(_ => _.Length % 2 == 0);

            Assert.IsTrue(either.TryGetLeft(out var left));
            Assert.AreEqual(value, left);
            Assert.IsFalse(either.TryGetRight(out var right));
        }

        [TestMethod]
        public void ToEitherPredicateBranchNotTaken()
        {
            var value = "asdf";

            var either = value.ToEither(_ => _.Length % 2 == 1);

            Assert.IsFalse(either.TryGetLeft(out var left));
            Assert.IsTrue(either.TryGetRight(out var right));
            Assert.AreNotEqual(value, right);
        }
    }
}
