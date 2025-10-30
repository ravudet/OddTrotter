namespace OddTrotter.Core.V3
{
    public interface IAwaitable<out T, out TAwaiter, out TConfiguredAwaitable>
        where T : allows ref struct
        where TAwaiter : IAwaiter<T>, allows ref struct
        where TConfiguredAwaitable : IAwaitable<T, TAwaiter, TConfiguredAwaitable>, allows ref struct
    {
        TAwaiter GetAwaiter();

        TConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext);
    }

    public interface IAwaitable<out T> : IAwaitable<T, IAwaiter<T>, IAwaitable<T>>
        where T : allows ref struct
    {
    }

    public interface IAwaiter<out T>
        where T : allows ref struct
    {
    }
}
