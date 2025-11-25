/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either.Mixins
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Either;

    public interface IDecomposeMixin<out TEither, TLeft, TRight> //// TODO covariance //// TODO i think the issue here is less about covariance and more about the fact that this mixin completely destroys the purpose of `ieither`; if you wanted covariance, you would have `decompose` return an interface and have an `out bool isLeft`; the interface would then have a `left` and `right` properties; but that instance would itself potentially be internally inconsistent (even if you put `isleft` on the interface); and all of this is what `ieither` is intended to model anyway //// TODO probably just look at the likelihood of someone needing to leverage covariance on `idecomposemixin` specifically
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
}
