/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class RealNullableUnitTests
    {
        [TestMethod]
        public void DefaultInitializer()
        {
            var nullable = new RealNullable<int?>();

            Assert.IsFalse(nullable.TryGetValue(out var value));
        }

        [TestMethod]
        public void Default()
        {
            RealNullable<int?> nullable = default;

            Assert.IsFalse(nullable.TryGetValue(out var value));
        }

        [TestMethod]
        public void Value()
        {
            var providedValue = 42;
            var nullable = new RealNullable<int?>(providedValue);

            Assert.IsTrue(nullable.TryGetValue(out var value));
            Assert.AreEqual(providedValue, value);
        }

        [TestMethod]
        public void Null()
        {
            int? providedValue = null;
            var nullable = new RealNullable<int?>(providedValue);

            Assert.IsTrue(nullable.TryGetValue(out var value));
            Assert.AreEqual(providedValue, value);
        }
    }
}
