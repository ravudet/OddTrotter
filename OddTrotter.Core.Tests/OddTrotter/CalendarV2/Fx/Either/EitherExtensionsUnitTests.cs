/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EitherExtensionsUnitTests
    {
        [TestMethod]
        public void ApplyNoContextNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                    .Apply(left => left.ToString().Count(), right => right));
        }

        [TestMethod]
        public void ApplyNoContextNullLeftMap()
        {
            var either = Either.Left("sadf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , right => right));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , right => right));
        }

        [TestMethod]
        public void ApplyNoContextNullRightMap()
        {
            var either = Either.Left("saf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply(left => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply(left => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void ApplyNoContextLeftMapException()
        {
            var exception = new InvalidOperationException();
            var either = Either.Left("asdF").Right<int>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => either.Apply(left => throw exception, right => right));
            Assert.AreEqual(exception, leftMapException.InnerException);

            either = Either.Left<string>().Right(42);

            either.Apply(left => throw exception, right => right);
        }

        [TestMethod]
        public void ApplyNoContextRightMapException()
        {
            var exception = new InvalidOperationException();
            var either = Either.Left("asdf").Right<int>();

            either.Apply(left => left, right => throw exception);

            either = Either.Left<string>().Right(42);

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => either.Apply(left => left, right => throw exception));
            Assert.AreEqual(exception, rightMapException.InnerException);
        }

        [TestMethod]
        public void ApplyNoContext()
        {
            var either = Either.Left("sadf").Right<int>();

            Assert.AreEqual(4, either.Apply(left => left.Count(), right => right));

            either = Either.Left<string>().Right(42);

            Assert.AreEqual(42, either.Apply(left => left.Count(), right => right));
        }

        [TestMethod]
        public void SelectNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .Select((left, context) => left, (right, context) => right, new Nothing()));
        }

        [TestMethod]
        public void SelectNullLeftSelector()
        {
            var either = Either.Left("asdF").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<string, Nothing, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , (right, context) => right, new Nothing()));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<string, Nothing, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , (right, context) => right, new Nothing()));
        }

        [TestMethod]
        public void SelectNullRightSelector()
        {
            var either = Either.Left("asdF").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
                (left, context) => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<int, Nothing, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , new Nothing()));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
                (left, context) => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<int, Nothing, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , new Nothing()));
        }

        [TestMethod]
        public void Select()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
 
            IEither<StringBuilder, IEnumerable<int>> result = either.Select(
                (left, context) => context.Item1 = new StringBuilder(left), 
                (right, context) => context.Item2 = right.Select(val => val * 2), tuple);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = either.Select(
                (left, context) => context.Item1 = new StringBuilder(left), 
                (right, context) => context.Item2 = right.Select(val => val * 2), tuple);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .Select(
                            (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, string>)((left, context) => 
                                throw invalidOperationException),
                            (right, context) => 
                                context.Item2 = right, 
                            tuple));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());

            either.Select(
                (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, string>)((left, context) => 
                    throw invalidOperationException), 
                (right, context) => 
                    context.Item2 = right, tuple);
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            either.Select(
                (left, context) => 
                    context.Item1 = new StringBuilder(left),
                (Func<IEnumerable<int>, TupleBuilder<StringBuilder, IEnumerable<int>>, int>)((right, context) => 
                    throw invalidOperationException),
                tuple);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());
            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .Select(
                            (left, context) => 
                                context.Item1 = new StringBuilder(left), 
                            (Func<IEnumerable<int>, TupleBuilder<StringBuilder, IEnumerable<int>>, int>)((right, context) => 
                                throw invalidOperationException), 
                            tuple));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

        }

        private sealed class TupleBuilder<T1, T2>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            public TupleBuilder()
            {
            }

            public T1? Item1 { get; set; }

            public T2? Item2 { get; set; }
        }

        [TestMethod]
        public void SelectLeftNullEither()
        {
            Either<string, IEnumerable<int>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;
            var tuple = new TupleBuilder<string, IEnumerable<int>>();

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectLeft((left, context) => context.Item1 = left, tuple));
        }

        [TestMethod]
        public void SelectLeftNullLeftSelector()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, Nothing, StringBuilder>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , new Nothing()));
        }

        [TestMethod]
        public void SelectLeft()
        {
            var either = Either.Left("safd").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = either.SelectLeft(
                (left, context) => context.Item1 = new StringBuilder(left),
                tuple);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = either.SelectLeft(
                (left, context) => context.Item1 = new StringBuilder(left),
                tuple);

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectLeftLeftMapException()
        {
            var either = Either.Left("sfd").Right<IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectLeft(
                            (Func<string, Nothing, string>)((left, _) => throw invalidOperationException), 
                            new Nothing()));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());

            either.SelectLeft((Func<string, Nothing, string>)((left, _) => throw invalidOperationException), new Nothing());
        }

        [TestMethod]
        public void SelectRightNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectRight((right, context) => right, new Nothing()));
        }

        [TestMethod]
        public void SelectRightNullRightSelector()
        {
            var either = Either.Left("asdf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, Nothing, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , new Nothing()));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, Nothing, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , new Nothing()));
        }

        [TestMethod]
        public void SelectRight()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<string, object>();

            IEither<string, object> result = either.SelectRight((right, context) => context.Item2 = right.First(), tuple);

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());
            tuple = new TupleBuilder<string, object>();

            result = either.SelectRight((right, context) => context.Item2 = right.First(), tuple);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectRightRightMapException()
        {
            var either = Either.Left("asdf").Right<int>();
            var invalidOperationException = new InvalidOperationException();

            either.SelectRight((Func<int, Nothing, int>)((right, _) => throw invalidOperationException), new Nothing());

            either = Either.Left<string>().Right(42);

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectRight(
                            (Func<int, Nothing, int>)((right, _) => throw invalidOperationException), 
                            new Nothing()));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public void SelectNoContextNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .Select(left => left, right => right));
        }

        [TestMethod]
        public void SelectNoContextNullLeftSelector()
        {
            var either = Either.Left("adsf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, string>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , right => right));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, string>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , right => right));
        }

        [TestMethod]
        public void SelectNoContextNullRightSelector()
        {
            var either = Either.Left("adsf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
                left => left,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.Select(
                left => left,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectNoContext()
        {
            var either = Either.Left("safd").Right<int>();
            var tuple = new TupleBuilder<StringBuilder, int[]>();

            IEither<StringBuilder, int[]> result = either.Select(
                left => tuple.Item1 = new StringBuilder(left), 
                right => tuple.Item2 = new[] { right });

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(42);
            tuple = new TupleBuilder<StringBuilder, int[]>();

            result = either.Select(left => tuple.Item1 = new StringBuilder(left), right => tuple.Item2 = new[] { right });

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectNoContextLeftMapException()
        {
            var either = Either.Left("ASdf").Right<int>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => either.Select((Func<string, string>)(left => throw invalidOperationException), right => right));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<string>().Right(42);

            either.Select((Func<string, string>)(left => throw invalidOperationException), right => right);
        }

        [TestMethod]
        public void SelectNoContextRightMapException()
        {
            var either = Either.Left("ASdf").Right<int>();
            var invalidOperationException = new InvalidOperationException();

            either.Select(left => left, (Func<int, int>)(right => throw invalidOperationException));

            either = Either.Left<string>().Right(42);

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => either.Select(left => left, (Func<int, int>)(right => throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public void SelectLeftNoContextNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectLeft(left => left));
        }

        [TestMethod]
        public void SelectLeftNoContextNullSelector()
        {
            var either = Either.Left("dsaf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, string>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, string>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectLeftNoContext()
        {
            var either = Either.Left("safd").Right<int>();
            var tuple = new TupleBuilder<StringBuilder, int[]>();

            either.SelectLeft(left => tuple.Item1 = new StringBuilder(left));

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(42);
            tuple = new TupleBuilder<StringBuilder, int[]>();

            either.SelectLeft(left => tuple.Item1 = new StringBuilder(left));

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectLeftNoContextLeftMapException()
        {
            var either = Either.Left("sadf").Right<int>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => either.SelectLeft((Func<string, string>)(left => throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<string>().Right(42);

            either.SelectLeft((Func<string, string>)(left => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectRightNoContextNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectRight(right => right));
        }

        [TestMethod]
        public void SelectRightNoContextNullSelector()
        {
            var either = Either.Left("sadf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<int, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectRightNoContext()
        {
            var either = Either.Left("safd").Right<int>();
            var tuple = new TupleBuilder<StringBuilder, int[]>();

            IEither<string, int[]> result = either.SelectRight(right => tuple.Item2 = new[] { right });

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(42);
            tuple = new TupleBuilder<StringBuilder, int[]>();

            result = either.SelectRight(right => tuple.Item2 = new[] { right });

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public void SelectRightNoContextRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left("sadf").Right<int>();

            either.SelectRight((Func<int, int>)(right => throw invalidOperationException));

            either = Either.Left<string>().Right(42);

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => either.SelectRight((Func<int, int>)(right => throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public void SelectManyPinnedRightNullEither()
        {
            Either<(string, Either<int, Exception>), Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.s
                ;

            Assert.ThrowsException<ArgumentNullException>(() => 
#pragma warning disable CS8604 // Possible null reference argument.
               either
#pragma warning restore CS8604 // Possible null reference argument.
               .SelectMany(left => left.Item2, (left, @int) => (left.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyPinnedRightNullSelector()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));

            either = Either.Left(("asfd", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyPinnedRightNullResultSelector()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left(("asfd", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyPinnedRight()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            IEither<(string, int), Exception> result = either.SelectMany(left => left.Item2, (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual("safd", leftValue.Item1);
            Assert.AreEqual(42, leftValue.Item2);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left(("asdf", Either.Left<int>().Right((Exception)invalidOperationException))).Right<Exception>();

            result = either.SelectMany(left => left.Item2, (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightTupleValue));
            Assert.AreEqual(invalidOperationException, rightTupleValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left<(string, Either<int, Exception>)>().Right((Exception)invalidCastException);

            result = either.SelectMany(left => left.Item2, (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(invalidCastException, rightValue);
        }

        [TestMethod]
        public void SelectManyPinnedRightSelectorLeftMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectMany(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException), 
                            (left, @int) =>
                                (left.Item1, @int)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left(("sadf", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectMany(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException), 
                            (left, @int) => 
                                (left.Item1, @int)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            either.SelectMany(
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => throw invalidOperationException), 
                (left, @int) => (left.Item1, @int));
        }

        [TestMethod]
        public void SelectManyPinnedRightResultSelectorLeftMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectMany(
                            left => 
                                left.Item2, 
                            (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left(("sadf", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            either.SelectMany(
                left => left.Item2, 
                (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => throw invalidOperationException));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            either.SelectMany(
                left => left.Item2, 
                (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyPinnedRightLinqQueryComprehension()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            IEither<(string, int), Exception> result = 
                from left in either
                from @int in left.Item2
                select (left.Item1, @int);

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual("safd", leftValue.Item1);
            Assert.AreEqual(42, leftValue.Item2);
        }

        [TestMethod]
        public void SelectManyLeftNullEither()
        {
            Either<(string, Either<int, Exception>), Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.s
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
               either
#pragma warning restore CS8604 // Possible null reference argument.
               .SelectManyLeft(left => left.Item2, (left, @int) => (left.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyLeftNullSelector()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));

            either = Either.Left(("asfd", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (left, @int) => (left.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyLeftNullResultSelector()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left(("asfd", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
                left => left.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyLeft()
        {
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            IEither<(string, int), Exception> result = either.SelectManyLeft(
                left => left.Item2, 
                (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual("safd", leftValue.Item1);
            Assert.AreEqual(42, leftValue.Item2);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left(("asdf", Either.Left<int>().Right((Exception)invalidOperationException))).Right<Exception>();

            result = either.SelectManyLeft(left => left.Item2, (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightTupleValue));
            Assert.AreEqual(invalidOperationException, rightTupleValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left<(string, Either<int, Exception>)>().Right((Exception)invalidCastException);

            result = either.SelectManyLeft(left => left.Item2, (left, @int) => (left.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(invalidCastException, rightValue);
        }

        [TestMethod]
        public void SelectManyLeftSelectorLeftMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectManyLeft(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException), 
                            (left, @int) => 
                                (left.Item1, @int)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left(("sadf", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectManyLeft(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException), 
                            (left, @int) => 
                                (left.Item1, @int)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            either.SelectManyLeft(
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                    throw invalidOperationException), 
                (left, @int) => 
                    (left.Item1, @int));
        }

        [TestMethod]
        public void SelectManyLeftResultSelectorLeftMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left(("safd", Either.Left(42).Right<Exception>())).Right<Exception>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectManyLeft(
                            left => 
                                left.Item2, 
                            (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left(("sadf", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            either.SelectManyLeft(
                left => left.Item2, 
                (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => throw invalidOperationException));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            either.SelectManyLeft(
                left => left.Item2, 
                (Func<(string, Either<int, Exception>), int, (string, int)>)((left, @int) => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyLeftNoResultSelectorNullEither()
        {
            Either<(string, Either<int, Exception>), Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectManyLeft(left => left.Item2));
        }

        [TestMethod]
        public void SelectManyLeftNoResultSelectorNullResultSelector()
        {
            var either = Either.Left(("asdf", Either.Left(42).Right<Exception>())).Right<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyLeft(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyLeftNoResultSelector()
        {
            var either = Either.Left(("Asdf", Either.Left(42).Right<Exception>())).Right<Exception>();

            IEither<int, Exception> result = either.SelectManyLeft(left => left.Item2);

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual(42, leftValue);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left(("asf", Either.Left<int>().Right((Exception)invalidOperationException))).Right<Exception>();

            result = either.SelectManyLeft(left => left.Item2);

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(invalidOperationException, rightValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left<(string, Either<int, Exception>)>().Right((Exception)invalidCastException);

            result = either.SelectManyLeft(left => left.Item2);

            Assert.IsTrue(result.TryGetRight(out rightValue));
            Assert.AreEqual(invalidCastException, rightValue);
        }

        [TestMethod]
        public void SelectManyLeftNoResultSelectorLeftMapException()
        {
            var either = Either.Left(("asfd", Either.Left(42).Right<Exception>())).Right<Exception>();

            var invalidOperationException = new InvalidOperationException();
            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectManyLeft(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left(("asf", Either.Left<int>().Right(new Exception()))).Right<Exception>();

            leftMapException = Assert.ThrowsException<LeftMapException>(
                () => 
                    either
                        .SelectManyLeft(
                            (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<(string, Either<int, Exception>)>().Right(new Exception());

            either.SelectManyLeft(
                (Func<(string, Either<int, Exception>), Either<int, Exception>>)(left => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyLeftNoSelectorsNullEither()
        {
            Either<Either<string, Exception>, Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectManyLeft());
        }

        [TestMethod]
        public void SelectManyLeftNoSelectors()
        {
            var either = Either.Left(Either.Left("asdf").Right<Exception>()).Right<Exception>();

            IEither<string, Exception> result = either.SelectManyLeft();

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual("asdf", leftValue);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left(Either.Left<string>().Right((Exception)invalidOperationException)).Right<Exception>();

            result = either.SelectManyLeft();

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(invalidOperationException, rightValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left<Either<string, Exception>>().Right((Exception)invalidCastException);

            result = either.SelectManyLeft();

            Assert.IsTrue(result.TryGetRight(out rightValue));
            Assert.AreEqual(invalidCastException, rightValue);
        }

        [TestMethod]
        public void SelectManyPinnedLeftNullEither()
        {
            Either<Exception, (string, Either<Exception, int>)> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.s
                ;

            Assert.ThrowsException<ArgumentNullException>(() => 
#pragma warning disable CS8604 // Possible null reference argument.
               either
#pragma warning restore CS8604 // Possible null reference argument.
               .SelectMany(right => right.Item2, (right, @int) => (right.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyPinnedLeftNullSelector()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));

            either = Either.Left<Exception>().Right(("asfd", Either.Left(new Exception()).Right<int>()));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyPinnedLeftNullResultSelector()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<Exception>().Right(("asfd", Either.Left(new Exception()).Right<int>()));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectMany(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyPinnedLeft()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            IEither<Exception, (string, int)> result = either.SelectMany(
                right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual("safd", rightValue.Item1);
            Assert.AreEqual(42, rightValue.Item2);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left<Exception>().Right(("asdf", Either.Left((Exception)invalidOperationException).Right<int>()));

            result = either.SelectMany(right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftTupleValue));
            Assert.AreEqual(invalidOperationException, leftTupleValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left((Exception)invalidCastException).Right<(string, Either<Exception, int>)>();

            result = either.SelectMany(right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual(invalidCastException, leftValue);
        }

        [TestMethod]
        public void SelectManyPinnedLeftSelectorRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectMany(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException), 
                            (right, @int) => 
                                (right.Item1, @int)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left<Exception>().Right(("sadf", Either.Left(new Exception()).Right<int>()));

            rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectMany(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException), 
                            (right, @int) => 
                                (right.Item1, @int)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            either.SelectMany(
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => throw invalidOperationException), 
                (right, @int) => (right.Item1, @int));
        }

        [TestMethod]
        public void SelectManyPinnedLeftResultSelectorRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectMany(
                            right => 
                                right.Item2, 
                            (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left<Exception>().Right(("sadf", Either.Left(new Exception()).Right<int>()));

            either.SelectMany(
                right => right.Item2, 
                (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => throw invalidOperationException));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            either.SelectMany(
                right => right.Item2, 
                (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyPinnedLeftLinqQueryComprehension()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            IEither<Exception, (string, int)> result = 
                from right in either
                from @int in right.Item2
                select (right.Item1, @int);

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual("safd", rightValue.Item1);
            Assert.AreEqual(42, rightValue.Item2);
        }

        [TestMethod]
        public void SelectManyRightNullEither()
        {
            Either<Exception, (string, Either<Exception, int>)> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.s
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
               either
#pragma warning restore CS8604 // Possible null reference argument.
               .SelectManyRight(right => right.Item2, (right, @int) => (right.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyRightNullSelector()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));

            either = Either.Left<Exception>().Right(("asfd", Either.Left(new Exception()).Right<int>()));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                , (right, @int) => (right.Item1, @int)));
        }

        [TestMethod]
        public void SelectManyRightNullResultSelector()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left<Exception>().Right(("asfd", Either.Left(new Exception()).Right<int>()));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
                right => right.Item2,
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), int, (string, int)>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyRight()
        {
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            IEither<Exception, (string, int)> result = either.SelectManyRight(
                right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual("safd", rightValue.Item1);
            Assert.AreEqual(42, rightValue.Item2);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left<Exception>().Right(("asdf", Either.Left((Exception)invalidOperationException).Right<int>()));

            result = either.SelectManyRight(right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftTupleValue));
            Assert.AreEqual(invalidOperationException, leftTupleValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left((Exception)invalidCastException).Right<(string, Either<Exception, int>)>();

            result = either.SelectManyRight(right => right.Item2, (right, @int) => (right.Item1, @int));

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual(invalidCastException, leftValue);
        }

        [TestMethod]
        public void SelectManyRightSelectorRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectManyRight(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException), 
                            (right, @int) =>
                            (right.Item1, @int)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left<Exception>().Right(("sadf", Either.Left(new Exception()).Right<int>()));

            rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectManyRight(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException), 
                            (right, @int) => 
                                (right.Item1, @int)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            either.SelectManyRight(
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => throw invalidOperationException),
                (right, @int) => (right.Item1, @int));
        }

        [TestMethod]
        public void SelectManyRightResultSelectorRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left<Exception>().Right(("safd", Either.Left<Exception>().Right(42)));

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectManyRight(
                            right => 
                                right.Item2, 
                            (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left<Exception>().Right(("sadf", Either.Left(new Exception()).Right<int>()));

            either.SelectManyRight(
                right => right.Item2, 
                (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => throw invalidOperationException));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            either.SelectManyRight(
                right => right.Item2, 
                (Func<(string, Either<Exception, int>), int, (string, int)>)((right, @int) => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyRightNoResultSelectorNullEither()
        {
            Either<Exception, (string, Either<Exception, int>)> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectManyRight(right => right.Item2));
        }

        [TestMethod]
        public void SelectManyRightNoResultSelectorNullResultSelector()
        {
            var either = Either.Left<Exception>().Right(("asdf", Either.Left<Exception>().Right(42)));

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            Assert.ThrowsException<ArgumentNullException>(() => either.SelectManyRight(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectManyRightNoResultSelector()
        {
            var either = Either.Left<Exception>().Right(("Asdf", Either.Left<Exception>().Right(42)));

            IEither<Exception, int> result = either.SelectManyRight(right => right.Item2);

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(42, rightValue);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left<Exception>().Right(("asf", Either.Left((Exception)invalidOperationException).Right<int>()));

            result = either.SelectManyRight(right => right.Item2);

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual(invalidOperationException, leftValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left((Exception)invalidCastException).Right<(string, Either<Exception, int>)>();

            result = either.SelectManyRight(right => right.Item2);

            Assert.IsTrue(result.TryGetLeft(out leftValue));
            Assert.AreEqual(invalidCastException, leftValue);
        }

        [TestMethod]
        public void SelectManyRightNoResultSelectorRightMapException()
        {
            var either = Either.Left<Exception>().Right(("asfd", Either.Left<Exception>().Right(42)));

            var invalidOperationException = new InvalidOperationException();
            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectManyRight(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left<Exception>().Right(("asf", Either.Left(new Exception()).Right<int>()));

            rightMapException = Assert.ThrowsException<RightMapException>(
                () => 
                    either
                        .SelectManyRight(
                            (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => 
                                throw invalidOperationException)));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);

            either = Either.Left(new Exception()).Right<(string, Either<Exception, int>)>();

            either.SelectManyRight(
                (Func<(string, Either<Exception, int>), Either<Exception, int>>)(right => throw invalidOperationException));
        }

        [TestMethod]
        public void SelectManyRightNoSelectorsNullEither()
        {
            Either<Exception, Either<Exception, string>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectManyRight());
        }

        [TestMethod]
        public void SelectManyRightNoSelectors()
        {
            var either = Either.Left<Exception>().Right(Either.Left<Exception>().Right("asdf"));

            IEither<Exception, string> result = either.SelectManyRight();

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual("asdf", rightValue);

            var invalidOperationException = new InvalidOperationException();
            either = Either.Left<Exception>().Right(Either.Left((Exception)invalidOperationException).Right<string>());

            result = either.SelectManyRight();

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual(invalidOperationException, leftValue);

            var invalidCastException = new InvalidCastException();
            either = Either.Left((Exception)invalidCastException).Right<Either<Exception, string>>();

            result = either.SelectManyRight();

            Assert.IsTrue(result.TryGetLeft(out leftValue));
            Assert.AreEqual(invalidCastException, leftValue);
        }

        [TestMethod]
        public void TryGetLeftNullEither()
        {
            Either<string, Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .TryGetLeft(out var left));
        }

        [TestMethod]
        public void TryGetLeftLeftValue()
        {
            var value = "asdf";
            var either = Either.Left(value).Right<Exception>();

            Assert.IsTrue(either.TryGetLeft(out var left));
            Assert.AreEqual(value, left);
        }

        [TestMethod]
        public void TryGetLeftRightValue()
        {
            var value = new Exception();
            var either = Either.Left<string>().Right(value);

            Assert.IsFalse(either.TryGetLeft(out var left));
            Assert.IsNull(left);
        }

        [TestMethod]
        public void TryGetRightNullEither()
        {
            Either<string, Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .TryGetRight(out var right));
        }

        [TestMethod]
        public void TryGetRightLeftValue()
        {
            var value = "asdf";
            var either = Either.Left(value).Right<Exception>();

            Assert.IsFalse(either.TryGetRight(out var right));
            Assert.IsNull(right);
        }

        [TestMethod]
        public void TryGetRightRightValue()
        {
            var value = new Exception();
            var either = Either.Left<string>().Right(value);

            Assert.IsTrue(either.TryGetRight(out var right));
            Assert.AreEqual(value, right);
        }

        [TestMethod]
        public void TryGetLeftSideNullEither()
        {
            Either<string, Nothing> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .TryGet(out var left));
        }

        [TestMethod]
        public void TryGetLeftSide()
        {
            var value = "Asdf";
            var either = Either.Left(value).Right<Nothing>();

            Assert.IsTrue(either.TryGet(out var left));
            Assert.AreEqual(value, left);

            either = Either.Left<string>().Right(new Nothing());

            Assert.IsFalse(either.TryGet(out left));
            Assert.IsNull(left);
        }

        [TestMethod]
        public void TryGetRightSideNullEither()
        {
            Either<Nothing, string> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .TryGet(out var right));
        }

        [TestMethod]
        public void TryGetRightSide()
        {
            var value = "ASdf";
            var either = Either.Left(new Nothing()).Right<string>();

            Assert.IsFalse(either.TryGet(out var right));
            Assert.IsNull(right);

            either = Either.Left<Nothing>().Right(value);

            Assert.IsTrue(either.TryGet(out right));
            Assert.AreEqual(value, right);
        }

        [TestMethod]
        public void CoalesceRightNullEither()
        {
            Either<int, string> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .CoalesceRight(int.Parse));
        }

        [TestMethod]
        public void CoalesceRightNullCoalescer()
        {
            var either = Either.Left(42).Right<string>();

            Assert.ThrowsException<ArgumentNullException>(() => either.CoalesceRight(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));

            either = Either.Left<int>().Right("asdf");

            Assert.ThrowsException<ArgumentNullException>(() => either.CoalesceRight(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void CoalesceRight()
        {
            var either = Either.Left(42).Right<string>();

            var result = either.CoalesceRight(int.Parse);

            Assert.AreEqual(42, result);

            either = Either.Left<int>().Right("13");

            result = either.CoalesceRight(int.Parse);

            Assert.AreEqual(13, result);
        }

        [TestMethod]
        public void CoalesceRightRightMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left(42).Right<string>();

            var result = either.CoalesceRight(right => throw invalidOperationException);

            Assert.AreEqual(42, result);

            either = Either.Left<int>().Right("asdf");

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => either.CoalesceRight(right => throw invalidOperationException));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public void CoalesceLeftNullEither()
        {
            Either<string, int> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .CoalesceLeft(int.Parse));
        }

        [TestMethod]
        public void CoalesceLeftNullCoalescer()
        {
            var either = Either.Left<string>().Right(42);

            Assert.ThrowsException<ArgumentNullException>(() => either.CoalesceLeft(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));

            either = Either.Left("asdf").Right<int>();

            Assert.ThrowsException<ArgumentNullException>(() => either.CoalesceLeft(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void CoalesceLeft()
        {
            var either = Either.Left<string>().Right(42);

            var result = either.CoalesceLeft(int.Parse);

            Assert.AreEqual(42, result);

            either = Either.Left("13").Right<int>();

            result = either.CoalesceLeft(int.Parse);

            Assert.AreEqual(13, result);
        }

        [TestMethod]
        public void CoalesceLeftLeftMapException()
        {
            var invalidOperationException = new InvalidOperationException();
            var either = Either.Left<string>().Right(42);

            var result = either.CoalesceLeft(left => throw invalidOperationException);

            Assert.AreEqual(42, result);

            either = Either.Left("asdf").Right<int>();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => either.CoalesceLeft(right => throw invalidOperationException));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);
        }

        [TestMethod]
        public void CoalesceNullEither()
        {
            Either<string, string> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .Coalesce());
        }

        [TestMethod]
        public void Coalesce()
        {
            var value1 = "Asfd";
            var either = Either.Left(value1).Right<string>();

            var result = either.Coalesce();

            Assert.AreEqual(value1, result);

            var value2 = "qwer";
            either = Either.Left<string>().Right(value2);

            result = either.Coalesce();

            Assert.AreEqual(value2, result);
        }

        [TestMethod]
        public void CoalesceWithLeftDefaultNullEither()
        {
            Either<string, Nothing> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .Coalesce("asfd"));
        }

        [TestMethod]
        public void CoalesceWithLeftDefault()
        {
            var value = "asdf";
            var @default = "Default";
            var either = Either.Left(value).Right<Nothing>();

            var result = either.Coalesce(@default);

            Assert.AreEqual(value, result);

            either = Either.Left<string>().Right(new Nothing());

            result = either.Coalesce(@default);

            Assert.AreEqual(@default, result);
        }

        [TestMethod]
        public void CoalesceWithRightDefaultNullEither()
        {
            Either<Nothing, string> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .Coalesce("ASFD"));
        }

        [TestMethod]
        public void CoalesceWithRightDefault()
        {
            var value = "Asdf";
            var @default = "default";
            var either = Either.Left<Nothing>().Right(value);

            var result = either.Coalesce(@default);

            Assert.AreEqual(value, result);

            either = Either.Left(new Nothing()).Right<string>();

            result = either.Coalesce(@default);

            Assert.AreEqual(@default, result);
        }

        [TestMethod]
        public void ThrowRightNullEither()
        {
            Either<string, Exception> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .ThrowRight());
        }

        [TestMethod]
        public void ThrowRight()
        {
            var value = "Asf";
            var exception = new Exception();
            var either = Either.Left(value).Right<Exception>();

            var resultValue = either.ThrowRight();

            Assert.AreEqual(value, resultValue);

            either = Either.Left<string>().Right(exception);

            var resultException = Assert.ThrowsException<Exception>(() => either.ThrowRight());

            Assert.AreEqual(exception, resultException);
        }

        [TestMethod]
        public void ThrowLeftNullEither()
        {
            Either<Exception, string> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .ThrowLeft());
        }

        [TestMethod]
        public void ThrowLeft()
        {
            var value = "asdf";
            var exception = new Exception();
            var either = Either.Left<Exception>().Right(value);

            var resultValue = either.ThrowLeft();

            Assert.AreEqual(value, resultValue);

            either = Either.Left(exception).Right<string>();

            var resultException = Assert.ThrowsException<Exception>(() => either.ThrowLeft());

            Assert.AreEqual(exception, resultException);
        }
    }
}
