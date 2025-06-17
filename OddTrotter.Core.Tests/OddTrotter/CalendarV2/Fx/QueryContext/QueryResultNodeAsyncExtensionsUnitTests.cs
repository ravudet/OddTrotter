/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed partial class QueryResultNodeAsyncExtensionsUnitTests
    {
        [TestMethod]
        public async Task SelectNullSource()
        {
            IQueryResultNodeAsync<string, Exception> node =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await
#pragma warning disable CS8604 // Possible null reference argument.
                    node
#pragma warning restore CS8604 // Possible null reference argument.
                        .Select(val => val.Length)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectNullSelector()
        {
            var value = "asdf";
            var node = Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNodeAsync();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await
                        node
                            .Select(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                (Func<string, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                                )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectNoElements()
        {
            var node = 
                Either
                    .Left<MockElementAsync>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(
                                MockEmpty.Instance))
                    .ToQueryResultNodeAsync();

            var result = await node.Select(val => val.Length).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task SelectNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var node =
                Either
                    .Left<MockElementAsync>()
                    .Right(
                        Either
                            .Left(
                                new MockError(invalidOperationException))
                            .Right<MockEmpty>())
                    .ToQueryResultNodeAsync();

            var result = await node.Select(val => val.Length).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task SelectNoError()
        {
            var value = "asdf";
            var node = Either.Left(new MockElementAsync(value)).Right<IEither<MockError, MockEmpty>>().ToQueryResultNodeAsync();

            var result = await node.Select(val => val.Length).ConfigureAwait(false);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));
        }

        [TestMethod]
        public async Task SelectElementFollowedByError()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node =
                Either
                    .Left(
                        new MockElementAsync(
                            value,
                            Either
                                .Left<MockElementAsync>()
                                .Right(
                                    Either
                                        .Left(
                                            new MockError(invalidOperationException))
                                        .Right<MockEmpty>())
                                .ToQueryResultNodeAsync()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNodeAsync();

            var result = await node.Select(val => val.Length).ConfigureAwait(false);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));
        }
    }
}
