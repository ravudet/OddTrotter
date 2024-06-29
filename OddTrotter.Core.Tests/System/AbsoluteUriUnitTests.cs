namespace System
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.AccessControl;

    /// <summary>
    /// Unit tests for <see cref="AbsoluteUri"/>
    /// </summary>
    [TestClass]
    public sealed class AbsoluteUriUnitTests
    {
        /// <summary>
        /// Creates a <see cref="AbsoluteUri"/> for a <see langword="null"/> <see cref="Uri"/>
        /// </summary>
        [TestMethod]
        public void NullUri()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsException<ArgumentNullException>(() => new AbsoluteUri(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Creates a <see cref="AbsoluteUri"/> from a <see cref="Uri"/> with a relative URI
        /// </summary>
        [TestMethod]
        public void RelativeUri()
        {
            var uri = new Uri("/index.html", UriKind.Relative);
            Assert.ThrowsException<ArgumentException>(() => new AbsoluteUri(uri));

            uri = new Uri("/index.html", UriKind.RelativeOrAbsolute);
            Assert.ThrowsException<ArgumentException>(() => new AbsoluteUri(uri));
        }

        /// <summary>
        /// Creates a <see cref="AbsoluteUri"/> from a <see cref="Uri"/> with an absolute URI
        /// </summary>
        [TestMethod]
        public void AbsoluteUri()
        {
            var uri = new Uri("https://www.test.com/index.html", UriKind.Absolute);
            var absoluteUri = new AbsoluteUri(uri);
            Assert.IsTrue(absoluteUri.IsAbsoluteUri);

            uri = new Uri("https://www.test.com/index.html", UriKind.RelativeOrAbsolute);
            absoluteUri = new AbsoluteUri(uri);
            Assert.IsTrue(absoluteUri.IsAbsoluteUri);
        }
    }

    public interface IOrdering<TOrdered, TOrdering> where TOrdering : struct, IOrdering<TOrdered, TOrdering>
    {
        int Order(TOrdered x, TOrdered y);
    }

    public struct DefaultOrdering<TOrdered> : IOrdering<TOrdered, DefaultOrdering<TOrdered>>
    {
        public int Order(TOrdered x, TOrdered y)
        {
            return System.Collections.Generic.Comparer<TOrdered>.Default.Compare(x, y);
        }
    }

    public sealed class Ordering<TOrdered, TOrdering> : IComparer<TOrdered> where TOrdering : struct, IOrdering<TOrdered, TOrdering>
    {
        public Ordering(IOrdering<TOrdered, TOrdering> ordering)
        {
            var @default = default(TOrdering);
            if (!System.Collections.Generic.EqualityComparer<IOrdering<TOrdered, TOrdering>>.Default.Equals(@default, ordering))
            {
                throw new ArgumentException("TODO");
            }

            this.OrderingInstance = @default;
        }

        public TOrdering OrderingInstance { get; }

        public int Compare(TOrdered? x, TOrdered? y)
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

            return this.OrderingInstance.Order(x, y);
        }
    }

    public static class OrderingExtensions
    {
        public static Ordering<TOrdered, TOrdering> ToOrdering<TOrdered, TOrdering>(this IOrdering<TOrdered, TOrdering> ordering) where TOrdering : struct, IOrdering<TOrdered, TOrdering>
        {
            return new Ordering<TOrdered, TOrdering>(ordering);
        }
    }

    public interface IOrderedSequence<TElement, TOrdering> : System.Collections.Generic.IEnumerable<TElement> where TOrdering : struct, IOrdering<TElement, TOrdering>
    {
        Ordering<TElement, TOrdering> Ordering { get; }
    }

    public static class OrderedSequenceExtensions
    {
        public static IOrderedSequence<TElement, TOrdering> OrderSequenceBy<TElement, TOrdering>(this IEnumerable<TElement> enumerable, Ordering<TElement, TOrdering> ordering) where TOrdering : struct, IOrdering<TElement, TOrdering>
        {
            return new OrderedSequence<TElement, TOrdering>(System.Linq.Enumerable.OrderBy(enumerable, _ => _, ordering), ordering);
        }

        public static IOrderedSequence<TElement, TOrdering> Merge<TElement, TOrdering>(this IOrderedSequence<TElement, TOrdering> first, IOrderedSequence<TElement, TOrdering> second) where TOrdering : struct, IOrdering<TElement, TOrdering>
        {
            if (!System.Collections.Generic.EqualityComparer<IOrdering<TElement, TOrdering>>.Default.Equals(first.Ordering.OrderingInstance, second.Ordering.OrderingInstance))
            {
                throw new ArgumentException("TODO");
            }

            return new OrderedSequence<TElement, TOrdering>(MergeIterator(first, second, first.Ordering.OrderingInstance), first.Ordering);
        }

        private static IEnumerable<TElement> MergeIterator<TElement, TOrdering>(IEnumerable<TElement> first, IEnumerable<TElement> second, TOrdering ordering) where TOrdering : struct, IOrdering<TElement, TOrdering>
        {
            using (var firstEnumerator = first.GetEnumerator())
            using (var secondEnumerator = second.GetEnumerator())
            {
                if (!firstEnumerator.MoveNext())
                {
                    foreach (var element in second)
                    {
                        yield return element;
                    }
                }

                if (!secondEnumerator.MoveNext())
                {
                    foreach (var element in first)
                    {
                        yield return element;
                    }
                }

                var firstMoveNext = true;
                var secondMoveNext = true;
                do
                {
                    if (ordering.Order(firstEnumerator.Current, secondEnumerator.Current) < 0)
                    {
                        yield return firstEnumerator.Current;
                        firstMoveNext = firstEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return secondEnumerator.Current;
                        secondMoveNext = secondEnumerator.MoveNext();
                    }
                }
                while (firstMoveNext && secondMoveNext);

                //// TODO implement the rest...
            }
        }

        private sealed class OrderedSequence<TElement, TOrdering> : IOrderedSequence<TElement, TOrdering> where TOrdering : struct, IOrdering<TElement, TOrdering>
        {
            private readonly IEnumerable<TElement> enumerable;

            public OrderedSequence(IEnumerable<TElement> enumerable, Ordering<TElement, TOrdering> ordering)
            {
                this.enumerable = enumerable;
                Ordering = ordering;
            }

            public Ordering<TElement, TOrdering> Ordering { get; }

            public IEnumerator<TElement> GetEnumerator()
            {
                return this.enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }

    [TestClass]
    public sealed class OrderingTests
    {
        public struct Ordering1 : IOrdering<string, Ordering1>
        {
            public int Order(string x, string y)
            {
                throw new NotImplementedException();
            }
        }

        public struct Ordering2 : IOrdering<string, Ordering2>
        {
            public Ordering2(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public int Order(string x, string y)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void TestComparingOrderings()
        {
            new Ordering1().ToOrdering();
            new Ordering2().ToOrdering();
            Assert.ThrowsException<ArgumentException>(() => new Ordering2(5).ToOrdering());
            new DefaultOrdering<int>().ToOrdering();
            new DefaultOrdering<double>().ToOrdering();
            new DefaultOrdering<char>().ToOrdering();
        }

        [TestMethod]
        public void TestMerging()
        {
            var first = new[] { "asdF", "zxcV", "qwer", "1234" };
            var second = new[] { "rtyu", "sdf", "asdf" };

            /*
            first
                .OrderSequenceBy(new DefaultOrdering<string>().ToOrdering())
                .Merge(second); // this line correctly won't compile because second isn't ordered
            */
            /*
            first
                .OrderSequenceBy(new DefaultOrdering<string>().ToOrdering())
                .Merge(second.OrderSequenceBy(new CustomStringOrdering().ToOrdering())); // this line correctly doesn't compile because a different ordering is used for second
            */
            Assert.ThrowsException<ArgumentException>(() => first
                .OrderSequenceBy(new CustomStringOrdering2(5).ToOrdering())
                .Merge(second.OrderSequenceBy(new CustomStringOrdering2().ToOrdering())));
            var merged = first
                .OrderSequenceBy(new DefaultOrdering<string>().ToOrdering())
                .Merge(second.OrderSequenceBy(new DefaultOrdering<string>().ToOrdering()));
        }

        public struct CustomStringOrdering : IOrdering<string, CustomStringOrdering>
        {
            public int Order(string x, string y)
            {
                return -1;
            }
        }

        public struct CustomStringOrdering2 : IOrdering<string, CustomStringOrdering>
        {
            public CustomStringOrdering2(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public int Order(string x, string y)
            {
                return this.Value;
            }
        }
    }
}