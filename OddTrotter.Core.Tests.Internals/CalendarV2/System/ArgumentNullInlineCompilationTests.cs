/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Collections.Immutable;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class ArgumentNullInlineCompilationTests
    {
        [TestMethod]
        public void ThrowIfNullStruct()
        {
            var compilerOutput = this.CompileTestResource();

            Assert.AreEqual(1, compilerOutput.Length);
            Assert.AreEqual("CS0452", compilerOutput[0].Id);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="testName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="testName"/> was explicitly provided a <see langword="null"/> value by the caller
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown if resource length for the test with name <paramref name="testName"/> is greater than
        /// <see cref="Int64.MaxValue"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if the resource for the test with name <paramref name="testName"/> cannot be loaded into memory because there
        /// is insufficient memory to allocate a buffer.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown if an I/O error occurs while reading the resource for the test with name <paramref name="testName"/>.
        /// </exception>
        private ImmutableArray<Diagnostic> CompileTestResource([CallerMemberName] string? testName = null)
        {
            ArgumentNullException.ThrowIfNull(testName);

            var code = this.GetTestResourceString(testName);

            var script = CSharpScript.Create(
                code,
                ScriptOptions.Default
                    .WithReferences(typeof(ArgumentNullInline).Assembly));

             return script.Compile();
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="testName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="testName"/> was explicitly provided a <see langword="null"/> value by the caller
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown if resource length for the test with name <paramref name="testName"/> is greater than
        /// <see cref="Int64.MaxValue"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if the resource for the test with name <paramref name="testName"/> cannot be loaded into memory because there
        /// is insufficient memory to allocate a buffer.
        /// </exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
        private string GetTestResourceString([CallerMemberName] string? testName = null)
        {
            ArgumentNullException.ThrowIfNull(testName);

            var type = this.GetType();
            var assembly = type.Assembly;

            var path = $"{type.Namespace}.{type.Name}Resources.{testName}.cs";

            return GetResourceString(assembly, path);
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="resourceAssembly"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="resourceAssembly"/> or <paramref name="resourcePath"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="resourcePath"/> is <see cref="string.Empty"/>
        /// </exception>
        /// <exception cref="FileLoadException">
        /// Thrown if a file that was found could not be loaded. This won't be thrown for <paramref name="resourceAssembly"/>s
        /// that are obtained by calling <see cref="object.GetType"/> or <see langword="typeof"/>
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if <paramref name="resourcePath"/> was not found. This won't be thrown for
        /// <paramref name="resourceAssembly"/>s that are obtained by calling <see cref="object.GetType"/> or
        /// <see langword="typeof"/>
        /// </exception>
        /// <exception cref="BadImageFormatException">
        /// Thrown if <paramref name="resourcePath"/> is not a valid assembly. This won't be thrown for
        /// <paramref name="resourceAssembly"/>s that are obtained by calling <see cref="object.GetType"/> or
        /// <see langword="typeof"/>
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown if resource length is greater than <see cref="Int64.MaxValue"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no resource could be found at <paramref name="resourcePath"/> embedded in
        /// <paramref name="resourceAssembly"/>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if there is insufficient memory to allocate a buffer for the returned <see cref="string"/>.
        /// </exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
        /// <remarks>
        /// you should figure out when ioexceptions occur and repro that for better documentation
        /// </remarks>
        private static string GetResourceString(Assembly resourceAssembly, string resourcePath)
        {
            ArgumentNullException.ThrowIfNull(resourceAssembly);
            ArgumentException.ThrowIfNullOrEmpty(resourcePath);

            using (var resourceStream = GetResourceStream(resourceAssembly, resourcePath))
            {
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="resourceAssembly"></param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="resourceAssembly"/> or <paramref name="resourcePath"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="resourcePath"/> is <see cref="string.Empty"/>
        /// </exception>
        /// <exception cref="FileLoadException">
        /// Thrown if a file that was found could not be loaded. This won't be thrown for <paramref name="resourceAssembly"/>s
        /// that are obtained by calling <see cref="object.GetType"/> or <see langword="typeof"/>
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if <paramref name="resourcePath"/> was not found. This won't be thrown for
        /// <paramref name="resourceAssembly"/>s that are obtained by calling <see cref="object.GetType"/> or
        /// <see langword="typeof"/>
        /// </exception>
        /// <exception cref="BadImageFormatException">
        /// Thrown if <paramref name="resourcePath"/> is not a valid assembly. This won't be thrown for
        /// <paramref name="resourceAssembly"/>s that are obtained by calling <see cref="object.GetType"/> or
        /// <see langword="typeof"/>
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown if resource length is greater than <see cref="Int64.MaxValue"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no resource could be found at <paramref name="resourcePath"/> embedded in
        /// <paramref name="resourceAssembly"/>
        /// </exception>
        /// <remarks>
        /// `assembly.getmanifestresourcestream` throws some exceptions that only happen during dynamic assembly load situations
        /// (and not from stuff like `object.gettype` or `typeof`):
        /// 
        /// 1. fileloadexception: EcmaAssembly.GetManifestResourceStream -> EcmaModule.GetInternalManifestResourceInfo ->
        /// MetadataLoadContext.resolveassembly -> tryresolveassembly -> resolvetoassemblyorexceptionassembly -> 
        /// tryfindassemblybycallingresolvehandler
        /// 2. filenotfoundexception: EcmaAssembly.Getmanifestresourcestream -> RoAssembly.getfile -> new filestream
        /// 3. badimageformatexception: EcmaAssembly.Getmanifestresourcestream -> EcmaModule.GetInternalManifestResourceInfo
        /// 
        /// you should figure out how to repro these cases; you should also create a "runtimetype" that derives `type` and a
        /// runtimeassembly` that derives `assembly` and have `runtimetype.assembly` return `runtimeassembly`; then, you can have
        /// an extension method that looks like `gettype` but returns a `runtimetype` and methods like *this* one could take in a
        /// `runtimeassembly` instead of `assembly` and know that they won't get the above 3 exceptions
        /// 
        /// also consider fixing the msdn docs for these exceptions; things like `filenotfoundexception` and
        /// badimageformatexception` are certainly not correct, and probably `fileloadexception` could be significantly more
        /// clear
        /// </remarks>
        private static Stream GetResourceStream(Assembly resourceAssembly, string resourcePath)
        {
            ArgumentNullException.ThrowIfNull(resourceAssembly);
            ArgumentException.ThrowIfNullOrEmpty(resourcePath);

            Stream? resourceStream = null;
            try
            {
                
                resourceStream = resourceAssembly.GetManifestResourceStream(resourcePath);
                if (resourceStream == null)
                {
                    var exceptionMessage = $"No embeded resource was found in assembly '{resourceAssembly.FullName}' at the resource path '{resourcePath}'.";
                    throw new InvalidOperationException(exceptionMessage);
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
