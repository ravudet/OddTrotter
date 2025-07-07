namespace Generator
{
    using Microsoft.CodeAnalysis;

#pragma warning disable RS1041 // Compiler extensions should be implemented in assemblies targeting netstandard2.0
    [Generator]
#pragma warning restore RS1041 // Compiler extensions should be implemented in assemblies targeting netstandard2.0
#pragma warning disable RS1042 // Implementations of this interface are not allowed
#pragma warning disable RS1036 // Specify analyzer banned API enforcement setting
    public class Class1 : ISourceGenerator
#pragma warning restore RS1036 // Specify analyzer banned API enforcement setting
#pragma warning restore RS1042 // Implementations of this interface are not allowed
    {
        public void Execute(GeneratorExecutionContext context)
        {
#pragma warning disable RS1035 // Specify analyzer banned API enforcement setting
            context.AddSource(
                "ravudettest",
"""
namespace Foo
{
    public class Bar
    {
        public void DoWork()
        {
            System.Console.WriteLine("hello");
        }
    }
}
"""
                );
#pragma warning restore RS1035 // Specify analyzer banned API enforcement setting
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //// TODO?
        }
    }
}
