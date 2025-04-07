namespace Fx.Either.Mixins
{
    using System;
    using System.Threading.Tasks;

    public interface IAsyncMixin<out TLeft, out TRight> : IEither<TLeft, TRight>
    {
        Task<TResult> ApplyAsync<TResult, TContext>(
            Func<TLeft, TContext, Task<TResult>> leftMap,
            Func<TRight, TContext, Task<TResult>> rightMap,
            TContext context);
    }
}
