/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    [AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]
    public interface ITask<out T> where T : allows ref struct
    {
        /// <inheritdoc cref="Task{TResult}.GetAwaiter"/>
        ITaskAwaiter<T> GetAwaiter();

        /// <inheritdoc cref="Task{TResult}.ConfigureAwait(bool)"/>
        IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext);
		
        //// TODO put this on its own interface
        //// TODO update the delegate parameter to not take `itask` but something that represents the actual result
		ITask<TResult> ContinueWith<TResult>(Func<ITask<T>,TResult> continuationFunction) //// TODO this should take in like a "futureValue" or something that has the exception, canceled, value DU
			where TResult : allows ref struct;
    }
}
