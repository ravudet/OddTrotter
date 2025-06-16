/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Either;
    using Fx.Try;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed partial class QueryResultNodeAsyncExtensionsUnitTests
    {
        
        [TestMethod]
        public void SelectNullSource()
        {
            IQueryResultNode<string, Exception> node =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                node
#pragma warning restore CS8604 // Possible null reference argument.
                .Select(val => val.Length));
        }

        [TestMethod]
        public void SelectNullSelector()
        {
            var value = "asdf";
            var node = Either.Left(new MockElement(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNode();

            Assert.ThrowsException<ArgumentNullException>(() => node.Select(
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                (Func<string, int>)null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ));
        }

        [TestMethod]
        public void SelectNoElements()
        {
            var node = Either.Left<MockElement>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)).ToQueryResultNode();

            var result = node.Select(val => val.Length);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var node =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(
                                new MockError(invalidOperationException))
                            .Right<MockEmpty>())
                    .ToQueryResultNode();

            var result = node.Select(val => val.Length);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectNoError()
        {
            var value = "asdf";
            var node = Either.Left(new MockElement(value)).Right<IEither<MockError, MockEmpty>>().ToQueryResultNode();

            var result = node.Select(val => val.Length);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void SelectElementFollowedByError()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node =
                Either
                    .Left(
                        new MockElement(
                            value,
                            Either
                                .Left<MockElement>()
                                .Right(
                                    Either
                                        .Left(
                                            new MockError(invalidOperationException))
                                        .Right<MockEmpty>())
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var result = node.Select(val => val.Length);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(4, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondElement()
        {
            var first =
                Either
                    .Left<IElement<string, int?>>()
                    .Right(
                        Either
                            .Left(
                                new Error<int?>(null))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var secondValue = "asdf";
            var second =
                Either
                    .Left(
                        new NullableErrorElement(secondValue))
                    .Right<IEither<IError<int?>, IEmpty>>()
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondError()
        {
            var first =
                Either
                    .Left<IElement<string, int?>>()
                    .Right(
                        Either
                            .Left(
                                new Error<int?>(null))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var secondError = 42;
            var second =
                Either
                    .Left<IElement<string, int?>>()
                    .Right(
                        Either
                            .Left(
                                new Error<int?>(secondError))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(2, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
            Assert.AreEqual(secondError, error.Value[1]);
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondEmpty()
        {
            var first =
                Either
                    .Left<IElement<string, int?>>()
                    .Right(
                        Either
                            .Left(
                                new Error<int?>(null))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var second =
                Either
                    .Left<IElement<string, int?>>()
                    .Right(
                        Either
                            .Left<IError<int?>>()
                            .Right(
                                MockEmpty.Instance))
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
        }

        [TestMethod]
        public void DistinctByNullSource()
        {
            IQueryResultNode<string, Exception> queryResult =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                queryResult
#pragma warning restore CS8604 // Possible null reference argument.
                .DistinctBy(element => element[0], EqualityComparer<char>.Default));
        }

        [TestMethod]
        public void DistinctByNullKeySelector()
        {
            var queryResult =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            Assert.ThrowsException<ArgumentNullException>(() => queryResult.DistinctBy(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , EqualityComparer<char>.Default));
        }

        [TestMethod]
        public void DistinctByNullComparer()
        {
            var queryResult =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            Assert.ThrowsException<ArgumentNullException>(() => queryResult.DistinctBy(element => element[0],
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void DistinctByEmpty()
        {
            var queryResult =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsFalse(distinctByed.TryGetLeft(out var element));
            Assert.IsTrue(distinctByed.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void DistinctByNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(
                                new MockError(invalidOperationException))
                            .Right<MockEmpty>())
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsFalse(distinctByed.TryGetLeft(out var element));
            Assert.IsTrue(distinctByed.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void DistinctByNoDuplicatesNoError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsFalse(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.IsTrue(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByNoDuplicatesError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left<MockElement>()
                                            .Right(
                                                Either
                                                    .Left(
                                                        new MockError(invalidOperationException))
                                                    .Right<MockEmpty>())
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(invalidOperationException, nextNextError.Value);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesNoError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left(
                                                new MockElement(
                                                    thirdValue))
                                            .Right<IEither<MockError, MockEmpty>>()
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsFalse(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.IsTrue(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left(
                                                new MockElement(
                                                    thirdValue,
                                                    Either
                                                        .Left<MockElement>()
                                                        .Right(
                                                            Either
                                                                .Left(
                                                                    new MockError(invalidOperationException))
                                                                .Right<MockEmpty>())
                                                        .ToQueryResultNode()))
                                            .Right<IEither<MockError, MockEmpty>>()
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(invalidOperationException, nextNextError.Value);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByNoDuplicatesNoErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "qwer";
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsFalse(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.IsTrue(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This should not be productionized. It likely only works for ASCII characters.
        /// </remarks>
        private sealed class CharCaseInsensitiveComparer : IEqualityComparer<char>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            private CharCaseInsensitiveComparer()
            {
            }

            /// <summary>
            /// placeholder
            /// </summary>
            public static CharCaseInsensitiveComparer Instance { get; } = new CharCaseInsensitiveComparer();

            /// <inheritdoc/>
            public bool Equals(char x, char y)
            {
                return EqualityComparer<char>.Default.Equals(char.ToUpper(x), char.ToUpper(y));
            }

            /// <inheritdoc/>
            public int GetHashCode([DisallowNull] char obj)
            {
                return char.ToUpper(obj).GetHashCode();
            }
        }

        [TestMethod]
        public void DistinctByNoDuplicatesErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "qwer";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left<MockElement>()
                                            .Right(
                                                Either
                                                    .Left(
                                                        new MockError(invalidOperationException))
                                                    .Right<MockEmpty>())
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(invalidOperationException, nextNextError.Value);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesNoErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left(
                                                new MockElement(
                                                    thirdValue))
                                            .Right<IEither<MockError, MockEmpty>>()
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(nextTerminal.TryGetLeft(out var nextError));
            Assert.IsTrue(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            firstValue,
                            Either
                                .Left(
                                    new MockElement(
                                        secondValue,
                                        Either
                                            .Left(
                                                new MockElement(
                                                    thirdValue,
                                                    Either
                                                        .Left<MockElement>()
                                                        .Right(
                                                            Either
                                                                .Left(
                                                                    new MockError(invalidOperationException))
                                                                .Right<MockEmpty>())
                                                        .ToQueryResultNode()))
                                            .Right<IEither<MockError, MockEmpty>>()
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(invalidOperationException, nextError.Value);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(distinctByed.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByReusingElement()
        {
            var queryResult =
                Either
                    .Left(
                        new MockElement(
                            "asdf",
                            Either
                                .Left(
                                    new MockElement(
                                        "qwer",
                                        Either
                                            .Left(
                                                new MockElement(
                                                    "asdf",
                                                    Either
                                                        .Left(
                                                            new MockElement(
                                                                "asdf",
                                                                Either
                                                                    .Left(
                                                                        new MockElement(
                                                                            "zxcv"))
                                                                    .Right<IEither<MockError, MockEmpty>>()
                                                                    .ToQueryResultNode()))
                                                        .Right<IEither<MockError, MockEmpty>>()
                                                        .ToQueryResultNode()))
                                            .Right<IEither<MockError, MockEmpty>>()
                                            .ToQueryResultNode()))
                                .Right<IEither<MockError, MockEmpty>>()
                                .ToQueryResultNode()))
                    .Right<IEither<MockError, MockEmpty>>()
                    .ToQueryResultNode();

            var distincted = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distincted.TryGetLeft(out var element));
            Assert.AreEqual("asdf", element.Value);

            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual("qwer", nextElement.Value);

            var nextNext = element.Next();
            Assert.IsTrue(nextNext.TryGetLeft(out var nextNextElement));
            Assert.AreEqual("qwer", nextNextElement.Value);
        }

        [TestMethod]
        public void TrySelectNullSource()
        {
            IQueryResultNode<string, Exception> source =
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
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithoutError<Exception>().Nodes;

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
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithoutError<Exception>().Nodes;

            var selected = source.TrySelect(IntTryParse);

            Assert.IsTrue(selected.TryGetLeft(out var element));
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
            var source = new[] { "asdf", "42", "67", "qwer" }.ToQueryResult().WithError(invalidOperationException).Nodes;

            var selected = source.TrySelect(IntTryParse);

            Assert.IsTrue(selected.TryGetLeft(out var element));
            Assert.AreEqual(42, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(67, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
        }

        [TestMethod]
        public void SelectErrorNullSource()
        {
            IQueryResultNode<string, Exception> source =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                source
#pragma warning restore CS8604 // Possible null reference argument.
                .SelectError(error => error.Message));
        }

        [TestMethod]
        public void SelectErrorNullSelector()
        {
            IQueryResultNode<string, Exception> source = 
                new[] { "asdf", "42", "67", "qwer" }
                    .ToQueryResult()
                    .WithoutError<Exception>()
                    .Nodes;

            Assert.ThrowsException<ArgumentNullException>(() => source.SelectError(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                (Func<Exception, string>)null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void SelectErrorNoElementsNoError()
        {
            IQueryResultNode<string, Exception> source = Array.Empty<string>().ToQueryResult().WithoutError<Exception>().Nodes;

            IQueryResultNode<string, string> selected = source.SelectError(error => error.Message);

            Assert.IsFalse(selected.TryGetLeft(out var element));
            Assert.IsTrue(selected.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectErrorNoElementsError()
        {
            var message = "this is a message";
            var invalidOperationException = new InvalidOperationException(message);
            IQueryResultNode<string, Exception> source = Array
                .Empty<string>()
                .ToQueryResult()
                .WithError<Exception>(invalidOperationException)
                .Nodes;

            IQueryResultNode<string, string> selected = source.SelectError(error => error.Message);

            Assert.IsFalse(selected.TryGetLeft(out var element));
            Assert.IsTrue(selected.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(message, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectErrorElementsNoError()
        {
            IQueryResultNode<string, Exception> source = new[] { "asdf", "42" }.ToQueryResult().WithoutError<Exception>().Nodes;

            IQueryResultNode<string, string> selected = source.SelectError(error => error.Message);

            Assert.IsTrue(selected.TryGetLeft(out var element));
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsFalse(nextNextTerminal.TryGetLeft(out var error));
            Assert.IsTrue(nextNextTerminal.TryGetRight(out var empty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(selected.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void SelectErrorElementsError()
        {
            var message = "this is a message";
            var invalidOperationException = new InvalidOperationException(message);
            IQueryResultNode<string, Exception> source = 
                new[] { "asdf", "42" }
                    .ToQueryResult()
                    .WithError<Exception>(invalidOperationException)
                    .Nodes;

            IQueryResultNode<string, string> selected = source.SelectError(error => error.Message);

            Assert.IsTrue(selected.TryGetLeft(out var element));
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var error));
            Assert.AreEqual(message, error.Value);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var empty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(selected.TryGetRight(out var terminal));
        }
    }
}
