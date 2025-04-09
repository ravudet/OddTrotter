namespace Fx.Either
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static partial class EitherExtensions
    {
        public static async ValueTask<Either<TLeftResult, TRightResult>> SelectAsync //// TODO should return `ieither`
            <
                TLeftValue,
                TRightValue,
                TLeftResult,
                TRightResult
            >
            (
                this IEither<TLeftValue, TRightValue> either,
                Func<TLeftValue, Task<TLeftResult>> leftSelector,
                Func<TRightValue, Task<TRightResult>> rightSelector
            )
        {
            ArgumentNullException.ThrowIfNull(either);
            ArgumentNullException.ThrowIfNull(leftSelector);
            ArgumentNullException.ThrowIfNull(rightSelector);

            //// TODO you are here
            return await either
                .ApplyAsync(
                    async (left, _) =>
                    {
                        //// TODO does using blocks instead of expression cause issues?
                        var newLeft = await leftSelector(left).ConfigureAwait(false);
                        return Either.Left(newLeft).Right<TRightResult>();
                    },
                    async (right, _) =>
                    {
                        var newRight = await rightSelector(right).ConfigureAwait(false);
                        return Either.Left<TLeftResult>().Right(newRight);
                    },
                    new Nothing())
                .ConfigureAwait(false);
        }

        public static async TaskLike<string> FooAsync()
        {
            await Task.Yield();
            await default(TaskLike<string>);
            return "Asdf";
        }

        public sealed class TaskLikeMethodBuilder<T>
        {
            public TaskLikeMethodBuilder()
                => Console.WriteLine(".ctor");

            public static TaskLikeMethodBuilder<T> Create()
                => new TaskLikeMethodBuilder<T>();

            public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
            {
                Console.WriteLine("Start");
                stateMachine.MoveNext();
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new Exception("TODO");

            public void SetException(Exception exception) => throw new Exception("TODO");

            public void SetResult(T result) => throw new Exception("TODO");

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
                => throw new Exception("TODO");

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
                ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
                 => throw new Exception("TODO");

            public TaskLike<T> Task => default(TaskLike<T>);
        }

        [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(TaskLikeMethodBuilder<>))]
        public struct TaskLike<T>
        {
            public TaskLikeAwaiter GetAwaiter() => default(TaskLikeAwaiter);
        }

        public struct TaskLikeAwaiter : INotifyCompletion
        {
            public void GetResult() { }

            public bool IsCompleted => true;

            public void OnCompleted(Action continuation) { }
        }
    }
}
