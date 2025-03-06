/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            public ITask<T> GetValue()
            {
                // we need to delegate to something that uses the .NET task so that we can wrap it; we aren't actually
                // implementing a task here
                return new TaskWrapper<T>(this.GetValueImpl());
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
            private async Task<T> GetValueImpl()
            {
                // we can't use `task.fromresult` because that returns a task that is already completed; so, we need to use
                // something that starts out "not completed" so that the state machine will actually call the `oncompleted`
                // methods
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }

            /// <summary>
            /// placeholder
            /// </summary>
            /// <returns></returns>
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

            /// <summary>
            /// placeholder
            /// </summary>
            public MockSynchronizationContext()
            {
                this.queue = new ConcurrentQueue<(SendOrPostCallback, object?)>();
                this.thread = new Thread(Schedule);
                thread.Start();
            }

            /// <summary>
            /// placeholder
            /// </summary>
            public int ThreadId => this.thread.ManagedThreadId;

            /// <inheritdoc/>
            public override void Send(SendOrPostCallback d, object? state)
            {
                this.queue.Enqueue((d, state));
            }

            /// <inheritdoc/>
            public override void Post(SendOrPostCallback d, object? state)
            {
                ArgumentNullException.ThrowIfNull(d);

                this.queue.Enqueue((d, state));
            }

            /// <summary>
            /// placeholder
            /// </summary>
            private void Schedule()
            {
                SynchronizationContext.SetSynchronizationContext(this);

                while (true)
                {
                    while (this.queue.TryDequeue(out var item))
                    {
                        try
                        {
                            item.Item1(item.Item2);
                        }
                        catch
                        {
                            // When we receive callbacks from an `await` operation, the state machine already handles exceptions, so those callbacks will not throw to us. However, because there may be other use cases for this synchronization context, we may receive callbacks that *do* throw. As a result, we need to handle those exceptions or else the thread will crash and we will not process subsequent callbacks. It is unfortunate that we cannot communicate back to the provider of the callback that an exception occurred. If this becomes possible at some pointer, we should do that.
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task ConfigureAwaitTrue()
        {
            var synchronizationContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            synchronizationContext.Send(null!, null);

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
    }
}
