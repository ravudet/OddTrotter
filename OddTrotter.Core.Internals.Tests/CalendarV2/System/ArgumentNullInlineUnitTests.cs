namespace System
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInlineUnitTests
    {
        [TestMethod]
        public void ThrowIfNullNullClass()
        {
            MockClass @class =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@class));
        }

        private sealed class MockClass
        {
        }

        [TestMethod]
        public void ThrowIfNullNullNullableClass()
        {
            MockClass? @class = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@class));
        }

        [TestMethod]
        public void ThrowIfNullNonNullClass()
        {
            var @class = new MockClass();

            var returned = ArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }
    }
}
