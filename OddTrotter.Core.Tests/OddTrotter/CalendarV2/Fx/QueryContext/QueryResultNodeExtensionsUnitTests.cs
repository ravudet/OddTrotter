/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class QueryResultNodeExtensionsUnitTests
    {
        [TestMethod]
        public void ToQueryResultNodeNullNode()
        {
            IEither<IElement<string, Exception>, IEither<IError<Exception>, IEmpty>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .ToQueryResultNode());
        }

        [TestMethod]
        public void ToQueryResultNode()
        {
            var value = "asdf";
            var node = Either.Left(new MockElement(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNode();

            var result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value + value, result);

            node =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(
                                new MockError(
                                    new Exception(value)))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value, result);

            node = Either.Left<MockElement>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)).ToQueryResultNode();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void WhereNullSource()
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
                .Where(val => val.Length % 2 == 0));
        }

        [TestMethod]
        public void WhereNullPredicate()
        {
            var value = "asdf";
            var node = Either.Left(new MockElement(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNode();

            Assert.ThrowsException<ArgumentNullException>(() => node.Where(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void WhereNoElements()
        {
            var node = Either.Left<MockElement>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)).ToQueryResultNode();

            var result = node.Where(_ => true);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereNoElementsError()
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

            var result = node.Where(_ => true);

            Assert.IsFalse(result.TryGetLeft(out var element));
            Assert.IsTrue(result.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereNoError()
        {
            var value = "asdf";
            var node = Either.Left(new MockElement(value)).Right<IEither<MockError, MockEmpty>>().ToQueryResultNode();

            var result = node.Where(_ => true);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsFalse(secondTerminal.TryGetLeft(out var secondError));
            Assert.IsTrue(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));

            result = node.Where(_ => false);

            Assert.IsFalse(result.TryGetLeft(out element));
            Assert.IsTrue(result.TryGetRight(out var terminal2));
            Assert.IsFalse(terminal2.TryGetLeft(out var error));
            Assert.IsTrue(terminal2.TryGetRight(out var empty));
        }

        [TestMethod]
        public void WhereElementFollowedByError()
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

            var result = node.Where(_ => true);

            Assert.IsTrue(result.TryGetLeft(out var element));
            Assert.AreEqual(value, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var secondElement));
            Assert.IsTrue(next.TryGetRight(out var secondTerminal));
            Assert.IsTrue(secondTerminal.TryGetLeft(out var secondError));
            Assert.AreEqual(invalidOperationException, secondError.Value);
            Assert.IsFalse(secondTerminal.TryGetRight(out var secondEmpty));
            Assert.IsFalse(result.TryGetRight(out var terminal));

            result = node.Where(_ => false);

            Assert.IsFalse(result.TryGetLeft(out element));
            Assert.IsTrue(result.TryGetRight(out var terminal2));
            Assert.IsTrue(terminal2.TryGetLeft(out var error));
            Assert.AreEqual(invalidOperationException, error.Value);
            Assert.IsFalse(terminal2.TryGetRight(out var empty));
        }

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
        public void ConcatNullFirst()
        {
            IQueryResultNode<string, InvalidOperationException> first =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            var second =
                Either
                    .Left(new MockElement("saf"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();

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
                Either
                    .Left(new MockElement("saf"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();

            IQueryResultNode<string, InvalidOperationException> second =
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
                Either
                    .Left(new MockElement("saf"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();
            var second =
                Either
                    .Left(new MockElement("qwer"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();

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
                Either
                    .Left(new MockElement("saf"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();
            var second =
                Either
                    .Left(new MockElement("qwer"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();

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
                Either
                    .Left(new MockElement("saf"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();
            var second =
                Either
                    .Left(new MockElement("qwer"))
                    .Right<IEither<IError<InvalidCastException>, IEmpty>>()
                    .ToQueryResultNode();

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
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.TryGetLeft(out var element));
            Assert.IsTrue(concated.TryGetRight(out var terminal));
            Assert.IsFalse(terminal.TryGetLeft(out var error));
            Assert.IsTrue(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementNoErrorSecondNoElementError()
        {
            var first =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();
            var invalidCastException = new InvalidCastException();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidCastException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.TryGetLeft(out var element));
            Assert.IsTrue(concated.TryGetRight(out var terminal));
            Assert.IsTrue(terminal.TryGetLeft(out var error));
            Assert.AreEqual(1, error.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, error.Value.InnerExceptions[0]);
            Assert.IsFalse(terminal.TryGetRight(out var empty));
        }

        [TestMethod]
        public void ConcatFirstNoElementNotErrorSecondElementNoError()
        {
            var first =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var second =
                Either
                    .Left(new MockElement(secondValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextTerminal));
            Assert.IsTrue(next.TryGetRight(out var empty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementNoErrorSecondElementError()
        {
            var first =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
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
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondNoElementNoError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidOperationException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.TryGetLeft(out var element));
            Assert.IsTrue(concated.TryGetRight(out var terminal));
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
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidOperationException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var invalidCastException = new InvalidCastException();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidCastException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsFalse(concated.TryGetLeft(out var element));
            Assert.IsTrue(concated.TryGetRight(out var terminal));
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
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidOperationException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var second =
                Either
                    .Left(new MockElement(secondValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstNoElementErrorSecondElementError()
        {
            var invalidOperationException = new InvalidOperationException();
            var first =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidOperationException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
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
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(secondValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(2, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[1]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondNoElementNoError()
        {
            var firstValue = "Asdf";
            var first =
                Either
                    .Left(
                        new MockElement(
                                firstValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsFalse(nextTerminal.TryGetLeft(out var nextError));
            Assert.IsTrue(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondNoElementError()
        {
            var firstValue = "Asdf";
            var first =
                Either
                    .Left(
                        new MockElement(
                                firstValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();
            var invalidCastException = new InvalidCastException();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidCastException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondElementNoError()
        {
            var firstValue = "Asdf";
            var first =
                Either
                    .Left(
                        new MockElement(
                                firstValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var second =
                Either
                    .Left(new MockElement(secondValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
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
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementNoErrorSecondElementError()
        {
            var firstValue = "Asdf";
            var first =
                Either
                    .Left(
                        new MockElement(
                                firstValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
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
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
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
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondNoElementNoError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
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
                    .ToQueryResultNode();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left<MockError>()
                            .Right(MockEmpty.Instance))
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(1, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondNoElementError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
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
                    .ToQueryResultNode();
            var invalidCastException = new InvalidCastException();
            var second =
                Either
                    .Left<MockElement>()
                    .Right(
                        Either
                            .Left(new MockError(invalidCastException))
                            .Right<IEmpty>())
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
            Assert.AreEqual(firstValue, element.Value);
            var next = element.Next();
            Assert.IsFalse(next.TryGetLeft(out var nextElement));
            Assert.IsTrue(next.TryGetRight(out var nextTerminal));
            Assert.IsTrue(nextTerminal.TryGetLeft(out var nextError));
            Assert.AreEqual(2, nextError.Value.InnerExceptions.Count);
            Assert.AreEqual(invalidOperationException, nextError.Value.InnerExceptions[0]);
            Assert.AreEqual(invalidCastException, nextError.Value.InnerExceptions[1]);
            Assert.IsFalse(nextTerminal.TryGetRight(out var nextEmpty));
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondElementNoError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
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
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var second =
                Either
                    .Left(new MockElement(secondValue))
                    .Right<IEither<IError<Exception>, IEmpty>>()
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
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
            Assert.IsFalse(concated.TryGetRight(out var terminal));
        }

        [TestMethod]
        public void ConcatFirstElementErrorSecondElementError()
        {
            var firstValue = "Asdf";
            var invalidOperationException = new InvalidOperationException();
            var first =
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
                    .ToQueryResultNode();
            var secondValue = "qwer";
            var invalidCastException = new InvalidCastException();
            var second =
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
                    .ToQueryResultNode();

            var concated = first.Concat(
                second,
                firstError => new AggregateException(firstError),
                secondError => new AggregateException(secondError),
                (firstError, secondError) => new AggregateException(firstError, secondError));

            Assert.IsTrue(concated.TryGetLeft(out var element));
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
            Assert.IsFalse(concated.TryGetRight(out var terminal));
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
    }
}
