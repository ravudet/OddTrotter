namespace OddTrotter.CalendarV1.Tokenization.Json2
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public interface IAsyncable<TTask, TValue, out TBuilder>
        where TTask : allows ref struct
        where TValue : allows ref struct
        where TBuilder : IAsyncBuilder<TTask, TValue, TBuilder>, allows ref struct
    {
        TBuilder CreateBuilder();
    }

    public interface IAsyncBuilder<TTask, TValue, out TBuilder>
        where TTask : allows ref struct
        where TValue : allows ref struct
        where TBuilder : IAsyncBuilder<TTask, TValue, TBuilder>, allows ref struct
    {
        abstract static TBuilder Create();

        void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine;

        void SetStateMachine(IAsyncStateMachine stateMachine);

        void SetException(Exception exception);

        void SetResult(TValue result);

        void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine;

        void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine;

        TTask Task { get; }
    }

    public interface IAwaitable<out TValue, out TAwaiter>
        where TValue : allows ref struct
        where TAwaiter : IAwaiter<TValue>
    {
        TAwaiter GetAwaiter();
    }

    public interface IAwaiter<out T> : ICriticalNotifyCompletion
        where T : allows ref struct
    {
        /// <inheritdoc cref="TaskAwaiter{TResult}.IsCompleted"/>
        bool IsCompleted { get; }

        /// <inheritdoc cref="TaskAwaiter{TResult}.GetResult"/>
        T GetResult();
    }

    [AsyncMethodBuilder(typeof(Builder<>))]
    public readonly ref struct AsyncableAwaitable<TValue> : IAwaitable<TValue, AsyncableAwaitable<TValue>.Awaiter>, IAsyncable<AsyncableAwaitable<TValue>, TValue, Builder<TValue>>
        where TValue : allows ref struct
    {
        private readonly Awaiter awaiter;

        public AsyncableAwaitable(Awaiter awaiter)
        {
            this.awaiter = awaiter;
        }

        public static Func<TValue> ValueFactory { get; set; } = () => default!;

        public Builder<TValue> CreateBuilder()
        {
            return new Builder<TValue>();
        }

        public Awaiter GetAwaiter()
        {
            return this.awaiter;
        }

        public struct Awaiter : IAwaiter<TValue>
        {
            private readonly ValueTaskAwaiter valueTaskAwaiter;

            public Awaiter(ValueTaskAwaiter valueTaskAwaiter)
            {
                this.valueTaskAwaiter = valueTaskAwaiter;
            }

            public bool IsCompleted
            {
                get
                {
                    return this.valueTaskAwaiter.IsCompleted;
                }
            }

            public TValue GetResult()
            {
                return AsyncableAwaitable<TValue>.ValueFactory();
            }

            public void OnCompleted(Action continuation)
            {
                this.valueTaskAwaiter.OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                this.valueTaskAwaiter.UnsafeOnCompleted(continuation);
            }
        }
    }

    public struct Builder<TValue> : IAsyncBuilder<AsyncableAwaitable<TValue>, TValue, Builder<TValue>>
        where TValue : allows ref struct
    {
        private AsyncValueTaskMethodBuilder builder;

        public AsyncableAwaitable<TValue> Task
        {
            get
            {
                return new AsyncableAwaitable<TValue>(new AsyncableAwaitable<TValue>.Awaiter(builder.Task.GetAwaiter()));
            }
        }

        public static Builder<TValue> Create()
        {
            return new Builder<TValue>();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        public void SetException(Exception exception)
        {
            this.builder.SetException(exception);
        }

        public void SetResult(TValue result)
        {
            this.builder.SetResult();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException("TODO");
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            this.builder.Start(ref stateMachine);
        }
    }
}
