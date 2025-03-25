/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class QueryResultExtensionsUnitTests
    {
        [TestMethod]
        public void WhereNullSource()
        {
            IQueryResult<string, Exception> queryResult =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                queryResult
#pragma warning restore CS8604 // Possible null reference argument.
                .Where(_ => true));
        }

        private sealed class MockQueryResult : IQueryResult<string, Exception>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="nodes"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <see langword="null"/></exception>
            public MockQueryResult(IQueryResultNode<string, Exception> nodes)
            {
                ArgumentNullException.ThrowIfNull(nodes);

                this.Nodes = nodes;
            }

            /// <inheritdoc/>
            public IQueryResultNode<string, Exception> Nodes { get; }
        }

        [TestMethod]
        public void WhereNullPredicate()
        {
            var value = "asdf";
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(value))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(() => queryResult.Where(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }
        [TestMethod]
        public void WhereNoElements()
        {
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(
                                    MockEmpty.Instance))
                        .ToQueryResultNode());

            var whered = queryResult.Where(_ => true);

            Assert.IsFalse(whered.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(whered.Nodes.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(
                                    new MockError(invalidOperationException))
                                .Right<MockEmpty>())
                        .ToQueryResultNode());

            var whered = queryResult.Where(_ => true);

            Assert.IsFalse(whered.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(whered.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereNoError()
        {
            var value = "asdf";
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(value))
                        .Right<IEither<MockError, MockEmpty>>()
                        .ToQueryResultNode());

            var whered = queryResult.Where(_ => true);

            Assert.IsTrue(whered.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(whered.Nodes.TryGetRight(out var terminal));

            whered = queryResult.Where(_ => false);

            Assert.IsFalse(whered.Nodes.TryGetLeft(out element));
            Assert.IsTrue(whered.Nodes.TryGetRight(out var terminal2));
            Assert.IsFalse(terminal2.TryGetLeft(out var error));
            Assert.IsTrue(terminal2.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereElementFollowedByError()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var whered = queryResult.Where(_ => true);

            Assert.IsTrue(whered.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(whered.Nodes.TryGetRight(out var terminal));

            whered = queryResult.Where(_ => false);

            Assert.IsFalse(whered.Nodes.TryGetLeft(out element));
            Assert.IsTrue(whered.Nodes.TryGetRight(out var terminal2));
            Assert.IsTrue(terminal2.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal2.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereDeferredExecutionSkippingFirstElement()
        {
            var queryResult = new[] { 1, 2, 3, 4 }.ToQueryResult().WithoutError<Exception>();
            var instrumentedQueryResult = new InstrumentedQueryResult<int, Exception>(queryResult);

            var evens = instrumentedQueryResult.Where(element => element % 2 == 0);

            Assert.AreEqual(0, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.IsTrue(evens.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(2, element.Value);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[1]);

            Assert.IsTrue(evens.Nodes.TryGetLeft(out var element2));
            Assert.AreEqual(2, element2.Value);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[1]);

            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(4, nextElement.Value);
            Assert.AreEqual(4, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[1]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[2]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[3]);
        }

        [TestMethod]
        public void WhereDeferredExecutionTakingFirstElement()
        {
            var queryResult = new[] { 1, 2, 3, 4 }.ToQueryResult().WithoutError<Exception>();
            var instrumentedQueryResult = new InstrumentedQueryResult<int, Exception>(queryResult);

            var odds = instrumentedQueryResult.Where(element => element % 2 != 0);

            Assert.AreEqual(0, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.IsTrue(odds.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(1, element.Value);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);

            Assert.IsTrue(odds.Nodes.TryGetLeft(out var element2));
            Assert.AreEqual(1, element2.Value);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);

            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(3, nextElement.Value);
            Assert.AreEqual(3, instrumentedQueryResult.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(2, instrumentedQueryResult.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[1]);
            Assert.AreEqual(1, instrumentedQueryResult.IndexToRetrievalCountMapping[2]);
        }

        [TestMethod]
        public void SelectNullSource()
        {
            IQueryResult<string, Exception> queryResult =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                queryResult
#pragma warning restore CS8604 // Possible null reference argument.
                .Select(val => val.Length));
        }

        [TestMethod]
        public void SelectNullSelector()
        {
            var value = "asdf";
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(value))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(() => queryResult.Select(
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
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(
                                    MockEmpty.Instance))
                        .ToQueryResultNode());

            var selected = queryResult.Select(val => val.Length);

            Assert.IsFalse(selected.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(selected.Nodes.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(
                                    new MockError(invalidOperationException))
                                .Right<MockEmpty>())
                        .ToQueryResultNode());

            var selected = queryResult.Select(val => val.Length);

            Assert.IsFalse(selected.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(selected.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void SelectNoError()
        {
            var value = "asdf";
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(value))
                        .Right<IEither<MockError, MockEmpty>>()
                        .ToQueryResultNode());

            var selected = queryResult.Select(val => val.Length);

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
        public void SelectElementFollowedByError()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var selected = queryResult.Select(val => val.Length);

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

        [TestMethod]
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
        public void FirstOrDefaultNullSource()
        {
            IQueryResult<string, Exception> queryResult =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                queryResult
#pragma warning restore CS8604 // Possible null reference argument.
                .FirstOrDefault(42));
        }

        [TestMethod]
        public void FirstOrDefaultNoElements()
        {
            var defaultValue = 42;
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<IElement<string, Exception>>()
                        .Right(
                            Either
                                .Left<IError<Exception>>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var firstOrDefaultOrError = queryResult.FirstOrDefault(defaultValue);

            Assert.IsTrue(firstOrDefaultOrError.TryGetLeft(out var firstOrDefault));
            Assert.IsFalse(firstOrDefault.TryGetLeft(out var first));
            Assert.IsTrue(firstOrDefault.TryGetRight(out var @default));
            Assert.AreEqual(defaultValue, @default);
            Assert.IsFalse(firstOrDefaultOrError.TryGetRight(out var error));
        }

        [TestMethod]
        public void FirstOrDefaultNoElementsError()
        {
            var errorValue = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<IElement<string, Exception>>()
                        .Right(
                            Either
                                .Left(
                                    new MockError(errorValue))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var firstOrDefaultOrError = queryResult.FirstOrDefault(42);

            Assert.IsFalse(firstOrDefaultOrError.TryGetLeft(out var firstOrDefault));
            Assert.IsTrue(firstOrDefaultOrError.TryGetRight(out var error));
            Assert.AreEqual(errorValue, error);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            var element = "ASdf";
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left(new MockElement(element))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var firstOrDefaultOrError = queryResult.FirstOrDefault(42);

            Assert.IsTrue(firstOrDefaultOrError.TryGetLeft(out var firstOrDefault));
            Assert.IsTrue(firstOrDefault.TryGetLeft(out var first));
            Assert.AreEqual(element, first);
            Assert.IsFalse(firstOrDefault.TryGetRight(out var @default));
            Assert.IsFalse(firstOrDefaultOrError.TryGetRight(out var error));
        }

        [TestMethod]
        public void ConcatNullFirst()
        {
            IQueryResult<string, InvalidOperationException> first =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("saf"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(
                () =>
#pragma warning disable CS8604 // Possible null reference argument.
                    first
#pragma warning restore CS8604 // Possible null reference argument.
                        .Concat(
                            second,
                            firstError => new AggregateException(firstError),
                            secondError => new AggregateException(secondError),
                            (firstError, secondError) => new AggregateException(firstError, secondError)));
        }

        [TestMethod]
        public void ConcatNullSecond()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("saf"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());

            IQueryResult<string, InvalidOperationException> second =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    first
                        .Concat(
#pragma warning disable CS8604 // Possible null reference argument.
                            second
#pragma warning restore CS8604 // Possible null reference argument.
                            ,
                            firstError => new AggregateException(firstError),
                            secondError => new AggregateException(secondError),
                            (firstError, secondError) => new AggregateException(firstError, secondError)));
        }

        [TestMethod]
        public void ConcatNullFirstErrorSelector()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("saf"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("qwer"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    first
                        .Concat(
                            second,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            secondError => new AggregateException(secondError),
                            (firstError, secondError) => new AggregateException(firstError, secondError)));
        }

        [TestMethod]
        public void ConcatNullSecondErrorSelector()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("saf"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("qwer"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    first
                        .Concat(
                            second,
                            firstError => new AggregateException(firstError),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            (firstError, secondError) => new AggregateException(firstError, secondError)));
        }

        [TestMethod]
        public void ConcatNullErrorAggregator()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("saf"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement("qwer"))
                        .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                        .ToQueryResultNode());

            Assert.ThrowsException<ArgumentNullException>(
                () =>
                    first
                        .Concat(
                            second,
                            firstError => new AggregateException(firstError),
                            secondError => new AggregateException(secondError),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ));
        }

        [TestMethod]
        public void ConcatFirstNoElementNoErrorSecondNoElementNoError()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementNoErrorSecondNoElementError()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidCastException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, error.Value.InnerExceptions[0]);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementNotErrorSecondElementNoError()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement(secondValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextTerminal));
            Assert.IsTrue(next.TryGetRight(out var empty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementNoErrorSecondElementError()
        {
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                secondValue,
                                Either
                                    .Left<IElement<string, Exception>>()
                                    .Right(
                                        Either
                                            .Left(new MockError(invalidCastException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondNoElementNoError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidOperationException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, error.Value.InnerExceptions[0]);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondNoElementError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidOperationException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidCastException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(2, error.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, error.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, error.Value.InnerExceptions[1]);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondElementNoError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidOperationException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement(secondValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondElementError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidOperationException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                secondValue,
                                Either
                                    .Left<IElement<string, Exception>>()
                                    .Right(
                                        Either
                                            .Left(new MockError(invalidCastException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(2, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[1]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondNoElementNoError()
        {
            var firstValue = "Asdf";
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                    firstValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(nextTerminal.TryGetLeft(out var nextError));
            Assert.IsTrue(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondNoElementError()
        {
            var firstValue = "Asdf";
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                    firstValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidCastException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondElementNoError()
        {
            var firstValue = "Asdf";
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                    firstValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement(secondValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondElementError()
        {
            var firstValue = "Asdf";
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                    firstValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                secondValue,
                                Either
                                    .Left<IElement<string, Exception>>()
                                    .Right(
                                        Either
                                            .Left(new MockError(invalidCastException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(1, nextNextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, nextNextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondNoElementNoError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                firstValue,
                                Either
                                    .Left<MockElement>()
                                    .Right(
                                        Either
                                            .Left(
                                                new MockError(invalidOperationException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<MockError, IEmpty>>()
                        .ToQueryResultNode());
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondNoElementError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                firstValue,
                                Either
                                    .Left<MockElement>()
                                    .Right(
                                        Either
                                            .Left(
                                                new MockError(invalidOperationException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<MockError, IEmpty>>()
                        .ToQueryResultNode());
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(new MockError(invalidCastException))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(2, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[1]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondElementNoError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                firstValue,
                                Either
                                    .Left<MockElement>()
                                    .Right(
                                        Either
                                            .Left(
                                                new MockError(invalidOperationException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<MockError, IEmpty>>()
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var second =
                new MockQueryResult(
                    Either
                        .Left(new MockElement(secondValue))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(1, nextNextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextNextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondElementError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                firstValue,
                                Either
                                    .Left<MockElement>()
                                    .Right(
                                        Either
                                            .Left(
                                                new MockError(invalidOperationException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<MockError, IEmpty>>()
                        .ToQueryResultNode());
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
                new MockQueryResult(
                    Either
                        .Left(
                            new MockElement(
                                secondValue,
                                Either
                                    .Left<IElement<string, Exception>>()
                                    .Right(
                                        Either
                                            .Left(new MockError(invalidCastException))
                                            .Right<IEmpty>())
                                    .ToQueryResultNode()))
                        .Right<IEither<IError<Exception>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(secondValue, nextElement.Value);
            var nextNext = nextElement.Next();
            Assert.IsFalse(nextNext.TryGetLeft(out var nextNextElement));
            Assert.IsTrue(nextNext.TryGetRight(out var nextNextTerminal));
            Assert.IsTrue(nextNextTerminal.TryGetLeft(out var nextNextError));
            Assert.AreEqual(2, nextNextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextNextError.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, nextNextError.Value.InnerExceptions[1]);
            Assert.IsFalse(nextNextTerminal.TryGetRight(out var nextNextEmpty));
            Assert.IsFalse(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(concated.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondElement()
        {
            var first =
                new NullableErrorQueryResult(
                    Either
                        .Left<IElement<string, int?>>()
                        .Right(
                            Either
                                .Left(
                                    new Error<int?>(null))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var secondValue = "asdf";
            var second =
                new NullableErrorQueryResult(
                    Either
                        .Left(
                            new NullableErrorElement(secondValue))
                        .Right<IEither<IError<int?>, IEmpty>>()
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsTrue(next.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
        }

        private sealed class NullableErrorQueryResult : IQueryResult<string, int?>
        {
            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="nodes"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <see langword="null"/></exception>
            public NullableErrorQueryResult(IQueryResultNode<string, int?> nodes)
            {
                ArgumentNullException.ThrowIfNull(nodes);

                this.Nodes = nodes;
            }

            public IQueryResultNode<string, int?> Nodes { get; }
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondError()
        {
            var first =
                new NullableErrorQueryResult(
                    Either
                        .Left<IElement<string, int?>>()
                        .Right(
                            Either
                                .Left(
                                    new Error<int?>(null))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var secondError = 42;
            var second =
                new NullableErrorQueryResult(
                    Either
                        .Left<IElement<string, int?>>()
                        .Right(
                            Either
                                .Left(
                                    new Error<int?>(secondError))
                                .Right<IEmpty>())
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(2, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
            Assert.AreEqual(secondError, error.Value[1]);
        }

        [TestMethod]
        public void ConcatNullableErrorNullFirstErrorSecondEmpty()
        {
            var first =
                new NullableErrorQueryResult(
                    Either
                        .Left<IElement<string, int?>>()
                        .Right(
                            Either
                                .Left(
                                    new Error<int?>(null))
                                .Right<IEmpty>())
                        .ToQueryResultNode());
            var second =
                new NullableErrorQueryResult(
                    Either
                        .Left<IElement<string, int?>>()
                        .Right(
                            Either
                                .Left<IError<int?>>()
                                .Right(
                                    MockEmpty.Instance))
                        .ToQueryResultNode());

            var concated = first.Concat(
                second,
                firstError => new[] { firstError },
                secondError => new[] { secondError },
                (firstError, secondError) => new[] { firstError, secondError });

            Assert.IsTrue(concated.Nodes.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.Length);
            Assert.AreEqual(null, error.Value[0]);
        }

        [TestMethod]
        public void ConcatDeferredExecution()
        {
            var first = new[] { "asdf", "qwer" }.ToQueryResult().WithoutError<Exception>();
            var instrumentedFirst = new InstrumentedQueryResult<string, Exception>(first);
            var second = new[] { "zxcv", "1234" }.ToQueryResult().WithoutError<Exception>();
            var instrumentSecond = new InstrumentedQueryResult<string, Exception>(second);

            var concated = instrumentedFirst.Concat(instrumentSecond, _ => _, _ => _, (_, __) => _);

            Assert.AreEqual(0, instrumentedFirst.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(0, instrumentSecond.IndexToRetrievalCountMapping.Count);

            Assert.IsTrue(concated.Nodes.TryGetLeft(out var element));
            Assert.AreEqual("asdf", element.Value);
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(0, instrumentSecond.IndexToRetrievalCountMapping.Count);

            var next = element.Next();
            Assert.IsTrue(next.TryGetLeft(out var nextElement));
            Assert.AreEqual(2, instrumentedFirst.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping[1]);
            Assert.AreEqual(0, instrumentSecond.IndexToRetrievalCountMapping.Count);

            var nextNext = nextElement.Next();
            Assert.IsTrue(nextNext.TryGetLeft(out var nextNextElement));
            Assert.AreEqual(3, instrumentedFirst.IndexToRetrievalCountMapping.Count); // a third access means that we went "right"
                                                                                      // to the "terminal"
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping[0]);
            Assert.AreEqual(1, instrumentedFirst.IndexToRetrievalCountMapping[1]);
            Assert.AreEqual(1, instrumentSecond.IndexToRetrievalCountMapping.Count);
            Assert.AreEqual(1, instrumentSecond.IndexToRetrievalCountMapping[0]);
        }

        [TestMethod]
        public void DistinctByNullSource()
        {
            IQueryResult<string, Exception> queryResult =
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
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

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
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

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
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left<MockError>()
                                .Right(MockEmpty.Instance))
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsFalse(distinctByed.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(distinctByed.Nodes.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void DistinctByNoElementsError()
        {
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
                    Either
                        .Left<MockElement>()
                        .Right(
                            Either
                                .Left(
                                    new MockError(invalidOperationException))
                                .Right<MockEmpty>())
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsFalse(distinctByed.Nodes.TryGetLeft(out var element));
            Assert.IsTrue(distinctByed.Nodes.TryGetRight(out var terminal));
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
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByNoDuplicatesError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesNoError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesError()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], EqualityComparer<char>.Default);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByNoDuplicatesNoErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "qwer";
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
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
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
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
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesNoErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(nextTerminal.TryGetLeft(out var nextError));
            Assert.IsTrue(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void DistinctByDuplicatesErrorWithComparer()
        {
            var firstValue = "asdf";
            var secondValue = "Asdf";
            var thirdValue = "azxcv";
            var invalidOperationException = new InvalidOperationException();
            var queryResult =
                new MockQueryResult(
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
                        .ToQueryResultNode());

            var distinctByed = queryResult.DistinctBy(element => element[0], CharCaseInsensitiveComparer.Instance);

            Assert.IsTrue(distinctByed.Nodes.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(invalidOperationException, nextError.Value);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(distinctByed.Nodes.TryGetRight(out var terminal));
        }

        private sealed class InstrumentedQueryResult<TValue, TError> : IQueryResult<TValue, TError>
        {
            private readonly InstrumentedQueryResultNode queryResultNode;

            private readonly Dictionary<int, int> indexToRetrievalCountMapping;

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="queryResult"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="queryResult"/> is <see langword="null"/>
            /// </exception>
            public InstrumentedQueryResult(IQueryResult<TValue, TError> queryResult)
            {
                ArgumentNullException.ThrowIfNull(queryResult);

                this.indexToRetrievalCountMapping = new Dictionary<int, int>();
                this.queryResultNode = new InstrumentedQueryResultNode(queryResult.Nodes, this.indexToRetrievalCountMapping, 0);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            public IReadOnlyDictionary<int, int> IndexToRetrievalCountMapping
            {
                get
                {
                    return this.indexToRetrievalCountMapping;
                }
            }

            /// <inheritdoc/>
            public IQueryResultNode<TValue, TError> Nodes
            {
                get
                {
                    return this.queryResultNode;
                }
            }

            private sealed class InstrumentedQueryResultNode : IQueryResultNode<TValue, TError>
            {
                private readonly IQueryResultNode<TValue, TError> queryResultNode;
                private readonly Dictionary<int, int> indexToRetrievalCountMapping;
                private readonly int index;

                /// <summary>
                /// placeholder
                /// </summary>
                /// <param name="queryResultNode"></param>
                /// <param name="indexToRetrievalCountMapping"></param>
                /// <param name="index"></param>
                /// <exception cref="ArgumentNullException">
                /// Thrown if <paramref name="queryResultNode"/> or <paramref name="indexToRetrievalCountMapping"/> is
                /// <see langword="null"/>
                /// </exception>
                public InstrumentedQueryResultNode(
                    IQueryResultNode<TValue, TError> queryResultNode, 
                    Dictionary<int, int> indexToRetrievalCountMapping, 
                    int index)
                {
                    ArgumentNullException.ThrowIfNull(queryResultNode);
                    ArgumentNullException.ThrowIfNull(indexToRetrievalCountMapping);
                    // we could assert that `index` is within a reasonable range (not negative, for example), but if we are
                    // instrumenting, we probably actually want to track if that sort of thing has somehow occurred

                    this.queryResultNode = queryResultNode;
                    this.indexToRetrievalCountMapping = indexToRetrievalCountMapping;
                    this.index = index;
                }

                /// <inheritdoc/>
                public TResult Apply<TResult, TContext>(
                    Func<IElement<TValue, TError>, TContext, TResult> leftMap, 
                    Func<IEither<IError<TError>, IEmpty>, TContext, TResult> rightMap, 
                    TContext context)
                {
                    ArgumentNullException.ThrowIfNull(leftMap);
                    ArgumentNullException.ThrowIfNull(rightMap);

                    if (!this.indexToRetrievalCountMapping.TryGetValue(this.index, out var count))
                    {
                        count = 0;
                    }

                    ++count;
                    this.indexToRetrievalCountMapping[this.index] = count;

                    return this.queryResultNode.Apply(
                        (element, context) =>
                            leftMap(
                                new InstrumentedElement(element, this.indexToRetrievalCountMapping, this.index),
                                context),
                        (terminal, context) =>
                            rightMap(
                                terminal,
                                context),
                        context);
                }

                private sealed class InstrumentedElement : IElement<TValue, TError>
                {
                    private readonly IElement<TValue, TError> element;
                    private readonly Dictionary<int, int> indexToRetrievalCountMapping;
                    private readonly int index;

                    /// <summary>
                    /// placeholder
                    /// </summary>
                    /// <param name="element"></param>
                    /// <param name="indexToRetrievalCountMapping"></param>
                    /// <param name="index"></param>
                    /// <exception cref="ArgumentNullException">
                    /// Thrown if <paramref name="element"/> or <paramref name="indexToRetrievalCountMapping"/> is 
                    /// <see langword="null"/>
                    /// </exception>
                    public InstrumentedElement(
                        IElement<TValue, TError> element, 
                        Dictionary<int, int> indexToRetrievalCountMapping, 
                        int index)
                    {
                        ArgumentNullException.ThrowIfNull(element);
                        ArgumentNullException.ThrowIfNull(indexToRetrievalCountMapping);

                        this.element = element;
                        this.indexToRetrievalCountMapping = indexToRetrievalCountMapping;
                        this.index = index;
                    }

                    /// <inheritdoc/>
                    public TValue Value
                    {
                        get
                        {
                            return this.element.Value;
                        }
                    }

                    /// <inheritdoc/>
                    public IQueryResultNode<TValue, TError> Next()
                    {
                        return new InstrumentedQueryResultNode(
                            this.element.Next(), 
                            this.indexToRetrievalCountMapping,
                            this.index + 1);
                    }
                }
            }
        }
    }
}
