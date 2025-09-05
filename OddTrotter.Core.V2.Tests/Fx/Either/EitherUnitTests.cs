namespace Fx.Either
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Tests
    {
        public class LeftContainer
        {
            public static LeftContainer Create()
            {
                return new LeftContainer("left", 3);
            }

            private LeftContainer(string first, int second)
            {
                First = first;
                Second = second;
            }

            public string First { get; }
            public int Second { get; }
        }

        public class RightContainer
        {
            public RightContainer(IEnumerable<Exception> exceptions)
            {
                Exceptions = exceptions;
            }

            public IEnumerable<Exception> Exceptions { get; }
        }

        public ref struct ContextRefStruct
        {
            public ContextRefStruct(int mutable, int immutable)
            {
                Mutable = mutable;
                Immutable = immutable;
            }

            public int Mutable { get; private set; }
            public int Immutable { get; }

            public void Mutate()
            {
                this.Mutable = -5;
            }

            public static ContextRefStruct CreateInitial()
            {
                return new ContextRefStruct();
            }

            public static ContextRefStruct CreateSubsequent()
            {
                return new ContextRefStruct();
            }
        }

        public class ContextClass
        {
            public ContextClass(int mutable, int immutable)
            {
                Mutable = mutable;
                Immutable = immutable;
            }

            public int Mutable { get; set; }
            public int Immutable { get; }
        }

        public ref struct ResultRefStruct
        {
            public ResultRefStruct(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public class ResultClass
        {
            public ResultClass(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public ITask<ResultRefStruct> LeftAsyncRefContextualizedMapRefStructContextRefStructResult(LeftContainer value, ref ContextRefStruct context)
        {
            context.Mutate();
            this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState = 2;
            while (this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState != 3)
            {
            }

            context = ContextRefStruct.CreateSubsequent();

            return new FromResult<ResultRefStruct>(() => new ResultRefStruct(string.Join(string.Empty, Enumerable.Repeat(value.First, value.Second))));
        }

        public ITask<ResultRefStruct> RightAsyncRefContextualizedMapRefStructContextRefStructResult(RightContainer value, ref ContextRefStruct context)
        {
            return null!;
        }

        private sealed class FromResult<T> : ITask<T> where T : allows ref struct
        {
            private readonly Func<T> promise;

            public FromResult(Func<T> promise)
            {
                this.promise = promise;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }

            public ITaskAwaiter<T> GetAwaiter()
            {
                return new TaskAwaiter(this.promise);
            }

            private sealed class TaskAwaiter : ITaskAwaiter<T>
            {
                private readonly Func<T> promise;

                public TaskAwaiter(Func<T> promise)
                {
                    this.promise = promise;
                    //// TODO rewrite this from stratch, you copied it from somewhere
                }

                public bool IsCompleted { get; } = true;

                public T GetResult()
                {
                    return this.promise();
                }

                public void OnCompleted(Action continuation)
                {
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                }
            }
        }

        [TestMethod]
        public async Task ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResult()
        {
            //// TODO you are explicitly writing the types so that can ensure you are calling the correct overload
            IEither<LeftContainer, RightContainer> either = Either<LeftContainer, RightContainer>.Left(LeftContainer.Create());

            AsyncRefContextualizedMap<LeftContainer, ContextRefStruct, ResultRefStruct> leftMap = this.LeftAsyncRefContextualizedMapRefStructContextRefStructResult;
            AsyncRefContextualizedMap<RightContainer, ContextRefStruct, ResultRefStruct> rightMap = this.RightAsyncRefContextualizedMapRefStructContextRefStructResult;

            ContextRefStruct context = ContextRefStruct.CreateInitial();

            var assertMutated = this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultAssertMutated(ref context);

            //// TODO you are here
            //// TODO document the "locks"
            ResultRefStruct result = await either.Apply(leftMap, rightMap, ref context); //// TODO .ConfigureAwait(false);

            ///// TODO assert result

            await assertMutated.ConfigureAwait(false);
        }

        private unsafe Task ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultAssertMutated(ref ContextRefStruct context) //// TODO you'd prefer this to be `in`, but that seems to require a newer version of the framework
        {
            this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultPointer = (ContextRefStruct*)Unsafe.AsPointer(ref context);
            this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState = 1;

            return Task.Factory.StartNew(() =>
            {
                while (this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState != 2)
                {
                }

                var context = Unsafe.AsRef<ContextRefStruct>(this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultPointer);

                this.ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState = 3;

                Assert.AreNotEqual(ContextRefStruct.CreateInitial().Mutable, context.Mutable);
            });
        }

        private unsafe ContextRefStruct* ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultPointer;

        private int ApplyLeftEitherAsyncRefContextualizedMapAsyncRefContextualizedMapRefStructContextRefStructResultState = 0;

        /*[TestMethod]
        public void DoWork()
        {
            var context = new Context()
            {
                PlaceHolder = "asdf",
            };
            var result = new Either<string, Exception>("1234").ApplyRef(AdaptString, AdaptException, ref context);

            context.PlaceHolder = "zxcv";
            result = new Either<string, Exception>("1234").ApplyRef2(AdaptString2, AdaptException2, context);

            result = new Either<string, Exception>("1234").ApplyRef3(AdaptString2, AdaptException2, ref context);


            result = new Either<string, Exception>("1234").ApplyIn(AdaptString3, AdaptException3, in context);
        }

        public async Task DoWork2()
        {
            var context = new Context()
            {
                PlaceHolder = "asdf",
            };
            var result = await new Either<string, Exception>("1234").Apply(AdaptString4, AdaptException4, context).ConfigureAwait(false);
        }

        public ref struct Context2
        {
            public string PlaceHolder { get; set; }
        }

        public struct Context
        {
            public string PlaceHolder { get; set; }
        }

        public static int AdaptString2(string value, Context context)
        {
            context.PlaceHolder = "qwer";
            return 1;
        }

        public static int AdaptString3(string value, in Context context)
        {
            ////context.PlaceHolder = "qwer";
            return 1;
        }

        public static async ITask<Context2> AdaptString4(string value, Context context)
        {
            context.PlaceHolder = "qwer";
            return await new RefStructTask<Context2>(() => new Context2()).ConfigureAwait(false);
        }

        public sealed class RefStructTask<T> : ITask<T> where T : allows ref struct
        {
            private readonly Func<T> factory;

            public RefStructTask(Func<T> factory)
            {
                this.factory = factory;
            }

            public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.factory, continueOnCapturedContext);
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<T>
            {
                private readonly Func<T> factory;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(Func<T> factory, bool continueOnCapturedContext)
                {
                    this.factory = factory;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public ITaskAwaiter<T> GetAwaiter()
                {
                    return new TaskAwaiter(this.factory, this.continueOnCapturedContext);
                }

                private sealed class TaskAwaiter : ITaskAwaiter<T>
                {
                    private readonly Func<T> factory;

                    private System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

                    public TaskAwaiter(Func<T> factory, bool continueOnCaptureContext)
                    {
                        this.factory = factory;

                        this.taskAwaiter = Task.CompletedTask.ConfigureAwait(continueOnCaptureContext).GetAwaiter();
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            return true;
                        }
                    }

                    public T GetResult()
                    {
                        return this.factory();
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
                return new TaskAwaiter(this.factory);
            }

            private sealed class TaskAwaiter : ITaskAwaiter<T>
            {
                private readonly Func<T> factory;

                private System.Runtime.CompilerServices.TaskAwaiter taskAwaiter;

                public TaskAwaiter(Func<T> factory)
                {
                    this.factory = factory;

                    this.taskAwaiter = Task.CompletedTask.GetAwaiter();
                }

                public bool IsCompleted
                {
                    get
                    {
                        return true;
                    }
                }

                public T GetResult()
                {
                    return this.factory();
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

        public static int AdaptString(string value, ref Context context)
        {
            context.PlaceHolder = "qwer";
            return 1;
        }

        public static int AdaptException2(Exception value, Context context)
        {
            return 2;
        }

        public static int AdaptException3(Exception value, in Context context)
        {
            return 2;
        }

        public static async ITask<Context2> AdaptException4(Exception value, Context context)
        {
            return await new RefStructTask<Context2>(() => new Context2()).ConfigureAwait(false);
        }

        public static int AdaptException(Exception value, ref Context context)
        {
            return 2;
        }
    }

    public sealed class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private readonly TLeft? left;
        private readonly TRight? right;

        public Either(TLeft left)
        {
            this.left = left;
        }

        public Either(TRight right)
        {
            this.right = right;
        }

        public unsafe TResult Apply<TResult, TContext>(System.Func<TLeft, TContext, TResult> leftMap, System.Func<TRight, TContext, TResult> rightMap, TContext context)
        {
            if (left != null)
            {
                return leftMap(left, context);
            }
            else if (right != null)
            {
                return rightMap(right, context);
            }
            else
            {
                throw new System.Exception("TODO");
            }
        }

        public ITask<TResult> Apply<TResult, TContext>(System.Func<TLeft, TContext, ITask<TResult>> leftMap, System.Func<TRight, TContext, ITask<TResult>> rightMap, TContext context) where TResult : allows ref struct where TContext : allows ref struct
        {
            throw new System.NotImplementedException();
        }

        public TResult ApplyRef<TResult, TContext>(
            RefFunc<TLeft, TContext, TResult> leftMap,
            RefFunc<TRight, TContext, TResult> rightMap,
            ref TContext context)
        {
            if (left != null)
            {
                return leftMap(left, ref context);
            }
            else if (right != null)
            {
                return rightMap(right, ref context);
            }
            else
            {
                throw new System.Exception("TODO");
            }
        }
    }

    public static class Playground
    {
        public static unsafe TResult ApplyRef2<TLeft, TRight, TResult, TContext>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            TContext context)
        {
            return either.ApplyRef(
                (TLeft left, ref TContext context) => leftMap(left, context),
                (TRight right, ref TContext context) => rightMap(right, context),
                ref context);
        }

        public static unsafe TResult ApplyRef3<TLeft, TRight, TResult, TContext>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TContext, TResult> leftMap,
            Func<TRight, TContext, TResult> rightMap,
            ref TContext context)
        {
            return either.ApplyRef(
                (TLeft left, ref TContext context) => leftMap(left, context),
                (TRight right, ref TContext context) => rightMap(right, context),
                ref context);
        }


        public delegate TResult RefFunc<in T1, T2, out TResult>(T1 t1, ref T2 t2);

        public delegate TResult InFunc<in T1, T2, out TResult>(T1 t1, in T2 t2);



        public static TResult ApplyIn<TLeft, TRight, TResult, TContext>(
            this Either<TLeft, TRight> either,
            InFunc<TLeft, TContext, TResult> leftMap,
            InFunc<TRight, TContext, TResult> rightMap,
            in TContext context)
        {
            var wrapper = new Wrapper<TContext>(in context);
            return either.ApplyRef(
                (TLeft left, ref Wrapper<TContext> context) => leftMap(left, context.Context),
                (TRight right, ref Wrapper<TContext> context) => rightMap(right, context.Context),
                ref wrapper);
        }

        private unsafe readonly struct Wrapper<TContext>
        {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            private readonly TContext* context;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

            public Wrapper(in TContext context)
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                fixed (TContext* pointer = &context)
                {
                    this.context = pointer;
                }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                ////this.context = (TContext*)Unsafe.AsPointer(ref context);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }

            public ref TContext Context
            {
                get
                {
                    return ref Unsafe.AsRef<TContext>(this.context);
                }
            }
        }*/
    }
}
