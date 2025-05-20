namespace Fx.QueryContext
{
    using System;

    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed partial class QueryResultNodeExtensionsUnitTests
    {
        [TestMethod]
        public void ToQueryResultNodeAsyncNullNode()
        {
            IEither<IElementAsync<string, Exception>, IEither<IError<Exception>, IEmpty>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                either
#pragma warning restore CS8604 // Possible null reference argument.
                .ToQueryResultNodeAsync());
        }

        [TestMethod]
        public void ToQueryResultNodeAsync()
        {
            var value = "asdf";
            var node = Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>().ToQueryResultNodeAsync();

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
                    .Left<MockElementAsync>()
                    .Right(
                        Either
                            .Left(
                                new MockError(
                                    new Exception(value)))
                            .Right<IEmpty>())
                    .ToQueryResultNodeAsync();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value, result);

            node = Either
                .Left<MockElementAsync>()
                .Right(Either.Left<MockError>().Right(MockEmpty.Instance))
                .ToQueryResultNodeAsync();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(string.Empty, result);
        }
    }
}
