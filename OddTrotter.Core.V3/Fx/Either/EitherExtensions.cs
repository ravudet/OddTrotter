/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either.Mixins;
    using Fx.Realizable;

    public static class EitherExtensions
    {
        public static IEither<TLeftResult, TRightResult> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
        {
            var realizable = either.SelectAsync(
                left => (IContinuable<TLeftResult>)new TaskWrapper<TLeftResult>(Task.FromResult(leftMap(left))),
                right => (IContinuable<TRightResult>)new TaskWrapper<TRightResult>(Task.FromResult(rightMap(right))));

            if (realizable.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.GetAwaiter().GetResult();
            }
        }

        public static Realizable<IEither<TLeftResult, TRightResult>> SelectAsync<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, IContinuable<TLeftResult>> leftMap,
            Func<TRightSource, IContinuable<TRightResult>> rightMap)
        {
            return either.Apply<IEither<TLeftResult, TRightResult>, bool, Realizable<IEither<TLeftResult, TRightResult>>>(
                (TLeftSource left, ref bool context) =>
                    leftMap(left)
                    .ContinueWith(
                        result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                (TRightSource right, ref bool context) =>
                    rightMap(right)
                    .ContinueWith(
                        result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                ref Context);
        }

        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            var future = either.Apply<TResult, bool, TaskWrapper<TResult>>(
                (TLeft left, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(leftMap(left))),
                (TRight right, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(rightMap(right))),
                ref Context);

            if (future.TypeHolder.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.GetAwaiter().GetResult();
            }
        }

        public static Realizable<TResult> Apply<TLeft, TRight, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            return either.ContinueWith(
                result => result.Apply(leftMap, rightMap),
                exception => throw exception,
                canceled => throw canceled);
        }


        public static bool Decompose<TEither, TLeft, TRight>(
            this TypeHolder<TEither, TLeft, TRight> either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TEither : IEither<TLeft, TRight>, ICastable, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return Decompose(either.Self, out left, out right);
        }

        public static bool Decompose<TLeft, TRight>(
            this IEither<TLeft, TRight> either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return new DecomposeCastable<TLeft, TRight>(either).TypeHolder.Decompose(out left, out right);
        }

        private readonly ref struct DecomposeCastable<TLeft, TRight> : IEither<DecomposeCastable<TLeft, TRight>, TLeft, TRight>, ICastable
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            private readonly IEither<TLeft, TRight> either;

            public DecomposeCastable(IEither<TLeft, TRight> either)
            {
                this.either = either;
            }

            public TypeHolder<DecomposeCastable<TLeft, TRight>, TLeft, TRight> TypeHolder
            {
                get
                {
                    return new TypeHolder<DecomposeCastable<TLeft, TRight>, TLeft, TRight>(this);
                }
            }

            public Realizable<TResult> Apply<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
                where TContinuable : IContinuable<TResult>, allows ref struct
            {
                return this.either.Apply(leftMap, rightMap, ref context);
            }

            public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted)
                where TCasted : struct, allows ref struct
            {
                if (typeof(TCasted) == typeof(DecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>) && this.either is IDecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight> mixin)
                {
                    var decomposeMixin = new DecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>(
                        this.either,
                        (IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => ((IDecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>)either).Decompose(out left, out right));
                    casted = Unsafe.As<DecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>, TCasted>(ref decomposeMixin);
                    return true;
                }

                casted = default;
                return false;
            }
        }

        public static bool Decompose<TEither, TLeft, TRight>(
            this TEither either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TEither : IEither<TLeft, TRight>, ICastable, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            if (either.TryCast<DecomposeMixin<TEither, TLeft, TRight>>(out var casted))
            {
                return casted.Decompose(out left, out right);
            }

            var context = new DecomposeContext<TLeft, TRight>();
            var result = either.Apply<bool, DecomposeContext<TLeft, TRight>, Realizable<bool>>(
                (TLeft left, ref DecomposeContext<TLeft, TRight> context) =>
                {
                    context.Left = left;
                    return new Realizable<bool>(true);
                },
                (TRight right, ref DecomposeContext<TLeft, TRight> context) =>
                {
                    context.Right = right;
                    return new Realizable<bool>(true);
                },
                ref context);

            left = context.Left;
            right = context.Right;
            return context.IsLeft;
        }

        private ref struct DecomposeContext<TLeft, TRight>
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            public bool IsLeft { get; set; }
            public TLeft Left { get; set; }
            public TRight Right { get; set; }
        }

        private static bool Context = false;
    }
}
