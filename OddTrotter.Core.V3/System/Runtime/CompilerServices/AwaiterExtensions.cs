namespace Fx
{
    using System.Runtime.CompilerServices;

    public static class AwaiterExtensions
    {
        public static IAwaiter<T> GetAwaiter<T>(this IAwaiter<T> awaiter)
        {
            return awaiter;
        }

        /*public static TAwaiter GetAwaiter<TAwaiter>(this TAwaiter awaiter)
        {
            //// TODO i think this is interesting, but not sure if it's useful; it would let you await anything that is already duck-typed to be an awaiter (while the `iawaiter` variant only allow awaiting things that use my specific interface, which would sometimes require boxing); the "issue" i suppose with this method is that the error message is not as clear; for example, if you await `action<T>`, it doesn't yell at you about the fact that it's not awaitable, it yells at you because you've not implemented a specific awaitable requirement

            return awaiter;
        }*/
    }
}
