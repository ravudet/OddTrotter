/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class TaskAwaiterWrapperFailureTests
    {
        [TestMethod]
        public void OnCompletedNullContinuation()
        {
            var wrapper = new TaskAwaiterWrapper<string>(new TaskAwaiter<string>());

            Assert.ThrowsException<ArgumentNullException>(() => wrapper.OnCompleted(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public void UnsafeOnCompletedNullContinuation()
        {
            var wrapper = new TaskAwaiterWrapper<string>(new TaskAwaiter<string>());

            Assert.ThrowsException<ArgumentNullException>(() => wrapper.UnsafeOnCompleted(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }
    }
}
