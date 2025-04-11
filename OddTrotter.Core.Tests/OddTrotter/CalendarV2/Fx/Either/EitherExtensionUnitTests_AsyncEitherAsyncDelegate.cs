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
            var either = Either.Left("asdf").Right<int>();

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

            either = Either.Left<string>().Right(42);

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
            var either = Either.Left("asdf").Right<int>();

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
                                (Func<int, Nothing, Task<int>>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                ,
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);

            either = Either.Left<string>().Right(42);

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
                                (Func<int, Nothing, Task<int>>)null
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
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            IEither<StringBuilder, IEnumerable<int>> result = await either.SelectAsync(
                async (left, context) => await Task.FromResult(context.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                async (right, context) => await Task.FromResult(context.Item2 = right.Select(val => val * 2)).ConfigureAwait(false), tuple).ConfigureAwait(false);

            Assert.IsNotNull(tuple.Item1);
            Assert.IsNull(tuple.Item2);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());
            tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();

            result = await either.SelectAsync(
                async (left, context) => await Task.FromResult(context.Item1 = new StringBuilder(left)).ConfigureAwait(false),
                async (right, context) => await Task.FromResult(context.Item2 = right.Select(val => val * 2)).ConfigureAwait(false), tuple).ConfigureAwait(false);

            Assert.IsNull(tuple.Item1);
            Assert.IsNotNull(tuple.Item2);
        }

        [TestMethod]
        public async Task SelectAsyncLeftMapException()
        {
            var either = Either.Left("asdf").Right<IEnumerable<int>>();
            var tuple = new TupleBuilder<StringBuilder, IEnumerable<int>>();
            var invalidOperationException = new InvalidOperationException();

            var leftMapException = await Assert.ThrowsExceptionAsync<LeftMapException>(
                async () =>
                    await either
                        .SelectAsync(
                            (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<string>>)((left, context) =>
                                throw invalidOperationException),
                            async (right, context) =>
                                context.Item2 = await Task.FromResult(right).ConfigureAwait(false),
                            tuple).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            either = Either.Left<string>().Right(new[] { 42 }.AsEnumerable());

            await either.SelectAsync(
                (Func<string, TupleBuilder<StringBuilder, IEnumerable<int>>, Task<string>>)((left, context) =>
                    throw invalidOperationException),
                async (right, context) =>
                    await Task.FromResult(context.Item2 = right).ConfigureAwait(false),
                tuple)
                .ConfigureAwait(false);
        }
    }
}
