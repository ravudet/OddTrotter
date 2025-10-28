namespace OddTrotter.Core.V3
{
    public interface IAwaitable<out T, out TAwaiter, out TConfiguredAwaitable>
        where T : allows ref struct
        where TAwaiter : allows ref struct
        where TConfiguredAwaitable : allows ref struct
    {
        TAwaiter GetAWaiter();

        TConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext);
    }
}
