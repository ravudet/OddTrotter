/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class QueryResultNodeAsyncUnitTests
    {
        [TestMethod]
        public void InitializeNullNode()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new QueryResultNodeAsync<string, Exception>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void ApplyNullLeftMap()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            Assert.ThrowsException<ArgumentNullException>(() => node.Apply(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ,
                (terminal, context) => 
                    terminal.Apply(
                        (error, context) => error.Value.Message, 
                        (empty, context) => string.Empty, 
                        new Nothing()), 
                new Nothing()));
        }

        [TestMethod]
        public void ApplyNullRightMap()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            Assert.ThrowsException<ArgumentNullException>(() => node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , new Nothing()));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var leftMapException = Assert.ThrowsException<LeftMapException>(() => node.Apply(
                (element, context) => throw invalidOperationException,
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing()));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            var result = node.Apply(
                (element, context) => throw invalidOperationException,
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ApplyRightMapException()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => throw invalidOperationException,
                new Nothing());

            Assert.AreEqual(value + value, result);
            
            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            var rightMapException = Assert.ThrowsException<RightMapException>(() => node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => throw invalidOperationException,
                new Nothing()));

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public void Apply()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message, 
                    (empty, context) => string.Empty, 
                    new Nothing()), 
                new Nothing());

            Assert.AreEqual(value + value, result);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()), 
                new Nothing());

            Assert.AreEqual(value, result);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)));

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
        public async Task ApplyAsyncNullLeftMap()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await node
                        .Apply(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            async (terminal, context) => await terminal
                                .Apply(
                                    async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                                    async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                                    new Nothing())
                                .ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ApplyAsyncNullRightMap()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await node
                        .Apply(
                            async (element, context) => await Task
                                .FromResult(string.Concat(element.Value, element.Value))
                                .ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            , 
                            new Nothing())
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ApplyAsyncLeftMapException()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () => await node
                        .Apply(
                            (element, context) => throw invalidOperationException,
                            async (terminal, context) => await terminal
                                .Apply(
                                    async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                                    async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                                    new Nothing())
                                .ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            var result = await node
                .Apply(
                    (element, context) => throw invalidOperationException,
                    async (terminal, context) => await terminal
                        .Apply(
                            async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                            async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task ApplyAsyncRightMapException()
        {
            var value = "asdf";
            var invalidOperationException = new InvalidOperationException();
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var result = await node
                .Apply(
                    async (element, context) => await Task
                        .FromResult(
                            string.Concat(element.Value, element.Value))
                        .ConfigureAwait(false),
                    (terminal, context) => throw invalidOperationException,
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual(value + value, result);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    async () => await node
                        .Apply(
                            async (element, context) => await Task
                                .FromResult(string.Concat(element.Value, element.Value))
                                .ConfigureAwait(false),
                            (terminal, context) => throw invalidOperationException,
                            new Nothing())
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, rightMapException.InnerException);
        }

        [TestMethod]
        public async Task ApplyAsync()
        {
            var value = "asdf";
            var node = new QueryResultNodeAsync<string, Exception>(
                Either.Left(new MockElementAsync(value)).Right<IEither<IError<Exception>, IEmpty>>());

            var result = await node
                .Apply(
                    async (element, context) => await Task
                        .FromResult(string.Concat(element.Value, element.Value))
                        .ConfigureAwait(false),
                    async (terminal, context) => await terminal
                        .Apply(
                            async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                            async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual(value + value, result);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left(new MockError(new Exception(value))).Right<IEmpty>()));

            result = await node
                .Apply(
                    async (element, context) => await Task
                        .FromResult(string.Concat(element.Value, element.Value))
                        .ConfigureAwait(false),
                    async (terminal, context) => await terminal
                        .Apply(
                            async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                            async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual(value, result);

            node = new QueryResultNodeAsync<string, Exception>(
                Either.Left<MockElementAsync>().Right(Either.Left<MockError>().Right(MockEmpty.Instance)));

            result = await node
                .Apply(
                    async (element, context) => await Task
                        .FromResult(string.Concat(element.Value, element.Value))
                        .ConfigureAwait(false),
                    async (terminal, context) => await terminal
                        .Apply(
                            async (error, context) => await Task.FromResult(error.Value.Message).ConfigureAwait(false),
                            async (empty, context) => await Task.FromResult(string.Empty).ConfigureAwait(false),
                            new Nothing())
                        .ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
