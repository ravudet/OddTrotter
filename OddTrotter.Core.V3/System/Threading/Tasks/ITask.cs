/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    [AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]
    public interface ITask<out T> : ITask<IConfiguredAwaitable<T>, IAwaiter<T>, T>
        where T : allows ref struct
    {
    }
    
    /*public interface ITask<out T> : IContinuable<T> //// TODO call this awaitable //// TODO probably have a configure await on that; if it's not directly on iawaitable, there should be a configurableawaitable or something //// TODO iawaitable should have two generics, one for the return value and another for the awaiter type, this way you can accommodate value type awaiters
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();

        IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext);
    }*/

    public interface ITask<out TConfiguredAwaitable, out TAwaiter, out TValue> : IContinuable<TValue>
        where TConfiguredAwaitable : IConfiguredAwaitable<TAwaiter, TValue>, allows ref struct //// TODO should the configuredawaitable's awaiter type be parameterized?
        where TAwaiter : IAwaiter<TValue>, allows ref struct
        where TValue : allows ref struct
    {
        TAwaiter GetAwaiter();

        TConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext);
    }
}
