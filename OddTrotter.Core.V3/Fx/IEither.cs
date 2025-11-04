namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TContinuable"></typeparam>
        /// <typeparam name="TContinuableSource"></typeparam>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="LeftMapException"></exception>
        /// <exception cref="RightMapException"></exception>
        Realizable<TResult> Apply<TResult, TContext, TContinuable, TContinuableSource>(
            AsyncRefContextualizedMap<TLeft, TContext, TContinuable, TContinuableSource, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TContinuable, TContinuableSource, TResult> rightMap,
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
    }

    public interface IContinuableSource<out TSource>
        where TSource : allows ref struct
    {
        TResult Apply<TResult>( //// TODO you need to add the "canceled" case
            Func<TSource, TResult> source,
            Func<Exception, TResult> exception)
            where TResult : allows ref struct;
    }

    public interface IContinuable<out TSource, out TContinuableSource>
        where TSource : allows ref struct
        where TContinuableSource : IContinuableSource<TSource>, allows ref struct
    {
        Realizable<TResult> ContinueWith<TResult>(Func<TContinuableSource, TResult> continuation)
            where TResult : allows ref struct;
    }

    public delegate TContinuable AsyncRefContextualizedMap<in TValue, TContext, out TContinuable, out TContinuableSource, out TResult>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TContinuable : IContinuable<TResult, TContinuableSource>, allows ref struct
        where TContinuableSource : IContinuableSource<TResult>, allows ref struct
        where TResult : allows ref struct;







    public interface INullable<out T>
        where T : allows ref struct
    {
        T TryGetValue(out bool retrieved);
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
            AsyncRefContextualizedMap<TLeft, TContext, TContinuable, TContinuableSource, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TContinuable, TContinuableSource, TResult> rightMap, 
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
                            exception => throw new LeftMapException(exception)));
            }
            else if (this.right.TryGetValue(out var right))
            {
                return
                    rightMap(right, ref context)
                    .ContinueWith(source =>
                        source.Apply(
                            result => result,
                            exception => throw new RightMapException(exception)));
            }
            else
            {
                throw new Exception("TODO bug");
            }
        }
    }
}
