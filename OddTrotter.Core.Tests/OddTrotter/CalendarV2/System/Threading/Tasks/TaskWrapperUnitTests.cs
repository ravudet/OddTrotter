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

            public ITask<T> GetValueNoDelay()
            {
                return new TaskWrapper<T>(Task.FromResult(this.value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method and it's associated <see cref="SafeOnCompletedStateMachine"/> are slightly modified code from the compiler generated state machine for the following code:
        /// ```
        /// var providedValue = "Asdf";
        /// var value = await new AwaitedType<string>(providedValue).GetValue();
        /// Assert.AreEqual(providedValue, value);
        /// ```
        /// 
        /// The compiler generated code has been modified in the following way:
        /// 1. identifiers have been renamed to be legal
        /// 2. nullability issues have been suppressed, removed, or forgiven
        /// 3. the line `builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);` has been changed to `builder.AwaitOnCompleted(ref awaiter, ref stateMachine);`
        /// 
        /// The intent is to have a test which covers the case where the "safe" `OnCompleted` variant is called while still provided callers the more efficient "unsafe" variant.
        /// </remarks>
        [TestMethod]
        public Task SafeOnCompleted()
        {
            SafeOnCompletedStateMachine stateMachine = new SafeOnCompletedStateMachine();
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.self = this;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

        [CompilerGenerated]
        private sealed class SafeOnCompletedStateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder builder;

            public TaskWrapperUnitTests
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                self
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                providedValue
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                value
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                s3
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private object
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                u1;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

            private void MoveNext()
            {
                int num = state;
                try
                {
                    ITaskAwaiter<string> awaiter;
                    if (num != 0)
                    {
                        providedValue = "Asdf";
                        awaiter = new AwaitedType<string>(providedValue).GetValue().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 0);
                            u1 = awaiter;
                            SafeOnCompletedStateMachine stateMachine = this;
                            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = (ITaskAwaiter<string>)u1;
                        u1 = null!;
                        num = (state = -1);
                    }
                    s3 = awaiter.GetResult();
                    value = s3;
                    s3 = null!;
                    Assert.AreEqual(providedValue, value);
                }
                catch (Exception exception)
                {
                    state = -2;
                    providedValue = null!;
                    value = null!;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                providedValue = null!;
                value = null!;
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

        private sealed class MockSynchronizationContext : SynchronizationContext
        {
            private readonly ConcurrentQueue<(SendOrPostCallback, object?)> queue;
            private readonly Thread thread;

            public MockSynchronizationContext()
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
        public async Task ConfigureAwaitTrue()
        {
            var synchronizationContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            var providedValue = "Asdf";
            var value = await new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(true);
            Assert.AreEqual(providedValue, value);

            var currentContext = SynchronizationContext.Current;
            Assert.IsNotNull(currentContext);
            Assert.AreEqual(synchronizationContext, currentContext);
            Assert.AreEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        [TestMethod]
        public async Task ConfigureAwaitFalseWithDelay()
        {
            var synchronizationContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            var providedValue = "Asdf";
            var value = await new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(false);
            Assert.AreEqual(providedValue, value);

            var currentContext = SynchronizationContext.Current;

            // from [this article](https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html?s=03):
            // > ConfigureAwaitOptions.None is the same as ConfigureAwait(continueOnCapturedContext: false). In other words,
            // > await will behave perfectly normally, except that it will not capture the context; assuming the await does yield
            // > (i.e, the task is not already complete), then the async method will resume executing on any available thread
            // > pool thread.
            // 
            // so, we know that, because `Task.Delay` returns an unfinished `task` and therefore does "yield", that the context
            // will not be catpured; as a result, we can assert that the `currentContext` is *not* the same as the original
            // context
            Assert.AreNotEqual(synchronizationContext, currentContext);
            Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        }



        [TestMethod]
        public async Task ConfigureAwaitFalseWithNoDelay()
        {
            var synchronizationContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            var providedValue = "Asdf";
            var value = await new AwaitedType<string>(providedValue).GetValueNoDelay().ConfigureAwait(false);
            Assert.AreEqual(providedValue, value);

            // from [this article](https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html?s=03):
            // > ConfigureAwaitOptions.None is the same as ConfigureAwait(continueOnCapturedContext: false). In other words,
            // > await will behave perfectly normally, except that it will not capture the context; assuming the await does yield
            // > (i.e, the task is not already complete), then the async method will resume executing on any available thread
            // > pool thread.
            // 
            // so, we know that, because `Task.FromResult` returns a finished `task` and therefore does  *not* "yield", that the
            // context may or may not be preserved; as a result, we cannot assert anything about the current synchronization
            // context at this point; however, because `synchronizationContext` has its own thread dedicated to it, we *do* know
            // that the "continued with" delegate of the rest of this method will *not* be running on that thread, so we can assert that current thread is not the one used by `synchronizationContext`
            Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method and it's associated <see cref="SafeOnCompletedConfigureAwaitTrueStateMachine"/> are slightly modified code from the compiler generated state machine for the following code:
        /// ```
        /// var synchronizationContext = new MockSynchronizationContext();
        /// SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        /// 
        /// var providedValue = "Asdf";
        /// var value = await new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(true);
        /// Assert.AreEqual(providedValue, value);
        /// 
        /// var currentContext = SynchronizationContext.Current;
        /// Assert.IsNotNull(currentContext);
        /// Assert.AreEqual(synchronizationContext, currentContext);
        /// Assert.AreEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        /// ```
        /// 
        /// The compiler generated code has been modified in the following way:
        /// 1. identifiers have been renamed to be legal
        /// 2. nullability issues have been suppressed, removed, or forgiven
        /// 3. the line `builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);` has been changed to `builder.AwaitOnCompleted(ref awaiter, ref stateMachine);`
        /// 
        /// The intent is to have a test which covers the case where the "safe" `OnCompleted` variant is called while still provided callers the more efficient "unsafe" variant.
        /// </remarks>
        [TestMethod]
        public Task SafeOnCompletedConfigureAwaitTrue()
        {
            SafeOnCompletedConfigureAwaitTrueStateMachine stateMachine = new SafeOnCompletedConfigureAwaitTrueStateMachine();
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.self = this;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

        [CompilerGenerated]
        private sealed class SafeOnCompletedConfigureAwaitTrueStateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder builder;

            public TaskWrapperUnitTests
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                self
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private MockSynchronizationContext
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                synchronizationContext
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                providedValue
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                value
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private SynchronizationContext? currentContext;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                s5
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private object
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                u1
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private void MoveNext()
            {
                int num = state;
                try
                {
                    ITaskAwaiter<string> awaiter;
                    if (num != 0)
                    {
                        synchronizationContext = new MockSynchronizationContext();
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        providedValue = "Asdf";
                        awaiter = new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(true).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 0);
                            u1 = awaiter;
                            SafeOnCompletedConfigureAwaitTrueStateMachine stateMachine = this;
                            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = (ITaskAwaiter<string>)u1;
                        u1 = null!;
                        num = (state = -1);
                    }
                    s5 = awaiter.GetResult();
                    value = s5;
                    s5 = null!;
                    Assert.AreEqual(providedValue, value);
                    currentContext = SynchronizationContext.Current;
                    Assert.IsNotNull(currentContext);
                    Assert.AreEqual(synchronizationContext, currentContext);
                    Assert.AreEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception exception)
                {
                    state = -2;
                    synchronizationContext = null!;
                    providedValue = null!;
                    value = null!;
                    currentContext = null;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                synchronizationContext = null!;
                providedValue = null!;
                value = null!;
                currentContext = null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method and it's associated <see cref="SafeOnCompletedConfigureAwaitFalseWithDelayStateMachine"/> are slightly modified code from the compiler generated state machine for the following code:
        /// ```
        /// var synchronizationContext = new MockSynchronizationContext();
        /// SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        /// 
        /// var providedValue = "Asdf";
        /// var value = await new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(false);
        /// Assert.AreEqual(providedValue, value);
        /// 
        /// var currentContext = SynchronizationContext.Current;
        /// 
        /// // from [this article](https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html?s=03):
        /// // > ConfigureAwaitOptions.None is the same as ConfigureAwait(continueOnCapturedContext: false). In other words,
        /// // > await will behave perfectly normally, except that it will not capture the context; assuming the await does yield
        /// // > (i.e, the task is not already complete), then the async method will resume executing on any available thread
        /// // > pool thread.
        /// // 
        /// // so, we know that, because `Task.Delay` returns an unfinished `task` and therefore does "yield", that the context
        /// // will not be catpured; as a result, we can assert that the `currentContext` is *not* the same as the original
        /// // context
        /// Assert.AreNotEqual(synchronizationContext, currentContext);
        /// Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        /// ```
        /// 
        /// The compiler generated code has been modified in the following way:
        /// 1. identifiers have been renamed to be legal
        /// 2. nullability issues have been suppressed, removed, or forgiven
        /// 3. the line `builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);` has been changed to `builder.AwaitOnCompleted(ref awaiter, ref stateMachine);`
        /// 
        /// The intent is to have a test which covers the case where the "safe" `OnCompleted` variant is called while still provided callers the more efficient "unsafe" variant.
        /// </remarks>
        [TestMethod]
        public Task SafeOnCompletedConfigureAwaitFalseWithDelay()
        {            
            SafeOnCompletedConfigureAwaitFalseWithDelayStateMachine stateMachine = new SafeOnCompletedConfigureAwaitFalseWithDelayStateMachine();
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.self = this;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

        [CompilerGenerated]
        private sealed class SafeOnCompletedConfigureAwaitFalseWithDelayStateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder builder;

            public TaskWrapperUnitTests
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                self
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private MockSynchronizationContext
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                synchronizationContext
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                providedValue
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                value
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private SynchronizationContext? currentContext;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                s5
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private object
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                u1
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private void MoveNext()
            {
                int num = state;
                try
                {
                    ITaskAwaiter<string> awaiter;
                    if (num != 0)
                    {
                        synchronizationContext = new MockSynchronizationContext();
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        providedValue = "Asdf";
                        awaiter = new AwaitedType<string>(providedValue).GetValue().ConfigureAwait(false).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 0);
                            u1 = awaiter;
                            SafeOnCompletedConfigureAwaitFalseWithDelayStateMachine stateMachine = this;
                            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = (ITaskAwaiter<string>)u1;
                        u1 = null!;
                        num = (state = -1);
                    }
                    s5 = awaiter.GetResult();
                    value = s5;
                    s5 = null!;
                    Assert.AreEqual(providedValue, value);
                    currentContext = SynchronizationContext.Current;
                    Assert.AreNotEqual(synchronizationContext, currentContext);
                    Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception exception)
                {
                    state = -2;
                    synchronizationContext = null!;
                    providedValue = null!;
                    value = null!;
                    currentContext = null;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                synchronizationContext = null!;
                providedValue = null!;
                value = null!;
                currentContext = null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method and it's associated <see cref="SafeOnCompletedConfigureAwaitFalseWithNoDelayStateMachine"/> are slightly modified code from the compiler generated state machine for the following code:
        /// ```
        /// var synchronizationContext = new MockSynchronizationContext();
        /// SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        /// 
        /// var providedValue = "Asdf";
        /// var value = await new AwaitedType<string>(providedValue).GetValueNoDelay().ConfigureAwait(false);
        /// Assert.AreEqual(providedValue, value);
        /// 
        /// // from [this article](https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html?s=03):
        /// // > ConfigureAwaitOptions.None is the same as ConfigureAwait(continueOnCapturedContext: false). In other words,
        /// // > await will behave perfectly normally, except that it will not capture the context; assuming the await does yield
        /// // > (i.e, the task is not already complete), then the async method will resume executing on any available thread
        /// // > pool thread.
        /// // 
        /// // so, we know that, because `Task.FromResult` returns a finished `task` and therefore does  *not* "yield", that the
        /// // context may or may not be preserved; as a result, we cannot assert anything about the current synchronization
        /// // context at this point; however, because `synchronizationContext` has its own thread dedicated to it, we *do* know
        /// // that the "continued with" delegate of the rest of this method will *not* be running on that thread, so we can
        /// // assert that current thread is not the one used by `synchronizationContext`
        /// Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
        /// ```
        /// 
        /// The compiler generated code has been modified in the following way:
        /// 1. identifiers have been renamed to be legal
        /// 2. nullability issues have been suppressed, removed, or forgiven
        /// 3. the line `builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);` has been changed to `builder.AwaitOnCompleted(ref awaiter, ref stateMachine);`
        /// 
        /// The intent is to have a test which covers the case where the "safe" `OnCompleted` variant is called while still provided callers the more efficient "unsafe" variant.
        /// </remarks>
        [TestMethod]
        public Task SafeOnCompletedConfigureAwaitFalseWithNoDelay()
        {
            SafeOnCompletedConfigureAwaitFalseWithNoDelayStateMachine stateMachine = new SafeOnCompletedConfigureAwaitFalseWithNoDelayStateMachine();
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.self = this;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

        [CompilerGenerated]
        private sealed class SafeOnCompletedConfigureAwaitFalseWithNoDelayStateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder builder;

            public TaskWrapperUnitTests
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                self
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private MockSynchronizationContext
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                synchronizationContext
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                providedValue
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                value
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private string
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                s4
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private object
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                u1
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
                ;

            private void MoveNext()
            {
                int num = state;
                try
                {
                    ITaskAwaiter<string> awaiter;
                    if (num != 0)
                    {
                        synchronizationContext = new MockSynchronizationContext();
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        providedValue = "Asdf";
                        awaiter = new AwaitedType<string>(providedValue).GetValueNoDelay().ConfigureAwait(false).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (state = 0);
                            u1 = awaiter;
                            SafeOnCompletedConfigureAwaitFalseWithNoDelayStateMachine stateMachine = this;
                            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = (ITaskAwaiter<string>)u1;
                        u1 = null!;
                        num = (state = -1);
                    }
                    s4 = awaiter.GetResult();
                    value = s4;
                    s4 = null!;
                    Assert.AreEqual(providedValue, value);
                    Assert.AreNotEqual(synchronizationContext.ThreadId, Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception exception)
                {
                    state = -2;
                    synchronizationContext = null!;
                    providedValue = null!;
                    value = null!;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                synchronizationContext = null!;
                providedValue = null!;
                value = null!;
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





        //// TODO you pull the above state machine from sharplab: https://sharplab.io/#v2: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







    }
}
