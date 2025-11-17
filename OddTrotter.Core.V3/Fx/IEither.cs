/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.Core.V3;

    public static class Playground
    {
        //// TODO write these two methods as tests (need to implement a class `ieither` implementation first)
        //// TODO then split this into files
        //// TODO implement a test with ref structs
        //// TODO implement a test using actual async (like reading a file or something)
        //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
        //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
        //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles

        public static void DoWork(IEither<int, Exception> either)
        {
            bool context = false;
            var result = either.Apply<string, bool, TaskWrapper<string>>(
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

        
    }



    public static class Throwaway
    {
        public static Task<TNewResult> ContinueWith2<TOldResult, TNewResult>(
            this Task<TOldResult> task,
            Func<Task<TOldResult>, TNewResult> func)
        {
            return task.ContinueWith(func);
        }
    }

    public sealed class TaskWrapper<T> : IContinuable<T>
    {
        private readonly Task<T> task;

        public TaskWrapper(Task<T> task)
        {
            this.task = task;
        }

        public Realizable<TResult> ContinueWith<TResult>(
            Func<T, TResult> source, 
            Func<Exception, TResult> exception, 
            Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
        {
            return new Realizable<TResult>(
                new Continuation<TResult>(
                    this.task,
                    source,
                    exception,
                    canceled));
        }

        private sealed class Continuation<TContinued> : ITask<TContinued>
            where TContinued : allows ref struct
        {
            private readonly Task<T> task;
            private readonly Func<T, TContinued> source;
            private readonly Func<Exception, TContinued> exception;
            private readonly Func<OperationCanceledException, TContinued> canceled;

            public Continuation(
                Task<T> task, 
                Func<T, TContinued> source,
                Func<Exception, TContinued> exception,
                Func<OperationCanceledException, TContinued> canceled)
            {
                this.task = task;
                this.source = source;
                this.exception = exception;
                this.canceled = canceled;
            }

            public Realizable<TResult> ContinueWith<TResult>(
                Func<TContinued, TResult> source, 
                Func<Exception, TResult> exception, 
                Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
            {
                throw new NotImplementedException();
            }

            public IAwaiter<TContinued> GetAwaiter()
            {
                return new Awaiter(
                    this.task,
                    this.source,
                    this.exception,
                    this.canceled);
            }

            private sealed class Awaiter : IAwaiter<TContinued>
            {
                private readonly Task<T> task;
                private readonly TaskAwaiter<T> taskAwaiter;
                private readonly Func<T, TContinued> source;
                private readonly Func<Exception, TContinued> exception;
                private readonly Func<OperationCanceledException, TContinued> canceled;

                public Awaiter(
                    Task<T> task,
                    Func<T, TContinued> source,
                    Func<Exception, TContinued> exception,
                    Func<OperationCanceledException, TContinued> canceled)
                {
                    this.task = task;
                    this.source = source;
                    this.exception = exception;
                    this.canceled = canceled;

                    this.taskAwaiter = this.task.GetAwaiter();
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.taskAwaiter.IsCompleted;
                    }
                }

                public TContinued GetResult()
                {
                    if (this.task.Exception != null)
                    {
                        return exception(this.task.Exception);
                    }
                    else if (this.task.IsCanceled)
                    {
                        return canceled(new OperationCanceledException("TODO"));
                    }
                    else
                    {
                        //// TODO this means that the continuation function is not run asynchronously; you can maybe do better, but maybe it's not actually an issue at all?
                        return this.source(this.task.Result);
                    }
                }

                public void OnCompleted(Action continuation)
                {
                    this.taskAwaiter.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.taskAwaiter.UnsafeOnCompleted(continuation);
                }
            }
        }

        public IAwaiter<T> GetAwaiter()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class Either<TLeft, TRight> : IEither<TLeft, TRight>
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

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            if (this.left != null)
            {
                return
                    leftMap(this.left, ref context)
                    .ContinueWith(
                        result => result,
                        exception => throw new LeftMapException(exception),
                        canceled => throw canceled);
            }
            else if (this.right != null)
            {
                return
                    rightMap(this.right, ref context)
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
    }


















    public static class RealizableExtensions
    {
        public static IAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable)
        {
            if (realizable.TypeHolder.Decompose(out var left, out var right))
            {
                return new TaskWrapper<T>(Task.FromResult(left)).GetAwaiter();
            }
            else
            {
                return right.GetAwaiter();
            }
        }
    }


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

    public readonly ref struct Realizable<T> : IContinuable<T>, IEither<Realizable<T>, T, ITask<T>>, ICastable, IDecomposeMixin<Realizable<T>, T, ITask<T>>
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

        public TypeHolder<Realizable<T>, T, ITask<T>> TypeHolder
        {
            get
            {
                return new TypeHolder<Realizable<T>, T, ITask<T>>(this);
            }
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<T, TResult> source, Func<Exception, TResult> exception, Func<OperationCanceledException, TResult> canceled) where TResult : allows ref struct
        {
            if (either.TypeHolder.Decompose(out var value, out var future))
            {
                return new Realizable<TResult>(source(value));
            }
            else
            {
                return future.ContinueWith(
                    source,
                    exception,
                    canceled);
            }
        }

        public Realizable<TResult> Apply<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<T, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<ITask<T>, TContext, TContinuable, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
        {
            return this.either.Apply(leftMap, rightMap, ref context);
        }

        public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct
        {
            //// TODO can you put this code into a single place? you've duplicated it a few times
            if (typeof(TCasted) == typeof(DecomposeMixin<Realizable<T>, T, ITask<T>>))
            {
                var mixin = new DecomposeMixin<Realizable<T>, T, ITask<T>>(
                    this,
                    (Realizable<T> either, [MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right) => either.Decompose(out left, out right));
                casted = Unsafe.As<DecomposeMixin<Realizable<T>, T, ITask<T>>, TCasted>(ref mixin);
                return true;
            }

            casted = default;
            return false;
        }

        bool IDecomposeMixin<Realizable<T>, T, ITask<T>>.Decompose([MaybeNullWhen(false)] out T left, [MaybeNullWhen(true)] out ITask<T> right)
        {
            return this.either.Decompose(out left, out right);
        }
    }

    public interface ITask<out T> : IContinuable<T>
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





    public static class EitherExtensions
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
}
