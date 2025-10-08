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

    public static class Realized
    {
        /*public static IRealized<T> Result<T>(T result)
            where T : allows ref struct
        {
        }

        public static IRealized<T> Exception<T>(Exception exception)
            where T : allows ref struct
        {
        }

        public static IRealized<T> Canceled<T>()
            where T : allows ref struct
        {
        }*/
    }

    public interface IRealized<out T> where T : allows ref struct
    {
        internal protected void Internal();

        public interface IResult : IRealized<T> //// TODO do you want 3 interfaces? do you want an `apply` method? both? if it's just an `apply` method, then do you really need the `internal` method?
        {
            T Value { get; }
        }

        public interface IException : IRealized<T>
        {
            Exception Value { get; }
        }

        public interface ICanceled : IRealized<T>
        {
        }
    }
}
