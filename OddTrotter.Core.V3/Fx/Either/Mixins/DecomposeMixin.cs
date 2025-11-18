/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either.Mixins
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Either;

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
            return @delegate(either, out left, out right);
        }
    }
}
