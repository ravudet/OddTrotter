/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static System.ArgumentNullInlineCompilationTests;

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
                ScriptOptions.Default
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

            return GetResourceString(assembly, path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static string GetResourceString(Assembly assembly, string path)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            ArgumentException.ThrowIfNullOrEmpty(path);

            //// TODO better parameter names in all of the methods
            using (var resourceStream = GetResourceStream(assembly, path))
            {
                using (var textReader = new StreamReader(resourceStream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> or <paramref name="path"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="path"/> is <see cref="string.Empty"/></exception>
        /// <remarks>
        /// `assembly.getmanifestresourcestream` throws some exceptions that only happen during dynamic assembly load situations (and not from stuff like `object.gettype` or `typeof`):
        /// 
        /// fileloadexception: EcmaAssembly.GetManifestResourceStream -> EcmaModule.GetInternalManifestResourceInfo -> MetadataLoadContext.resolveassembly -> tryresolveassembly -> resolvetoassemblyorexceptionassembly -> tryfindassemblybycallingresolvehandler
        /// filenotfoundexception: EcmaAssembly.Getmanifestresourcestream -> RoAssembly.getfile -> new filestream
        /// badimageformatexception: EcmaAssembly.Getmanifestresourcestream -> EcmaModule.GetInternalManifestResourceInfo
        /// 
        /// you should figure out how to repro these cases; you should also create a "runtimetype" that derives `type` and a `runtimeassembly` that derives `assembly` and have `runtimetype.assembly` return `runtimeassembly`; then, you can have an extension method that looks like `gettype` but returns a `runtimetype` and methods like *this* one could take in a `runtimeassembly` instead of `assembly` and know that they won't get the above 3 exceptions
        /// </remarks>
        private static Stream GetResourceStream(Assembly assembly, string path)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            ArgumentException.ThrowIfNullOrEmpty(path);

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
