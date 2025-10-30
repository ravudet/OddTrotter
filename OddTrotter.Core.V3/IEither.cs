namespace OddTrotter.Core.V3
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public readonly ref struct Future<T>
        where T : allows ref struct
    {
    }

    public readonly ref struct Future2<T>
    {
        public static implicit operator Future2<T>(Future<T> future)
        {
            return new Future2<T>();
        }
    }

    public static class FutureExtensions
    {
        public sealed class Awaiter<T> : ICriticalNotifyCompletion
        {
            public bool IsCompleted { get; }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public T GetResult()
            {
                throw new Exception("TODO");
            }
        }

        public static Awaiter<T> GetAwaiter<T>(this Future2<T> future)
        {
            return new Awaiter<T>();
        }
    }

    public static class Play
    {
        public static async Task Another()
        {
            var future = new Future<int>();

            await (Future2<int>)future;
        }

        public static async Task Drive2(IEither<IEither<int, string>, IEither<int, Exception>> either)
        {
            var context = true;
            var result = await either.Apply(
                (IEither<int, string> value, ref bool context) =>
                    value.Apply(
                        (int left, ref bool context) => GetInt3(left),
                        (string right, ref bool context) => GetAnotherInt3(right),
                        ref context),
                (IEither<int, Exception> error, ref bool context) =>
                    error.Apply(
                        (int errorCode, ref bool context) => GetInt3(errorCode),
                        (Exception exception, ref bool context) => GetOtherInt3(exception),
                        ref context),
                ref context);

            var result2 = await either.Apply(
                (IEither<int, string> value, ref bool context) =>
                    value.Apply(
                        (int left, ref bool context) => GetInt(left),
                        (string right, ref bool context) => GetAnotherInt(right),
                        ref context),
                (IEither<int, Exception> error, ref bool context) =>
                    error.Apply(
                        (int errorCode, ref bool context) => GetInt(errorCode),
                        (Exception exception, ref bool context) => GetOtherInt(exception),
                        ref context),
                ref context);
        }

        public static unsafe Realizable<int> Drive(IEither<int, Exception> either)
        {
            var context = true;
            var realizable = either.Apply().Apply<int>().Apply(
                (int left, ref bool context) => GetInt(left),
                (Exception right, ref bool context) => GetOtherInt(right),
                ref context);

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
            return realizable;
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
        }

        public static Realizable2<int> GetInt(int value)
        {
            return new Realizable<int>();
        }

        public static Realizable2<int> GetAnotherInt(string value)
        {
            return new Realizable<int>();
        }

        public static Realizable2<int> GetOtherInt(Exception exception)
        {
            return new Realizable<int>();
        }

        public static Realizable<int> GetInt3(int value)
        {
            return new Realizable<int>();
        }

        public static Realizable<int> GetAnotherInt3(string value)
        {
            return new Realizable<int>();
        }

        public static Realizable<int> GetOtherInt3(Exception exception)
        {
            return new Realizable<int>();
        }


        public static IAwaitable<int> GetInt2(int value)
        {
            return null!;
        }

        public static IAwaitable<int> GetOtherInt2(Exception exception)
        {
            return null!;
        }
    }

    public static class Extensions
    {
        public static Realizable<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult, IAwaitable<TResult>> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult, IAwaitable<TResult>> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            return either.Apply(leftMap, rightMap, ref context);
        }

        public readonly ref struct Applied2<TLeft, TRight, TResult>
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            private readonly IEither<TLeft, TRight> either;

            public Realizable<TResult> Apply<TContext, TAwaitable>(
                AsyncRefContextualizedMap<TLeft, TContext, TResult, TAwaitable> leftMap,
                AsyncRefContextualizedMap<TRight, TContext, TResult, TAwaitable> rightMap,
                ref TContext context)
                where TContext : allows ref struct
                where TAwaitable : IAwaitable<TResult, IAwaiter<TResult>, TAwaitable>, allows ref struct
            {
                return this.either.Apply(leftMap, rightMap, ref context);
            }
        }

        public readonly ref struct Applied1<TLeft, TRight>
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            public Applied2<TLeft, TRight, TResult> Apply<TResult>()
                where TResult : allows ref struct
            {
                return new Applied2<TLeft, TRight, TResult>();
            }
        }

        public static Applied1<TLeft, TRight> Apply<TLeft, TRight>(this IEither<TLeft, TRight> either)
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return new Applied1<TLeft, TRight>();
        }
    }

    public interface IEither<out TLeft, out TRight>
        where TLeft : allows ref struct
        where TRight : allows ref struct
    {
        Realizable<TResult> Apply<TResult, TContext, TAwaitable>(
            AsyncRefContextualizedMap<TLeft, TContext, TResult, TAwaitable> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult, TAwaitable> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TAwaitable : IAwaitable<TResult, IAwaiter<TResult>, TAwaitable>, allows ref struct;

        Realizable<TResult> Apply<TResult, TContext>(
            AsyncRefContextualizedMap2<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap2<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;
    }


    //// TODO it's not clear that this needs to 
    public delegate TAwaitable AsyncRefContextualizedMap<in TValue, TContext, TResult, TAwaitable>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TResult : allows ref struct
        where TAwaitable : IAwaitable<TResult, IAwaiter<TResult>, TAwaitable>, allows ref struct;


    public delegate Realizable<TResult> AsyncRefContextualizedMap2<in TValue, TContext, TResult>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TResult : allows ref struct;

    //// TODO better name
    public readonly ref struct Realizable<T> : IEither<T, IAwaitable<T>>, IAwaitable<T, Realizable<T>.Awaiter, Realizable<T>>
        where T : allows ref struct
    {
        public Realizable<TResult> Apply<TResult, TContext, TAwaitable>(AsyncRefContextualizedMap<T, TContext, TResult, TAwaitable> leftMap, AsyncRefContextualizedMap<IAwaitable<T>, TContext, TResult, TAwaitable> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TAwaitable : IAwaitable<TResult, IAwaiter<TResult>, TAwaitable>, allows ref struct
        {
            throw new System.NotImplementedException();
        }

        public Realizable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new System.NotImplementedException();
        }

        public Awaiter GetAwaiter()
        {
            throw new System.NotImplementedException();
        }

        public readonly struct Awaiter : IAwaiter<T>, ICriticalNotifyCompletion
        {
            public bool IsCompleted { get; }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public T GetResult()
            {
                throw new Exception("TODO");
            }
        }
    }

    public readonly ref struct Realizable2<T> : IEither<T, IAwaitable<T>>, IAwaitable<T, IAwaiter<T>, Realizable2<T>>
    {
        public Realizable<TResult> Apply<TResult, TContext, TAwaitable>(AsyncRefContextualizedMap<T, TContext, TResult, TAwaitable> leftMap, AsyncRefContextualizedMap<IAwaitable<T>, TContext, TResult, TAwaitable> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TAwaitable : IAwaitable<TResult, IAwaiter<TResult>, TAwaitable>, allows ref struct
        {
            throw new System.NotImplementedException();
        }

        public Realizable2<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public IAwaiter<T> GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public static implicit operator Realizable2<T>(Realizable<T> realizable)
        {
            return new Realizable2<T>();
        }
    }
}
