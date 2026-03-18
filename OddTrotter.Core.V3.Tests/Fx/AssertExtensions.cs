namespace Fx
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.V2;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.ObjectiveC;
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

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool?)"/>
        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition)
        {
            Assert.IsTrue(condition);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool, string)"/>
        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool?, string)"/>
        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool, string, object[])"/>
        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool condition, string message, params object[] parameters)
        {
            Assert.IsTrue(condition, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsTrue(bool?, string, object[])"/>
        public static void IsTrue(this Assert assert, [DoesNotReturnIf(false)] bool? condition, string message, params object[] parameters)
        {
            Assert.IsTrue(condition, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool)"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition)
        {
            Assert.IsFalse(condition);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool?)"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition)
        {
            Assert.IsFalse(condition);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool, string)"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool?, string)"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool, string, object[])"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool condition, string message, params object[] parameters)
        {
            Assert.IsFalse(condition, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsFalse(bool?, string, object[])"/>
        public static void IsFalse(this Assert assert, [DoesNotReturnIf(true)] bool? condition, string message, params object[] parameters)
        {
            Assert.IsFalse(condition, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNull(object)"/>
        public static void IsNull(this Assert assert, object value)
        {
            Assert.IsNull(value);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNull(object, string)"/>
        public static void IsNull(this Assert assert, object value, string message)
        {
            Assert.IsNull(value, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNull(object, string, object[])"/>
        public static void IsNull(this Assert assert, object value, string message, params object[] parameters)
        {
            Assert.IsNull(value, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotNull(object)"/>
        public static void IsNotNull(this Assert assert, [NotNull] object value)
        {
            Assert.IsNotNull(value);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotNull(object, string)"/>
        public static void IsNotNull(this Assert assert, [NotNull] object value, string message)
        {
            Assert.IsNotNull(value, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotNull(object, string, object[])"/>
        public static void IsNotNull(this Assert assert, [NotNull] object value, string message, params object[] parameters)
        {
            Assert.IsNotNull(value, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreSame(object, object)"/>
        public static void AreSame(this Assert assert, object expected, object actual)
        {
            Assert.AreSame(expected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreSame(object, object, string)"/>
        public static void AreSame(this Assert assert, object expected, object actual, string message)
        {
            Assert.AreSame(expected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual{T}(T, T, string, object[])"/>
        public static void AreSame(this Assert assert, object expected, object actual, string message, params object[] parameters)
        {
            Assert.AreSame(expected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotSame(object, object)"/>
        public static void AreNotSame(this Assert assert, object notExpected, object actual)
        {
            Assert.AreNotSame(notExpected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotSame(object, object, string)"/>
        public static void AreNotSame(this Assert assert, object notExpected, object actual, string message)
        {
            Assert.AreNotSame(notExpected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotSame(object, object, string, object[])"/>
        public static void AreNotSame(this Assert assert, object notExpected, object actual, string message, params object[] parameters)
        {
            Assert.AreNotSame(notExpected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual{T}(T, T)"/>
        public static void AreEqual<T>(this Assert assert, T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual{T}(T, T, string)"/>
        public static void AreEqual<T>(this Assert assert, T expected, T actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual{T}(T, T, string, object[])"/>
        public static void AreEqual<T>(this Assert assert, T expected, T actual, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual{T}(T, T)"/>
        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual)
        {
            Assert.AreNotEqual(notExpected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual{T}(T, T, string)"/>
        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual, string message)
        {
            Assert.AreNotEqual(notExpected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual{T}(T, T, string, object[])"/>
        public static void AreNotEqual<T>(this Assert assert, T notExpected, T actual, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object)"/>
        public static void AreEqual(this Assert assert, object expected, object actual)
        {
            Assert.AreEqual(expected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object, string)"/>
        public static void AreEqual(this Assert assert, object expected, object actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object, string, object[])"/>
        public static void AreEqual(this Assert assert, object expected, object actual, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object)"/>
        public static void AreNotEqual(this Assert assert, object notExpected, object actual)
        {
            Assert.AreNotEqual(notExpected, actual);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object, string)"/>
        public static void AreNotEqual(this Assert assert, object notExpected, object actual, string message)
        {
            Assert.AreNotEqual(notExpected, actual, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(object, object, string, object[])"/>
        public static void AreNotEqual(this Assert assert, object notExpected, object actual, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(float, float, float)"/>
        public static void AreEqual(this Assert assert, float expected, float actual, float delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(float, float, float, string)"/>
        public static void AreEqual(this Assert assert, float expected, float actual, float delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(float, float, float, string, object[])"/>
        public static void AreEqual(this Assert assert, float expected, float actual, float delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(float, float, float)"/>
        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(float, float, float, string)"/>
        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(float, float, float, string, object[])"/>
        public static void AreNotEqual(this Assert assert, float notExpected, float actual, float delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(decimal, decimal, decimal)"/>
        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(decimal, decimal, decimal, string)"/>
        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(decimal, decimal, decimal, string, object[])"/>
        public static void AreEqual(this Assert assert, decimal expected, decimal actual, decimal delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(decimal, decimal, decimal)"/>
        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(decimal, decimal, decimal, string)"/>
        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(decimal, decimal, decimal, string, object[])"/>
        public static void AreNotEqual(this Assert assert, decimal notExpected, decimal actual, decimal delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(long, long, long)"/>
        public static void AreEqual(this Assert assert, long expected, long actual, long delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(long, long, long, string)"/>
        public static void AreEqual(this Assert assert, long expected, long actual, long delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(long, long, long, string, object[])"/>
        public static void AreEqual(this Assert assert, long expected, long actual, long delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(long, long, long)"/>
        public static void AreNotEqual(this Assert assert, long notExpected, long actual, long delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(long, long, long, string)"/>
        public static void AreNotEqual(this Assert assert, long notExpected, long actual, long delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(long, long, long, string, object[])"/>
        public static void AreNotEqual(this Assert assert, long notExpected, long actual, long delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(double, double, double)"/>
        public static void AreEqual(this Assert assert, double expected, double actual, double delta)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(double, double, double, string)"/>
        public static void AreEqual(this Assert assert, double expected, double actual, double delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(double, double, double, string, object[])"/>
        public static void AreEqual(this Assert assert, double expected, double actual, double delta, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(double, double, double)"/>
        public static void AreNotEqual(this Assert assert, double notExpected, double actual, double delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(double, double, double, string)"/>
        public static void AreNotEqual(this Assert assert, double notExpected, double actual, double delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(double, double, double, string, object[])"/>
        public static void AreNotEqual(this Assert assert, double notExpected, double actual, double delta, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool)"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase)
        {
            Assert.AreEqual(expected, actual, ignoreCase);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool, string)"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase, string message)
        {
            Assert.AreEqual(expected, actual, ignoreCase, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool, string, object[])"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, ignoreCase, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool, CultureInfo)"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase, CultureInfo culture)
        {
            Assert.AreEqual(expected, actual, ignoreCase, culture);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool, CultureInfo, string)"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            Assert.AreEqual(expected, actual, ignoreCase, culture, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreEqual(string, string, bool, CultureInfo, string, object[])"/>
        public static void AreEqual(this Assert assert, string expected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, ignoreCase, culture, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool)"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool, string)"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase, string message)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool, string, object[])"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool, CultureInfo)"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase, CultureInfo culture)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, culture);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool, CultureInfo, string)"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, culture, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.AreNotEqual(string, string, bool, CultureInfo, string, object[])"/>
        public static void AreNotEqual(this Assert assert, string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, culture, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsInstanceOfType(object, Type)"/>
        public static void IsInstanceOfType(this Assert assert, object value, Type expectedType)
        {
            Assert.IsInstanceOfType(value, expectedType);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsInstanceOfType(object, Type, string)"/>
        public static void IsInstanceOfType(this Assert assert, object value, Type expectedType, string message)
        {
            Assert.IsInstanceOfType(value, expectedType, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsInstanceOfType(object, Type, string, object[])"/>
        public static void IsInstanceOfType(this Assert assert, object value, Type expectedType, string message, params object[] parameters)
        {
            Assert.IsInstanceOfType(value, expectedType, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotInstanceOfType(object, Type)"/>
        public static void IsNotInstanceOfType(this Assert assert, object value, Type wrongType)
        {
            Assert.IsNotInstanceOfType(value, wrongType);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotInstanceOfType(object, Type, string)"/>
        public static void IsNotInstanceOfType(this Assert assert, object value, Type wrongType, string message)
        {
            Assert.IsNotInstanceOfType(value, wrongType, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.IsNotInstanceOfType(object, Type, string, object[])"/>
        public static void IsNotInstanceOfType(this Assert assert, object value, Type wrongType, string message, params object[] parameters)
        {
            Assert.IsNotInstanceOfType(value, wrongType, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Fail()"/>
        [DoesNotReturn]
        public static void Fail(this Assert assert)
        {
            Assert.Fail();
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Fail(string)"/>
        [DoesNotReturn]
        public static void Fail(this Assert assert, string message)
        {
            Assert.Fail(message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Fail(string, object[])"/>
        [DoesNotReturn]
        public static void Fail(this Assert assert, string message, params object[] parameters)
        {
            Assert.Fail(message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Inconclusive()"/>
        public static void Inconclusive(this Assert assert) //// TODO .NET doesn't mark this as `doesnotreturn` but i think it should
        {
            Assert.Inconclusive();
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Inconclusive(string)"/>
        public static void Inconclusive(this Assert assert, string message)
        {
            Assert.Inconclusive(message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.Inconclusive(string, object[])"/>
        public static void Inconclusive(this Assert assert, string message, params object[] parameters)
        {
            Assert.Inconclusive(message, parameters);
        }

        //// TODO .NET has the `equals` method to help people not use the wrong overload; that is not applicable for instances of `assert` because that `equals` overload only takes a single parameter and therefore won't be confused with `assert.areequal`; we are skipping it for that reason (and because an extension variant won't ever actually be found by the compiler)

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Action)"/>
        public static T ThrowsException<T>(this Assert assert, Action action) where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Action, string)"/>
        public static T ThrowsException<T>(this Assert assert, Action action, string message) where T : Exception
        {
            return Assert.ThrowsException<T>(action, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Action, string, object[])"/>
        public static T ThrowsException<T>(this Assert assert, Action action, string message, params object[] parameters) where T : Exception
        {
            return Assert.ThrowsException<T>(action, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Func{object})"/>
        public static T ThrowsException<T>(this Assert assert, Func<object> action) where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Func{object}, string)"/>
        public static T ThrowsException<T>(this Assert assert, Func<object> action, string message) where T : Exception
        {
            return Assert.ThrowsException<T>(action, message);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsException{T}(Func{object}, string, object[])"/>
        public static T ThrowsException<T>(this Assert assert, Func<object> action, string message, params object[] parameters) where T : Exception
        {
            return Assert.ThrowsException<T>(action, message, parameters);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsExceptionAsync{T}(Func{Task})"/>
        public static async Task<T> ThrowsExceptionAsync<T>(this Assert assert, Func<Task> action) where T : Exception
        {
            return await Assert.ThrowsExceptionAsync<T>(action);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsExceptionAsync{T}(Func{Task}, string)"/>
        public static async Task<T> ThrowsExceptionAsync<T>(this Assert assert, Func<Task> action, string message) where T : Exception
        {
            return await Assert.ThrowsExceptionAsync<T>(action, message).ConfigureAwait(false);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ThrowsExceptionAsync{T}(Func{Task}, string, object[])"/>
        public static async Task<T> ThrowsExceptionAsync<T>(this Assert assert, Func<Task> action, string message, params object[] parameters) where T : Exception
        {
            return await Assert.ThrowsExceptionAsync<T>(action, message, parameters).ConfigureAwait(false);
        }

        /// <param name="assert">The <see cref="Assert"/> instance that is being extended</param>
        /// <inheritdoc cref="Assert.ReplaceNullChars(string)"/>
        public static string ReplaceNullChars(this Assert assert, string input)
        {
            return Assert.ReplaceNullChars(input);
        }
    }
}
