namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertExtensions
    {
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
                //// TODO use the caller's `assert` instance
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

        public static async Task<T> ThrowsExceptionAsync<T>(this Assert assert, Func<Task> action) 
            where T : Exception
        {
            return await Assert.ThrowsExceptionAsync<T>(action);
        }

        public readonly ref struct RefStructPlaceHolder<TValue>
            where TValue : allows ref struct
        {
            private readonly TValue value;

            public RefStructPlaceHolder(TValue value)
            {
                this.value = value;
            }

            public TException ThrowsException<TException>(Action<TValue> action)
                where TException : Exception
            {
                try
                {
                    action(this.value);
                    //// TODO use the caller's `assert` instance
                    return Assert.That.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return Assert.That.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }
            }
        }

        public static RefStructPlaceHolder<TValue> RefStruct<TValue>(this Assert assert, TValue value)
            where TValue : allows ref struct
        {
            return new RefStructPlaceHolder<TValue>(value);
        }
    }
}
