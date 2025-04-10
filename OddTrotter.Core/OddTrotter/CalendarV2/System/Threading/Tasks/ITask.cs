/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    ////[AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]
    public interface ITask<out T>
    {
        /// <inheritdoc cref="Task{TResult}.GetAwaiter"/>
        ITaskAwaiter<T> GetAwaiter();

        /// <inheritdoc cref="Task{TResult}.ConfigureAwait(bool)"/>
        IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext);
    }
}
