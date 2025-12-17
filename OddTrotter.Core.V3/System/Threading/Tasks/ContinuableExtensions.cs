namespace System.Threading.Tasks
{
    using Fx.Realizable;

    public static class ContinuableExtensions
    {
        public static Realizable<TResult> ContinueWith<TValue, TResult>(this IContinuable<TValue> continuable, Func<TValue, TResult> sourceContinuation)
        {
            return continuable.ContinueWith(sourceContinuation, _ => throw _, _ => throw _); //// TODO this loses the stack traces
        }
    }
}
