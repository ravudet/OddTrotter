/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

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

    public interface ITask<out T> : IContinuable<T> //// TODO call this awaitable //// TODO probably have a configure await on that; if it's not directly on iawaitable, there should be a configurableawaitable or something //// TODO iawaitable should have two generics, one for the return value and another for the awaiter type
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<out T> : ICriticalNotifyCompletion
        where T : allows ref struct
    {
        bool IsCompleted { get; }

        T GetResult();
    }

    public interface IContinuableSource<out TSource>
        where TSource : allows ref struct
    {
        TResult Apply<TResult>(
            Func<TSource, TResult> source,
            Func<Exception, TResult> exception,
            Func<OperationCanceledException, TResult> canceled)
            where TResult : allows ref struct;
    }

    public interface IContinuable<out TSource>
        where TSource : allows ref struct
    {
        Realizable<TResult> ContinueWith<TResult>(
            Func<TSource, TResult> source,
            Func<Exception, TResult> exception,
            Func<OperationCanceledException, TResult> canceled)
            where TResult : allows ref struct;
    }

    public delegate TContinuable AsyncRefContextualizedContinuableMap<in TValue, TContext, out TContinuable, out TResult>(TValue value, ref TContext context) //// TODO you also need AsyncRefContextualizedTaskMap, AsyncRefContextualizedValueTaskMap, AsyncRefContextualizedITaskMap, AsyncRefContextualizedRealizableMap
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TContinuable : IContinuable<TResult>, allows ref struct
        where TResult : allows ref struct;





    public static class EitherExtensions2
    {
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
    }




    public readonly ref struct RefNullable<T>
        where T : allows ref struct
    {
        private readonly T value;

        private readonly bool hasValue;

        public RefNullable(T value)
        {
            this.value = value;

            this.hasValue = true;
        }

        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            if (this.hasValue)
            {
                value = this.value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public readonly ref struct RefEither<TLeft, TRight> : IEither<RefEither<TLeft, TRight>, TLeft, TRight>, ICastable, IDecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        private readonly RefNullable<TLeft> left;

        private readonly RefNullable<TRight> right;

        public RefEither(TLeft left)
        {
            this.left = new RefNullable<TLeft>(left);

            this.right = new RefNullable<TRight>();
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

            this.left = new RefNullable<TLeft>();
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
            if (this.left.TryGetValue(out value))
            {
                future = default;
                return true;
            }
            else if (this.right.TryGetValue(out future))
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
            if (typeof(TCasted) == typeof(DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>))
            {
                var mixin = new DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>(
                    this,
                    (RefEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => either.Decompose(out left, out right));
                casted = Unsafe.As<DecomposeMixin<RefEither<TLeft, TRight>, TLeft, TRight>, TCasted>(ref mixin);
                return true;
            }

            casted = default;
            return false;
        }
    }







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
    //// TODO implement a test with mapping exceptions being throw
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
}
