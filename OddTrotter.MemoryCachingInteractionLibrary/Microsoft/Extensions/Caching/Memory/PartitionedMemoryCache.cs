namespace Microsoft.Extensions.Caching.Memory
{
    using System;

    public sealed class PartitionedMemoryCache<T> : IMemoryCache
    {
        private readonly IMemoryCache delegateMemoryCache;

        private readonly T partitionId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateMemoryCache"></param>
        /// <param name="partitionId"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="delegateMemoryCache"/> or <paramref name="partitionId"/> is <see langword="null"/></exception>
        public PartitionedMemoryCache(IMemoryCache delegateMemoryCache, T partitionId)
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

        /// <inheritdoc/>
        public ICacheEntry CreateEntry(object key)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            return this.delegateMemoryCache.CreateEntry(keyContainer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void Remove(object key)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            this.delegateMemoryCache.Remove(keyContainer);
        }

        /// <inheritdoc/>
        public bool TryGetValue(object key, out object? value)
        {
            var keyContainer = new KeyContainer(this.partitionId, key);
            return this.delegateMemoryCache.TryGetValue(keyContainer, out value);
        }

        private sealed class KeyContainer : IEquatable<KeyContainer>
        {
            public KeyContainer(T partitionId, object key)
            {
                PartitionId = partitionId;
                Key = key;
            }

            public T PartitionId { get; }

            public object Key { get; }

            /// <inheritdoc/>
            public bool Equals(KeyContainer? other)
            {
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other == null)
                {
                    return false;
                }

                return
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    //// TODO
                    this.PartitionId.Equals(other.PartitionId) &&
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    this.Key.Equals(other.Key);
            }

            /// <inheritdoc/>
            public override bool Equals(object? obj)
            {
                if (obj is KeyContainer keyContainer)
                {
                    return this.Equals(keyContainer);
                }

                return base.Equals(obj);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    //// TODO
                    this.PartitionId.GetHashCode() ^
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    this.Key.GetHashCode();
            }
        }
    }
}
