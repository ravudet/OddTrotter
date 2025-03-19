namespace System
{
    extern alias OddTrotterCore;

    using ExternalArgumentNullInline = OddTrotterCore::System.ArgumentNullInline;

    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Reflection;

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
            //// TODO clean up csproj file and oddtrotter.core csproj if needed
            var code = this.GetResourceString("CalendarV2.System.ArgumentNullInlineCompilationTestResources.Play.cs");

            code = V2.GetResourceString("CalendarV2.System.ArgumentNullInlineCompilationTestResources.Play.cs");

            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(new[] { typeof(ArgumentNullInline).Assembly }));

            var compilerOutput = script.Compile();

            Assert.AreEqual(1, compilerOutput.Length);
            Assert.AreEqual("CS0452", compilerOutput[0].Id);
        }

        private string GetResourceString(string path)
        {
            var assembly = this.GetType().Assembly;

            return V2.GetResourceString(path, assembly);
        }

        private static string GetResourceString(Assembly assembly, string path)
        {
            assembly = Assembly.GetCallingAssembly();

            //// TODO better parameter names in all of the methods
            using (var resourceStream = GetResourceStream(assembly, path))
            {
                using (var textReader = new StreamReader(resourceStream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

        private static Stream GetResourceStream(Assembly assembly, string path)
        {
            Stream? resourceStream = null;
            try
            {
                resourceStream = assembly.GetManifestResourceStream(path);
                if (resourceStream == null)
                {
                    throw new InvalidOperationException("tODO");
                }
            }
            catch
            {
                resourceStream?.Dispose();
                throw;
            }

            return resourceStream;
        }

        private static class V2
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="path"></param>
            /// <param name="assembly">assembly containing the resource, or `null` if the caller's assembly contains the resource TODO rewrite this</param>
            /// <returns></returns>
            public static string GetResourceString(string path, Assembly? assembly = null)
            {
                if (assembly == null)
                {
                    assembly = Assembly.GetCallingAssembly();
                }

                //// TODO better parameter names in all of the methods
                using (var resourceStream = GetResourceStream(path, assembly))
                {
                    using (var textReader = new StreamReader(resourceStream))
                    {
                        return textReader.ReadToEnd();
                    }
                }
            }

            public static Stream GetResourceStream(string path, Assembly? assembly = null)
            {
                if (assembly == null)
                {
                    assembly = Assembly.GetCallingAssembly();
                }

                Stream? resourceStream = null;
                try
                {
                    resourceStream = assembly.GetManifestResourceStream(path);
                    if (resourceStream == null)
                    {
                        throw new InvalidOperationException("tODO");
                    }
                }
                catch
                {
                    resourceStream?.Dispose();
                    throw;
                }

                return resourceStream;
            }
        }
    }
}
