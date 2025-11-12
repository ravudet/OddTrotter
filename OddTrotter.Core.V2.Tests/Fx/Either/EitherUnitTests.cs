namespace Fx.Either
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void UnsafeAsInterface()
        {
            var foo = new Implementation();

            Thing<Implementation>();

            var casted = Unsafe.As<Implementation, IInterface>(ref foo);
            casted.TheMethod();
        }

        public void Thing<T>() where T : struct, allows ref struct
        {
            Console.WriteLine("hey");
        }

        public readonly ref struct Implementation : IInterface
        {
            public void TheMethod()
            {
                Console.WriteLine("hello");
            }
        }

        public interface IInterface
        {
            void TheMethod();
        }

        [TestMethod]
        public async Task TryCast()
        {
            var realizable = new Realizable<int>(37);
            if (realizable.TryCast<Realizable<int>.DecomposeDelegate>(out var decompose))
            {
                var decomposed = decompose(realizable, out var isLeft);
                Assert.IsTrue(isLeft);
                Assert.AreEqual(37, decomposed.Left);
            }
            else
            {
                throw new Exception("TODO");
            }

            realizable = new Realizable<int>(Constant(41));
            if (realizable.TryCast<Realizable<int>.DecomposeDelegate>(out decompose))
            {
                var decomposed = decompose(realizable, out var isLeft);
                Assert.IsFalse(isLeft);
                Assert.AreEqual(41, await decomposed.Right);
            }
            else
            {
                throw new Exception("TODO");
            }
        }

        private static async ITask<int> Constant(int value)
        {
            return await Task.FromResult(value).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TryCast2()
        {
            var realizable = new Realizable<int>(37);
            if (realizable.TryCast2<Realizable<int>.IDecomposable<Realizable<int>>>(out var decompose))
            {
                var decomposed = decompose.Decompose(realizable, out var isLeft);
                Assert.IsTrue(isLeft);
                Assert.AreEqual(37, decomposed.Left);
            }
            else
            {
                throw new Exception("TODO");
            }

            realizable = new Realizable<int>(Constant(41));
            if (realizable.TryCast2<Realizable<int>.IDecomposable<Realizable<int>>>(out decompose))
            {
                var decomposed = decompose.Decompose(realizable, out var isLeft);
                Assert.IsFalse(isLeft);
                Assert.AreEqual(41, await decomposed.Right);
            }
            else
            {
                throw new Exception("TODO");
            }
        }

        /*[TestMethod]
        public void ReflectionDecompose()
        {
            ReflectionDecompose(new Realizable<int>(100));
        }

        private void ReflectionDecompose<T>(T value)
            where T : allows ref struct
        {
            var type = typeof(T);
            var interfaces = type.GetInterfaces();

            var @interface = interfaces.Where(@interface => @interface == typeof(IDecomposeMixin<int, ITask<int>, Realizable<int>.Decomposed>)).First();

            var interfaceMethodInfos = @interface.GetMethods();
            /*var parameterModifier = new ParameterModifier(1);
            parameterModifier[0] = true;
            var interfaceMethodInfo = @interface.GetMethod("Decompose", new[] { typeof(bool) }, new[] { parameterModifier});*/
        /*var interfaceMethodInfo = interfaceMethodInfos.First();

        var methodInfos = type.GetMethods();
        var typeMethodInfo = methodInfos
            .Where(methodInfo =>
                methodInfo.Name == interfaceMethodInfo.Name &&
                methodInfo.GetParameters().Length == interfaceMethodInfo.GetParameters().Length)
            .First();
        ////.Where(methodInfo => methodInfo.MetadataToken == interfaceMethodInfo.MetadataToken)
        //// TODO because the method is on a ref struct, it's harder to compare conventionally
        /*.Where(methodInfo =>
            methodInfo.ContainsGenericParameters == interfaceMethodInfo.ContainsGenericParameters &&
            methodInfo
                .GetParameters()
                .Zip(interfaceMethodInfo.GetParameters())
                .All(parameterInfos => 
                    parameterInfos.First.IsIn == parameterInfos.Second.IsIn &&
                    parameterInfos.First.IsOut == parameterInfos.Second.IsOut &&
                    parameterInfos.First.IsRetval == parameterInfos.Second.IsRetval &&
                    //// parameterInfos.First.Name == parameterInfos.Second.Name &&
                    parameterInfos.First.ParameterType == parameterInfos.Second.ParameterType &&
                    parameterInfos.First.Position == parameterInfos.Second.Position
                    ) &&
            methodInfo.IsAbstract == interfaceMethodInfo.IsAbstract &&
            methodInfo.IsConstructor == interfaceMethodInfo.IsConstructor &&
            methodInfo.IsPrivate == interfaceMethodInfo.IsPrivate &&
            methodInfo.IsPublic == interfaceMethodInfo.IsPublic &&
            methodInfo.IsSpecialName == interfaceMethodInfo.IsSpecialName &&
            methodInfo.IsStatic == interfaceMethodInfo.IsStatic &&
            methodInfo.MemberType == interfaceMethodInfo.MemberType &&
            methodInfo.Name == interfaceMethodInfo.Name &&
            methodInfo.ReturnType == interfaceMethodInfo.ReturnType
            )
        .First();*/

        ////
        /*typeMethodInfo.Invoke(value, null);

        ReflectionDecompose2(value);

        new Placeholder<T>(value);
    }

    public readonly ref struct Placeholder<T> : IDecomposeMixin<int, ITask<int>, Realizable<int>.Decomposed>
        where T : IDecomposeMixin<int, ITask<int>, Realizable<int>.Decomposed>, allows ref struct
    {
        private readonly T value;

        public Placeholder(T value)
        {
            this.value = value;
        }

        public Realizable<int>.Decomposed Decompose(out bool isLeft)
        {
            throw new NotImplementedException();
        }
    }

    private void ReflectionDecompose2<T>(T value)
        where T : IDecomposeMixin<int, ITask<int>, Realizable<int>.Decomposed>, allows ref struct
    {
    }*/

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            MethodCallExpression? outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
            }

            return outermostExpression.Method;
        }

        [TestMethod]
        public async Task AsyncRealizable()
        {
            Console.WriteLine(await NewThing.Attempt5());
        }

        [TestMethod]
        public async Task AsyncRealizableRefStruct()
        {
            NewThing.ForAttempt4 forAttempt4;
            if (AsyncRealizableRefStruct2().TryRealize(out var realized, out var future))
            {
                forAttempt4 = realized;
            }
            else
            {
                forAttempt4 = await future.ConfigureAwait(false);
            }
        }

        private async Realizable<NewThing.ForAttempt4> AsyncRealizableRefStruct2()
        {
            if (NewThing.Attempt4().TryRealize(out var realized, out var future))
            {
                return realized;
            }
            else
            {
                return await future.ConfigureAwait(false);
                //// TODO this might be a compiler bug; if you use an intermediate variable, there's a compilation error:
                //// var result = await future.ConfigureAwait(false);
                //// return result;
                //// plus, the code doesn't even run because it violates the generic constraints on `task<T>` that are invoked through the asynctaskmethodbuilder when `await` is used above
            }
        }


        [TestMethod]
        public async Task AsyncRealizableRefStructAgain()
        {
            //// TODO to get the console output from the called method, i think that `realizable` needs to know about `realizable<T>`; but i don't think we have a way to do that, so i think what you need is for `realizable<T>` to have a `getnongeneric` method that returns the `realizable` that can be used for tracking

            await AsyncRealizableRefStructAgain3().ConfigureAwait(false);
            await AsyncRealizableRefStructAgain2().ConfigureAwait(false);
            ////var tracker = realizable.Tracker;
            ////await tracker;
        }

        [TestMethod]
        public async Task AsyncRealizableRefStructAgainSomeMore()
        {
            await NewThing.Attempt100().Tracker;
            await NewThing.Attempt101().Tracker;

        }

        private async Task AsyncRealizableRefStructAgain3()
        {
            NewThing.ForAttempt4 forAttempt4;
            if (NewThing.Attempt100().TryRealize(out var realized, out var future))
            {
                forAttempt4 = realized;
            }
            else
            {
                forAttempt4 = await future.ConfigureAwait(false);
            }

            Console.WriteLine(forAttempt4.Value);
        }

        private async Task AsyncRealizableRefStructAgain2()
        {
            NewThing.ForAttempt4 forAttempt4;
            if (NewThing.Attempt101().TryRealize(out var realized, out var future))
            {
                forAttempt4 = realized;
            }
            else
            {
                forAttempt4 = await future;
            }

            Console.WriteLine(forAttempt4.Value);
        }

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

            public ITask<TResult> ContinueWith<TResult>(Func<ITask<T>, TResult> continuationFunction) where TResult : allows ref struct
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
