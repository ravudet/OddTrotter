/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;

    using static Fx.Either.Playground;
    using static Fx.Either2.Playground;

    public interface IEither<out TLeft, out TRight> //// TODO these generics can allow ref struct without the concrete implementation allowing ref struct or the extension methods allowing ref struct
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
        ITask<TResult> Apply<TResult, TContext>(
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;
    }

    public sealed class Either<TLeft, TRight> : 
        IEither<TLeft, TRight>,
        IApply1<TLeft, TRight>,
        IApply2<TLeft, TRight>
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
                return new CustomTask<TResult>(rightMap(this.right, ref context), false);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        public Realizable<TResult> ApplyImpl1<TResult, TContext>(AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap, RefContextualizedMap<TRight, TContext, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            if (this.left != null)
            {
                return new Realizable<TResult>(
                    leftMap(this.left, ref context)
                    .ContinueWith(task =>
                    {
                        try
                        {
                            return task.GetAwaiter().GetResult();
                        }
                        catch (Exception exception)
                        {
                            throw new LeftMapException(exception);
                        }
                    }));
            }
            else if (this.right != null)
            {
                try
                {
                    return new Realizable<TResult>(rightMap(this.right, ref context));
                }
                catch (Exception exception)
                {
                    throw new RightMapException(exception);
                }

                //// TODO can you compute the result, then use a "memory" and have a task that transforms the memory back into the result?
                //// TODO you can also try casting `TResult` to `IBoxable<TResult>` (something you've done elsewhere, but isn't in this repo yet)

                /*try
                {
                    var result = rightMap(this.right, ref context);
					throw new Exception("TODO");
                    //// TODO here's the thing; this just can't work; you need to remove the `tresult` generic type constraint
                    //// to start with, `tresult` can be a ref struct; but it's returned from the resulting `itask`, which necessarily is boxable (and ref structs aren't); this means that whatever is returned can't actually directly reference the result; that type will have to generate the result when it's asked for, and to do that it will need a delegate that generates the result, or it will need to know the exact memory values of the thing being returned
                        //// if we go the route of a delegate, the delegate necessarily leverages a `tcontext` which is by reference and also possibly a ref struct; because of the use of `context`, we will need a closure somehow, either directly, or by the `itask` implementation; either way, though, because we want `context` by reference, we will need a field in the closure and that field needs to be a pointer; but there's the case where our caller initializes `context` and passes it to us, and then returns the resulting `itask` to their caller; in this situation, the `itask` contains a closure around a pointer to `context`, and when our caller returns, that pointer points to a stack frame that has already been popped
                            //// if we don't use a pointer and instead have the closure create a copy of `context`, we will lose the functional semantics of the `ref`, i.e. any changes made to `context` by `rightMap` will not be reflected in the caller's instance of `context`; the `in` overload would be able to leverage this trick *functionally*, but it would still lose out on the non-functional semantics of `in`, i.e. if `TContext` is very large, our caller will likely have performance degredation
                        //// if we go the route of the memory values, we can actually just compute the result up front, create an array with its memory values, and then have our resulting `itask` have that array as a field; this would prevent any issues of dangling pointers, though it doesn't actually compute anything asynchronously, which is not really possible for ref struct results, but if `tresult` *isn't* a ref struct, we *also* lose the asynchronous processing there; regardless, there is still another issue; if `TContext` has a reference type member and `TResult` that has the same reference type member and `rightMap` creates an instance of `TResult` with said member pointing to the same heap instance that `context` points to, the "result" root object is not being included in the garbage collector computations; this means that, if our caller initializes `context` and calls us, then we compute the result, and then we convert it to bytes and return an `itask` to our caller, and then the caller returns the `itask` to their caller, this stack frame can get the correct `tresult` instace, *but* if garbage collection happens after our caller returns and their callers uses the `tresult` instance, then the heap instance that `context` *was* pointing to will be collected, so when the `tresult` instance is recreated from its byte representation, it will contain a member that points to a location on the heap that no longer contains the data
                    //// it's possible that we avoid all of this by having a "task-like type" that is a ref struct which is a DU of "already completed ref struct result" and "some itask", and have that be the return type of each of our `apply` overloads
                        //// first of all, this *may* result in an inability to have covariance or casting in certain cases, though i haven't investigated that thoroughly
                        //// second of all, being a ref struct means that it cannot be `await`ed; this is because the state machine that the compiler generates for an `await` statement *may* box the "task-like type" in cases where the result hasn't already been computed; now, *we* know that when the result is a fully materialized ref struct, no boxing would actually occur, but the *compiler* doesn't know that, so it would generate the state machine
                            //// *but* the `await` statement is really a "convenience"; it would be *annoying* to have to write code to check which type of future is being returned and get the appropriate value or whatever, but it's "doable"; what is *not* doable is passing this value around; with the compiler's help, we are able to have a future but treat it as the resulting type, so if we have two `task<string>`, we can `await` both and then call `string.equals(first, second)`, despite the fact that calling `string.equals` actually is deferred until the values from `task` have become available; if we only had `future<string>` available and no way to `await`, then we would need a `string.equals(future<string>, future<string>)` overload that itself would return a `future<bool>`; basically, we now have a *usability* issue, because our callers would not be able to leverage any existing frameworks that don't already know about our `future<t>`
                            //// a "functional" approach to this would be to create our own infrastructure that allows composing futures and functions and such; but, this doesn't really work for 2 reasons
                                //// first, because everything now is deferred, //// TODO you are here it's not always deferred
                                //// second, we need to implement an analogous infrastructure method for each c# language construct, for example `if`; but because we are *not* actually functional, this means that we would need to pass context through each infrastructure method //// TODO why is this a deal-breaker? you *currently* can't pass ref structs through an `await`, for example; you'd just need to `if` overloads, one that keeps the context, and one that doesn't (you probably would just always have the one that keeps the context, but whatever)
                }
                catch (Exception exception)
                {
                    throw new RightMapException(exception);
                }*/
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        public TResult ApplyImpl2<TResult>(Map<TLeft, TResult> leftMap, Map<TRight, TResult> rightMap) where TResult : allows ref struct
        {
            if (this.left != null)
            {
                return leftMap(this.left);
            }
            else if (this.right != null)
            {
                return rightMap(this.right);
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
			
			public ITask<TResult> ContinueWith<TResult>(Func<ITask<T>, TResult> continuationFunction)
				where TResult : allows ref struct
			{
				throw new Exception("TODO");
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

    public static class RefFutureExtensions
    {
        public static void Foo()
        {
            var first = new RefFuture<string>();
            var second = new RefFuture<string>();
            var areEqual = first.Compose(second, string.Equals);
            ////if (areEqual)
            {
            }
        }

        public static ComposedRefFuture<TFirst, TSecond, TResult> Compose<TFirstFuture, TFirst, TSecondFuture, TSecond, TResult>(
            TFirstFuture first, TSecondFuture second, Func<TFirst, TSecond, TResult> func)
            where TFirstFuture : IRefFuture<TFirst>, allows ref struct
            where TFirst : allows ref struct
            where TSecondFuture : IRefFuture<TSecond>, allows ref struct
            where TSecond : allows ref struct
            where TResult : allows ref struct
        {
			throw new Exception("TODO");
        }
    }

    public readonly ref struct ComposedRefFuture<TFirst, TSecond, TResult>
        where TFirst: allows ref struct
        where TSecond : allows ref struct
        where TResult : allows ref struct
    {
        private readonly TFirst firstValue;
        private readonly ITask<TSecond>? secondTask;

        private readonly TSecond secondValue;
        private readonly ITask<TFirst>? firstTask;

        public ComposedRefFuture(TFirst firstValue, TSecond secondValue)
        {
            this.firstValue = firstValue;
            this.secondValue = secondValue;
        }

        public ComposedRefFuture(TFirst firstValue, ITask<TSecond> secondTask)
        {
            this.firstValue = firstValue;
            this.secondTask = secondTask;

            this.secondValue = default!;
        }

        public ComposedRefFuture(TSecond secondValue, ITask<TFirst> firstTask)
        {
            this.secondValue = secondValue;
            this.firstTask = firstTask;

            this.firstValue = default!;
        }

        public ComposedRefFuture(ITask<TFirst> firstTask, ITask<TSecond> secondTask)
        {
            this.firstTask = firstTask;
            this.secondTask = secondTask;

            this.firstValue = default!;
            this.secondValue = default!;
        }

        public bool Try([MaybeNullWhen(false)] out TResult value, [MaybeNullWhen(true)][NotNullWhen(false)] out ITaskAwaiter<TResult> taskAwaiter)
        {
            /*if (this.task == null)
            {
                value = this.value;
                taskAwaiter = null;
                return true;
            }
            else
            {
                value = default;
                taskAwaiter = this.task.GetAwaiter();
                return false;
            }*/
			throw new Exception("TODO");
        }
    }

    public interface IRefFuture<T> where T : allows ref struct
    {
        bool Try([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)][NotNullWhen(false)] out ITaskAwaiter<T> taskAwaiter);
    }

    public readonly ref struct RefFuture<T> where T : allows ref struct
    {
        //// TODO use correct nullables for all of these
        private readonly ITask<T>? task;
        private readonly T value;

        public RefFuture(ITask<T> task)
        {
            this.task = task;
            this.value = default!;
        }

        public RefFuture(T value)
        {
            this.value = value;
        }

        public bool Try([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] [NotNullWhen(false)] out ITask<T> task)
        {
            if (this.task == null)
            {
                value = this.value;
                task = null;
                return true;
            }
            else
            {
                value = default;
                task = this.task;
                return false;
            }
        }

        public RefFuture<TResult> Compose<TSecond, TResult>(RefFuture<TSecond> future, Func<T, TSecond, TResult> func)
            where TSecond : allows ref struct
            where TResult : allows ref struct
        {
            if (this.task == null && future.task == null)
            {
                return new RefFuture<TResult>(func(this.value, future.value));
            }
            else if (this.task != null && future.task == null)
            {
				throw new Exception("TODO");
            }
            else if (this.task == null && future.task != null)
            {
				throw new Exception("TODO");
            }
            else if (this.task != null && future.task != null)
            {
                return new RefFuture<TResult>(new Nested<T, TSecond, TResult>(this.task, future.task, func));
            }
            else
            {
                throw new Exception("TODO");
            }
        }

        private sealed class Nested<TFirst, TSecond, TResult> : ITask<TResult>
            where TFirst : allows ref struct
            where TSecond : allows ref struct
            where TResult : allows ref struct
        {
            private readonly ITask<TFirst> first;
            private readonly ITask<TSecond> second;
            private readonly Func<TFirst, TSecond, TResult> func;

            public Nested(ITask<TFirst> first, ITask<TSecond> second, Func<TFirst, TSecond, TResult> func)
            {
                this.first = first;
                this.second = second;
                this.func = func;
            }

            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<TResult> GetAwaiter()
            {
                return new TaskAwaiter(this.first.GetAwaiter(), this.second.GetAwaiter(), this.func);
            }
			
			public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<TResult>, TResult2> continuationFunction)
				where TResult2 : allows ref struct
			{
				throw new Exception("TODO");
			}


            private sealed class TaskAwaiter : ITaskAwaiter<TResult>
            {
                private readonly ITaskAwaiter<TFirst> first;
                private readonly ITaskAwaiter<TSecond> second;
                private readonly Func<TFirst, TSecond, TResult> func;

                public TaskAwaiter(ITaskAwaiter<TFirst> first, ITaskAwaiter<TSecond> second, Func<TFirst, TSecond, TResult> func)
                {
                    this.first = first;
                    this.second = second;
                    this.func = func;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.first.IsCompleted && this.second.IsCompleted;
                    }
                }

                public TResult GetResult()
                {
                    return this.func(this.first.GetResult(), this.second.GetResult());
                }

                public void OnCompleted(Action continuation)
                {
                    //// TODO is this the best way to delegate this call?
                    this.first.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    //// TODO is this the best way to delegate this call?
                    this.first.UnsafeOnCompleted(continuation);
                }
            }
        }

        public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public readonly ref struct ConfiguredAwaitable
        {
            public TaskAwaiter GetAwaiter()
            {
                throw new NotImplementedException();
            }

            public readonly ref struct TaskAwaiter : ITaskAwaiter<T>
            {
                public bool IsCompleted => throw new NotImplementedException();

                public T GetResult()
                {
                    throw new NotImplementedException();
                }

                public void OnCompleted(Action continuation)
                {
                    throw new NotImplementedException();
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public TaskAwaiter GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public readonly ref struct TaskAwaiter : ITaskAwaiter<T>
        {
            public bool IsCompleted => throw new NotImplementedException();

            public T GetResult()
            {
                throw new NotImplementedException();
            }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                throw new NotImplementedException();
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

    public interface IApply1<out TLeft, out TRight>
    {
        Realizable<TResult> ApplyImpl1<TResult, TContext>(
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
            where TResult : allows ref struct;
    }

    public interface IApply2<out TLeft, out TRight>
    {
        TResult ApplyImpl2<TResult>(
            Map<TLeft, TResult> leftMap,
            Map<TRight, TResult> rightMap)
            where TResult : allows ref struct;
    }
	
	public readonly ref struct Foo
	{
	}

	public static class NewThing
	{
		public static async Task<string> Attempt()
		{
			var foo = new Foo();
			var value = await foo;
			return value.ToString();
		}
		
		public static ITaskAwaiter<int> GetAwaiter(this Foo foo)
		{
			throw new Exception("TODO");
		}
		
		public static Realizable<T> Attempt2<T>()
			where T : allows ref struct
		{
			return new Realizable<T>(default(T)!);
		}

        public ref struct ForAttempt4
        {
            public ForAttempt4(int value)
            {
                this.Value = value;
            }

            public int Value { get; }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Realizable<ForAttempt4> Attempt4()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new ForAttempt4(31);
        }

        public static Realizable<ForAttempt4> Attempt100()
        {
            return new Realizable<ForAttempt4>(new ForAttempt4(67));
        }

        public static Realizable<ForAttempt4> Attempt101()
        {
            return new Realizable<ForAttempt4>(new Attempt101Task(71));
        }

        private sealed class Attempt101Task : ITask<ForAttempt4>
        {
            private readonly int value;

            private readonly Task delay;

            public Attempt101Task(int value)
            {
                this.value = value;

                this.delay = Task.Delay(1000).ContinueWith(task =>
                {
                    Console.WriteLine("got result");
                });
            }

            public IConfiguredAwaitable<ForAttempt4> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITask<TResult> ContinueWith<TResult>(Func<ITask<ForAttempt4>, TResult> continuationFunction) where TResult : allows ref struct
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<ForAttempt4> GetAwaiter()
            {
                return new Awaiter(this.value, this.delay.GetAwaiter());
            }

            private sealed class Awaiter : ITaskAwaiter<ForAttempt4>
            {
                private readonly int value;
                private readonly TaskAwaiter delay;

                public Awaiter(int value, TaskAwaiter delay)
                {
                    this.value = value;
                    this.delay = delay;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.delay.IsCompleted;
                    }
                }

                public ForAttempt4 GetResult()
                {
                    return new ForAttempt4(this.value);
                }

                public void OnCompleted(Action continuation)
                {
                    this.delay.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.delay.UnsafeOnCompleted(continuation);
                }
            }
        }

        public static async Realizable<int> Attempt5()
        {
            return await Task.FromResult(5);
        }

        public static async Task Attempt3()
		{
			await Attempt2<int>();
		}
		
		public static async Task<IEither<int, string>> Attempt4(IEither<string, Exception> either)
		{
			return await either
				.Select2(
					left => new TaskWrapper<int>(Task.FromResult(left.Length)),
					right => new TaskWrapper<Exception>(Task.FromResult(right)))
				.Select3(
					left => new TaskWrapper<int>(Task.FromResult(left * 2)),
					right => new TaskWrapper<string>(Task.FromResult(right.ToString())))
				.Select3(
					left => new TaskWrapper<int>(Task.FromResult(left % 3)),
					right => new TaskWrapper<string>(Task.FromResult(right.Substring(0, 5))));
		}
	}
	
	public readonly ref struct NullableRef<T> where T : allows ref struct
	{
		private readonly T value;
		
		private readonly bool hasValue;
		
		public NullableRef()
		{
			this.value = default!;
			this.hasValue = false;
		}
		
		public NullableRef(T value)
		{
			this.value = value;
			
			this.hasValue = true;
		}
		
		public bool TryGetValue([MaybeNullWhen(false)] out T value)
		{
			value = this.value;
			return this.hasValue;
		}
	}


    public interface IContinuable<T>
        where T : allows ref struct
    {
        public readonly ref struct Result
        {
            public Result(T value, Exception exception)
            {
                this.Value = value;
                this.Exception = exception;
            }

            public T Value { get; }

            public Exception Exception { get; }
        }

        public readonly ref struct Result2 : IEither2<T, Exception>
        {
            private readonly NullableRef<T> value;

            private readonly Exception? exception;

            private readonly Func<T>? func;

            private readonly ITask<T>? future;

            private readonly IContinuable<T>? continuable;

            public Result2(T value)
            {
                this.value = new NullableRef<T>(value);
            }

            public Result2(Exception exception)
            {
                this.exception = exception;
            }

            public Result2(Func<T> func)
            {
                this.func = func;
            }

            public Result2(ITask<T> future)
            {
                this.future = future;
            }

            public Result2(IContinuable<T> continuable)
            {
                this.continuable = continuable;
            }

            public Realizable<TResult> Apply<TResult, TContext>(AsyncRefContextualizedMap2<T, TContext, TResult> leftMap, AsyncRefContextualizedMap2<Exception, TContext, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
            {
                throw new NotImplementedException();
            }
        }

        Continuable<TResult> ContinueWith<TResult>(Func<Result, TResult> continuationFunction)
            where TResult : allows ref struct;

        Result2 Realize();
    }

    public readonly ref struct Continuable<T> : IContinuable<T>
        where T : allows ref struct
    {
        private readonly NullableRef<T> value;

        private readonly Func<T>? func;

        private readonly ITask<T>? future;

        private readonly IContinuable<T>? continuable;

        public Continuable(T value)
        {
            this.value = new NullableRef<T>(value);
        }

        public Continuable(Func<T> func)
        {
            this.func = func;
        }

        public Continuable(ITask<T> future)
        {
            this.future = future;
        }

        public Continuable(IContinuable<T> continuable)
        {
            this.continuable = continuable;
        }

        public Continuable<TResult> ContinueWith<TResult>(Func<IContinuable<T>.Result, TResult> continuationFunction)
            where TResult : allows ref struct
        {
            if (this.value.TryGetValue(out var realized))
            {
                return new Continuable<TResult>(continuationFunction(new IContinuable<T>.Result(realized, null!)));
            }
            else if (this.func != null)
            {
                //// TODO when `T` is *not* a ref struct, you don't have to realize the `func`s
                var result = continuationFunction(new IContinuable<T>.Result(this.func(), null!));
                return new Continuable<TResult>(result);
            }
            else if (this.future != null)
            {
                return new Continuable<TResult>(this.future.ContinueWith(_ => continuationFunction(new IContinuable<T>.Result(_.ConfigureAwait(false).GetAwaiter().GetResult(), null!))));
            }
            else if (this.continuable != null)
            {
                return this.continuable.ContinueWith(continuationFunction);
            }
            else
            {
                throw new Exception("TODO can't reach");
            }
        }

        public IContinuable<T>.Result2 Realize()
        {
            if (this.value.TryGetValue(out var realized))
            {
                return new IContinuable<T>.Result2(realized);
            }
            else if (this.func != null)
            {
                return new IContinuable<T>.Result2(this.func);
            }
            else if (this.future != null)
            {
                return new IContinuable<T>.Result2(this.future);
            }
            else if (this.continuable != null)
            {
                return new IContinuable<T>.Result2(this.continuable);
            }
            else
            {
                throw new Exception("TODO can't reach");
            }
        }
    }

    public static class ContinuableExtensions
    {
        /*public static ITaskAwaiter<T> GetAwaiter<T>(this Continuable<T> continuable)
        {
            //// TODO i don't see how to do this without some `internal` stuff and breaking separation of concerns
            //// TODO i think you need to do a minimal fleshing out of this `realizable`, `continuable`, `realized`, etc. stuff and implement `queryresult` on top to make sure it all still works

            if (continuable.TryRealize(out var realized, out var future))
            {
                future = new TaskWrapper<T>(Task.FromResult(realized));
            }

            return future.GetAwaiter();
        }*/
    }




    public struct RealizableMethodBuilder<T>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <remarks>
        /// Must not be `readonly` because the underlying type mutates its state. I can't find any reference material that
        /// explains this, but the behavior can be reproduced using the following code:
        /// ```
        /// [TestMethod]
        /// public void SetVal()
        /// {
        ///     var customBuilder = new CustomBuilder();
        ///     customBuilder.AwaitOnCompleted();
        /// 
        ///     Assert.AreEqual(42, customBuilder.Task);
        /// }
        /// 
        /// public struct CustomBuilder
        /// {
        ///     private readonly BuildInBuilder builtInBuilder;
        /// 
        ///     public void AwaitOnCompleted()
        ///     {
        ///         this.builtInBuilder.AwaitOnCompleted();
        ///     }
        /// 
        ///     public int Task
        ///     {
        ///         get
        ///         {
        ///             return this.builtInBuilder.Task;
        ///         }
        ///     }
        /// }
        /// 
        /// public struct BuildInBuilder
        /// {
        ///     private int task;
        /// 
        ///     public void AwaitOnCompleted()
        ///     {
        ///         this.task = 42;
        ///     }
        /// 
        ///     public int Task
        ///     {
        ///         get
        ///         {
        ///             return this.task;
        ///         }
        ///     }
        /// }
        /// ```
        /// </remarks>
        private AsyncTaskMethodBuilder<T> builder;

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Create"/>
        public static RealizableMethodBuilder<T> Create()
            => new RealizableMethodBuilder<T>();

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Start{TStateMachine}(ref TStateMachine)"/>
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            ArgumentNullException.ThrowIfNull(stateMachine);

            this.builder.Start(ref stateMachine);
        }

        private const string setStateMachineMessage =
$$"""
'{nameof(SetStateMachine)}' is not supported. '{nameof(TaskMethodBuilder<T>)}' is only intended to target the .NET runtime, which doesn't make use of the '{nameof(SetStateMachine)}' call; only the .NET framework runtime will make use of the '{nameof(SetStateMachine)}' call. You can find more details [here](https://devblogs.microsoft.com/dotnet/how-async-await-really-works/):

> Note that line which the source comments as "important". This takes the place of that complicated SetStateMachine dance in .NET Framework, **such that `SetStateMachine` isn't actually used at all in .NET Core.**

The **intended** implementation of this method for .NET framework would be:

```
builder.SetStateMachine(stateMachine);
```

For this method to be invoked, there are 3 requirements:
1. the binary needs to be compiled with the `Release` configuration
2. the binary needs to target .NET framework (.NET framework 4.8.1 was used to reproduce these steps)
3. the method leveraging the '{nameof(TaskMethodBuilder<T>)}' must actually await (i.e. it needs to not just return a completed task)

The third requirement can be met using the following code:

```
[TestClass]
public sealed class Test
{
    private sealed class AwaiterType<T>
    {
        private readonly T value;

        public AwaiterType(T value)
        {
            this.value = value;
        }

        public async ITask<T> GetValueWithDelay()
        {
            await Task.Delay(100).ConfigureAwait(false);
            return this.value;
        }
    }

    [TestMethod]
    public async Task Await()
    {
        var value = "asdf";
        var result = await new AwaiterType<string>(value).GetValueWithDelay().ConfigureAwait(false);

        Assert.AreEqual(value, result);
    }
}
```
""";

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <exception cref="NotSupportedException">
        /// Always thrown because this method is not supported by the currently supported .NET runtimes
        /// </exception>
        /// <remarks>
        /// This method always throws <see cref="NotSupportedException"/>
        /// </remarks>
        [ExcludeFromCodeCoverage(Justification = setStateMachineMessage)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException(setStateMachineMessage);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.SetException(Exception)"/>
        public void SetException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            this.builder.SetException(exception);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.SetResult(TResult)"/>
        public void SetResult(T result)
        {
            this.builder.SetResult(result);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.AwaitOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.AwaitUnsafeOnCompleted{TAwaiter, TStateMachine}(ref TAwaiter, ref TStateMachine)"/>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <inheritdoc cref="AsyncTaskMethodBuilder{TResult}.Task"/>
        public Realizable<T> Task
        {
            get
            {
                return new Realizable<T>(new TaskWrapper<T>(this.builder.Task));
            }
        }
    }


    [AsyncMethodBuilder(typeof(Realizable.AsyncTaskMethodBuilder))]
    public readonly struct Realizable
    {
        public IConfiguredAwaitable<int> ConfigureAwait(bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public ITask<TResult> ContinueWith<TResult>(Func<ITask<int>, TResult> continuationFunction) where TResult : allows ref struct
        {
            throw new NotImplementedException();
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }

            public int GetResult()
            {
                return 17;
            }

            public void OnCompleted(Action continuation)
            {
            }

            public void UnsafeOnCompleted(Action continuation)
            {
            }
        }

        public struct AsyncTaskMethodBuilder
        {
            private System.Runtime.CompilerServices.AsyncTaskMethodBuilder asyncTaskMethodBuilder;

            private Realizable? m_task;

            public static AsyncTaskMethodBuilder Create() => default;

            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
                asyncTaskMethodBuilder.Start(ref stateMachine);

            public void SetStateMachine(IAsyncStateMachine stateMachine) =>
                asyncTaskMethodBuilder.SetStateMachine(stateMachine);

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(
                ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine =>
                asyncTaskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
                ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine =>
                asyncTaskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

            public Realizable Task
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_task.HasValue ? m_task.Value : (m_task = new Realizable()).Value;
            }

            public void SetResult()
            {
                m_task = new Realizable();
            }

            public void SetException(Exception exception) =>
                asyncTaskMethodBuilder.SetException(exception);

        }
    }



    [AsyncMethodBuilder(typeof(RealizableMethodBuilder<>))]
    public readonly ref struct Realizable<T> : IEither2<T, ITask<T>>
        where T : allows ref struct
	{
		private readonly NullableRef<T> value;
		
		private readonly ITask<T>? future;

        private readonly Task tracker;

        public Realizable(T value)
        {
            this.value = new NullableRef<T>(value);

            this.future = null;
            this.tracker = Task.CompletedTask;
        }

        public Realizable(ITask<T> future)
        {
            this.future = future;

            this.value = default!;
            this.tracker = Task.Factory.StartNew(state =>
            {
                if (!(state is ITask<T> future))
                {
                    throw new Exception("tODO will this actaully get exposed?");
                }

                var awaiter = future/*.ConfigureAwait(false) TODO add this back*/.GetAwaiter();
                while (!awaiter.IsCompleted)
                {
                    //// TODO can you use `task.delay` instead?
                    Thread.Sleep(100);
                }
            },
            this.future);
        }

        public Task Tracker
        {
            get
            {
                return this.tracker;
            }
        }

        public readonly ref struct Result
        {
            public Result(T value, Exception exception)
            {
                this.Value = value;
                this.Exception = exception;
            }

            public T Value { get; }

            public Exception Exception { get; }
        }

        public Realizable<TResult> ContinueWith<TResult>(Func<Result, TResult> continuationFunction)
            where TResult : allows ref struct
        {
            /*if (this.TryRealize(out var realized, out var future))
            {
                return new Realizable<TResult>(continuationFunction(new Result(realized, null!)));
            }
            else
            {
                return new Realizable<TResult>(
                    future
                        .ContinueWith(result => 
                            continuationFunction(
                                new Result(
                                    result.ConfigureAwait(false).GetAwaiter().GetResult(), null!))));
            }*/


            //// TODO you are trying to see if `apply` is the kernel
            return this.Extensions.ApplyRef2(
                realized => new Realizable<TResult>(continuationFunction(new Result(realized, null!))),
                future =>
                    new Realizable<TResult>(
                    future
                        .ContinueWith(result =>
                            continuationFunction(
                                new Result(
                                    result.ConfigureAwait(false).GetAwaiter().GetResult(), null!)))));


            /*if (this.value.TryGetValue(out var realized))
            {
                return new Realizable<TResult>(continuationFunction(new Result(realized, null!)));
            }
            else if (this.future != null)
            {
                return new Realizable<TResult>(
                    future
                        .ContinueWith(result =>
                            continuationFunction(
                                new Result(
                                    result.ConfigureAwait(false).GetAwaiter().GetResult(), null!))));
            }
            else
            {
                throw new Exception("TODO");
            }*/
        }

        public Realizable<TResult> Apply<TResult, TContext>(AsyncRefContextualizedMap2<T, TContext, TResult> leftMap, AsyncRefContextualizedMap2<ITask<T>, TContext, TResult> rightMap, ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            if (this.value.TryGetValue(out var realized))
            {
                return 
                    leftMap(realized, ref context) //// TODO you also need to wrap the call to `leftmap` and `rightmap`
                    .ContinueWith(result =>
                    {
                        if (result.Exception == null)
                        {
                            return result.Value;
                        }
                        else
                        { 
                            throw new LeftMapException(result.Exception);
                        }
                    });
            }
            else if (this.future != null)
            {
                return 
                    rightMap(this.future, ref context)
                    .ContinueWith(result =>
                    {
                        if (result.Exception == null)
                        {
                            return result.Value;
                        }
                        else
                        {
                            throw new RightMapException(result.Exception);
                        }
                    });
            }
            else
            {
                throw new Exception("TODO");
            }
        }

        public bool TryRealize([MaybeNullWhen(false)] out T realized, [NotNullWhen(false)] [MaybeNullWhen(true)] out ITask<T> future)
		{
            var context = new Context<bool, T, ITask<T>>();
            var result = this.Apply(
                (T left, ref Context<bool, T, ITask<T>> context) =>
                {
                    context.Item1 = true;
                    context.Item2 = left;
                    return new Realizable<bool>(true);
                },
                (ITask<T> right, ref Context<bool, T, ITask<T>> context) =>
                {
                    context.Item1 = false;
                    context.Item3 = right;
                    return new Realizable<bool>(true);
                },
                ref context);

            realized = context.Item2;
            future = context.Item3;
            return context.Item1;
		}

        private ref struct Context<T1, T2, T3>
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }
        }

        public Extensions<Realizable<T>, T, ITask<T>> Extensions
        {
            get
            {
                return new Extensions<Realizable<T>, T, ITask<T>>(this);
            }
        }
	}

    public interface IEither2<out TLeft, out TRight> 
        where TLeft : allows ref struct 
        where TRight : allows ref struct
    {
        Realizable<TResult> Apply<TResult, TContext>(
            AsyncRefContextualizedMap2<TLeft, TContext, TResult> leftMap,
            AsyncRefContextualizedMap2<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct;
    }

    public sealed class Either2<TLeft, TRight> : IEither2<TLeft, TRight>
    {
        private readonly TLeft? left;
        private readonly TRight? right;

        public Either2(TLeft left)
        {
            this.left = left;
        }

        public Either2(TRight right)
        {
            this.right = right;
        }

        public Realizable<TResult> Apply<TResult, TContext>(
            AsyncRefContextualizedMap2<TLeft, TContext, TResult> leftMap, 
            AsyncRefContextualizedMap2<TRight, TContext, TResult> rightMap, 
            ref TContext context)
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            if (this.left != null)
            {
                return
                    leftMap(this.left, ref context)
                    .ContinueWith(result =>
                    {
                        if (result.Exception == null)
                        {
                            return result.Value;
                        }
                        else
                        {
                            throw new LeftMapException(result.Exception);
                        }
                    });
            }
            else if (this.right != null)
            {
                return
                    rightMap(this.right, ref context)
                    .ContinueWith(result =>
                    {
                        if (result.Exception == null)
                        {
                            return result.Value;
                        }
                        else
                        {
                            throw new RightMapException(result.Exception);
                        }
                    });
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }
    }



    public delegate TResult Map2<in TValue, out TResult>(TValue value)
        where TValue : allows ref struct
        where TResult : allows ref struct;

    public delegate Realizable<TResult> AsyncRefContextualizedMap2<in TValue, TContext, TResult>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult RefContextualizedMap2<in TValue, TContext, out TResult>(TValue value, ref TContext context)
        where TValue : allows ref struct
        where TContext : allows ref struct
        where TResult : allows ref struct;

    public interface IExtensible<TSelf, T1, T2>
        where TSelf : IExtensible<TSelf, T1, T2>, allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        TSelf Extensions { get; }
    }

    public readonly ref struct Extensions<TSelf, T1, T2>
        where TSelf : /*IExtensible<TSelf, T1, T2>, */allows ref struct
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        public Extensions(TSelf self)
        {
            Self = self;
        }

        public TSelf Self { get; }
    }

    public static class RealizableExtensions
	{
		public static ITaskAwaiter<T> GetAwaiter<T>(this Realizable<T> realizable)
		{
            //// TODO realizable needs a `configureawait`
            
			if (realizable.TryRealize(out var realized, out var future))
			{
				future = new TaskWrapper<T>(Task.FromResult(realized));
			}
			
			return future.GetAwaiter();
		}

        public static TaskAwaiter GetAwaiter2<T>(this Realizable<T> realizable)
            where T : allows ref struct
        {
            //// TODO realizable needs a `configureawait`


            //// this will let you await any realizable, so long as you don't need the result (e.g. realizable<nothing>)

            return realizable.Tracker.GetAwaiter();
        }

        public static void TestSelect<T>(Realizable<T> realizable)
        {
            var result = realizable.SelectRef<Realizable<T>, T, ITask<T>, string, int>(left => new TaskWrapper<string>(Task.FromResult("asdf")), right => new TaskWrapper<int>(Task.FromResult(1)));


            var result2 = realizable.Extensions.SelectRef2(
                left => new TaskWrapper<string>(Task.FromResult("asdf")), 
                right => new TaskWrapper<int>(Task.FromResult(1)));
        }
	}

    public static partial class EitherExtensions
    {
        public static Realizable<IEither2<TLeftResult, TRightResult>> SelectRef2<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this Extensions<TEither, TLeftSource, TRightSource> extensions,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
            where TEither : IEither2<TLeftSource, TRightSource>, allows ref struct
        {
            return
                extensions.Self.ApplyRef<TEither, TLeftSource, TRightSource, IEither2<TLeftResult, TRightResult>>(
                    async left => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await leftSelector(left).ConfigureAwait(false)),
                    async right => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await rightSelector(right).ConfigureAwait(false)));
        }

        public static Realizable<IEither2<TLeftResult, TRightResult>> SelectRef<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this TEither either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
            where TEither : IEither2<TLeftSource, TRightSource>, allows ref struct
        {
            return 
                either.ApplyRef<TEither, TLeftSource, TRightSource, IEither2<TLeftResult, TRightResult>>(
                    async left => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await leftSelector(left).ConfigureAwait(false)),
                    async right => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await rightSelector(right).ConfigureAwait(false)));
        }

        public static Realizable<TResult> ApplyRef<TEither, TLeft, TRight, TResult>(
            this TEither either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap)
            where TEither : IEither2<TLeft, TRight>, allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert16<TLeft, bool, TResult>(leftMap),
                Convert16<TRight, bool, TResult>(rightMap),
                ref EitherExtensions.Context);
        }

        public static TResult ApplyRef3<TEither, TLeft, TRight, TResult, TContext>(
            this TEither either,
            RefContextualizedMap2<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap2<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TEither : IEither2<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
            where TContext : allows ref struct
        {

            //// TODO if you look at `realizable` not as implementing `itask` but instead implementing `ieither2` (so, it's `refeither` or something instead of `realizable`, then the return type of `apply` needs to be isomorphic with `ieither`; and the return type on `ieither` is `itask`, so for `ieither2`, the return type needs to 1. be continuable and 2. be realizable; so `realizable` has these two requirements; now we are recursive if we treat `realizable` as `ieither2`, so we cannot implement `apply` on `realizable` unless we have `realizable` expose something to continue and something to realize, otherwise we end up recursive
            //// TODO expose `decompose` as a mixin and then have `realizable` implement that mixin instead of having `tryrealize`




            if (either.Decompose(out var left, out var right))
            {
                try
                {
                    return leftMap(left, ref context);
                }
                catch (Exception exception)
                {
                    throw new LeftMapException(exception);
                }
            }
            else
            {
                try
                {
                    return rightMap(right, ref context);
                }
                catch (Exception exception)
                {
                    throw new RightMapException(exception);
                }
            }
        }

        private static bool Decompose2<TEither, TLeft, TRight>(this TEither either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
            where TEither : IEither2<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            var context = true;
            var tempLeft = default(TLeft);
            var tempRight = default(TRight);
            var result = either
                .Apply(
                    (TLeft value, ref bool nothing) =>
                    {
                        tempLeft = value;
                        return new TaskWrapper<bool>(Task.FromResult(true));
                    },
                    (TRight value, ref bool nothing) =>
                    {
                        tempRight = value;
                        return new TaskWrapper<bool>(Task.FromResult(false));
                    },
                    ref context);

            left = tempLeft;
            right = tempRight;
            return result.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static TResult ApplyRef2<TEither, TLeft, TRight, TResult>(
            this TEither either,
            Map2<TLeft, TResult> leftMap,
            Map2<TRight, TResult> rightMap)
            where TEither : IEither2<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            TResult context = default!;
            var result = either
                .ApplyRef3(
                    (TLeft left, ref TResult context) =>
                    {
                        context = leftMap(left);
                        return true;
                    },
                    (TRight right, ref TResult context) =>
                    {
                        context = rightMap(right);
                        return true;
                    },
                    ref context);

            return context;
        }

        public static TResult ApplyRef2<TEither, TLeft, TRight, TResult>(
            this Extensions<TEither, TLeft, TRight> extensions,
            Map2<TLeft, TResult> leftMap,
            Map2<TRight, TResult> rightMap)
            where TEither : IEither2<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return ApplyRef2(extensions.Self, leftMap, rightMap);
        }
    }

    public static partial class EitherExtensions
    {
        public static Realizable<IEither2<TLeftResult, TRightResult>> Select3<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this Realizable<IEither2<TLeftSource, TRightSource>> either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
        {
            if (either.TryRealize(out var realized, out var future))
            {
                return realized.Select2(leftSelector, rightSelector);
            }
            else
            {
                return new Realizable<IEither2<TLeftResult, TRightResult>>(
                    future.ContinueWith(task => task.GetAwaiter().GetResult().Select2(leftSelector, rightSelector).GetAwaiter().GetResult()));
            }
        }

        public static Realizable<IEither2<TLeftResult, TRightResult>> Select2<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither2<TLeftSource, TRightSource> either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
        {
            return
                either.Apply(
                    async left => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await leftSelector(left).ConfigureAwait(false)),
                    async right => (IEither2<TLeftResult, TRightResult>)new Either2<TLeftResult, TRightResult>(await rightSelector(right).ConfigureAwait(false)));
        }

        public static Realizable<TResult> Apply<TLeft, TRight, TResult>(
            this IEither2<TLeft, TRight> either,
            AsyncMap<TLeft, TResult> leftMap,
            AsyncMap<TRight, TResult> rightMap)
            where TResult : allows ref struct
        {
            return either.Apply(
                Convert16<TLeft, bool, TResult>(leftMap),
                Convert16<TRight, bool, TResult>(rightMap),
                ref EitherExtensions.Context);
        }

        public static bool Context = true;

        private static AsyncRefContextualizedMap2<TValue, TContext, TResult> Convert16<TValue, TContext, TResult>(AsyncMap<TValue, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => new Realizable<TResult>(map(value));
        }

        private static AsyncRefContextualizedMap2<TValue, TContext, TResult> Convert17<TValue, TContext, TResult>(Map2<TValue, TResult> map)
            where TValue : allows ref struct
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => new Realizable<TResult>(map(value));
        }







        public static Realizable<IEither<TLeftResult, TRightResult>> Select3<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this Realizable<IEither<TLeftSource, TRightSource>> either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
        {
			if (either.TryRealize(out var realized, out var future))
			{
				return realized.Select2(leftSelector, rightSelector);
			}
			else
			{
				return new Realizable<IEither<TLeftResult, TRightResult>>(
					future.ContinueWith(task => task.GetAwaiter().GetResult().Select2(leftSelector, rightSelector).GetAwaiter().GetResult()));
			}
        }
		
		public static Realizable<IEither<TLeftResult, TRightResult>> Select2<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
        {
            return new Realizable<IEither<TLeftResult, TRightResult>>(
				either.Apply(
					async left => Either<TLeftResult, TRightResult>.Left(await leftSelector(left).ConfigureAwait(false)),
					async right => Either<TLeftResult, TRightResult>.Right(await rightSelector(right).ConfigureAwait(false))));
        }
		
        public static IEither<TLeftResult, TRightResult> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftSelector,
            Func<TRightSource, TRightResult> rightSelector)
        {
            var context = new SelectContext<TLeftSource, TRightSource, TLeftResult, TRightResult>(
                leftSelector,
                rightSelector);
            return either.Apply(
                (TLeftSource left, in SelectContext<TLeftSource, TRightSource, TLeftResult, TRightResult> context) => Either<TLeftResult, TRightResult>.Left(context.LeftSelector(left)),
                (TRightSource right, in SelectContext<TLeftSource, TRightSource, TLeftResult, TRightResult> context) => Either<TLeftResult, TRightResult>.Right(context.RightSelector(right)),
                in context);
        }

        private readonly ref struct SelectContext<TLeftSource, TRightSource, TLeftResult, TRightResult>
        {
            public SelectContext(
                Func<TLeftSource, TLeftResult> leftSelector,
                Func<TRightSource, TRightResult> rightSelector)
            {
                LeftSelector = leftSelector;
                RightSelector = rightSelector;
            }

            public Func<TLeftSource, TLeftResult> LeftSelector { get; }
            public Func<TRightSource, TRightResult> RightSelector { get; }
        }

        /*public static async ITask<IEither<TLeftResult, TRightResult>> SelectAsync<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, ITask<TLeftResult>> leftSelector,
            Func<TRightSource, ITask<TRightResult>> rightSelector)
        {
            var context = new SelectAsyncContext<TLeftSource, TRightSource, TLeftResult, TRightResult>(
                leftSelector,
                rightSelector);
            return await either.Apply(
                async (TLeftSource left, SelectAsyncContext<TLeftSource, TRightSource, TLeftResult, TRightResult> context) => Either<TLeftResult, TRightResult>.Left(await context.LeftSelector(left)),
                async (TRightSource right, SelectAsyncContext<TLeftSource, TRightSource, TLeftResult, TRightResult> context) => Either<TLeftResult, TRightResult>.Right(await context.RightSelector(right)),
                in context)
                .ConfigureAwait(false);
        }*/

        private readonly ref struct SelectAsyncContext<TLeftSource, TRightSource, TLeftResult, TRightResult>
        {
            public SelectAsyncContext(
                Func<TLeftSource, ITask<TLeftResult>> leftSelector,
                Func<TRightSource, ITask<TRightResult>> rightSelector)
            {
                LeftSelector = leftSelector;
                RightSelector = rightSelector;
            }

            public Func<TLeftSource, ITask<TLeftResult>> LeftSelector { get; }
            public Func<TRightSource, ITask<TRightResult>> RightSelector { get; }
        }
		
		public static Realizable<TResult> Apply2<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
			where TResult : allows ref struct
        {
            if (either is IApply1<TLeft, TRight> apply)
            {
                return apply.ApplyImpl1(leftMap, rightMap, ref context);
            }

			if (either.Decompose(out var left, out var right))
			{
				var resultFuture = leftMap(left, ref context); //// TODO you also need to wrap the call to `leftmap`
				return new Realizable<TResult>(
					resultFuture.ContinueWith(
						future =>
						{
							try
							{
								return future.GetAwaiter().GetResult();
							}
							catch (Exception exception)
							{
								throw new LeftMapException(exception);
							}
						}));
			}
			else
			{
                try
                {
                    var result = rightMap(right, ref context);
                    return new Realizable<TResult>(result);
                }
                catch (Exception exception)
                {
                    throw new RightMapException(exception);
                }
			}
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            AsyncRefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
        {
            if (either is IApply1<TLeft, TRight> apply)
            {
                apply.ApplyImpl1(leftMap, rightMap, ref context).TryRealize(out _, out var task);
                return task!;
            }

			if (either.Decompose(out var left, out var right))
			{
				var resultFuture = leftMap(left, ref context);
				return resultFuture.ContinueWith(
					future =>
					{
						try
						{
							return future.GetAwaiter().GetResult();
						}
						catch (Exception exception)
						{
							throw new LeftMapException(exception);
						}
					});
			}
			else
			{
				var result = rightMap(right, ref context);
				return new FromResult<TResult>(result);
				/*return resultFuture.ContinueWith(
					future =>
					{
						try
						{
							return future.GetAwaiter().GetResult();
						}
						catch (Exception exception)
						{
							throw new RightMapException(exception);
						}
					});*/
			}
        }
		
		//// TODO there's a "proper" name for this, I think
		private static bool Decompose<TLeft, TRight>(this IEither<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
		{
            var context = true;
			var tempLeft = default(TLeft);
			var tempRight = default(TRight);
			var result = either
				.Apply(
					(TLeft value, ref bool nothing) =>
					{
						tempLeft = value;
						return new TaskWrapper<bool>(Task.FromResult(true));
					},
					(TRight value, ref bool nothing) =>
					{
						tempRight = value;
						return new TaskWrapper<bool>(Task.FromResult(false));
					},
					ref context);
				
			left = tempLeft;
			right = tempRight;
			return result.ConfigureAwait(false).GetAwaiter().GetResult();
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
        {
            return either.Apply(
                Convert(leftMap),
                rightMap,
                ref context);
        }

        public static TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            RefContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
			where TResult : allows ref struct
        {
			if (either.Decompose(out var left, out var right))
			{
				try
				{
					return leftMap(left, ref context);
				}
				catch (Exception exception)
				{
					throw new LeftMapException(exception);
				}
			}
			else
			{
				try
				{
					return rightMap(right, ref context);
				}
				catch (Exception exception)
				{
					throw new RightMapException(exception);
				}
			}
        }

        public static ITask<TResult> Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefContextualizedMap<TLeft, TContext, TResult> leftMap,
            AsyncInContextualizedMap<TRight, TContext, TResult> rightMap,
            ref TContext context)
            where TContext : allows ref struct
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
            where TResult : allows ref struct
        {
            var context = true; //// TODO use nothing instead
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context);
        }

		//// TODO you temporarily are commenting this out because of an ambigous call error from the compiler
        /*public static TResult Apply<TLeft, TRight, TResult>( //// TODO is this a fold?
            this IEither<TLeft, TRight> either,
            Map<TLeft, TResult> leftMap,
            Map<TRight, TResult> rightMap)
            where TResult : allows ref struct
        {
            if (either is IApply2<TLeft, TRight> apply)
            {
                return apply.ApplyImpl2(leftMap, rightMap);
            }

            var context = true;
            return either.Apply(
                Convert<TLeft, bool, TResult>(leftMap),
                Convert<TRight, bool, TResult>(rightMap),
                ref context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }*/

        //// there are 39 overloads that can't have `tresult : allows ref struct`
        //// 
        //// TODO can you have a have the return type be a ref struct implementation of itask that is a union on an itask or a fromresult(ref struct)? you would lose covariance of tresult, how does that impact things downstream like chaining stuff together? you can test this by implementing a select and then chaining them together; does an implicit converter from the ref struct itask to itask help?
        ////    you tried doing this, but the problem is that a `ref struct` can't have `await` called on it, so regardless of anything else, that is a blocker
        ////
        //// TODO what happens if you the kernel `apply` is sync instead of async?
        ////    you tried this, and the result was that `tresult` can't be a ref struct *ever* (this is a bit of an exaggeration, but it's pretty close to true)
        ////
        //// TODO you are here
        //// TODO this file wins out i think over the other file, assuming that the functionality is correct for this file, so start testing the functionality
        //// TODO your testing also needs to make sure that the exceptions are thrown correctly
        //// TODO then implement the monad scaffolding
        //// TODO then implement the mixins for this design
        //// TODO do any of the apply overloads create closures that can be removed by leveraging the context parameter?
        ////
        //// TODO go through the `apply2`s
        //// TODO other TODOs
        //// TODO should `ieither` allow ref structs even though it can't really be implemented? this is like ienumerable doing it;
        //// TODO regardless of the `ieither` interface, should the map definitions have `tvalue : allows ref struct` even though it doesn't actually add any value currently? (if you do this, you should also apply it to as many extensions as possible)
    }

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
        private static AsyncRefContextualizedMap<TValue, ContextWrapper<TContext>, TResult> Wrap<TValue, TContext, TResult>(AsyncRefContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref ContextWrapper<TContext> context) => map(value, ref context.Context);
        }

        private static InContextualizedMap<TValue, TContext, TResult> Adapt<TValue, TContext, TResult>(ContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, in TContext context) => map(value, context);
        }

        private static AsyncInContextualizedMap<TValue, ContextWrapper<TContext>, TResult> Wrap<TValue, TContext, TResult>(AsyncContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, in ContextWrapper<TContext> context) => map(value, context.Context);
        }

        private static InContextualizedMap<TValue, ContextWrapper<TContext>, TResult> Wrap<TValue, TContext, TResult>(InContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, in ContextWrapper<TContext> context) => map(value, context.Context);
        }

        private static AsyncInContextualizedMap<TValue, ContextWrapper<TContext>, TResult> Wrap<TValue, TContext, TResult>(AsyncInContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, in ContextWrapper<TContext> context) => map(value, context.Context);
        }

        private readonly unsafe ref struct ContextWrapper<TContext> where TContext : allows ref struct
        {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            private readonly TContext* context;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

            public ContextWrapper(in TContext context)
            {
                //// TODO is this actually safe? it appears to be based on your current `either` implemntation, but you need to test it; also, you need to decide if you're ok exposing this to other `ieither` implementations
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                this.context = (TContext*)Unsafe.AsPointer(in context);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }

            public ref TContext Context
            {
                get
                {
                    return ref System.Runtime.CompilerServices.Unsafe.AsRef<TContext>(this.context);
                }
            }
        }

        private static class Unsafe
        {
            public static unsafe void* AsPointer<T>(in T value) where T : allows ref struct
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                fixed (void* pointer = &value)
                {
                    return pointer;
                }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }
        }

        /*private readonly ref struct ContextWrapper<TContext> where TContext : allows ref struct
        {
            public ContextWrapper(in TContext context)
            {
                this.Context = context;
            }

            public TContext Context { get; }
        }*/

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(Map<TValue, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => new FromResult3<TResult>(() => map(value));
        }

        private sealed class FromResult3<T> : ITask<T> where T : allows ref struct
        {
            private readonly Func<T> promise;

            public FromResult3(Func<T> promise)
            {
                this.promise = promise;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.promise,
                    Task.CompletedTask.ConfigureAwait(continueOnCapturedContext).GetAwaiter()); //// TODO is it ok to use this awaitable? //// TODO i still don't know if it's ok, but i think you need to not call `getawaiter` yet
            }
			
			
			public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<T>, TResult2> continuationFunction)
				where TResult2 : allows ref struct
			{
				throw new Exception("TODO");
			}


            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
            {
                private readonly Func<T> promise;
                private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                public ConfiguredAwaitable(Func<T> promise, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                {
                    this.promise = promise;
                    this.taskAwaiter = taskAwaiter;
                }

                public ITaskAwaiter<T> GetAwaiter()
                {
                    return new TaskAwaiter(this.promise, this.taskAwaiter);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<T>
                {
                    private readonly Func<T> promise;
                    private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                    public TaskAwaiter(Func<T> promise, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                    {
                        this.promise = promise;
                        this.taskAwaiter = taskAwaiter;
                    }

                    public bool IsCompleted { get; } = true;

                    public T GetResult()
                    {
                        return this.promise();
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

            public ITaskAwaiter<T> GetAwaiter()
            {
                return new TaskAwaiter(this.promise, Task.CompletedTask.GetAwaiter());
            }

            private sealed class TaskAwaiter : ITaskAwaiter<T>
            {
                private readonly Func<T> promise;
                private readonly System.Runtime.CompilerServices.TaskAwaiter taskAwaiter;

                public TaskAwaiter(Func<T> promise, System.Runtime.CompilerServices.TaskAwaiter taskAwaiter)
                {
                    this.promise = promise;
                    this.taskAwaiter = taskAwaiter;
                }

                public bool IsCompleted { get; } = true;

                public T GetResult()
                {
                    return this.promise();
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

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(AsyncMap<TValue, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => map(value);
        }

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(ContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
        {
            return (TValue value, ref TContext context) => new FromResult<TResult>(map(value, context));
        }

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert2<TValue, TContext, TResult>(ContextualizedMap<TValue, TContext, TResult> map)
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => new FromResult2<TContext, TResult>(context, _ => map(value, _));
        }

        private sealed class FromResult2<TContext, TResult> : ITask<TResult> where TResult : allows ref struct
        {
            private readonly TContext context;
            private readonly Func<TContext, TResult> promise;

            public FromResult2(TContext context, Func<TContext, TResult> promise)
            {
                this.context = context;
                this.promise = promise;
            }

            public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.context, this.promise, Task.CompletedTask.ConfigureAwait(continueOnCapturedContext).GetAwaiter()); //// TODO is using the complete task awaiter ok? you may need to use the "same" "instance", so you might actually need to have the `taskawaiter` take in the `configuredawaitable` and reference the field
            }

			public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<TResult>, TResult2> continuationFunction)
				where TResult2 : allows ref struct
			{
				throw new Exception("TODO");
			}

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TResult>
            {
                private readonly TContext context;
                private readonly Func<TContext, TResult> promise;
                private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                public ConfiguredAwaitable(TContext context, Func<TContext, TResult> promise, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                {
                    this.context = context;
                    this.promise = promise;
                    this.taskAwaiter = taskAwaiter;
                }

                public ITaskAwaiter<TResult> GetAwaiter()
                {
                    return new TaskAwaiter(this.context, this.promise, this.taskAwaiter);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<TResult>
                {
                    private readonly TContext context;
                    private readonly Func<TContext, TResult> promise;
                    private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                    public TaskAwaiter(TContext context, Func<TContext, TResult> promise, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                    {
                        this.context = context;
                        this.promise = promise;
                        this.taskAwaiter = taskAwaiter;
                    }

                    public bool IsCompleted { get; } = true;

                    public TResult GetResult()
                    {
                        return this.promise(this.context);
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

            public ITaskAwaiter<TResult> GetAwaiter()
            {
                return new TaskAwaiter(this.context, this.promise, Task.CompletedTask.GetAwaiter()); //// TODO is using the complete task awaiter ok?
            }

            private sealed class TaskAwaiter : ITaskAwaiter<TResult>
            {
                private readonly TContext context;
                private readonly Func<TContext, TResult> promise;
                private readonly System.Runtime.CompilerServices.TaskAwaiter taskAwaiter;

                public TaskAwaiter(TContext context, Func<TContext, TResult> promise, System.Runtime.CompilerServices.TaskAwaiter taskAwaiter)
                {
                    this.context = context;
                    this.promise = promise;
                    this.taskAwaiter = taskAwaiter;
                }

                public bool IsCompleted { get; } = true;

                public TResult GetResult()
                {
                    return this.promise(this.context);
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

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(AsyncContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => map(value, context);
        }

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(InContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
        {
            return (TValue value, ref TContext context) => new FromResult<TResult>(map(value, in context));
        }

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(AsyncInContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return (TValue value, ref TContext context) => map(value, in context);
        }

        private static AsyncRefContextualizedMap<TValue, TContext, TResult> Convert<TValue, TContext, TResult>(RefContextualizedMap<TValue, TContext, TResult> map)
            where TContext : allows ref struct
        {
            return (TValue value, ref TContext context) => new FromResult<TResult>(map(value, ref context));
        }

        private sealed class FromResult<T> : ITask<T> //// TODO make this public?
        {
            private readonly T value;

            public FromResult(T value)
            {
                this.value = value;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    this.value,
                    Task.CompletedTask.ConfigureAwait(continueOnCapturedContext).GetAwaiter()); //// TODO is it ok to use this awaitable?
            }

			public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<T>, TResult2> continuationFunction)
				where TResult2 : allows ref struct
			{
				throw new Exception("TODO");
			}

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
            {
                private readonly T value;
                private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                public ConfiguredAwaitable(T value, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                {
                    this.value = value;
                    this.taskAwaiter = taskAwaiter;
                }

                public ITaskAwaiter<T> GetAwaiter()
                {
                    return new TaskAwaiter(this.value, this.taskAwaiter);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<T>
                {
                    private readonly T value;
                    private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                    public TaskAwaiter(T value, ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
                    {
                        this.value = value;
                        this.taskAwaiter = taskAwaiter;
                    }

                    public bool IsCompleted { get; } = true;

                    public T GetResult()
                    {
                        return this.value;
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

            public ITaskAwaiter<T> GetAwaiter()
            {
                return new TaskAwaiter(this.value, Task.CompletedTask.GetAwaiter());
            }

            private sealed class TaskAwaiter : ITaskAwaiter<T>
            {
                private readonly T value;
                private readonly System.Runtime.CompilerServices.TaskAwaiter taskAwaiter;

                public TaskAwaiter(T value, System.Runtime.CompilerServices.TaskAwaiter taskAwaiter)
                {
                    this.value = value;
                    this.taskAwaiter = taskAwaiter;
                }

                public bool IsCompleted { get; } = true;

                public T GetResult()
                {
                    return this.value;
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
    //// demonstrate ref struct result being added to a list
    //// demonstrate ref struct context being added to a list
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
			
			public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<int>, TResult2> continuationFunction)
				where TResult2 : allows ref struct
			{
				throw new Exception("TODO");
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
    ////
    //// TODO document the use of itask for the kernel
}
