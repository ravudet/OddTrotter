/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System.Threading.Tasks;

    using Fx;
    using Fx.Realizable;

    public interface IAsAble<TSelf, T1, T2> 
        where TSelf : IAsAble<TSelf, T1, T2>, allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        TypeHolder<TSelf, T1, T2> As { get; }
    }

    public interface IEither<TEither, TLeft, TRight> : IEither<TLeft, TRight>, IAsAble<TEither, TLeft, TRight>
        where TEither : IEither<TEither, TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        TypeHolder<TEither, TLeft, TRight> AsEither { get; }
    }

    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        /// <exception cref="LeftMapException" awaited="true">thrown if the <typeparamref name="TContinuable"/> returned by <paramref name="leftMap"/> throws an exception</exception>
        /// <exception cref="RightMapException" awaited="true">thrown if the <typeparamref name="TContinuable"/> returned by <paramref name="rightMap"/> throws an exception</exception>
        /// <exception cref="LeftGenerationException" awaited="true">thrown if <paramref name="leftMap"/> throws when generating the <typeparamref name="TContinuable"/></exception> //// TODO you are having the "generation" exceptions be awaited so that the caller doesn't have to catch when `apply` is called *and then again* when `await` is called
        /// <exception cref="RightGenerationException" awaited="true">thrown if <paramref name="rightMap"/> throws when generating the <typeparamref name="TContinuable"/></exception> //// TODO you are having the "generation" exceptions be awaited so that the caller doesn't have to catch when `apply` is called *and then again* when `await` is called
        Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct;
    }

    public delegate TContinuable AsyncRefContextualizedContinuableMap<in TValue, TContext, out TContinuable, out TResult>(TValue value, ref TContext context) //// TODO you also need AsyncRefContextualizedTaskMap, AsyncRefContextualizedValueTaskMap, AsyncRefContextualizedITaskMap, AsyncRefContextualizedRealizableMap
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TContinuable : IContinuable<TResult>, allows ref struct
        where TResult : allows ref struct;
}
