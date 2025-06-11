/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed partial class EitherExtensionsUnitTests
    {
        [TestMethod]
        public async Task SelectFutureEitherNullEither()
        {
            ITask<IEither<string, int>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await
#pragma warning disable CS8604 // Possible null reference argument.
                        either
#pragma warning restore CS8604 // Possible null reference argument.
                            .Select(
                                (left, context) => left,
                                (right, context) => right,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNullLeftSelector()
        {
            var either = CreateAsyncLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Nothing, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                (right, context) => right,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateAsyncRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Nothing, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                (right, context) => right,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNullRightSelector()
        {
            var either = CreateAsyncLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
                                (left, context) => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Nothing, IEnumerable<int>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateAsyncRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
                                (left, context) => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Nothing, IEnumerable<int>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEither()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .Select(
                    (left, context) =>
                        context.Item1 = new StringBuilder(left),
                    (right, context) => context.Item2 = right.Select(val => val * 2),
                    tuple)
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .Select(
                    (left, context) => context.Item1 = new StringBuilder(left),
                    (right, context) => context.Item2 = right.Select(val => val * 2),
                    tuple)
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectFutureEitherLeftMapException()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .Select(
                                (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, string>)((left, context) =>
                                    throw invalidOperationException),
                                (right, context) => context.Item2 = right,
                                tuple)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateAsyncRight();

            await either
                .Select(
                    (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, string>)((left, context) =>
                        throw invalidOperationException),
                    (right, context) => context.Item2 = right,
                    tuple)
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherRightMapException()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            await either
                .Select(
                    (left, context) => context.Item1 = new StringBuilder(left),
                    (Func<IEnumerable<int>, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<int>>)((right, context) =>
                        throw invalidOperationException),
                    tuple)
                .ConfigureAwait(false);

            either = CreateAsyncRight();
            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    async () => await
                        either
                            .Select(
                                (left, context) => context.Item1 = new StringBuilder(left),
                                (
                                    Func
                                        <
                                            IEnumerable<int>,
                                            TupleBuilder<StringBuilder, IEnumerable<int>>,
                                            int
                                        >
                                )((right, context) =>
                                    throw invalidOperationException),
                                tuple)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextNullEither()
        {
            ITask<IEither<string, int>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await
#pragma warning disable CS8604 // Possible null reference argument.
                        either
#pragma warning restore CS8604 // Possible null reference argument.
                            .Select(
                                left => left,
                                right => right)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextNullLeftSelector()
        {
            var either = CreateAsyncLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                right => right)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateAsyncRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                right => right)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextNullRightSelector()
        {
            var either = CreateAsyncLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
                                left => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, IEnumerable<int>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateAsyncRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .Select(
                                left => left,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, IEnumerable<int>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContext()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .Select(
                    left => tuple.Item1 = new StringBuilder(left),
                    right => tuple.Item2 = right.Select(val => val * 2))
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .Select(
                    left => tuple.Item1 = new StringBuilder(left),
                    right => tuple.Item2 = right.Select(val => val * 2))
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextLeftMapException()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .Select(
                                (Func<string, Task<string>>)(left =>
                                    throw invalidOperationException),
                                right =>  right)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either
                .Select(
                    (Func<string, Task<string>>)(left =>
                        throw invalidOperationException),
                    right => tuple.Item2 = right)
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextRightMapException()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            await either
                .Select(
                    left => tuple.Item1 = new StringBuilder(left),
                    (Func<IEnumerable<int>, Task<int>>)(right =>
                        throw invalidOperationException))
                .ConfigureAwait(false);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    async () =>
                        await either
                            .Select(
                                left => tuple.Item1 = new StringBuilder(left),
                                (Func<IEnumerable<int>, Task<int>>)(right =>
                                    throw invalidOperationException))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }
        [TestMethod]
        public async Task SelectLeftFutureEitherNoContextNullEither()
        {
            ITask<IEither<string, int>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await
#pragma warning disable CS8604 // Possible null reference argument.
                        either
#pragma warning restore CS8604 // Possible null reference argument.
                            .SelectLeft(
                                left => left)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectLeftFutureEitherNoContextNullLeftSelector()
        {
            var either = CreateAsyncLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectLeft(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateAsyncRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectLeft(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectLeftFutureEitherNoContext()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .SelectLeft(
                    left => tuple.Item1 = new StringBuilder(left))
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .SelectLeft(
                    left => tuple.Item1 = new StringBuilder(left))
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectLeftFutureEitherNoContextLeftMapException()
        {
            var either = CreateAsyncLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .SelectLeft(
                                (Func<string, string>)(left =>
                                    throw invalidOperationException))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateAsyncRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either
                .SelectLeft(
                    (Func<string, string>)(left =>
                        throw invalidOperationException))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectManyLeftFutureEitherNoSelectorsNullEither()
        {
            ITask<Either<Either<string, Exception>, Exception>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await
#pragma warning disable CS8604 // Possible null reference argument.
                        either
#pragma warning restore CS8604 // Possible null reference argument.
                            .SelectManyLeft()
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectManyLeftFutureEitherNoSelectors()
        {
            var either = new TaskWrapper<Either<Either<string, Exception>, Exception>>(Task.FromResult(Either.Left(Either.Left("asdf").Right<Exception>()).Right<Exception>()));

            IEither<string, Exception> result = await either.SelectManyLeft().ConfigureAwait(false);

            Assert.IsTrue(result.TryGetLeft(out var leftValue));
            Assert.AreEqual("asdf", leftValue);

            var invalidOperationException = new InvalidOperationException();
            either = new TaskWrapper<Either<Either<string, Exception>, Exception>>(Task.FromResult(Either.Left(Either.Left<string>().Right((Exception)invalidOperationException)).Right<Exception>()));

            result = await either.SelectManyLeft().ConfigureAwait(false);

            Assert.IsTrue(result.TryGetRight(out var rightValue));
            Assert.AreEqual(invalidOperationException, rightValue);

            var invalidCastException = new InvalidCastException();
            either = new TaskWrapper<Either<Either<string, Exception>, Exception>>(Task.FromResult(Either.Left<Either<string, Exception>>().Right((Exception)invalidCastException)));

            result = await either.SelectManyLeft().ConfigureAwait(false);

            Assert.IsTrue(result.TryGetRight(out rightValue));
            Assert.AreEqual(invalidCastException, rightValue);
        }

        [TestMethod]
        public async Task SelectManyRightFutureEitherNoSelectorsNullEither()
        {
            ITask<Either<Exception, Either<Exception, string>>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await 
#pragma warning disable CS8604 // Possible null reference argument.
                        either
#pragma warning restore CS8604 // Possible null reference argument.
                        .SelectManyRight()
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public void SelectManyRightFutureEitherNoSelectors()
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
    }
}
