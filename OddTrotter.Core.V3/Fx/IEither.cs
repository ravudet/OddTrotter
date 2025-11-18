/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    public interface IEither<TEither, TLeft, TRight> : IEither<TLeft, TRight>
        where TEither : IEither<TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        TypeHolder<TEither, TLeft, TRight> TypeHolder { get; }
    }

    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        /// <exception cref="LeftMapException"></exception>
        /// <exception cref="RightMapException"></exception>
        Realizable<TResult> Apply<TResult, TContext, TContinuable>(
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













    //// TODO then split this into files
    //// TODO go through files and remove the comment block
    //// TODO implement a test with mapping exceptions being throw
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
}
