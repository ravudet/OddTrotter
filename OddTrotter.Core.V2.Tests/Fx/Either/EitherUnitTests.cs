namespace Fx.Either
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            ////var result = new Either<string, Exception>("asdf").Apply(AdaptString, AdaptException, ref context);
        }

        public struct Context
        {
            public string PlaceHolder { get; set; }
        }

        public static int AdaptString(string value, ref Context context)
        {
            context.PlaceHolder = "qwer";
            return 1;
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

        public Task<TResult> Apply<TResult, TContext>(System.Func<TLeft, TContext, Task<TResult>> leftMap, System.Func<TRight, TContext, Task<TResult>> rightMap, TContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    /*public static class Playground
    {
        public delegate TResult RefFunc<in T1, T2, out TResult>(T1 t1, ref T2 t2);

        public static unsafe TResult Apply<TLeft, TRight, TResult, TContext>(
            this IEither<TLeft, TRight> either,
            RefFunc<TLeft, TContext, TResult> leftMap,
            RefFunc<TRight, TContext, TResult> rightMap,
            ref TContext context)
        {
            var wrapper = new Wrapper<TContext>(ref context);
            wrapper.Context = default!;
            return either.Apply(
                (left, context) => leftMap(left, ref Unsafe.AsRef<TContext>(context.Context)),
                (right, context) => rightMap(right, ref Unsafe.AsRef<TContext>(context.Context)),
                wrapper);
        }

        private unsafe struct Wrapper<TContext>
        {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            private TContext* context;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

            public Wrapper(ref TContext context)
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                this.context = (TContext*)Unsafe.AsPointer(ref context);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }

            public unsafe ref TContext Context
            {
                get
                {
                    return ref Unsafe.AsRef(this.context);
                }
                set
                {
                    this.context = (TContext*)Unsafe.AsPointer(ref value);
                }
            }
        }
    }*/
}
