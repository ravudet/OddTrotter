namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class TaskMethodBuilderUnitTests
    {
        [TestMethod]
        public async Task AwaitInterface()
        {
            var value = "asdf";
            var result = await new AwaitedType<string>(value).GetValue().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        private sealed class AwaitedType<T>
        {
            private readonly T value;

            public AwaitedType(T value)
            {
                this.value = value;
            }

            public async ITask<T> GetValue()
            {
                return await Task.FromResult(this.value).ConfigureAwait(false);
            }

            public async ITask<T> GetValueWithDelay()
            {
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }

            [AsyncStateMachine(typeof(AwaitedType<>.GetValueWithDelaySafeOnCompletedStateMachine))]
            [DebuggerStepThrough]
            public ITask<T> GetValueWithDelaySafeOnCompleted()
            {
                GetValueWithDelaySafeOnCompletedStateMachine stateMachine = new GetValueWithDelaySafeOnCompletedStateMachine();
                stateMachine.builder = TaskMethodBuilder<T>.Create();
                stateMachine.self = this;
                stateMachine.state = -1;
                stateMachine.builder.Start(ref stateMachine);
                return stateMachine.builder.Task;
            }

            [CompilerGenerated]
            private sealed class GetValueWithDelaySafeOnCompletedStateMachine : IAsyncStateMachine
            {
                public int state;

                public TaskMethodBuilder<T> builder;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                public AwaitedType<T> self;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

                private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter u1;

                private void MoveNext()
                {
                    int num = state;
                    T value;
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                        if (num != 0)
                        {
                            awaiter = Task.Delay(100).ConfigureAwait(false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                num = (state = 0);
                                u1 = awaiter;
                                GetValueWithDelaySafeOnCompletedStateMachine stateMachine = this;
                                builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                                return;
                            }
                        }
                        else
                        {
                            awaiter = u1;
                            u1 = default(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter);
                            num = (state = -1);
                        }
                        awaiter.GetResult();
                        value = self.value;
                    }
                    catch (Exception exception)
                    {
                        state = -2;
                        builder.SetException(exception);
                        return;
                    }
                    state = -2;
                    builder.SetResult(value);
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

            public async ITask<T> GetValueFromNested()
            {
                return await this.GetValue().ConfigureAwait(false);
            }

            public async ITask<T> GetValueWithException(Exception exception)
            {
                Throw(exception);
                return await this.GetValue().ConfigureAwait(false);
            }

            private static void Throw(Exception exception)
            {
                throw exception;
            }
        }

        [TestMethod]
        public async Task AwaitInterfaceWithDelay()
        {
            var value = "asdf";
            var result = await new AwaitedType<string>(value).GetValueWithDelay().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithNested()
        {
            var value = "asdf";
            var result = await new AwaitedType<string>(value).GetValueFromNested().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithException()
        {
            var message = "a message";
            var exception = new InvalidOperationException(message);

            var thrownException =
                await Assert
                    .ThrowsExceptionAsync<InvalidOperationException>(
                        async () => await new AwaitedType<string>("asdf")
                            .GetValueWithException(exception)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);

            Assert.AreEqual(exception, thrownException);
        }

        [TestMethod]
        public async Task GetValueWithDelaySafeOnCompleted()
        {
            var value = "asdf";
            var result = await new AwaitedType<string>(value).GetValueWithDelaySafeOnCompleted().ConfigureAwait(false);

            Assert.AreEqual(value, result);

        }
    }
}
