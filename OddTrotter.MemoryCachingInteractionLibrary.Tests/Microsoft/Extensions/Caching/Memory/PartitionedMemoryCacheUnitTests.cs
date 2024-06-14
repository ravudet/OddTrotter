namespace Microsoft.Extensions.Caching.Memory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    /// <summary>
    /// Unit tests for <see cref="PartitionedMemoryCache{T}"/>
    /// </summary>
    [TestClass]
    public sealed class PartitionedMemoryCacheUnitTests
    {
        /// <summary>
        /// Creates a <see cref="PartitionedMemoryCache{T}"/> with a <see langword="null"/> delegate
        /// </summary>
        [TestMethod]
        public void NullDelegateMemoryCache()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PartitionedMemoryCache<string>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                string.Empty).Dispose());
        }

        /// <summary>
        /// Creates a <see cref="PartitionedMemoryCache{T}"/> with a <see langword="null"/> partition ID
        /// </summary>
        [TestMethod]
        public void NullPartitionId()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PartitionedMemoryCache<string>(
                new MemoryCache(new MemoryCacheOptions()),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ).Dispose());
        }

        /// <summary>
        /// Retrieves a cache entry that doesn't exist
        /// </summary>
        [TestMethod]
        public void RetrieveNonexistentEntry()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var partitionId = nameof(RetrieveNonexistentEntry);
                using (var partitionedMemoryCache = new PartitionedMemoryCache<string>(memoryCache, partitionId))
                {
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue("test", out var value));
                }
            }
        }

        /// <summary>
        /// Removes a cache entry that doesn't exist
        /// </summary>
        [TestMethod]
        public void RemoveNonexistentEntry()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions()))
            {
                var partitionId = nameof(RemoveNonexistentEntry);
                using (var partitionedMemoryCache = new PartitionedMemoryCache<string>(memoryCache, partitionId))
                {
                    partitionedMemoryCache.Remove("test");
                }
            }
        }

        /// <summary>
        /// Creates a cache entry, retrieves it, then removes it
        /// </summary>
        [TestMethod]
        public void CreateRetrieveAndRemove()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromDays(1) }))
            {
                var partitionId = nameof(RemoveNonexistentEntry);
                using (var partitionedMemoryCache = new PartitionedMemoryCache<string>(memoryCache, partitionId))
                {
                    var entryKey = "test";
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue(entryKey, out _));

                    partitionedMemoryCache.GetOrCreate(entryKey, entry => "somevalue");
                    Assert.IsTrue(partitionedMemoryCache.TryGetValue(entryKey, out _));

                    partitionedMemoryCache.Remove(entryKey);
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue(entryKey, out _));
                }
            }
        }

        /// <summary>
        /// Creates two cache entries and retrieves them in a single partition
        /// </summary>
        [TestMethod]
        public void TwoEntriesInOnePartition()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromDays(1) }))
            {
                var partitionId = nameof(RemoveNonexistentEntry);
                using (var partitionedMemoryCache = new PartitionedMemoryCache<string>(memoryCache, partitionId))
                {
                    var entryKey1 = "test";
                    var entryKey2 = "test2";
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue(entryKey1, out _));
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue(entryKey2, out _));

                    partitionedMemoryCache.GetOrCreate(entryKey1, entry => "somevalue");
                    Assert.IsTrue(partitionedMemoryCache.TryGetValue(entryKey1, out _));
                    Assert.IsFalse(partitionedMemoryCache.TryGetValue(entryKey2, out _));

                    partitionedMemoryCache.GetOrCreate(entryKey2, entry => "somevalue");
                    Assert.IsTrue(partitionedMemoryCache.TryGetValue(entryKey1, out _));
                    Assert.IsTrue(partitionedMemoryCache.TryGetValue(entryKey2, out _));
                }
            }
        }

        /// <summary>
        /// Creates the same cache entry, retrieves it, and removes it in two different partitions
        /// </summary>
        [TestMethod]
        public void CreateRetrieveAndRemoveAcrossPartitions()
        {
            using (var memoryCache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromDays(1) }))
            {
                var partitionId1 = "first";
                using (var partitionedMemoryCache1 = new PartitionedMemoryCache<string>(memoryCache, partitionId1))
                {
                    var partitionId2 = "second";
                    using (var partitionedMemoryCache2 = new PartitionedMemoryCache<string>(memoryCache, partitionId2))
                    {
                        var entryKey = "test";
                        Assert.IsFalse(partitionedMemoryCache1.TryGetValue(entryKey, out _));
                        Assert.IsFalse(partitionedMemoryCache2.TryGetValue(entryKey, out _));

                        partitionedMemoryCache1.GetOrCreate(entryKey, entry => "somevalue");
                        Assert.IsTrue(partitionedMemoryCache1.TryGetValue(entryKey, out _));
                        Assert.IsFalse(partitionedMemoryCache2.TryGetValue(entryKey, out _));

                        partitionedMemoryCache2.GetOrCreate(entryKey, entry => "somevalue");
                        Assert.IsTrue(partitionedMemoryCache1.TryGetValue(entryKey, out _));
                        Assert.IsTrue(partitionedMemoryCache2.TryGetValue(entryKey, out _));

                        partitionedMemoryCache1.Remove(entryKey);
                        Assert.IsFalse(partitionedMemoryCache1.TryGetValue(entryKey, out _));
                        Assert.IsTrue(partitionedMemoryCache2.TryGetValue(entryKey, out _));

                        partitionedMemoryCache2.Remove(entryKey);
                        Assert.IsFalse(partitionedMemoryCache1.TryGetValue(entryKey, out _));
                        Assert.IsFalse(partitionedMemoryCache2.TryGetValue(entryKey, out _));
                    }
                }
            }
        }
    }
}
