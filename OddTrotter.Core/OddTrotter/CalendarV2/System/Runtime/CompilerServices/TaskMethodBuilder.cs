/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    public struct TaskMethodBuilder<T>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <remarks>
        /// Must not be `readonly` because the underlying type mutates its state. I can't find any reference material that
        /// explains this, but the behavior can be reproduced using the following code:
        /// ```
        /// [TestMethod]
        /// public void SetVal()
        /// {
        ///     var customBuilder = new CustomBuilder();
        ///     customBuilder.AwaitOnCompleted();
        /// 
        ///     Assert.AreEqual(42, customBuilder.Task);
        /// }
        /// 
        /// public struct CustomBuilder
        /// {
        ///     private readonly BuildInBuilder builtInBuilder;
        /// 
        ///     public void AwaitOnCompleted()
        ///     {
        ///         this.builtInBuilder.AwaitOnCompleted();
        ///     }
        /// 
        ///     public int Task
        ///     {
        ///         get
        ///         {
        ///             return this.builtInBuilder.Task;
        ///         }
        ///     }
        /// }
        /// 
        /// public struct BuildInBuilder
        /// {
        ///     private int task;
        /// 
        ///     public void AwaitOnCompleted()
        ///     {
        ///         this.task = 42;
        ///     }
        /// 
        ///     public int Task
        ///     {
        ///         get
        ///         {
        ///             return this.task;
        ///         }
        ///     }
        /// }
        /// ```
        /// </remarks>
        private AsyncTaskMethodBuilder<T> builder;

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Create"/>
        public static TaskMethodBuilder<T> Create()
            => new TaskMethodBuilder<T>();

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Start{TStateMachine}(ref TStateMachine)"/>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            ArgumentNullException.ThrowIfNull(stateMachine);

            this.builder.Start(ref stateMachine);
        }

        private const string setStateMachineMessage =
$$"""
'{nameof(SetStateMachine)}' is not supported. '{nameof(TaskMethodBuilder<T>)}' is only intended to target the .NET runtime, which doesn't make use of the '{nameof(SetStateMachine)}' call; only the .NET framework runtime will make use of the '{nameof(SetStateMachine)}' call. You can find more details [here](https://devblogs.microsoft.com/dotnet/how-async-await-really-works/):

> Note that line which the source comments as "important". This takes the place of that complicated SetStateMachine dance in .NET Framework, **such that `SetStateMachine` isn't actually used at all in .NET Core.**

The **intended** implementation of this method for .NET framework would be:

```
builder.SetStateMachine(stateMachine);
```

For this method to be invoked, there are 3 requirements:
1. the binary needs to be compiled with the `Release` configuration
2. the binary needs to target .NET framework (.NET framework 4.8.1 was used to reproduce these steps)
3. the method leveraging the '{nameof(TaskMethodBuilder<T>)}' must actually await (i.e. it needs to not just return a completed task)

The third requirement can be met using the following code:

```
[TestClass]
public sealed class Test
{
    private sealed class AwaiterType<T>
    {
        private readonly T value;

        public AwaiterType(T value)
        {
            this.value = value;
        }

        public async ITask<T> GetValueWithDelay()
        {
            await Task.Delay(100).ConfigureAwait(false);
            return this.value;
        }
    }

    [TestMethod]
    public async Task Await()
    {
        var value = "asdf";
        var result = await new AwaiterType<string>(value).GetValueWithDelay().ConfigureAwait(false);

        Assert.AreEqual(value, result);
    }
}
```
""";

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <exception cref="NotSupportedException">
        /// Always thrown because this method is not supported by the currently supported .NET runtimes
        /// </exception>
        /// <remarks>
        /// This method always throws <see cref="NotSupportedException"/>
        /// </remarks>
        [ExcludeFromCodeCoverage(Justification = setStateMachineMessage)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException(setStateMachineMessage);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.SetException(Exception)"/>
        public void SetException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            this.builder.SetException(exception);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.SetResult(TResult)"/>
        public void SetResult(T result)
        {
            this.builder.SetResult(result);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.AwaitOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.AwaitUnsafeOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Task"/>
        public ITask<T> Task
        {
            get
            {
                return new TaskWrapper<T>(this.builder.Task);
            }
        }
    }
}
