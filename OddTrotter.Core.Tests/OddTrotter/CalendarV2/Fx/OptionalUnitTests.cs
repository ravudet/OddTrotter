/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class OptionalUnitTests
    {
        [TestMethod]
        public void DefaultInitializer()
        {
            var optional = new Optional<int?>();

            Assert.IsFalse(optional.TryGetValue(out var value));
        }

        [TestMethod]
        public void Default()
        {
            Optional<int?> optional = default;

            Assert.IsFalse(optional.TryGetValue(out var value));
        }

        [TestMethod]
        public void Value()
        {
            var providedValue = 42;
            var optional = new Optional<int?>(providedValue);

            Assert.IsTrue(optional.TryGetValue(out var value));
            Assert.AreEqual(providedValue, value);
        }

        [TestMethod]
        public void Null()
        {
            int? providedValue = null;
            var optional = new Optional<int?>(providedValue);

            Assert.IsTrue(optional.TryGetValue(out var value));
            Assert.AreEqual(providedValue, value);
        }
    }
}
