namespace Microsoft.Extensions.Caching.Memory
{
    using System;
    using System.Linq.Expressions;

    public sealed class PartitionedMemoryCache<T> : IMemoryCache
    {
        private readonly IMemoryCache delegateMemoryCache;

        private readonly T partitionId;

        public PartitionedMemoryCache(Expression<Func<IMemoryCache>> delegateMemoryCacheFactory, T partitionId)
        {
            //// TODO what if you create a type called "nonclosure" that's a subset of expression's tree nodes, and then take in a non-closure; the non-closure will need to have an input parameter (which you will provide using an additional argument to this constructor) so that closure-like things can be provided without actually using a closure; does this solve anything? couldn't the input argument just contain the memory cache that you're trying to guard against?
            this.delegateMemoryCache = delegateMemoryCacheFactory.Compile().Invoke();
            var statistics = this.delegateMemoryCache.GetCurrentStatistics();
            if (statistics?.CurrentEntryCount != 0)
            {
                throw new InvalidOperationException("TODO");
            }

            this.partitionId = partitionId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateMemoryCache"></param>
        /// <param name="partitionId"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="delegateMemoryCache"/> or <paramref name="partitionId"/> is <see langword="null"/></exception>
        private PartitionedMemoryCache(IMemoryCache delegateMemoryCache, T partitionId)
        {
            if (delegateMemoryCache == null)
            {
                throw new ArgumentNullException(nameof(delegateMemoryCache));
            }

            if (partitionId == null)
            {
                throw new ArgumentNullException(nameof(partitionId));
            }

            this.delegateMemoryCache = delegateMemoryCache;
            this.partitionId = partitionId;
        }

        public ICacheEntry CreateEntry(object key)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            return this.delegateMemoryCache.CreateEntry(keyContainer);
        }

        public void Dispose()
        {
            //// TODO
        }

        public void Remove(object key)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            this.delegateMemoryCache.Remove(keyContainer);
        }

        public bool TryGetValue(object key, out object? value)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            return this.delegateMemoryCache.TryGetValue(keyContainer, out value);
        }

        private sealed class KeyContainer
        {
            public KeyContainer(T partitionId, object key)
            {
                PartitionId = partitionId;
                Key = key;
            }

            public T PartitionId { get; }
            public object Key { get; }
        }
    }
}
