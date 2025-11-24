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
        private static Realizable<TResult> ToRealizable<TState, TResult>(TState state, Func<TState, TResult> func)
            where TState : allows ref struct
            where TResult : allows ref struct
        {
            TResult value;
            try
            {
                value = func(state);
            }
            catch (Exception exception)
            {
                return Realizable.FromException<TResult>(exception);
            }

            return Realizable.FromResult(value);
        }

        private static TaskWrapper<TResult> ToTaskWrapper<TState, TResult>(TState state, Func<TState, TResult> func)
        {
            return new TaskWrapper<TResult>(ToTask(state, func));
        }

        private static Task<TResult> ToTask<TState, TResult>(TState state, Func<TState, TResult> func)
        {
            TResult value;
            try
            {
                value = func(state);
            }
            catch (Exception exception)
            {
                return Task.FromException<TResult>(exception);
            }

            return Task.FromResult(value);
        }

        public static IEither<TLeftResult, TRightResult> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            //// TODO can you have a variant that allows `tleftresult` and `trightresult` to be `ref struct`s and the method returns `refeither`?
        {
            //// TODO update this to use the generic overload
            var realizable = either.SelectAsync(
                left => ToTaskWrapper(left, leftMap), //// TODO every non-async variant needs to use this adapter
                right => ToTaskWrapper(right, rightMap));

            if (realizable.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.GetAwaiter().GetResult();
            }
        }

        public static RefEither<TLeftResult, TRightResult> Select<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this TypeHolder<TEither, TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            //// TODO are you happy with this return type? can you do better?
            return either.Self.Select(leftMap, rightMap);
        }

        public static RefEither<TLeftResult, TRightResult> Select<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this TEither either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            //// TODO are you happy with this return type? can you do better?
            var realizable = SelectAsync<TEither, TLeftSource, TRightSource, Realizable<TLeftResult>, TLeftResult, Realizable<TRightResult>, TRightResult>(
                either,
                left => ToRealizable(left, leftMap), //// TODO every non-async variant needs to use this adapter
                right => ToRealizable(right, rightMap));

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
            //// TODO update this to use the generic overload
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

        public static Realizable<RefEither<TLeftResult, TRightResult>> SelectAsync<TEither, TLeftSource, TRightSource, TLeftContinuable, TLeftResult, TRightContinuable, TRightResult>(
            this TEither either,
            Func<TLeftSource, TLeftContinuable> leftMap,
            Func<TRightSource, TRightContinuable> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftContinuable : IContinuable<TLeftResult>, allows ref struct
            where TLeftResult : allows ref struct
            where TRightContinuable : IContinuable<TRightResult>, allows ref struct
            where TRightResult : allows ref struct
        {
            return either.Apply<RefEither<TLeftResult, TRightResult>, bool, Realizable<RefEither<TLeftResult, TRightResult>>>(
                (TLeftSource left, ref bool context) =>
                    leftMap(left)
                    .ContinueWith(
                        result => new RefEither<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                (TRightSource right, ref bool context) =>
                    rightMap(right)
                    .ContinueWith(
                        result => new RefEither<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                ref Context);
        }

        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            //// TODO update this to use the generic overload
            var future = either.Apply<TResult, bool, TaskWrapper<TResult>>(
                (TLeft left, ref bool context) => ToTaskWrapper(left, leftMap),
                (TRight right, ref bool context) => ToTaskWrapper(right, rightMap),
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

        public static TResult Apply<TEither, TLeft, TRight, TResult>(
            this TypeHolder<TEither, TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.Self.Apply(leftMap, rightMap);
        }

        public static Realizable<TResult> Apply<TEither, TLeft, TRight, TResult>(
            this Realizable<TEither> realizable,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return realizable.ContinueWith(
                either => either.Apply(leftMap, rightMap),
                exception => throw exception,
                canceled => throw canceled);
        }

        public static TResult Apply<TEither, TLeft, TRight, TResult>(
            this TEither either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            var future = either.Apply<TResult, bool, Realizable<TResult>>(
                (TLeft left, ref bool context) => ToRealizable(left, leftMap),
                (TRight right, ref bool context) => ToRealizable(right, rightMap),
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
