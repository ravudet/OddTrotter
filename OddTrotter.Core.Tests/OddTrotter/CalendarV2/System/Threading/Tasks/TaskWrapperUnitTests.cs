namespace System.Threading.Tasks
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [TestClass]
    public sealed class TaskWrapperUnitTests
    {
        [TestMethod]
        public void InitializeNullTask()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TaskWrapper<string>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }

        [TestMethod]
        public async Task UnsafeOnCompleted()
        {
            var providedValue = "Asdf";
            var value = await new AwaitedType<string>(providedValue).GetValue();
            Assert.AreEqual(providedValue, value);
        }

        private sealed class AwaitedType<T>
        {
            private readonly T value;

            public AwaitedType(T value)
            {
                this.value = value;
            }

            public ITask<T> GetValue()
            {
                // we need to delegate to something that uses the .NET task so that we can wrap it; we aren't actually
                // implementing a task here
                return new TaskWrapper<T>(GetValueImpl());
            }

            private async Task<T> GetValueImpl()
            {
                // we can't use `task.fromresult` because that returns a task that is already completed; so, we need to use
                // something that starts out "not completed" so that the state machine will actually call the `oncompleted`
                // methods
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }
        }




















        private sealed class Context : SynchronizationContext
        {
            private readonly ConcurrentQueue<(SendOrPostCallback, object?)> queue;
            private readonly Thread thread;

            public Context()
            {
                this.queue = new ConcurrentQueue<(SendOrPostCallback, object?)>();
                this.thread = new Thread(Schedule);
                thread.Start();
            }

            public int ThreadId => this.thread.ManagedThreadId;

            public override void Send(SendOrPostCallback d, object? state)
            {
                this.queue.Enqueue((d, state));
            }

            public override void Post(SendOrPostCallback d, object? state)
            {
                this.queue.Enqueue((d, state));
            }

            private void Schedule()
            {
                SynchronizationContext.SetSynchronizationContext(this);

                while (true)
                {
                    while (this.queue.TryDequeue(out var item))
                    {
                        item.Item1(item.Item2);
                    }
                }
            }
        }

        [TestMethod]
        public async Task AnotherTest()
        {
            var current = Thread.CurrentThread;

            SynchronizationContext.SetSynchronizationContext(new Context());

            ////int hashCode;
            /*if (SynchronizationContext.Current == null)
            {
                var context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
                SynchronizationContext.Current?.Post(state => Console.WriteLine(state), "post");
                hashCode = context.GetHashCode();
            }
            else
            {
                SynchronizationContext.Current.Post(state => Console.WriteLine(state), "post");
                hashCode = SynchronizationContext.Current.GetHashCode();
            }*/

            ////var getValueTask = new AwaitedType().GetValue().ConfigureAwait(true);

            var taskId = Task.CurrentId;

            await Task.Delay(1).ConfigureAwait(true);

            if (SynchronizationContext.Current is Context context)
            {
                var threadId2 = context.ThreadId;
            }
            else
            {
                Assert.Fail();
            }

            await Task.Delay(100).ConfigureAwait(true);

            if (SynchronizationContext.Current is Context context2)
            {
                var threadId2 = context2.ThreadId;
            }
            else
            {
                Assert.Fail();
            }

            /*var value = await getValueTask;

            Assert.AreEqual("asdf", value);*/

            var taskId2 = Task.CurrentId;

            Assert.IsNotNull(SynchronizationContext.Current);
            ////Assert.AreEqual(hashCode, SynchronizationContext.Current.GetHashCode());
        }

        private static object Lock = new object();

        [TestMethod]
        public Task Test2()
        {//// TODO use this to create a test which runs the safe oncompleted method: https://sharplab.io/#v2:CYLg1APgAgTAjAWAFBQAwAIpwKwG5lqZwB0ASgK4B2ALgJYC2ApsQMID29ADrQDaMBOAZQEA3WgGNGAZ3woAzJhjoW6AN4EFUAByYAbOgAq06gAoAlMnVJ0N9CICG/O/Z7lG6ALyYAnOkqMAd3QAQQD7WmpGYAMAT05Gc2IAcUZqADUXN3NZAF9kZE5+WgdI9ClGFyjFELCIqNj4y2RbTAUASShdAB4sVAA+dBT0zISLa1srFpaoAHY/QL0AdX57Tnj+Hrh+kwAxNjZzM1kWvKRm2yhNHU7N/vQ9g7GWyambKF9O4gARRh57GJMW1QZlYbEoADNaABzcj8RihcKmcEucpHc6vWboABE9ikwHBWOOtlOpxa6Na6FoNAEyMk6A63TY5Gohj6TXGNgZCLqGwMAyG3Mi/GyyFJtnJlzKFT4wGqnWWq3WXT56BA9JufPZLUKxXspTh9mAYJ4MT0yoG1FxAGtZBLNLoFWsBCYNRbrU8JuSWtQABa0KTES1SK2edBBm3ksU2O3q3SCgTmwapePCj02F4Yub+IKdFOOpV8ky+/2B63JZO1IWHIk2EnkmNUoW09xcysJpkslVqtosIp0cQuABybDo4Ji7C4fDoYK1tgARvsePSpBPOFOqqp0FDUrh0KdyQYk9RSNJyDxTGikFH0DHypVZbA9HmVk7eQNu7m22/ZzYdSV3AaRqUCaT5fom4YpraHIUp+iICPmzqwTy4HWimaZqF6tjFgGEFfqGuFwfwNZ7vk0GSgubBLm0K4cGuqRROSGZTNu1CYembFTJi2GlsGKbENRq7rsAxEnJGpHTAoh5DCeUhnhejEcVxfo4ahX7lsep7niK0H7mRmgACzoAA8pQgn0cALpwEo4hgnQlDkHqtBguhTHespPFWnxJlmZEFk2TQVIOdOlCXqJZx6ZghkAKqUFI9jgow3m0UJlnWbZgWOc5CnQW5JYETyxAxXFCVJZO5kmP5dlBU5IXEXWV5AA=
            StateMachine stateMachine = new StateMachine();
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.self = this;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// This class is a copy of the compiler-generated code for the async test method with the following body:
        /// ```
        /// var value = await new AwaitedType().GetValue();
        /// Assert.AreEqual("asdf", value);
        /// ```
        /// 
        /// It has only been modified to use <see cref="AsyncTaskMethodBuilder.AwaitOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/> instead of <see cref="AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/> in order to get code covereage on the <see cref="TaskAwaiterWrapper{T}.OnCompleted(Action)"/> method.
        /// </remarks>
        private sealed class StateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder builder;

            public TaskWrapperUnitTests? self;

            private string? value;

            private string? s;

            private object? u;

            private void MoveNext()
            {
                int num = state;
                try
                {
                    ITaskAwaiter<string> awaiter;
                    if (num != 0)
                    {
                        awaiter = new AwaitedType<string>("asfd").GetValue().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 0);
                            u = awaiter;
                            StateMachine stateMachine = this;
                            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = ((ITaskAwaiter<string>)u!); //// TODO you addeed the bang, is that ok?
                        u = null;
                        num = (state = -1);
                    }
                    s = awaiter.GetResult();
                    value = s;
                    s = null;
                    Assert.AreEqual("asdf", value);
                }
                catch (Exception exception)
                {
                    state = -2;
                    value = null;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                value = null;
                builder.SetResult();
            }

            void IAsyncStateMachine.MoveNext()
            {
                //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                this.MoveNext();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }

            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                this.SetStateMachine(stateMachine);
            }
        }
    }
}
