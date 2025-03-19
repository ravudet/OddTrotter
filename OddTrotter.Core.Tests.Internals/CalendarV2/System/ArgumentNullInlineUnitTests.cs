namespace System
{
    extern alias OddTrotterCore;

    using ExternalArgumentNullInline = OddTrotterCore::System.ArgumentNullInline;

    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;

    [TestClass]
    public sealed class ExternalArgumentNullInlineUnitTests
    {
        [TestMethod]
        public void ThrowIfNullNullClass()
        {
            MockClass @class =
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                null
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                ;

            Assert.ThrowsException<ArgumentNullException>(() => ExternalArgumentNullInline.ThrowIfNull(@class));
        }

        private sealed class MockClass
        {
        }

        [TestMethod]
        public void ThrowIfNullNonNullClass()
        {
            var @class = new MockClass();

            var returned = ExternalArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullNullableClass()
        {
            MockClass? @class = null;

            Assert.ThrowsException<ArgumentNullException>(() => ExternalArgumentNullInline.ThrowIfNull(@class));
        }

        [TestMethod]
        public void ThrowIfNullNonNullNullableClass()
        {
            MockClass? @class = new MockClass();

            var returned = ExternalArgumentNullInline.ThrowIfNull(@class);

            Assert.AreEqual(@class, returned);
        }

        [TestMethod]
        public void ThrowIfNullNullStruct()
        {
            MockStruct? @struct = null;

            Assert.ThrowsException<ArgumentNullException>(() => ExternalArgumentNullInline.ThrowIfNull(@struct));
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

            var returned = ExternalArgumentNullInline.ThrowIfNull(@struct);

            Assert.AreEqual(@struct, returned);
        }

        [TestMethod]
        public void Play()
        {
            var code = "";

            var assembly = this.GetType().Assembly;
            var names = assembly.GetManifestResourceNames();
            using (var resourceStream = assembly.GetManifestResourceStream("CalendarV2.System.ArgumentNullInlineCompilationTestResources.Play.cs"))
            {
                //// TODO this is all terrible, but particularly this line
                Assert.IsNotNull(resourceStream);

                using (var textReader = new StreamReader(resourceStream))
                {
                    code = textReader.ReadToEnd();
                }
            }

            //// TODO are embedded resources working?
            /*var code =
"""
using System;

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
""";*/
            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(new[] { typeof(ArgumentNullInline).Assembly }));

            var compilerOutput = script.Compile();

            Assert.AreEqual(1, compilerOutput.Length);
            Assert.AreEqual("CS0452", compilerOutput[0].Id);
        }
    }
}
