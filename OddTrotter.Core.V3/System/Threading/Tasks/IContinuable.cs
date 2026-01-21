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
            Func<OperationCanceledException, TResult> canceledContinuation) //// TODO should there be overloads that don't have the exception and cancelation stuff, that way the standalone `throw` statement can still preserve the stack?
            where TResult : allows ref struct;
    }
}
