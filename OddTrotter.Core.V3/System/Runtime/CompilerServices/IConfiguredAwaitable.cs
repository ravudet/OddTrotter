namespace System.Runtime.CompilerServices
{
    public interface IConfiguredAwaitable<out T> //// TODO this is design like .NET does it, but i don't know if i like that; we could have `configureawait` return a new instance of `iawaitable` or something
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();
    }
}
