namespace System.Runtime.CompilerServices
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class TaskMethodBuilderUnitTests
    {
        [TestMethod]
        public async Task AwaitInterface()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValue().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        private sealed class AwaiterType<T>
        {
            private readonly T value;

            public AwaiterType(T value)
            {
                this.value = value;
            }

            public async ITask<T> GetValue()
            {
                return await Task.FromResult(this.value).ConfigureAwait(false);
            }

            public async ITask<T> GetValueWithDelay()
            {
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }

            public async ITask<T> GetValueFromNested()
            {
                return await this.GetValue().ConfigureAwait(false);
            }

            public async ITask<T> GetValueWithException(Exception exception)
            {
                Throw(exception);
                return await this.GetValue().ConfigureAwait(false);
            }

            private static void Throw(Exception exception)
            {
                throw exception;
            }
        }

        [TestMethod]
        public async Task AwaitInterfaceWithDelay()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValueWithDelay().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithNested()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValueFromNested().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithException()
        {
            var message = "a message";
            var exception = new InvalidOperationException(message);

            var thrownException =
                await Assert
                    .ThrowsExceptionAsync<InvalidOperationException>(
                        async () => await new AwaiterType<string>("asdf")
                            .GetValueWithException(exception)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);

            Assert.AreEqual(exception, thrownException);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithStructStateMachine()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).Decompiled2().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        //// TODO https://devblogs.microsoft.com/dotnet/how-async-await-really-works/
        //// TODO must be release, must be framework (you used 4.8.1), "with delay"
    }
}
