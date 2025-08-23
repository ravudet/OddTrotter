/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public interface ITaskAwaiter<out T> : ICriticalNotifyCompletion
    {
        /// <inheritdoc cref="TaskAwaiter{TResult}.IsCompleted"/>
        bool IsCompleted { get; }

        /// <inheritdoc cref="TaskAwaiter{TResult}.GetResult"/>
        T GetResult();
    }
}
