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
            IEither<string, int> either = null;

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => 
                        await either
                            .SelectAsync(
                                async (left, context) => await Task.FromResult(left).ConfigureAwait(false), 
                                async (right, context) => await Task.FromResult(right).ConfigureAwait(false), 
                                new Nothing())
                            .ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}
