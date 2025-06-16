/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed partial class QueryResultNodeExtensionsUnitTests
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IEither<IElementAsync<string, Exception>, IEither<IError<Exception>, IEmpty>> CreateAsyncNodeWithElement(
            string value)
        {
            return Either
                .Left(new MockElementAsync(value))
                .Right<IEither<IError<Exception>, IEmpty>>();
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IEither<IElementAsync<string, Exception>, IEither<IError<Exception>, IEmpty>> CreateAsyncNodeWithError(
            string value)
        {
            return Either
                    .Left<MockElementAsync>()
                    .Right(
                        Either
                            .Left(
                                new MockError(
                                    new Exception(value)))
                            .Right<IEmpty>());
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        private static IEither<IElementAsync<string, Exception>, IEither<IError<Exception>, IEmpty>> CreateAsyncNodeWithEmpty()
        {
            return Either
                .Left<MockElementAsync>()
                .Right(Either.Left<MockError>().Right(MockEmpty.Instance));
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static async 
            ITask
                <
                    IEither
                        <
                            IElementAsync
                                <
                                    string, 
                                    Exception
                                >, 
                            IEither
                                <
                                    IError
                                        <
                                            Exception
                                        >, 
                                    IEmpty
                                >
                        >
                > 
            CreateAsyncNodeWithElementAsync(string value)
        {
            return await Task
                .FromResult(
                    CreateAsyncNodeWithElement(value))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static async 
            ITask
                <
                    IEither
                        <
                            IElementAsync
                                <
                                    string,
                                    Exception
                                >,
                            IEither
                                <
                                    IError
                                        <
                                            Exception
                                        >,
                                    IEmpty
                                >
                        >
                >
            CreateAsyncNodeWithErrorAsync(string value)
        {
            return await Task
                .FromResult(
                    CreateAsyncNodeWithError(value))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        private static async 
            ITask
                <
                    IEither
                        <
                            IElementAsync
                                <
                                    string,
                                    Exception
                                >,
                            IEither
                                <
                                    IError
                                        <
                                            Exception
                                        >, 
                                    IEmpty
                                >
                        >
                > 
            CreateAsyncNodeWithEmptyAsync()
        {
            return await Task
                .FromResult(
                    CreateAsyncNodeWithEmpty())
                .ConfigureAwait(false);
        }

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
            var node = CreateAsyncNodeWithElement(value)
                .ToQueryResultNodeAsync();

            var result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value + value, result);

            node = CreateAsyncNodeWithError(value)
                .ToQueryResultNodeAsync();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value, result);

            node = CreateAsyncNodeWithEmpty()
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

        [TestMethod]
        public async Task ToQueryResultNodeAsyncFutureNodeNullNode()
        {
            ITask<IEither<IElementAsync<string, Exception>, IEither<IError<Exception>, IEmpty>>> either =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await
#pragma warning disable CS8604 // Possible null reference argument.
                    either
#pragma warning restore CS8604 // Possible null reference argument.
                        .ToQueryResultNodeAsync()
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ToQueryResultNodeAsyncFutureNode()
        {
            var value = "asdf";
            var node = await CreateAsyncNodeWithElementAsync(value)
                .ToQueryResultNodeAsync()
                .ConfigureAwait(false);

            var result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value + value, result);

            node = await CreateAsyncNodeWithErrorAsync(value)
                .ToQueryResultNodeAsync();

            result = node.Apply(
                (element, context) => string.Concat(element.Value, element.Value),
                (terminal, context) => terminal.Apply(
                    (error, context) => error.Value.Message,
                    (empty, context) => string.Empty,
                    new Nothing()),
                new Nothing());

            Assert.AreEqual(value, result);

            node = await CreateAsyncNodeWithEmptyAsync()
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
