namespace System.Runtime.CompilerServices
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ConfiguredTaskAwaiterWrapperFailureTests
    {
        [TestMethod]
        public void OnCompletedNullContinuation()
        {
            var wrapper = new ConfiguredTaskAwaiterWrapper<string>(new ConfiguredTaskAwaitable<string>.ConfiguredTaskAwaiter());

            Assert.ThrowsException<ArgumentNullException>(() => wrapper.OnCompleted(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }


        [TestMethod]
        public void UnsafeOnCompletedNullContinuation()
        {
            var wrapper = new ConfiguredTaskAwaiterWrapper<string>(new ConfiguredTaskAwaitable<string>.ConfiguredTaskAwaiter());

            Assert.ThrowsException<ArgumentNullException>(() => wrapper.UnsafeOnCompleted(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }
    }
}
