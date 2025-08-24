/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public interface IConfiguredAwaitable<out T> where T : allows ref struct
    {
        /// <inheritdoc cref="ConfiguredTaskAwaitable{TResult}.GetAwaiter"/>
        ITaskAwaiter<T> GetAwaiter();
    }
}
