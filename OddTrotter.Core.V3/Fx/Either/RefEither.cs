/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx;
    using Fx.Either.Mixins;
    using Fx.Realizable;

    public readonly ref struct RefEither<TLeft, TRight> : IEither<RefEither<TLeft, TRight>, TLeft, TRight>, ICastable, IDecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        private readonly RefNullable<TLeft> left;

        private readonly RefNullable<TRight> right;

        public RefEither(TLeft left)
        {
            this.left = new RefNullable<TLeft>(left);

            right = new RefNullable<TRight>();
        }

        public TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight> TypeHolder
        {
            get
            {
                return new TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight>(this);
            }
        }

        public RefEither(TRight right)
        {
            this.right = new RefNullable<TRight>(right);

            left = new RefNullable<TLeft>();
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, 
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            if (this.left.TryGetValue(out var left))
            {
                return
                    leftMap(left, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new LeftMapException(exception),
                        canceled => throw canceled);
            }
            else if (this.right.TryGetValue(out var right))
            {
                return
                    rightMap(right, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new RightMapException(exception),
                        canceled => throw canceled);
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        public bool Decompose([MaybeNullWhen(false)] out TLeft value, [MaybeNullWhen(true)] out TRight future)
        {
            if (left.TryGetValue(out value))
            {
                future = default;
                return true;
            }
            else if (right.TryGetValue(out future))
            {
                value = default;
                return false;
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted)
            where TCasted : struct, allows ref struct
        {
            if (Realizable<int>.TryDecompose(
                this,
                (RefEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => either.Decompose(out left, out right),
                out casted))
            {
                return true;
            }

            /*if (typeof(TCasted) == typeof(DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>))
            {
                var mixin = new DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>(
                    this,
                    (RefEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => either.Decompose(out left, out right));
                casted = Unsafe.As<DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>, TCasted>(ref mixin);
                return true;
            }*/

            casted = default;
            return false;
        }
    }
}
