/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Realizable
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;

    public static class RealizableExtensions
    {
        public static IAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable)
        {
            //// TODO implement a configure await
            if (realizable.TypeHolder.Decompose(out var left, out var right))
            {
                return new TaskWrapper<T>(Task.FromResult(left)).GetAwaiter();
            }
            else
            {
                return right.GetAwaiter();
            }
        }
    }
}
