namespace Fx.Either
{
    using System;
    using System.Linq.V2;
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
            return "Asdf";
        }

        public readonly struct TaskLikeMethodBuilder<T>
        {
            private readonly AsyncTaskMethodBuilder<T> builder;

            public static TaskLikeMethodBuilder<T> Create()
                => new TaskLikeMethodBuilder<T>();

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

            public TaskLike<T> Task => new TaskLike<T>(builder.Task);
        }

        [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(TaskLikeMethodBuilder<>))]
        public sealed class TaskLike<T> : ITask<T>
        {
            private readonly Task<T> task;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="task"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/></exception>
            public TaskLike(Task<T> task)
            {
                ArgumentNullException.ThrowIfNull(task);

                this.task = task;
            }

            /// <inheritdoc/>
            public ITaskAwaiter<T> GetAwaiter()
            {
                return new TaskAwaiterWrapper<T>(this.task.GetAwaiter());
            }

            /// <inheritdoc/>
            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitableWrapper<T>(this.task.ConfigureAwait(continueOnCapturedContext));
            }
        }
    }
}
