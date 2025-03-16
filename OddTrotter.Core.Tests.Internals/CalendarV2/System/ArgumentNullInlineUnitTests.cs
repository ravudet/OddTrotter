namespace System
{
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInlineUnitTests
    {
        [TestMethod]
        public void ThrowIfNullNullClass()
        {
            MockClass @class =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@class));
        }

        private sealed class MockClass
        {
        }

        [TestMethod]
        public void ThrowIfNullNonNullClass()
        {
            var @class = new MockClass();

            var returned = ArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullNullableClass()
        {
            MockClass? @class = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@class));
        }

        [TestMethod]
        public void ThrowIfNullNonNullNullableClass()
        {
            MockClass? @class = new MockClass();

            var returned = ArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullStruct()
        {
            MockStruct? @struct = null;

            Assert.ThrowsException<ArgumentNullException>(() => ArgumentNullInline.ThrowIfNull(@struct));
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

            var returned = ArgumentNullInline.ThrowIfNull(@struct);

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

    ArgumentNullInline.ThrowIfNull(@struct);
}
}
""";
            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default.WithReferences(new[] { typeof(ArgumentNullInline).Assembly }).AddImports(new[] { "System" }));

            var compilerOutput = script.Compile();
        }       
    }
}
