namespace Fx
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EitherUnitTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var either = new Either<int, Exception>(42);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual("42", result);
        }

        private static async Task<string> TestMethod1Impl(IEither<int, Exception> either)
        {
            bool context = false;
            return await either.Apply<string, bool, TaskWrapper<string>>(
                (int value, ref bool context) => ToString(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref context);
        }

        public static TaskWrapper<string> ToString(int value)
        {
            return new TaskWrapper<string>(ToStringImpl(value));
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static TaskWrapper<string> ToString(Exception exception)
        {
            return new TaskWrapper<string>(ToStringImpl(exception));
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }
    }
}
