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
                (int value, ref bool context) => Parse(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref context);

            //// TODO implement an example that uses nested eithers to demonstrate that the maps passed to `apply` can leverage `realizable`
        }


        public static TaskWrapper<string> Parse(int value)
        {
            return new TaskWrapper<string>(ParseImpl(value));
        }

        private static async Task<string> ParseImpl(int value)
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

    public readonly ref struct Realizable<T>
        where T : allows ref struct
    {
        //// TODO implement continuable and decompose
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

    public readonly ref struct RefEither<TLeft, TRight> : IEither<TLeft, TRight>
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
    }
}
