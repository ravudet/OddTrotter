namespace Fx
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertExtensions
    {
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
                /*var assert = this.assert;
                try
                {
                    var state = await this.awaitable.ConfigureAwait(false);
                    action(state);
                    return assert.ThrowsException<TException>(() => { });
                }
                catch (Exception exception)
                {
                    return assert.ThrowsException<TException>(() => throw exception); //// TODO this loses the call stack
                }*/

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
    }

    public static class SomeExtensions //// TODO separate this into a file
    {
        public static IAwaiter<T> GetAwaiter<T>(this IAwaiter<T> awaiter)
        {
            return awaiter;
        }
    }
}
