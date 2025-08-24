namespace Fx.Either
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Fx.Either.Playground;

    [TestClass]
    public class Tests
    {
        [TestMethod]
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

        /*public async Task DoWork2()
        {
            var context = new Context2()
            {
                PlaceHolder = "asdf",
            };
            var result = await new Either<string, Exception>("1234").Apply(AdaptString4, AdaptException4, context).ConfigureAwait(false);
        }*/

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

        public static async ITask<int> AdaptString4(string value, Context context)
        {
            //// TODO this should be able to return itask<context2> if you implement a refstructtask that implements itask

            context.PlaceHolder = "qwer";
            return await Task.FromResult(1).ConfigureAwait(false);
        }

        public readonly ref struct RefStructTask
        {
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

        public static async ITask<int> AdaptException4(Exception value, Context context)
        {
            return await Task.FromResult(2).ConfigureAwait(false);
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
        }
    }
}
