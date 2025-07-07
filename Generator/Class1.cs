namespace Generator
{
    using Microsoft.CodeAnalysis;

    [Generator]
    public class Class1 : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
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
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //// TODO?
        }
    }
}
