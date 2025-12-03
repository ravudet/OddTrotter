namespace System.Runtime.CompilerServices
{
    public interface IConfiguredAwaitable<out T>
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();
    }
}
