/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    public static class EitherExtensions
    {
        public static Realizable<IEither<TLeftResult, TRightResult>> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, IContinuable<TLeftResult>> leftMap,
            Func<TRightSource, IContinuable<TRightResult>> rightMap)
        {
            return either.Apply<IEither<TLeftResult, TRightResult>, bool, Realizable<IEither<TLeftResult, TRightResult>>>(
                (TLeftSource left, ref bool context) =>
                    leftMap(left)
                    .ContinueWith(
                        result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                (TRightSource right, ref bool context) =>
                    rightMap(right)
                    .ContinueWith(
                        result => (IEither<TLeftResult, TRightResult>)new Either<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                ref Context);
        }

        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            var future = either.Apply<TResult, bool, TaskWrapper<TResult>>(
                (TLeft left, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(leftMap(left))),
                (TRight right, ref bool context) => new TaskWrapper<TResult>(Task.FromResult(rightMap(right))),
                ref Context);

            if (future.TypeHolder.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.GetAwaiter().GetResult();
            }
        }

        public static Realizable<TResult> Apply<TLeft, TRight, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
        {
            return either.ContinueWith(
                result => result.Apply(leftMap, rightMap),
                exception => throw exception,
                canceled => throw canceled);
        }

        private static bool Context = false;
    }
}
