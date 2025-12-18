namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Fx.AssertExtensions;

    public static class AssertExtensions
    {
        public readonly ref struct ContinuableAwaitable<TContinuable, TValue> : ITask<ContinuableAwaitable<TContinuable, TValue>.ConfiguredAwaitable, IAwaiter<TValue>, TValue>
            where TContinuable : IContinuable<TValue>, allows ref struct
        {
            private readonly TContinuable continuable;

            public ContinuableAwaitable(TContinuable continuable)
            {
                this.continuable = continuable;
            }

            public TypeHolder<ContinuableAwaitable<TContinuable, TValue>, ContinuableAwaitable<TContinuable, TValue>.ConfiguredAwaitable, IAwaiter<TValue>, TValue> AsAwaitable()
            {
                return new TypeHolder<ContinuableAwaitable<TContinuable, TValue>, ContinuableAwaitable<TContinuable, TValue>.ConfiguredAwaitable, IAwaiter<TValue>, TValue>(this);
            }

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.continuable, continueOnCapturedContext);
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<TValue, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                return this.continuable.ContinueWith(sourceContinuation, exceptionContinuation, canceledContinuation);
            }

            public IAwaiter<TValue> GetAwaiter()
            {
                return this.continuable.ContinueWith(_ => _, _ => throw _, _ => throw _).GetAwaiter();
            }

            public readonly ref struct ConfiguredAwaitable : IConfiguredAwaitable<TValue>
            {
                private readonly TContinuable continuable;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(TContinuable continuable, bool continueOnCapturedContext)
                {
                    this.continuable = continuable;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public IAwaiter<TValue> GetAwaiter()
                {

                    var continuedAgain = this.continuable.ContinueWith(_ => _, _ => throw _, _ => throw _);
                    var configured = continuedAgain.ConfigureAwait(this.continueOnCapturedContext);
                    var awaiter = configured.GetAwaiter();
                    return awaiter;

                    ////return this.continuable.ContinueWith(_ => _, _ => throw _, _ => throw _).ConfigureAwait(this.continueOnCapturedContext).GetAwaiter();
                }
            }
        }

        public readonly ref struct RefStructAwaitablePlaceholder<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue>
            where TAwaitable : ITask<TConfiguredAwaitable, TAwaiter, TValue>, allows ref struct
            where TConfiguredAwaitable : IConfiguredAwaitable<TAwaiter, TValue>, allows ref struct
            where TAwaiter : IAwaiter<TValue>
            where TValue : allows ref struct
        {
            private readonly TAwaitable awaitable;

            public RefStructAwaitablePlaceholder(TAwaitable awaitable)
            {
                this.awaitable = awaitable;
            }

            public Task<TException> Throws<TException>()
                where TException : Exception
            {
                return this.Throws<TException>(_ => { });
            }

            public Task<TException> Throws<TException>(Action<TValue> action)
                where TException : Exception
            {
                return CommitImpl<TException>(Assert.That, this.awaitable.ConfigureAwait(false).GetAwaiter(), action);
            }

            private static async Task<TException> CommitImpl<TException>(Assert assert, TAwaiter awaitable, Action<TValue> action)
                where TException : Exception
            {
                try
                {
                    var state = await awaitable;
                    action(state);
                    return assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }

        }

        public static RefStructAwaitablePlaceholder<ContinuableAwaitable<Realizable<T>, T>, ContinuableAwaitable<Realizable<T>, T>.ConfiguredAwaitable, IAwaiter<T>, T> RefStructAwaitable<T>(this Assert assert, Realizable<T> realizable)
        {
            return assert.RefStructAwaitable(realizable.AsContinuable());
        }

        public static RefStructAwaitablePlaceholder<ContinuableAwaitable<TContinuable, TValue>, ContinuableAwaitable<TContinuable, TValue>.ConfiguredAwaitable, IAwaiter<TValue>, TValue> RefStructAwaitable<TContinuable, TValue>(this Assert assert, TypeHolder<TContinuable, TValue> typeHolder)
            where TContinuable : IContinuable<TValue>, allows ref struct
        {
            return assert.RefStructAwaitable<TContinuable, TValue>(typeHolder.Self);
        }

        public static RefStructAwaitablePlaceholder<ContinuableAwaitable<TContinuable, TValue>, ContinuableAwaitable<TContinuable, TValue>.ConfiguredAwaitable, IAwaiter<TValue>, TValue> RefStructAwaitable<TContinuable, TValue>(this Assert assert, TContinuable continuable)
            where TContinuable : IContinuable<TValue>, allows ref struct
        {
            return assert.RefStructAwaitable(new ContinuableAwaitable<TContinuable, TValue>(continuable).AsAwaitable());
        }

        public static RefStructAwaitablePlaceholder<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue> RefStructAwaitable<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue>(this Assert assert, TypeHolder<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue> typeHolder)
            where TAwaitable : ITask<TConfiguredAwaitable, TAwaiter, TValue>, allows ref struct
            where TConfiguredAwaitable : IConfiguredAwaitable<TAwaiter, TValue>, allows ref struct
            where TAwaiter : IAwaiter<TValue>
            where TValue : allows ref struct
        {
            return assert.RefStructAwaitable<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue>(typeHolder.Self);
        }

        public static RefStructAwaitablePlaceholder<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue> RefStructAwaitable<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue>(this Assert assert, TAwaitable awaitable)
            where TAwaitable : ITask<TConfiguredAwaitable, TAwaiter, TValue>, allows ref struct
            where TConfiguredAwaitable : IConfiguredAwaitable<TAwaiter, TValue>, allows ref struct
            where TAwaiter : IAwaiter<TValue>
            where TValue : allows ref struct
        {
            return new RefStructAwaitablePlaceholder<TAwaitable, TConfiguredAwaitable, TAwaiter, TValue>(awaitable);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static void AreEqual<T>(this Assert assert, T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void IsInstanceOfType(this Assert assert, object? value, Type expectedType)
        {
            Assert.IsInstanceOfType(value, expectedType);
        }

        public static T ThrowsException<T>(this Assert assert, Action action)
            where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }

        public readonly ref struct ThrowsExceptionBuilder<TState>
            where TState : allows ref struct
        {
            private readonly Assert assert;
            private readonly TState state;

            public ThrowsExceptionBuilder(Assert assert, TState state)
            {
                this.assert = assert;
                this.state = state;
            }

            public TException Commit<TException>(Action<TState> action)
                where TException : Exception
            {
                try
                {
                    action(this.state);
                    return this.assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return this.assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }
        }

        public static ThrowsExceptionBuilder<TState> ThrowsException<TState>(this Assert assert, TState state)
            where TState : allows ref struct
        {
            return new ThrowsExceptionBuilder<TState>(assert, state);
        }

        public readonly ref struct ThrowsExceptionAsyncBuilder<TState>
        {
            private readonly Assert assert;
            private readonly Realizable<TState> awaitable;

            public ThrowsExceptionAsyncBuilder(Assert assert, Realizable<TState> awaitable)
            {
                this.assert = assert;
                this.awaitable = awaitable;
            }

            public Task<TException> Commit<TException>()
                where TException : Exception
            {
                return this.Commit<TException>(state => { });
            }

            public Task<TException> Commit<TException>(Action<TState> action)
                where TException : Exception
            {
                return CommitImpl<TException>(this.assert, this.awaitable.GetAwaiter(), action);
            }

            private static async Task<TException> CommitImpl<TException>(Assert assert, IAwaiter<TState> awaitable, Action<TState> action)
                where TException : Exception
            {
                try
                {
                    var state = await awaitable;
                    action(state);
                    return assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }

            /*public async Task<TException> CommitAsync<TException>(Func<TState, Task> action)
                where TException : Exception
            {
                //// TODO implement this

                try
                {
                    var state = await this.awaitable.ConfigureAwait(false);
                    await action(state).ConfigureAwait(false);
                    return this.assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return this.assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }*/
        }

        public static ThrowsExceptionAsyncBuilder<TState> ThrowsExceptionAsync<TState>(this Assert assert, Realizable<TState> state)
        {
            return new ThrowsExceptionAsyncBuilder<TState>(assert, state);
        }

        public static async Task<T> ThrowsExceptionAsync<T>(this Assert assert, Func<Task> action) where T : Exception
        {
            return await Assert.ThrowsExceptionAsync<T>(action).ConfigureAwait(false);
        }
    }
}
