namespace Fx.Either
{
    public delegate IEitherMonad<TLeft, TRight> Unit<TLeft, TRight>(IEither<TLeft, TRight> source);

    public interface IEitherMonad<out TLeft, out TRight> : IEither<TLeft, TRight>
    {
        IEither<TLeft, TRight> Source { get; }

        Unit<TLeft2, TRight2> Unit<TLeft2, TRight2>(); //// TODO better names for the type parameters
    }
}
