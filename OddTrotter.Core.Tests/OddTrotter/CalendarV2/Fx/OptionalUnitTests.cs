/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using global::OddTrotter.CalendarV1.Tokenization.Json2;

namespace Fx
{
    using System.Threading.Tasks;

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

        [TestMethod]
        public async System.Threading.Tasks.Task Await()
        {
            var foo = await DoWork();

            Assert.AreEqual(42, foo.Value);
        }

        public readonly ref struct Foo
        {
            public Foo(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private static async AsyncableAwaitable<Foo> DoWork()
        {
            AsyncableAwaitable<Foo>.ValueFactory = () => new Foo(42);

            await Task.Delay(100);

            return new Foo(61);
        }
    }
}
