namespace System
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInlineCompilationTests
    {
        [TestMethod]
        public void ThrowIfNullStruct()
        {
            //// TODO clean up csproj file and oddtrotter.core csproj if needed
            //// TODO clean up the embedded resource code
            //// TODO getcallingassembly doesn't work for "production" code, is there a different way to get at this data?
            
            var code = this.GetResourceString();

            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(new[] { typeof(ArgumentNullInline).Assembly }));

            var compilerOutput = script.Compile();

            Assert.AreEqual(1, compilerOutput.Length);
            Assert.AreEqual("CS0452", compilerOutput[0].Id);
        }

        private string GetResourceString([CallerMemberName] string? callingMethod = null)
        {
            var type = this.GetType();
            var assembly = type.Assembly;

            var path = $"{type.Namespace}.{type.Name}Resources.{callingMethod}.cs";

            return GetResourceString(path, assembly);
        }

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
