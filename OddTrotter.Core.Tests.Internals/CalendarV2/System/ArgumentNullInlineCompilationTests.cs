////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Collections.Immutable;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInlineCompilationTests
    {
        [TestMethod]
        public void ThrowIfNullStruct()
        {
            var compilerOutput = this.Compile();

            Assert.AreEqual(1, compilerOutput.Length);
            Assert.AreEqual("CS0452", compilerOutput[0].Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callingMethod"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callingMethod"/> was explicitly provided a <see langword="null"/> value by the caller</exception>
        private ImmutableArray<Diagnostic> Compile([CallerMemberName] string? callingMethod = null)
        {
            ArgumentNullException.ThrowIfNull(callingMethod);

            var code = this.GetResourceString(callingMethod);

            var script = CSharpScript.Create(
                code,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(new[] { typeof(ArgumentNullInline).Assembly }));

             return script.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callingMethod"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callingMethod"/> was explicitly provided a <see langword="null"/> value by the caller</exception>
        private string GetResourceString([CallerMemberName] string? callingMethod = null)
        {
            ArgumentNullException.ThrowIfNull(callingMethod);

            var type = this.GetType();
            var assembly = type.Assembly;

            var path = $"{type.Namespace}.{type.Name}Resources.{callingMethod}.cs";

            return GetResourceString(path, assembly);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetResourceString(string path, Assembly assembly)
        {
            //// TODO better parameter names in all of the methods
            using (var resourceStream = GetResourceStream(path, assembly))
            {
                using (var textReader = new StreamReader(resourceStream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

        public static Stream GetResourceStream(string path, Assembly assembly)
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
    }
}
