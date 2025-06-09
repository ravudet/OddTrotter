/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="value"></param>
            public AwaitedType(T value)
            {
                this.value = value;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
            public async ITask<T> GetValue()
            {
                return await Task.FromResult(this.value).ConfigureAwait(false);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
            public async ITask<T> GetValueWithDelay()
            {
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// This method and it's associated <see cref="GetValueWithDelaySafeOnCompletedStateMachine"/> are slightly modified
            /// code from the compiler generated state machine for the following code:
            /// ```
            /// await Task.Delay(100).ConfigureAwait(false);
            /// return this.value;
            /// ```
            /// 
            /// The compiler generated code has been modified in the following way:
            /// 1. identifiers have been renamed to be legal
            /// 2. nullability issues have been suppressed, removed, or forgiven
            /// 3. the line `builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);` has been changed to
            /// `builder.AwaitOnCompleted(ref awaiter, ref stateMachine);`
            /// 
            /// The intent is to have a test which covers the case where the "safe" `OnCompleted` variant is called while still
            /// providing callers the more efficient "unsafe" variant.
            /// </remarks>
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

                public AwaitedType<T>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                    self
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                    ;

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

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
            public async ITask<T> GetValueFromNested()
            {
                return await this.GetValue().ConfigureAwait(false);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="exception"></param>
            /// <returns></returns>
            /// <exception cref="TException">Throws <paramref name="exception"/></exception>
            public async ITask<T> GetValueWithException<TException>(TException exception) where TException : Exception
            {
                Throw(exception);
                return await this.GetValue().ConfigureAwait(false);
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <param name="exception"></param>
            /// <exception cref="TException">Throws <paramref name="exception"/></exception>
            private static void Throw<TException>(TException exception) where TException : Exception
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
