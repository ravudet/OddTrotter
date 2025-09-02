/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either2
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Transactions;
    using static Fx.Either.Playground;

    public interface IEither<out TLeft, out TRight>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="leftMap"/> or <paramref name="rightMap"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="LeftMapException">
        /// Thrown if <paramref name="leftMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="leftMap"/> threw.
        /// </exception>
        /// <exception cref="RightMapException">
        /// Thrown if <paramref name="rightMap"/> throws an exception. The <see cref="Exception.InnerException"/> will be set to
        /// whatever exception <paramref name="rightMap"/> threw.
        /// </exception>
        /// <remarks>
        /// This is named `apply`. Other names proposed:
        /// 1. `visit` - This leaks the design detail that the visitor pattern is used to implement the method; it's a fine name,
        /// but if we can do better, we should.
        /// 2. `aggregate`/`fold` - This is just definitely not a `fold`. The return type being a completely new, non-`ieither`
        /// value distracted me when considering this option, but ultimately there is only a single value in the `ieither`
        /// structure, so there is really no traversal happening that is essential to a `fold`, as noted 
        /// [here](https://en.wikipedia.org/wiki/Fold_(higher-order_function):
        /// > functions that analyze a recursive data structure and through use of a given combining operation, recombine the
        /// > results of recursively processing its constituent parts
        /// 3. `fmap` - Haskell has an `fmap` function
        /// ([reference](https://en.wikipedia.org/wiki/Functor#Computer_implementations)) that takes a functor. A functor maps
        /// [morphisms](https://en.wikipedia.org/wiki/Morphism) and morphisms are structure-preserving. In this method,
        /// <paramref name="leftMap"/> and <paramref name="rightMap"/> are the components of the piecewise function and (together
        /// or individually) they do *not* preserve structure (though they may be written in a way which *does* preserve
        /// structure). As a result, that piecewise function is *not* a functor, and therefore this is *not* `fmap`.
        /// 4. `morph` - This was an option because it seemed to be the "verb" form (and therefore more idiomatic to c#) of
        /// "morphism". However, as described above in `fmap`, <paramref name="leftMap"/> and <paramref name="rightMap"/> form a
        /// piecewise function that is *not* structure preserving and therefore is not a morphism.
        /// 5. `switch` - Similar to `visit`, this leaks the design detail that a discriminated union is being used to implement
        /// the method. Although this works well as an analog to the c#
        /// [switch expression](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression),
        /// the name obfuscates the monadic nature of `ieither`.
        /// 
        /// [`apply`](https://en.wikipedia.org/wiki/Apply) was chosen because this method is applying the piecewise map composed
        /// of <paramref name="leftMap"/> and <paramref name="rightMap"/> to that map's `ieither` argument.
        /// 
        /// This method throws <see cref="LeftMapException"/> or <see cref="RightMapException"/>. This is a divergence from how
        /// the LINQ APIs document delegates. With LINQ, the expectation is that the delegates don't throw. However, this is by
        /// convention. If it's documented, it is documented in a general LINQ document rather than on the individual APIs,
        /// making it more difficult to discover. My intention with wrapping the thrown exceptions into two new exception types
        /// is 4-fold:
        /// 1. I treat interfaces as contracts, and as a result, I document *at the interface level* what exceptions can be
        /// thrown. If an exception is not documented, the expectation should be that that exception will not be thrown.
        /// Following this logic, if <see cref="Apply"/> did not document *anything* for the cases where
        /// <paramref name="leftMap"/> or <paramref name="rightMap"/> throw, then callers should expect that no exceptions will
        /// be thrown in those cases.
        /// 2. There are use-cases where it is very useful for <paramref name="leftMap"/> or <paramref name="rightMap"/> to throw
        /// (consider the <see cref="Fx.Either.EitherExtensions.ThrowRight"/> method), so narrowing the scope of this method to
        /// only maps that don't throw does not match the intended purpose.
        /// 3. The caller of <see cref="Apply"/> is not necessarily the author of the functions provided for the maps. As a
        /// result, they will not know which exceptions they need to catch unless they restrict their own callers to only provide
        /// functions that conform to a certain contract. This option was *also* considered for <see cref="Apply"/>, though it
        /// was rejected as well (see next point).
        /// 4. I could introduce a new interface that specifies the allowed exceptions for <paramref name="leftMap"/> and
        /// <paramref name="rightMap"/> and then take instances of those interfaces as parameters of <see cref="Apply"/>.
        /// However, doing this would now require all map authors to essentially wrap their functions in a try catch and adapt
        /// their natural exceptions to conform to the contract. This is effectively what implementers of
        /// <see cref="IEither{TLeft, TRight}"/> will need to do, but having just the implementers of the interface do it, 
        /// instead of every caller, is less error-prone and reduces the barrier to entry.
        /// </remarks>
        TResult Apply<TResult, TContext>(
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;
    }

    public sealed class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private readonly TLeft? left;
        private readonly TRight? right;

        private Either(TLeft left)
        {
            this.left = left;
        }

        private Either(TRight right)
        {
            this.right = right;
        }

        public static Either<TLeft, TRight> Left(TLeft value)
        {
            return new Either<TLeft, TRight>(value);
        }

        public static Either<TLeft, TRight> Right(TRight value)
        {
            return new Either<TLeft, TRight>(value);
        }

        public ITask<TResult> Apply<TResult, TContext>(AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap, AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            if (this.left != null)
            {
                return new CustomTask<TResult>(leftMap(this.left, ref context), true);
            }
            else if (this.right != null)
            {
                return new CustomTask<TResult>(rightMap(this.right, ref context), true);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        private sealed class CustomTask<T> : ITask<T> where T : allows ref struct
        {
            private readonly ITask<T> task;
            private readonly bool isLeft;

            public CustomTask(ITask<T> task, bool isLeft)
            {
                this.task = task;
                this.isLeft = isLeft;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<T> GetAwaiter()
            {
                return new TaskAwaiter(this.task.GetAwaiter(), this.isLeft);
            }

            private sealed class TaskAwaiter : ITaskAwaiter<T>
            {
                private readonly ITaskAwaiter<T> taskAwaiter;
                private readonly bool isLeft;

                public TaskAwaiter(ITaskAwaiter<T> taskAwaiter, bool isLeft)
                {
                    this.taskAwaiter = taskAwaiter;
                    this.isLeft = isLeft;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.taskAwaiter.IsCompleted;
                    }
                }

                public T GetResult()
                {
                    try
                    {
                        return this.taskAwaiter.GetResult();
                    }
                    catch (Exception exception)
                    {
                        if (this.isLeft)
                        {
                            throw new LeftMapException(exception);
                        }
                        else
                        {
                            throw new RightMapException(exception);
                        }
                    }
                }

                public void OnCompleted(Action continuation)
                {
                    this.taskAwaiter.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.taskAwaiter.UnsafeOnCompleted(continuation);
                }
            }
        }
    }

    public sealed class LeftMapException : Exception
    {
        public LeftMapException(Exception exception)
        {
        }
    }

    public sealed class RightMapException : Exception
    {
        public RightMapException(Exception exception)
        {
        }
    }

    public static partial class EitherExtensions
    {
        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            Func<TResult> foo = () => either.Apply(
                Convert(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply2<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert2(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert<TRight, TContext, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                leftMap,
                Convert<TRight, TContext, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                rightMap,
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            //// TODO format this correctly
            return either
                .Apply(
                    Convert(leftMap),
                    Convert(rightMap),
                    ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either
                .Apply(
                    Convert(leftMap),
                    Convert(rightMap),
                    ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert<TRight, TContext, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert<TRight, TContext, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                rightMap,
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(Adapt(rightMap))),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncInContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(rightMap),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            InContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(rightMap),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Convert(Wrap(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(rightMap),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(Convert(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            ContextualizedMap<TLeft, TContext, TResult> leftMap,
            Map<TRight, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert(leftMap)),
                Wrap(Convert<TRight, TContext, TResult>(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                rightMap,
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert<TLeft, TContext, TResult>(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert<TLeft, TContext, TResult>(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var context = true; //// TODO use `nothing` instead
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            Map<TRight, TResult> rightMap)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var context = true;
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context);
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                rightMap,
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert<TLeft, TContext, TResult>(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            InContextualizedMap<TRight, TContext, TResult> rightMap,
            in TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var contextWrapper = new ContextWrapper<TContext>(context);
            return either.Apply(
                Wrap(Convert<TLeft, TContext, TResult>(leftMap)),
                Convert(Wrap(rightMap)),
                ref contextWrapper)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            AsyncContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            ContextualizedMap<TRight, TContext, TResult> rightMap,
            TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert<TLeft, TContext, TResult>(leftMap),
                Convert(rightMap),
                ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var context = true; //// TODO use nothing instead
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult>( //// TODO is this a fold?
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            Map<TRight, TResult> rightMap)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            var context = true;
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }

    //// TODO implement the extensions

    public delegate ITask<TResult> AsyncRefContextualizedMap<in TValue, TContext, out TResult>(TValue value, ref TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult RefContextualizedMap<in TValue, TContext, out TResult>(TValue value, ref TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate ITask<TResult> AsyncInContextualizedMap<in TValue, TContext, out TResult>(TValue value, in TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult InContextualizedMap<in TValue, TContext, out TResult>(TValue value, in TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate ITask<TResult> AsyncContextualizedMap<in TValue, in TContext, out TResult>(TValue value, TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult ContextualizedMap<in TValue, in TContext, out TResult>(TValue value, TContext context)
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate ITask<TResult> AsyncMap<in TValue, out TResult>(TValue value)
        where TResult : allows ref struct;

    public delegate TResult Map<in TValue, out TResult>(TValue value)
        where TResult : allows ref struct;

    public static partial class EitherExtensions
    {
        private static RefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(AsyncRefContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => map(value, ref context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private sealed class FromResult<TResult> : ITask<TResult> where TResult : allows ref struct
        {
            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<TResult> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }
    }


    
    //// TODO overloads that take in itask<ieither>


    //// left vs right
    //// async vs sync
    //// ref context vs in context vs normal context
    ////
    //// leftfuture     leftparam   rightfuture     rightparam
    //// async          ref         async           ref
    //// async          ref         async           in
    //// async          ref         async           none
    //// async          ref         sync            ref
    //// async          ref         sync            in
    //// async          ref         sync            none
    //// async          in          async           ref
    //// async          in          async           in
    //// async          in          async           none
    //// async          in          sync            ref
    //// async          in          sync            in
    //// async          in          sync            none
    //// async          none        async           ref
    //// async          none        async           in
    //// async          none        async           none
    //// async          none        sync            ref
    //// async          none        sync            in
    //// async          none        sync            none
    //// sync           ref         async           ref
    //// sync           ref         async           in
    //// sync           ref         async           none
    //// sync           ref         sync            ref
    //// sync           ref         sync            in
    //// sync           ref         sync            none
    //// sync           in          async           ref
    //// sync           in          async           in
    //// sync           in          async           none
    //// sync           in          sync            ref
    //// sync           in          sync            in
    //// sync           in          sync            none
    //// sync           none        async           ref
    //// sync           none        async           in
    //// sync           none        async           none
    //// sync           none        sync            ref
    //// sync           none        sync            in
    //// sync           none        sync            none
    ////
    //// demonstrate ref struct result
    //// demonstrate ref struct context
    //// demonstrate unsafe


    public static class Playground
    {
        public static void DoWork()
        {
        }

        public struct Context
        {
            public Context(List<string> strings)
            {
                Strings = strings;
            }

            public List<string> Strings { get; } = new List<string>();
        }

        public static ITask<int> DataManipulation(string value, ref Context context)
        {
            //// TODO the issue here is that the context is updated before the caller awaits the future
            context.Strings.Add(value);
            return DataManipulation(value);
        }

        private static async ITask<int> DataManipulation(string value)
        {
            return await Task.FromResult(value.Length).ConfigureAwait(false);
        }

        public static ITask<int> DataManipulation2(string value, ref Context context)
        {
            //// TODO flesh this out
            //// TODO it's actually ok if no one can actually call this overload of `apply` because it's not possible to implement the parameters, so long as it's supported once people can implement the parameters
            return new CustomTask();
        }

        private sealed class CustomTask : ITask<int>
        {
            public IConfiguredAwaitable<int> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<int> GetAwaiter()
            {
                throw new NotImplementedException();
            }
        }
    }



    //// TODO FUTURE there are the other `apply` variants as used by the visitor pattern in the concrete implementation:
    //// async
    //// unsafe
    //// result allows ref struct
    //// context allows ref struct
    //// context by reference
    //// there are likely others
    ////
    //// are these mixins? are they standalone types? what is the best way to handle this? is there a kernel? for example,
    //// most of the others appear that they can be built on top of an async unsafe implementation that allows ref structs
    //// and takes the context by reference; would mixins then let you do everything else?
}
