/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    public interface ITask<out T> : IContinuable<T> //// TODO call this awaitable //// TODO probably have a configure await on that; if it's not directly on iawaitable, there should be a configurableawaitable or something //// TODO iawaitable should have two generics, one for the return value and another for the awaiter type
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();

        IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext);
    }
}
