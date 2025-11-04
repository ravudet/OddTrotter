namespace Fx
{
    using System;

    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        Realizable<TResult> Apply<TResult, TContext, TContinuable>(
            AsyncRefContextualizedMap<TLeft, TContext, TContinuable, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TContinuable, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct;
    }

    public readonly ref struct Realizable<T>
        where T : allows ref struct
    {
    }

    public interface IContinuable<out TSource>
        where TSource : allows ref struct
    {
        Realizable<TResult> ContinueWith<TResult>(Func<TSource, TResult> continuation)
            where TResult : allows ref struct;
    }

    public delegate TContinuable AsyncRefContextualizedMap<in TValue, TContext, out TContinuable, out TResult>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TContinuable : IContinuable<TResult>, allows ref struct
        where TResult : allows ref struct;






    public readonly ref struct RefEither
    {
    }
}
