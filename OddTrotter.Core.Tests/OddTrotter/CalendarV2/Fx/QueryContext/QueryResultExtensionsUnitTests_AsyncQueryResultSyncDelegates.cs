/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Try;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed partial class QueryResultExtensionsUnitTests
    {
        [TestMethod]
        public async Task SelectFutureQueryResultNullSource()
        {
            Task<IQueryResult<string, Exception>> queryResult =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await 
#pragma warning disable CS8604 // Possible null reference argument.
                        queryResult
#pragma warning restore CS8604 // Possible null reference argument.
                        .Select(val => val.Length)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SelectFutureQueryResultNullSelector()
        {
            var value = "asdf";
            var queryResult =
                Task.FromResult(
                    new[] { value }
                        .ToQueryResult()
                        .WithoutError<Exception>());

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await queryResult
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
        public async Task SelectFutureQueryResultNoElements()
        {
            var queryResult =
                Task.FromResult(
                    Array
                        .Empty<string>()
                        .ToQueryResult()
                        .WithoutError<Exception>());

            var selected = await queryResult
                .Select(val => val.Length)
                .ConfigureAwait(false);

            Assert.IsFalse(selected.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(selected.Nodes.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task SelectFutureQueryResultNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Task.FromResult(
                    Array
                        .Empty<string>()
                        .ToQueryResult()
                        .WithError(invalidOperationException));

            var selected = await queryResult
                .Select(val => val.Length)
                .ConfigureAwait(false);

            Assert.IsFalse(selected.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(selected.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public async Task SelectFutureQueryResultNoError()
        {
            var value = "asdf";
            var queryResult =
                Task.FromResult(
                    new[] { value }
                        .ToQueryResult()
                        .WithoutError<Exception>());

            var selected = await queryResult
                .Select(val => val.Length)
                .ConfigureAwait(false);

            Assert.IsTrue(selected.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(selected.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public async Task SelectFutureEitherElementFollowedByError()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Task.FromResult(
                    new[] { value }
                        .ToQueryResult()
                        .WithError(invalidOperationException));

            var selected = await queryResult
                .Select(val => val.Length)
                .ConfigureAwait(false);

            Assert.IsTrue(selected.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(selected.Nodes.TryGetRight(out var terminal));
        }

        /*[TestMethod]
        public void SelectDeferredExecution()
        {
            var queryResult = new[] { "asdf", "qwer", "zxcv", "1234" }.ToQueryResult().WithoutError<Exception>();
            var instrumentedQueryResult = new InstrumentedQueryResult<string, Exception>(queryResult);

            var firstCharacters = instrumentedQueryResult.Select(element => element[0]);

            Assert.AreEqual(0, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.IsTrue(firstCharacters.Nodes.TryGetLeft(out var element));
            Assert.AreEqual('a', element.Value);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);

            Assert.IsTrue(firstCharacters.Nodes.TryGetLeft(out var element2));
            Assert.AreEqual('a', element2.Value);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);

            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual('q', nextElement.Value);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[1]);
        }

        [TestMethod]
        public void TrySelectNullSource()
        {
            IQueryResult<string, Exception> source =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                source
#pragma warning restore CS8604 // Possible null reference argument.
                .TrySelect(IntTryParse));
        }

        /// <summary>
        /// placeholder
        /// </summary>
        private static Try<string, int> IntTryParse { get; } = int.TryParse;

        [TestMethod]
        public void TrySelectNullTry()
        {
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithoutError<Exception>();

            Assert.ThrowsException<ArgumentNullException>(() => source.TrySelect(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Try<string, int>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void TrySelectWithoutError()
        {
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithoutError<Exception>();

            var selected = source.TrySelect(IntTryParse);

            Assert.IsTrue(selected.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(42, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(67, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void TrySelectWithError()
        {
            var invalidOperationException = new InvalidOperationException();
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithError(invalidOperationException);

            var selected = source.TrySelect(IntTryParse);

            Assert.IsTrue(selected.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(42, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(67, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
        }*/
    }
}
