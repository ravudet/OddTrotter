/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    public struct TaskMethodBuilder<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// must be mutable because TODO
        /// </remarks>
        private AsyncTaskMethodBuilder<T> builder;

        public static TaskMethodBuilder<T> Create()
            => new TaskMethodBuilder<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            builder.Start(ref stateMachine);
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

        [ExcludeFromCodeCoverage(Justification = setStateMachineMessage)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException(setStateMachineMessage);
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
