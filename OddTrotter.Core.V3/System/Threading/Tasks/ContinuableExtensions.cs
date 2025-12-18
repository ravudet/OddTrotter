namespace System.Threading.Tasks
{
    using Fx.Realizable;

    public static class ContinuableExtensions
    {
        public static Realizable<TResult> ContinueWith<TValue, TResult>(this IContinuable<TValue> continuable, Func<TValue, TResult> sourceContinuation)
        {
            return continuable.ContinueWith(sourceContinuation, _ => throw _, _ => throw _); //// TODO this loses the stack traces //// TODO .net uses something called `restoredispatchstate` to handle this, actually: https://source.dot.net/#System.Private.CoreLib/src/System/Exception.CoreCLR.cs,50a6552033907120,references
        }
    }
}
