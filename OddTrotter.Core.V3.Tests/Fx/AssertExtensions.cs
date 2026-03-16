namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Security.AccessControl;
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




























        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool)"/>
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

        public static void IsNull(this Assert assert, object value, string message)
        {
            Assert.IsNull(value, message);
        }

        public static void IsNull(this Assert assert, object value, string message, params object[] parameters)
        {
            Assert.IsNull(value, message, parameters);
        }

        public static void IsNotNull(this Assert assert, [NotNull] object value)
        {
            Assert.IsNotNull(value);
        }

        public static void IsNotNull(this Assert assert, [NotNull] object value, string message)
        {
            Assert.IsNotNull(value, message);
        }

        public static void IsNotNull(this Assert assert, [NotNull] object value, string message, params object[] parameters)
        {
            Assert.IsNotNull(value, message, parameters);
        }

        public static void AreSame(this Assert assert, object expected, object actual)
        {
            Assert.AreSame(expected, actual);
        }

        public static void AreSame(this Assert assert, object expected, object actual, string message)
        {
            Assert.AreSame(expected, actual, message);
        }

        public static void AreSame(this Assert assert, object expected, object actual, string message, params object[] parameters)
        {
            Assert.AreSame(expected, actual, message, parameters);
        }

        public static void AreNotSame(this Assert assert, object notExpected, object actual)
        {
            Assert.AreNotSame(notExpected, actual);
        }

        public static void AreNotSame(this Assert assert, object notExpected, object actual, string message)
        {
            Assert.AreNotSame(notExpected, actual, message);
        }

        public static void AreNotSame(this Assert assert, object notExpected, object actual, string message, params object[] parameters)
        {
            Assert.AreNotSame(notExpected, actual, message, parameters);
        }

        public static void AreEqual<T>(this Assert assert, T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual<T>(this Assert assert, T expected, T actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void AreEqual<T>(this Assert assert, T expected, T actual, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, message, parameters);
        }

        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual)
        {
            Assert.AreNotEqual(notExpected, actual);
        }

        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual, string message)
        {
            Assert.AreNotEqual(notExpected, actual, message);
        }

        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, message, parameters);
        }

        public static void AreEqual(this Assert assert, object expected, object actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(this Assert assert, object expected, object actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void AreEqual(this Assert assert, object expected, object actual, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, message, parameters);
        }

        public static void AreNotEqual(this Assert assert, object notExpected, object actual)
        {
            Assert.AreNotEqual(notExpected, actual);
        }

        public static void AreNotEqual(this Assert assert, object notExpected, object actual, string message)
        {
            Assert.AreNotEqual(notExpected, actual, message);
        }

        public static void AreNotEqual(this Assert assert, object notExpected, object actual, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, message, parameters);
        }

        public static void AreEqual(this Assert assert, float expected, float actual, float delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        public static void AreEqual(this Assert assert, float expected, float actual, float delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        public static void AreEqual(this Assert assert, float expected, float actual, float delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
        }

        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
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
