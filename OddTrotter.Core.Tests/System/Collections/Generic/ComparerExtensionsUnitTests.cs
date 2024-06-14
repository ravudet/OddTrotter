namespace System.Collections.Generic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.NetworkInformation;

    /// <summary>
    /// Unit tests for <see cref="ComparerExtensions"/>
    /// </summary>
    [TestClass]
    public sealed class ComparerExtensionsUnitTests
    {
        /// <summary>
        /// Finds the max of two values using a <see langword="null"> comparer
        /// </summary>
        [TestMethod]
        public void NullComparer()
        {
            IComparer<int>? comparer = null;
            Assert.ThrowsException<ArgumentNullException>(() =>
#pragma warning disable CS8604 // Possible null reference argument.
                comparer
#pragma warning restore CS8604 // Possible null reference argument.
                .Max(4, 3));
        }

        /// <summary>
        /// Gets the max of two numbers when the larger is first
        /// </summary>
        [TestMethod]
        public void MaxLarger()
        {
            var comparer = new MockComparer<IntContainer>(container => container.Value);
            var four = new IntContainer(4);
            var three = new IntContainer(3);

            Assert.AreEqual(four, comparer.Max(four, three));
        }

        /// <summary>
        /// Gets the max of two numbers when the smaller is first
        /// </summary>
        [TestMethod]
        public void MaxSmaller()
        {
            var comparer = new MockComparer<IntContainer>(container => container.Value);
            var four = new IntContainer(4);
            var three = new IntContainer(3);

            Assert.AreEqual(four, comparer.Max(three, four));
        }

        /// <summary>
        /// Gets the max of two numbers when they are equal
        /// </summary>
        [TestMethod]
        public void MaxEquals()
        {
            var comparer = new MockComparer<IntContainer>(container => container.Value);
            var four = new IntContainer(4);
            var otherFour = new IntContainer(4);

            Assert.AreEqual(four, comparer.Max(otherFour, four));
            Assert.AreEqual(otherFour, comparer.Max(four, otherFour));
        }

        private sealed class IntContainer
        {
            public IntContainer(int value)
            {
                this.Value = value;
            }

            public int Value { get; }
        }

        private sealed class MockComparer<T> : IComparer<T>
        {
            private readonly Func<T, int> selector;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="selector"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is <see langword="null"/></exception>
            public MockComparer(Func<T, int> selector)
            {
                if (selector == null)
                {
                    throw new ArgumentNullException(nameof(selector));
                }

                this.selector = selector;
            }

            public int Compare(T? x, T? y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return Comparer<int>.Default.Compare(this.selector(x), this.selector(y));
            }
        }
    }
}
