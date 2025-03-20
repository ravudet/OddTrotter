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
        private static Stream GetResourceStream(Assembly assembly, string path)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            ArgumentException.ThrowIfNullOrEmpty(path);

            Stream? resourceStream = null;
            try
            {
                //// fileloadexception: EcmaAssembly.GetManifestResourceStream -> EcmaModule.GetInternalManifestResourceInfo -> MetadataLoadContext.resolveassembly -> tryresolveassembly -> resolvetoassemblyorexceptionassembly -> tryfindassemblybycallingresolvehandler
                //// filenotfoundexception: EcmaAssembly.Getmanifestresourcestream -> RoAssembly.getfile -> new filestream
                //// badimageformatexception: EcmaAssembly.Getmanifestresourcestream -> EcmaModule.GetInternalManifestResourceInfo
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

        public void Frob()
        {
            ////Bar(new Frub());
        }

        public void Bar(dynamic bar)
        {
            new Foo().GetType();

            bar.GetType();

            new Foo().GetRuntimeType();

            new object().GetRuntimeType();
        }

        public ref struct Frub
        {
        }

        public struct Foo
        {
        }

        public sealed class RuntimeAssembly : Assembly
        {
            private readonly Assembly assembly;

            private RuntimeAssembly(Assembly assembly)
            {
                this.assembly = assembly;
            }

            public static RuntimeAssembly Create1<T>(T instance) where T : class
            {
                return new RuntimeAssembly(instance.GetType().Assembly);
            }

            public static RuntimeAssembly Create2<T>(T instance) where T : struct
            {
                return new RuntimeAssembly(instance.GetType().Assembly);
            }
        }

        public sealed class RuntimeType<T> : Type
        {
            private readonly T instance;
            private readonly Type type;

            private RuntimeType(T instance)
            {
                this.type = instance!.GetType(); //// TODO why is forgiving needed here?
                this.instance = instance;

                this.RuntimeAssemblyProp = RuntimeAssembly.Create2
            }

            public static RuntimeType<T2> Create1<T2>(T2 instance) where T2 : class
            {
                return new RuntimeType<T2>(instance);
            }

            public static RuntimeType<T2> Create2<T2>(T2 instance) where T2 : struct
            {
                return new RuntimeType<T2>(instance);
            }

            public override Assembly Assembly => throw new NotImplementedException();

            public Assembly RuntimeAssemblyProp { get; } //// TODO can you get a better name?

            public override string? AssemblyQualifiedName => throw new NotImplementedException();

            public override Type? BaseType => throw new NotImplementedException();

            public override string? FullName => throw new NotImplementedException();

            public override Guid GUID => throw new NotImplementedException();

            public override Module Module => throw new NotImplementedException();

            public override string? Namespace => throw new NotImplementedException();

            public override Type UnderlyingSystemType => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override Type? GetElementType()
            {
                throw new NotImplementedException();
            }

            public override EventInfo? GetEvent(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo? GetField(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
            public override Type? GetInterface(string name, bool ignoreCase)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetInterfaces()
            {
                throw new NotImplementedException();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type? GetNestedType(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsArrayImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsByRefImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsCOMObjectImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPointerImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPrimitiveImpl()
            {
                throw new NotImplementedException();
            }
        }
    }

    public static class RuntimeTypeExtensions1
    {
        public static RuntimeType<T> GetRuntimeType<T>(this T instance) where T : class
        {
            return RuntimeType<T>.Create1(instance);
        }
    }

    public static class RuntimeTypeExtensions2
    {
        public static RuntimeType<T> GetRuntimeType<T>(this T instance) where T : struct
        {
            return RuntimeType<T>.Create2(instance);
        }
    }
}
