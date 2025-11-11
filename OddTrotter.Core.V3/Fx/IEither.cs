namespace Fx
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    public static class Playground
    {
        public static void DoWork(IEither<int, Exception> either)
        {
            bool context = false;
            var result = either.Apply<string, bool, TaskWrapper<string>, TaskWrapper<string>.ContinuableSource>(
                (int value, ref bool context) => ToString(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref context);
        }

        public static Realizable<string> DoWork2(IEither<string, Exception> either)
        {
            var parsed = either.Select(
                value => Parse(value),
                error => new TaskWrapper<Exception>(Task.FromResult(error)));

            return parsed.Apply(
                actualParsing => actualParsing.Apply(
                    actuallyParsed => actuallyParsed.ToString(),
                    parseError => parseError.ToString()),
                readError => readError.ToString());
        }


        public static Realizable<IEither<TLeftResult, TRightResult>> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, IContinuable<TLeftResult, TaskWrapper<TLeftResult>.ContinuableSource>> leftMap,
            Func<TRightSource, IContinuable<TRightResult, TaskWrapper<TRightResult>.ContinuableSource>> rightMap)
        {
            return either.Apply<IEither<TLeftResult, TRightResult>, bool, Realizable<IEither<TLeftResult, TRightResult>>, Realizable<IEither<TLeftResult, TRightResult>>.ContinuationSource>(
                (TLeftSource left, ref bool context) =>
                    leftMap(left)
                    .ContinueWith(source =>
                        source.Apply(
                            result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                            exception => throw exception,
                            canceled => throw canceled)),
                (TRightSource right, ref bool context) =>
                    rightMap(right)
                    .ContinueWith(source =>
                        source.Apply(
                            result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                            exception => throw exception,
                            canceled => throw canceled)),
                ref Context);
        }


        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            var future = either.Apply<TResult, bool, TaskWrapper<TResult>, TaskWrapper<TResult>.ContinuableSource>(
                (TLeft left, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(leftMap(left))),
                (TRight right, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(rightMap(right))),
                ref Context);

            if (future.Decompose(out var result, out var task))
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
                source => source.Apply(
                    result => result.Apply(leftMap, rightMap),
                    exception => throw exception,
                    canceled => throw canceled));
        }


        private static bool Context = false;

        public static TaskWrapper<IEither<int, Exception>> Parse(string value)
        {
            return new TaskWrapper<IEither<int, Exception>>(ParseImpl(value));
        }

        private static async Task<IEither<int, Exception>> ParseImpl(string value)
        {
            return await Task.FromResult(ParseInner(value)).ConfigureAwait(false);
        }

        private static IEither<int, Exception> ParseInner(string value)
        {
            try
            {
                return new Either<int, Exception>(int.Parse(value));
            }
            catch (Exception exception)
            {
                return new Either<int, Exception>(exception);
            }
        }

        private sealed class Either<TLeft, TRight> : IEither<TLeft, TRight>
        {
            private readonly TLeft? left;
            private readonly TRight? right;

            public Either(TLeft left)
            {
                this.left = left;

                this.right = default;
            }

            public Either(TRight right)
            {
                this.right = right;

                this.left = default;
            }

            public Realizable<TResult> Apply<TResult, TContext, TContinuable, TContinuableSource>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TContinuableSource, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TContinuableSource, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
                where TContinuable : IContinuable<TResult, TContinuableSource>, allows ref struct
                where TContinuableSource : IContinuableSource<TResult>, allows ref struct
            {
                if (this.left != null)
                {
                    return
                        leftMap(this.left, ref context)
                        .ContinueWith(source =>
                            source.Apply(
                                result => result,
                                exception => throw new LeftMapException(exception),
                                canceled => throw canceled));
                }
                else if (this.right != null)
                {
                    return
                        rightMap(this.right, ref context)
                        .ContinueWith(source =>
                            source.Apply(
                                result => result,
                                exception => throw new RightMapException(exception),
                                canceled => throw canceled));
                }
                else
                {
                    throw new Exception("TODO bug");
                }
            }
        }

        public static TaskWrapper<string> ToString(int value)
        {
            return new TaskWrapper<string>(ToStringImpl(value));
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static TaskWrapper<string> ToString(Exception exception)
        {
            return new TaskWrapper<string>(ToStringImpl(exception));
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }

        public sealed class TaskWrapper<T> : IContinuable<T, TaskWrapper<T>.ContinuableSource>
        {
            private readonly Task<T> task;

            public TaskWrapper(Task<T> task)
            {
                this.task = task;
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<ContinuableSource, TResult> continuation) where TResult : allows ref struct
            {
                throw new NotImplementedException();
            }

            public readonly ref struct ContinuableSource : IContinuableSource<T>
            {
                public TResult Apply<TResult>(Func<T, TResult> source, Func<Exception, TResult> exception, Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
                {
                    throw new NotImplementedException();
                }
            }
        }
    }



    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        /// <exception cref="LeftMapException"></exception>
        /// <exception cref="RightMapException"></exception>
        Realizable<TResult> Apply<TResult, TContext, TContinuable, TContinuableSource>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TContinuableSource, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TContinuableSource, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult, TContinuableSource>, allows ref struct
            where TContinuableSource : IContinuableSource<TResult>, allows ref struct;
    }

    public sealed class LeftMapException : Exception
    {
        public LeftMapException(Exception exception)
            : base(null, exception)
        {
        }
    }

    public sealed class RightMapException : Exception
    {
        public RightMapException(Exception exception)
            : base(null, exception)
        {
        }
    }

    public readonly ref struct Realizable<T> : IContinuable<T, Realizable<T>.ContinuationSource> //// TODO make this implement ieither, and remove the public `decompose` method so you can find what callers should actually be calling the extension `decompose` variant
        where T : allows ref struct
    {
        private readonly RefEither<T, ITask<T>> either;

        public Realizable(T value)
        {
            this.either = new RefEither<T, ITask<T>>(value);
        }

        public Realizable(ITask<T> future)
        {
            //// TODO do you really want `future` to be `itask` specifically, or should this be a generic on `realizable`?
            this.either = new RefEither<T, ITask<T>>(future);
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<ContinuationSource, TResult> continuation) where TResult : allows ref struct
        {
            if (either.Decompose(out var value, out var future))
            {
                return new Realizable<TResult>(continuation(new ContinuationSource(new RefEither<T, IContinuableSource<T>>(value))));
            }
            else
            {
                return future.ContinueWith(continuableSource => continuation(new ContinuationSource(new RefEither<T, IContinuableSource<T>>(continuableSource))));
            }
        }

        //// TODO implement decompose

        public readonly ref struct ContinuationSource : IContinuableSource<T>
        {
            private readonly RefEither<T, IContinuableSource<T>> either;

            public ContinuationSource(RefEither<T, IContinuableSource<T>> either)
            {
                this.either = either;
            }

            public TResult Apply<TResult>(Func<T, TResult> source, Func<Exception, TResult> exception, Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
            {
                if (either.Decompose(out var value, out var continuableSource))
                {
                    return source(value);
                }
                else
                {
                    return continuableSource.Apply(source, exception, canceled);
                }
            }
        }

        public bool Decompose([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out ITask<T> future)
        {
            return this.either.Decompose(out value, out future);
        }
    }

    public interface ITask<out T> : IContinuable<T, IContinuableSource<T>>
        where T : allows ref struct
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<out T>
        where T : allows ref struct
    {
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

    public interface IContinuable<out TSource, out TContinuableSource>
        where TSource : allows ref struct
        where TContinuableSource : IContinuableSource<TSource>, allows ref struct
    {
        Realizable<TResult> ContinueWith<TResult>(Func<TContinuableSource, TResult> continuation)
            where TResult : allows ref struct;
    }

    public delegate TContinuable AsyncRefContextualizedContinuableMap<in TValue, TContext, out TContinuable, out TContinuableSource, out TResult>(TValue value, ref TContext context) //// TODO you also need AsyncRefContextualizedTaskMap, AsyncRefContextualizedValueTaskMap, AsyncRefContextualizedITaskMap, AsyncRefContextualizedRealizableMap
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TContinuable : IContinuable<TResult, TContinuableSource>, allows ref struct
        where TContinuableSource : IContinuableSource<TResult>, allows ref struct
        where TResult : allows ref struct;





    public static class EitherExtensions
    {
        public static bool Decompose<TEither, TLeft, TRight>(
            this TEither either,
            out TLeft left,
            out TRight right)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            var context = new DecomposeContext<TLeft, TRight>();
            var result = either.Apply<bool, DecomposeContext<TLeft, TRight>, Realizable<bool>, Realizable<bool>.ContinuationSource>(
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

    public readonly ref struct RefEither<TLeft, TRight> : IEither<TLeft, TRight>, ICastable
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

        public RefEither(TRight right)
        {
            this.right = new RefNullable<TRight>(right);

            this.left = new RefNullable<TLeft>();
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable, TContinuableSource>(
            AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TContinuableSource, TResult> leftMap,
            AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TContinuableSource, TResult> rightMap, 
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult, TContinuableSource>, allows ref struct
            where TContinuableSource : IContinuableSource<TResult>, allows ref struct
        {
            if (this.left.TryGetValue(out var left))
            {
                return
                    leftMap(left, ref context)
                    .ContinueWith(source =>
                        source.Apply(
                            result => result,
                            exception => throw new LeftMapException(exception),
                            canceled => throw canceled));
            }
            else if (this.right.TryGetValue(out var right))
            {
                return
                    rightMap(right, ref context)
                    .ContinueWith(source =>
                        source.Apply(
                            result => result,
                            exception => throw new RightMapException(exception),
                            canceled => throw canceled));
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }

        public bool Decompose([MaybeNullWhen(false)] out TLeft value, [MaybeNullWhen(true)] out TRight future)
        {
            //// TODo implement this as an extension, mixin, monad combo
            throw new NotImplementedException();
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted)
        {
            if (typeof(TCasted) == typeof(IDecomposerMixin<RefEither<TLeft, TRight>, TLeft, TRight, Decomposed<TLeft, TRight>>))
            {
                casted = (TCasted)(IDecomposerMixin<RefEither<TLeft, TRight>, TLeft, TRight, Decomposed<TLeft, TRight>>)Decomposer.Instance;
                return true;
            }

            casted = default;
            return false;
        }

        private sealed class Decomposer : IDecomposerMixin<RefEither<TLeft, TRight>, TLeft, TRight, Decomposed<TLeft, TRight>>
        {
            private Decomposer()
            {
            }

            public static Decomposer Instance { get; } = new Decomposer();

            public Decomposed<TLeft, TRight> Decompose(RefEither<TLeft, TRight> either, out bool isLeft)
            {
                if (either.left.TryGetValue(out var left))
                {
                    isLeft = true;
                    return new Decomposed<TLeft, TRight>(left);
                }
                else if (either.right.TryGetValue(out var right))
                {
                    isLeft = false;
                    return new Decomposed<TLeft, TRight>(right);
                }
                else
                {
                    throw new Exception("TODO bug");
                }
            }
        }
    }


    public interface ICastable
    {
        bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted); //// note: `tcasted` should *not* allow ref struct; the point of this interface is to allow ref structs to be cast to interfaces //// TODO just because that's your narrow use-case right now doesn't mean it could never be useful for casts from ref structs to ref structs...
    }

    public interface IDecomposerMixin<in TEither, out TLeft, out TRight, out TDecomposed>
        where TEither : IEither<TLeft, TRight>, allows ref struct
        where TLeft : allows ref struct
        where TRight : allows ref struct
        where TDecomposed : IDecomposed<TLeft, TRight>, allows ref struct
    {
        TDecomposed Decompose(TEither either, out bool isLeft);
    }

    public readonly ref struct Decomposed<TLeft, TRight> : IDecomposed<TLeft, TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        public Decomposed(TLeft left)
        {
            Left = left;

            this.Right = default!;
        }

        public Decomposed(TRight right)
        {
            Right = right;

            this.Left = default!;
        }

        public TLeft Left { get; }

        public TRight Right { get; }
    }

    public interface IDecomposed<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        TLeft Left { get; }

        TRight Right { get; }
    }
}
