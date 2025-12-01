namespace Fx.Either.Mixins
{
    using System.Runtime.CompilerServices;

    public static class DecomposeMixin
    {
        public static bool TryCreate<TCasted, TEither, TLeft, TRight>(TEither either, DecomposeDelegate<TEither, TLeft, TRight> decomposeDelegate, out TCasted casted)
            where TCasted : struct, allows ref struct
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            if (typeof(TCasted) == typeof(DecomposeMixin<TEither, TLeft, TRight>))
            {
                var mixin = new DecomposeMixin<TEither, TLeft, TRight>(
                    either,
                    decomposeDelegate);
                casted = Unsafe.As<DecomposeMixin<TEither, TLeft, TRight>, TCasted>(ref mixin);
                return true;
            }

            casted = default;
            return false;
        }
    }
}
