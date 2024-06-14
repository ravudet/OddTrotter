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
            IMemoryCache cache = null;

            new PartitionedMemoryCache<int>(() => null, 10);
            new PartitionedMemoryCache<int>(() => cache, 10);

            //// TODO
            Assert.ThrowsException<ArgumentNullException>(() => new PartitionedMemoryCache<string>(null, string.Empty));
        }
    }
}
