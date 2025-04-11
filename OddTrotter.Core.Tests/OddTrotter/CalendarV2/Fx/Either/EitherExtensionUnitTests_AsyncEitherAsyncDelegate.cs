namespace Fx.Either
{
    using System;
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
    }
}
