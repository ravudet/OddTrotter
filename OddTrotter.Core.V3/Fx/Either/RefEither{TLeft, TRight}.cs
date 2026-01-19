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

        public TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight> AsEither
        {
            get
            {
                return new TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight>(this);
            }
        }

        TypeHolder<RefEither<TLeft, TRight>, TLeft, TRight> IAsAble<RefEither<TLeft, TRight>, TLeft, TRight>.As => AsEither;

        public RefEither(TRight right)
        {
            this.right = new RefNullable<TRight>(right);

            left = new RefNullable<TLeft>();
        }

        public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, 
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            if (this.left.TryGetValue(out var left))
            {
                try
                {
                    return
                        leftMap(left, ref context)
                        .ContinueWith(
                            result => result,
                            exception => throw new LeftMapException(exception),
                            canceled => throw canceled);
                }
                catch (Exception exception)
                {
                    return Realizable.FromException<TResult>(new LeftGenerationException(exception));
                }
            }
            else if (this.right.TryGetValue(out var right))
            {
                try
                {
                    return
                        rightMap(right, ref context)
                        .ContinueWith(
                            result => result,
                            exception => throw new RightMapException(exception),
                            canceled => throw canceled);
                }
                catch (Exception exception)
                {
                    return Realizable.FromException<TResult>(new RightGenerationException(exception));
                }
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        bool IDecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>.Decompose([MaybeNullWhen(false)] out TLeft value, [MaybeNullWhen(true)] out TRight future)
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
            if (DecomposeMixin.TryCreate<TCasted, RefEither<TLeft, TRight>, TLeft, TRight>(
                this,
                out casted))
            {
                return true;
            }

            casted = default;
            return false;
        }
    }
}
