/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Try;

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
            var node = 
                Either
                    .Left(
                        new MockElementAsync(value))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNodeAsync();

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

        [TestMethod]
        public async Task TrySelectNullSource()
        {
            IQueryResultNodeAsync<string, Exception> source =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await
#pragma warning disable CS8604 // Possible null reference argument.
                        source
#pragma warning restore CS8604 // Possible null reference argument.
                            .TrySelect(IntTryParse)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        private static Try<string, int> IntTryParse { get; } = int.TryParse;

        [TestMethod]
        public async Task TrySelectNullTry()
        {
            var source = await
                new[] { "asdf", "42", "67", "qwer" }
                    .ToQueryResultAsync()
                    .WithoutError<Exception>()
                    .GetNodes()
                    .ConfigureAwait(false);

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await 
                        source
                            .TrySelect(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                (Try<string, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TrySelectWithoutError()
        {
            var source = await
                new[] { "asdf", "42", "67", "qwer" }
                    .ToQueryResultAsync()
                    .WithoutError<Exception>()
                    .GetNodes()
                    .ConfigureAwait(false);

            var selected = await source.TrySelect(IntTryParse).ConfigureAwait(false);

            Assert.IsTrue(selected.TryGetLeft(out var element));
            Assert.AreEqual(42, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(67, nextElement.Value);
            var nextNext = await nextElement.Next().ConfigureAwait(false);
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task TrySelectWithError()
        {
            var invalidOperationException = new InvalidOperationException();
            var source = await
                new[] { "asdf", "42", "67", "qwer" }
                    .ToQueryResultAsync()
                    .WithError(invalidOperationException)
                    .GetNodes()
                    .ConfigureAwait(false);

            var selected = await source.TrySelect(IntTryParse).ConfigureAwait(false);

            Assert.IsTrue(selected.TryGetLeft(out var element));
            Assert.AreEqual(42, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(67, nextElement.Value);
            var nextNext = await nextElement.Next().ConfigureAwait(false);
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
        }

        [TestMethod]
        public async Task WhereNullSource()
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
                            .Where(val => val.Length % 2 == 0)
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task WhereNullPredicate()
        {
            var value = "asdf";
            var node = Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNodeAsync();

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await 
                        node
                            .Where(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            )
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task WhereNoElements()
        {
            var node = Either.Left<MockElementAsync>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)).ToQueryResultNodeAsync();

            var result = await node.Where(_ => true).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task WhereNoElementsError()
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

            var result = await node.Where(_ => true).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task WhereNoError()
        {
            var value = "asdf";
            var node = Either.Left(new MockElementAsync(value)).Right<IEither<MockError, MockEmpty>>().ToQueryResultNodeAsync();

            var result = await node.Where(_ => true).ConfigureAwait(false);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));

            result = await node.Where(_ => false).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out element));
            Assert.IsTrue(result.TryGetRight(out var terminal2));
            Assert.IsFalse(terminal2.TryGetLeft(out var error));
            Assert.IsTrue(terminal2.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task WhereElementFollowedByError()
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

            var result = await node.Where(_ => true).ConfigureAwait(false);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = await element.Next().ConfigureAwait(false);
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));

            result = await node.Where(_ => false).ConfigureAwait(false);

            Assert.IsFalse(result.TryGetLeft(out element));
            Assert.IsTrue(result.TryGetRight(out var terminal2));
            Assert.IsTrue(terminal2.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal2.TryGetRight(out var empty));
        }

    }
}
