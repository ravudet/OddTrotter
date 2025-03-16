namespace System
{
    extern alias OddTrotterCore;

    using ArgumentNullInline2 = OddTrotterCore::System.ArgumentNullInline;

    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInline2UnitTests
    {
        [TestMethod]
        public void ThrowIfNullNullClass()
        {
            MockClass @class =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline2.ThrowIfNull(@class));
        }

        private sealed class MockClass
        {
        }

        [TestMethod]
        public void ThrowIfNullNonNullClass()
        {
            var @class = new MockClass();

            var returned = ArgumentNullInline2.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullNullableClass()
        {
            MockClass? @class = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline2.ThrowIfNull(@class));
        }

        [TestMethod]
        public void ThrowIfNullNonNullNullableClass()
        {
            MockClass? @class = new MockClass();

            var returned = ArgumentNullInline2.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullStruct()
        {
            MockStruct? @struct = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline2.ThrowIfNull(@struct));
        }

        private readonly struct MockStruct
        {
            public MockStruct(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TestMethod]
        public void ThrowIfNullNonNullStruct()
        {
            MockStruct? @struct = new MockStruct(42);

            var returned = ArgumentNullInline2.ThrowIfNull(@struct);

            Assert.AreEqual(@struct, returned);
        }

        [TestMethod]
        public void Play()
        {
            var code =
"""
public static class Foo
{
private readonly struct MockStruct
{
}

public static void Test()
{
    var @struct = new MockStruct();

    ArgumentNullInline2.ThrowIfNull(@struct);
}
}
""";
            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(new[] { typeof(_.Foo.ArgumentNullInline2).Assembly }) //// the alias above makes argumentnullinline2 come from oddtrottercore; maybe that's your issue?
                .AddImports(new[] { "System", "_.Foo" }));

            var compilerOutput = script.Compile();
        }
    }
}
