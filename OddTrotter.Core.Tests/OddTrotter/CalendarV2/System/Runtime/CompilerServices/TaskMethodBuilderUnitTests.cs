namespace OddTrotter.CalendarV2.System.Runtime.CompilerServices
{
    using global::System.Diagnostics;
    using global::System.Runtime.CompilerServices;
    using global::System.Runtime.InteropServices;
    using global::System.Threading.Tasks;
    using global::System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class TaskMethodBuilderUnitTests
    {
        [TestMethod]
        public async Task AwaitInterface()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValue().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        private sealed class AwaiterType<T>
        {
            private readonly T value;

            public AwaiterType(T value)
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

            [AsyncStateMachine(typeof(AwaiterType<>.GetValueDecompiledStateMachine))]
            public ITask<int> GetValueDecompiled()
            {
                GetValueDecompiledStateMachine stateMachine = default(GetValueDecompiledStateMachine);
                stateMachine.builder = TaskMethodBuilder<int>.Create();
                stateMachine.self = this;
                stateMachine.state = -1;
                stateMachine.builder.Start(ref stateMachine);
                return stateMachine.builder.Task;
            }

            [StructLayout(LayoutKind.Auto)]
            [CompilerGenerated]
            private struct GetValueDecompiledStateMachine : IAsyncStateMachine
            {
                public int state;

                public TaskMethodBuilder<int> builder;

                public AwaiterType<T> self;

                private string _5_2;

                private int _5_3;

                private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter u1;

                private void MoveNext()
                {
                    int num = state;
                    AwaiterType<T> awaiterType = self;
                    int result;
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                        if (num != 0)
                        {
                            if (num == 1)
                            {
                                awaiter = u1;
                                u1 = default(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter);
                                num = (state = -1);
                                goto IL_010b;
                            }
                            T value = awaiterType.value;
                            _5_2 = value!.ToString()!;
                            awaiter = Task.Delay(1000).ConfigureAwait(false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                num = (state = 0);
                                u1 = awaiter;
                                builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
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
                        _5_3 = _5_2!.Length;
                        awaiter = Task.Delay(1000).ConfigureAwait(false).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 1);
                            u1 = awaiter;
                            builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                            return;
                        }
                        goto IL_010b;
                    IL_010b:
                        awaiter.GetResult();
                        result = _5_3 * 2;
                    }
                    catch (Exception exception)
                    {
                        state = -2;
                        _5_2 = null!;
                        builder.SetException(exception);
                        return;
                    }
                    state = -2;
                    _5_2 = null!;
                    builder.SetResult(result);
                }

                void IAsyncStateMachine.MoveNext()
                {
                    //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                    this.MoveNext();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    builder.SetStateMachine(stateMachine);
                }

                void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                    this.SetStateMachine(stateMachine);
                }
            }

            [StructLayout(LayoutKind.Auto)]
            [CompilerGenerated]
            private struct _003CGetValueWithDelay_003Ed__2 : IAsyncStateMachine
            {
                public int _003C_003E1__state;

                public TaskMethodBuilder<T> _003C_003Et__builder;

                public AwaiterType<T> _003C_003E4__this;

                private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter _003C_003Eu__1;

                private void MoveNext()
                {
                    int num = _003C_003E1__state;
                    AwaiterType<T> awaiterType = _003C_003E4__this;
                    T value;
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                        if (num != 0)
                        {
                            awaiter = Task.Delay(100).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                num = (_003C_003E1__state = 0);
                                _003C_003Eu__1 = awaiter;
                                _003C_003Et__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, _003CGetValueWithDelay_003Ed__2>(ref awaiter, ref this);
                                return;
                            }
                        }
                        else
                        {
                            awaiter = _003C_003Eu__1;
                            _003C_003Eu__1 = default(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter);
                            num = (_003C_003E1__state = -1);
                        }

                        awaiter.GetResult();
                        value = awaiterType.value;
                    }
                    catch (Exception exception)
                    {
                        _003C_003E1__state = -2;
                        _003C_003Et__builder.SetException(exception);
                        return;
                    }

                    _003C_003E1__state = -2;
                    _003C_003Et__builder.SetResult(value);
                }

                void IAsyncStateMachine.MoveNext()
                {
                    //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                    this.MoveNext();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    _003C_003Et__builder.SetStateMachine(stateMachine);
                }

                void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                    this.SetStateMachine(stateMachine);
                }
            }

            [AsyncStateMachine(typeof(AwaiterType<>._003CGetValueWithDelay_003Ed__2))]
            public ITask<T> Decompiled2()
            {
                //IL_0002: Unknown result type (might be due to invalid IL or missing references)
                //IL_0007: Unknown result type (might be due to invalid IL or missing references)
                _003CGetValueWithDelay_003Ed__2 _003CGetValueWithDelay_003Ed__ = default(_003CGetValueWithDelay_003Ed__2);
                _003CGetValueWithDelay_003Ed__._003C_003Et__builder = TaskMethodBuilder<T>.Create();
                _003CGetValueWithDelay_003Ed__._003C_003E4__this = this;
                _003CGetValueWithDelay_003Ed__._003C_003E1__state = -1;
                _003CGetValueWithDelay_003Ed__._003C_003Et__builder.Start<_003CGetValueWithDelay_003Ed__2>(ref _003CGetValueWithDelay_003Ed__);
                return _003CGetValueWithDelay_003Ed__._003C_003Et__builder.Task;
            }
        }

        [TestMethod]
        public async Task AwaitInterfaceWithDelay()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValueWithDelay().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithNested()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValueFromNested().ConfigureAwait(false);

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
                        async () => await new AwaiterType<string>("asdf")
                            .GetValueWithException(exception)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);

            Assert.AreEqual(exception, thrownException);
        }

        [TestMethod]
        public async Task AwaitInterfaceWithStructStateMachine()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).Decompiled2().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }

        //// TODO https://devblogs.microsoft.com/dotnet/how-async-await-really-works/
        //// TODO must be release, must be framework (you used 4.8.1), "with delay"
    }
}
