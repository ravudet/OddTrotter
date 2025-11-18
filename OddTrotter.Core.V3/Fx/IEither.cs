/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using System.Diagnostics.CodeAnalysis;
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







    public interface IDecomposeMixin<out TEither, TLeft, TRight> //// TODO covariance
        where TEither : IEither<TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        bool Decompose([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right);
    }

    public delegate bool DecomposeDelegate<TEither, TLeft, TRight>(TEither either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
        where TEither : IEither<TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct;

    public readonly ref struct DecomposeMixin<TEither, TLeft, TRight> : IDecomposeMixin<TEither, TLeft, TRight>
        where TEither : IEither<TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        private readonly TEither either;
        private readonly DecomposeDelegate<TEither, TLeft, TRight> @delegate;

        public DecomposeMixin(TEither either, DecomposeDelegate<TEither, TLeft, TRight> @delegate)
        {
            this.either = either;
            this.@delegate = @delegate;
        }

        public bool Decompose([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
        {
            return this.@delegate(this.either, out left, out right);
        }
    }







    public interface ICastable
    {
        bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct; //// note: `tcasted` *must* be a struct for mixin purposes when ref structs are allowed because we can't actually return an interface that also encapsulates the thing being cast
    }

    public readonly ref struct TypeHolder<TSelf, T1, T2>
        where TSelf : allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        public TypeHolder(TSelf self)
        {
            this.Self = self;
        }

        public TSelf Self { get; }
    }













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
