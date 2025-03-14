/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

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

        /*[TestMethod]
        public void Play()
        {
            int? value = null;

            ArgumentNullException.ThrowIfNull(value);

            ArgumentNullException.ThrowIfNull(new Foo());

            ArgumentNullInline.ThrowIfNull(new Foo());
            
            ArgumentNullInline.ThrowIfNull(value);
        }*/

        public struct Foo
        {
        }

        [TestMethod]
        public void PerfStruct()
        {
            Foo? foo = new Foo();
            int iterations = 10000000;
            System.Diagnostics.Stopwatch stopwatch;

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull2(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            /*foo = null;

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull2(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);*/
        }

        public sealed class Bar
        {
        }

        [TestMethod]
        public void PerfClass()
        {
            Bar? foo = new Bar();
            int iterations = 10000000;
            System.Diagnostics.Stopwatch stopwatch;

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull2(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            /*foo = null;

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                ArgumentNullInline.ThrowIfNull2(foo);
            }
            Console.WriteLine(stopwatch.ElapsedTicks);*/
        }

        [TestMethod]
        public void Play()
        {
            var settings = new SomeSettings();

            var value = ArgumentNullInline.ThrowIfNull(ArgumentNullInline.ThrowIfNull(settings).NestedSettings).Value;

            var value2 = settings.ThrowIfNull().NestedSettings.ThrowIfNull().Value;
        }

        public sealed class SomeSettings
        {
            public NestedSettings NestedSettings { get; } = new NestedSettings();
        }

        public sealed class NestedSettings
        {
            public int Value { get; }
        }
    }
}
