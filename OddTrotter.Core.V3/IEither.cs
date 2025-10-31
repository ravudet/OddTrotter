namespace OddTrotter.Core.V3
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.AccessControl;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;


    public readonly ref struct TypeContainer<TValue, T1, T2>
        where TValue : allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        public TypeContainer(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; }
    }

    public readonly ref struct TypeContainer<TValue, T1>
        where TValue : allows ref struct
        where T1 : allows ref struct
    {
        public TypeContainer(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; }
    }

    public readonly ref struct Foo<T1, T2> : IFoo<T1, T2>
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        public void Work()
        {
            throw new NotImplementedException();
        }

        public TypeContainer<Foo<T1, T2>, T1, T2> Container
        {
            get
            {
                return this;
            }
        }

        public static implicit operator TypeContainer<Foo<T1, T2>, T1, T2>(Foo<T1, T2> foo)
        {
            return new TypeContainer<Foo<T1, T2>, T1, T2>(foo);
        }
    }

    public interface IFoo<T1, T2>
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        void Work();
    }

    public static class FooExtensions
    {
        public static void Something<TFoo, T1, T2>(TFoo foo)
            where TFoo : IFoo<T1, T2>, allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
        {
            foo.Work();
        }

        public static void Something<TFoo, T1, T2>(this TypeContainer<TFoo, T1, T2> container)
            where TFoo : IFoo<T1, T2>, allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
        {
            Something<TFoo, T1, T2>(container.Value);
        }

        public static void Play()
        {
            var foo = new Foo<int, string>();
            foo.Something();
        }


        public sealed class Fizz
        {
            public static implicit operator Buzz(Fizz fizz)
            {
                return new Buzz();
            }
        }

        public sealed class Buzz
        {
        }

        public static void Extension(this Buzz buzz)
        {
        }

        public static void FizzBuzzPlay()
        {
            var fizz = new Fizz();
            fizz.Extension();
            Extension(fizz);
        }
    }



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



            var result3 = await either.Apply2(
                (IEither<int, string> value, ref bool context) =>
                    value
                        .Apply2(
                            (int left, ref bool context) => GetInt3(left).Container,
                            (string right, ref bool context) => GetAnotherInt3(right).Container,
                            ref context)
                        .Container,
                (IEither<int, Exception> error, ref bool context) =>
                    error
                        .Apply2(
                            (int errorCode, ref bool context) => GetInt3(errorCode).Container,
                            (Exception exception, ref bool context) => GetOtherInt3(exception).Container,
                            ref context)
                        .Container,
                ref context);


            var result4 = await either.Apply4<IEither<int, string>, IEither<int, Exception>, int>(
                async value =>
                    await value
                        .Apply4<int, string, int>(
                            async left => await GetInt3(left),
                            async right => await GetAnotherInt3(right)),
                async error =>
                    await error
                        .Apply4(
                            errorCode => GetInt3(errorCode),
                            exception => GetOtherInt3(exception)),
                ref context);
        }

        public static Realizable<TResult> Apply4<TLeft, TRight, TResult>(this IEither<TLeft, TRight> either, Func<TLeft, Realizable<TResult>> leftMap, Func<TRight, Realizable<TResult>> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {

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





        Realizable<TResult> Apply2<TResult, TContext, TAwaitable, TAwaiter>(
            AsyncRefContextualizedMap3<TLeft, TContext, TResult, TAwaitable, TAwaiter> leftMap,
            AsyncRefContextualizedMap3<TRight, TContext, TResult, TAwaitable, TAwaiter> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
            where TAwaitable : IAwaitable<TResult, TAwaiter, TAwaitable>, allows ref struct
            where TAwaiter : IAwaiter<TResult>, allows ref struct;


    }



    public delegate TypeContainer<TAwaitable, TResult, TAwaiter> AsyncRefContextualizedMap3<in TValue, TContext, TResult, TAwaitable, TAwaiter>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TResult : allows ref struct
        where TAwaitable : IAwaitable<TResult, TAwaiter, TAwaitable>, allows ref struct
        where TAwaiter : IAwaiter<TResult>, allows ref struct;


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

        public static implicit operator TypeContainer<Realizable<T>, T, Realizable<T>.Awaiter>(Realizable<T> realizable)
        {
            return new TypeContainer<Realizable<T>, T, Awaiter>(realizable);
        }

        public TypeContainer<Realizable<T>, T, Realizable<T>.Awaiter> Container
        {
            get
            {
                return this;
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
