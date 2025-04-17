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
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        private static IEither<string, IEnumerable<int>> CreateLeft()
        {
            return Either.Left("asdf").Right<IEnumerable<int>>();
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        private static IEither<string, IEnumerable<int>> CreateRight()
        {
            return Either.Left<string>().Right(new[] { 42 });
        }

        [TestMethod]
        public async Task SelectAsyncNullEither()
        {
            IEither<string, int> either =
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
                            .SelectAsync(
                                async (left, context) => await Task.FromResult(left).ConfigureAwait(false),
                                async (right, context) => await Task.FromResult(right).ConfigureAwait(false),
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNullLeftSelector()
        {
            var either = CreateLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Nothing, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                async (right, context) =>
                                    await Task
                                        .FromResult(right)
                                        .ConfigureAwait(false),
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Nothing, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                async (right, context) =>
                                    await Task
                                        .FromResult(right)
                                        .ConfigureAwait(false),
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNullRightSelector()
        {
            var either = CreateLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
                                async (left, context) =>
                                    await Task
                                        .FromResult(left)
                                        .ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Nothing, Task<IEnumerable<int>>>)null
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
                            .SelectAsync(
                                async (left, context) =>
                                    await Task
                                        .FromResult(left)
                                        .ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Nothing, Task<IEnumerable<int>>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsync()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .SelectAsync(
                    async (left, context) =>
                        await Task
                            .FromResult(
                                context.Item1 = new StringBuilder(left))
                            .ConfigureAwait(false),
                    async (right, context) =>
                        await Task
                            .FromResult(
                                context.Item2 = right.Select(val => val * 2))
                            .ConfigureAwait(false),
                    tuple)
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .SelectAsync(
                    async (left, context) =>
                        await Task
                            .FromResult(
                                context.Item1 = new StringBuilder(left))
                            .ConfigureAwait(false),
                    async (right, context) =>
                        await Task
                            .FromResult(
                                context.Item2 = right.Select(val => val * 2))
                            .ConfigureAwait(false),
                    tuple)
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectAsyncLeftMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .SelectAsync(
                                (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<string>>)((left, context) =>
                                    throw invalidOperationException),
                                async (right, context) =>
                                    context.Item2 = await Task.FromResult(right).ConfigureAwait(false),
                                tuple)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateRight();

            await either.SelectAsync(
                (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<string>>)((left, context) =>
                    throw invalidOperationException),
                async (right, context) =>
                    await Task.FromResult(context.Item2 = right).ConfigureAwait(false),
                tuple)
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncRightMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            await either
                .SelectAsync(
                    async (left, context) =>
                        await Task.FromResult(context.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                    (Func<IEnumerable<int>, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<int>>)((right, context) =>
                        throw invalidOperationException),
                    tuple)
                .ConfigureAwait(false);

            either = CreateRight();
            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    () =>
                        either
                            .SelectAsync(
                                async (left, context) =>
                                    await Task.FromResult(context.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                                (
                                    Func
                                        <
                                            IEnumerable<int>,
                                            TupleBuilder<StringBuilder, IEnumerable<int>>,
                                            Task<int>
                                        >
                                )((right, context) =>
                                    throw invalidOperationException),
                                tuple))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public async Task SelectAsyncNoContextNullEither()
        {
            IEither<string, int> either =
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
                            .SelectAsync(
                                async left => await Task.FromResult(left).ConfigureAwait(false),
                                async right => await Task.FromResult(right).ConfigureAwait(false))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNoContextNullLeftSelector()
        {
            var either = CreateLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                async right =>
                                    await Task
                                        .FromResult(right)
                                        .ConfigureAwait(false))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = CreateRight();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<string, Task<string>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                async right =>
                                    await Task
                                        .FromResult(right)
                                        .ConfigureAwait(false))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNoContextNullRightSelector()
        {
            var either = CreateLeft();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () =>
                        await either
                            .SelectAsync(
                                async left =>
                                    await Task
                                        .FromResult(left)
                                        .ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Task<IEnumerable<int>>>)null
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
                            .SelectAsync(
                                async left =>
                                    await Task
                                        .FromResult(left)
                                        .ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Func<IEnumerable<int>, Task<IEnumerable<int>>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNoContext()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either
                .SelectAsync(
                    async left =>
                        await Task
                            .FromResult(
                                tuple.Item1 = new StringBuilder(left))
                            .ConfigureAwait(false),
                    async right =>
                        await Task
                            .FromResult(
                                tuple.Item2 = right.Select(val => val * 2))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either
                .SelectAsync(
                    async left =>
                        await Task
                            .FromResult(
                                tuple.Item1 = new StringBuilder(left))
                            .ConfigureAwait(false),
                    async right =>
                        await Task
                            .FromResult(
                                tuple.Item2 = right.Select(val => val * 2))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectAsyncNoContextLeftMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .SelectAsync(
                                (Func<string, Task<string>>)(left =>
                                    throw invalidOperationException),
                                async right =>
                                    tuple.Item2 = await Task
                                        .FromResult(right)
                                        .ConfigureAwait(false))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either
                .SelectAsync(
                    (Func<string, Task<string>>)(left =>
                        throw invalidOperationException),
                    async right =>
                        await Task.FromResult(tuple.Item2 = right).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectAsyncNoContextRightMapException()
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
                    async () =>
                        await either
                            .SelectAsync(
                                async left =>
                                    await Task.FromResult(tuple.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                                (Func<IEnumerable<int>, Task<int>>)(right =>
                                    throw invalidOperationException))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }
        [TestMethod]
        public async Task SelectLeftAsyncNoContextNullEither()
        {
            IEither<string, int> either =
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
        public async Task SelectLeftAsyncNoContextNullLeftSelector()
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
        public async Task SelectLeftAsyncNoContext()
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
        public async Task SelectLeftAsyncNoContextLeftMapException()
        {
            var either = CreateLeft();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () =>
                        await either
                            .SelectLeftAsync(
                                (Func<string, Task<string>>)(left =>
                                    throw invalidOperationException))
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = CreateRight();
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            await either
                .SelectLeftAsync(
                    (Func<string, Task<string>>)(left =>
                        throw invalidOperationException))
                .ConfigureAwait(false);
        }
    }
}
