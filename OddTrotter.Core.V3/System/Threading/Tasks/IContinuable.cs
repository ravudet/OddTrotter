/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System;

    using Fx.Realizable;

    public interface IContinuable<out TSource>
        where TSource : allows ref struct
    {
        Realizable<TResult> ContinueWith<TResult>(
            Func<TSource, TResult> sourceContinuation,
            Func<Exception, TResult> exceptionContinuation,
            Func<OperationCanceledException, TResult> canceledContinuation)
            where TResult : allows ref struct;
    }
}
