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
        public void ThrowIfNullNonNullClass()
        {
            var @class = new MockClass();

            var returned = ArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullNullableClass()
        {
            MockClass? @class = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@class));
        }

        [TestMethod]
        public void ThrowIfNullNonNullNullableClass()
        {
            MockClass? @class = new MockClass();

            var returned = ArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullStruct()
        {
            MockStruct? @struct = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@struct));
        }

        private readonly struct MockStruct
        {
            public MockStruct(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TestMethod]
        public void ThrowIfNullNonNullStruct()
        {
            MockStruct? @struct = new MockStruct(42);

            var returned = ArgumentNullInline.ThrowIfNull(@struct);

            Assert.AreEqual(@struct, returned);
        }
    }
}
