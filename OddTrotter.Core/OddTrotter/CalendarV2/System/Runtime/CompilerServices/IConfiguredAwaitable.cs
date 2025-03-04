/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public interface IConfiguredAwaitable<out T>
    {
        /// <inheritdoc cref="ConfiguredTaskAwaitable{TResult}.GetAwaiter"/>
        ITaskAwaiter<T> GetAwaiter();
    }
}
