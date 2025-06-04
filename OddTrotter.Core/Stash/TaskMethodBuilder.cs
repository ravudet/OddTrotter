namespace Stash
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;


    ////TODO add this to the interface: [AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]

    public struct TaskMethodBuilder<T>
    {
        private AsyncTaskMethodBuilder<T> builder;

        public static TaskMethodBuilder<T> Create()
            => new TaskMethodBuilder<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            builder.Start(ref stateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            builder.SetStateMachine(stateMachine);
        }

        public void SetException(Exception exception)
        {
            builder.SetException(exception);
        }

        public void SetResult(T result)
        {
            builder.SetResult(result);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        public ITask<T> Task
        {
            get
            {
                return new TaskWrapper<T>(builder.Task);
            }
        }
    }
}
