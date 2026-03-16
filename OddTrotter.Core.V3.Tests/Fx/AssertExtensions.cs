namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NuGet.Frameworks;

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

            public Task<TException> ThrowsException<TException>()
                where TException : Exception
            {
                return this.ThrowsException<TException>(_ => { });
            }

            public Task<TException> ThrowsException<TException>(Action<TValue> action)
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





























        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition)
        {
            Assert.IsTrue(condition);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition, string message, params object[] parameters)
        {
            Assert.IsTrue(condition, message, parameters);
        }

        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition, string message, params object[] parameters)
        {
            Assert.IsTrue(condition, message, parameters);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition)
        {
            Assert.IsFalse(condition);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition)
        {
            Assert.IsFalse(condition);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition, string message, params object[] parameters)
        {
            Assert.IsFalse(condition, message, parameters);
        }

        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition, string message, params object[] parameters)
        {
            Assert.IsFalse(condition, message, parameters);
        }

        public static void IsNull(this Assert assert, object value)
        {
            Assert.IsNull(value);
        }

        public static void 










        public static void AreEqual<T>(this Assert assert, T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void IsNull(this Assert assert, object? value)
        {
            Assert.IsNull(value);
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
    }
}
