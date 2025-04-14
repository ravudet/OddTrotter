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
            Task<IEither<string, int>> either =
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
            var either = CreateLeft();

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

            either = CreateRight();

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
            var either = CreateLeft();

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

            either = CreateRight();

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
            var either = CreateLeft();
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

            either = CreateRight();
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
            var either = CreateLeft();
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

            either = CreateRight();

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
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            await either
                .Select(
                    (left, context) => context.Item1 = new StringBuilder(left),
                    (Func<IEnumerable<int>, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<int>>)((right, context) =>
                        throw invalidOperationException),
                    tuple)
                .ConfigureAwait(false);

            either = CreateRight();
            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    () =>
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
                                tuple))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public async Task SelectFutureEitherNoContextNullEither()
        {
            Task<IEither<string, int>> either =
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
            var either = CreateLeft();

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

            either = CreateRight();

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
            var either = CreateLeft();

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

            either = CreateRight();

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
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .Select(
                    left => tuple.Item1 = new StringBuilder(left),
                    right => tuple.Item2 = right.Select(val => val * 2))
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateRight();
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
            var either = CreateLeft();
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

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either
                .Select(
                    (Func<string, Task<string>>)(left =>
                        throw invalidOperationException),
                    right => tuple.Item2 = right)
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncFutureEitherNoContextRightMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            await either
                .SelectAsync(
                    async left =>
                        await Task.FromResult(tuple.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                    (Func<IEnumerable<int>, Task<int>>)(right =>
                        throw invalidOperationException))
                .ConfigureAwait(false);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    () =>
                        either
                            .SelectAsync(
                                async left =>
                                    await Task.FromResult(tuple.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                                (Func<IEnumerable<int>, Task<int>>)(right =>
                                    throw invalidOperationException)))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }
        [TestMethod]
        public async Task SelectLeftAsyncFutureEitherNoContextNullEither()
        {
            Task<IEither<string, int>> either =
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
                            .SelectLeftAsync(
                                async left => await Task.FromResult(left).ConfigureAwait(false))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectLeftAsyncFutureEitherNoContextNullLeftSelector()
        {
            var either = CreateLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectLeftAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectLeftAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectLeftAsyncFutureEitherNoContext()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .SelectLeftAsync(
                    async left => await Task
                        .FromResult(tuple.Item1 = new StringBuilder(left))
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .SelectLeftAsync(
                    async left =>
                        await Task
                            .FromResult(tuple.Item1 = new StringBuilder(left))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectLeftAsyncFutureEitherNoContextLeftMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert.ThrowsExceptionAsync<LeftMapException>(
                async () =>
                    await either
                        .SelectLeftAsync(
                            (Func<string, Task<string>>)(left =>
                                throw invalidOperationException)).ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either.SelectLeftAsync(
                (Func<string, Task<string>>)(left =>
                    throw invalidOperationException))
                .ConfigureAwait(false);
        }
    }
}
