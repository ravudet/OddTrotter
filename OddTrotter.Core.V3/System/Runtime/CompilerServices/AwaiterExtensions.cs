namespace Fx
{
    using System.Runtime.CompilerServices;

    public static class AwaiterExtensions
    {
        public static IAwaiter<T> GetAwaiter<T>(this IAwaiter<T> awaiter)
        {
            return awaiter;
        }
    }
}
