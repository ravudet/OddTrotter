/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public interface IAwaiter<out T> : ICriticalNotifyCompletion
        where T : allows ref struct
    {
        bool IsCompleted { get; }

        T GetResult();
    }
}
