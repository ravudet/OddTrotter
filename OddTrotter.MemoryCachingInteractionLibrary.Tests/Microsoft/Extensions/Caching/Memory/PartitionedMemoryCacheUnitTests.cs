namespace Microsoft.Extensions.Caching.Memory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public sealed class PartitionedMemoryCacheUnitTests
    {
        [TestMethod]
        public void NullDelegateMemoryCache()
        {
            //// TODO
            Assert.ThrowsException<ArgumentNullException>(() => new PartitionedMemoryCache<string>(null, string.Empty));
        }
    }
}
