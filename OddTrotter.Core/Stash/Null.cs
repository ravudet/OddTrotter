using Fx.Either;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stash
{
    public sealed class Null
    {
    }

    public sealed class Null<T>
    {
        public static implicit operator Null<T>(Null @null)
        {
            return new Null<T>();
        }

        public static implicit operator Null(Null<T> @null)
        {
            return new Null();
        }
    }

    public static class NullPlaygroundExtension
    {
        public static EitherHelper2<TLeft> Left<TLeft>(this TLeft left)
        {
            return new EitherHelper2<TLeft>();
        }



        public sealed class EitherHelper2<TValue>
        {
            public NullPlayground.NewEither<TValue, TRight> Right<TRight>()
            {
                return null!;
            }
        }
    }

    public static class NullPlayground
    {
        public static void DoWork()
        {
            global::System.Collections.Generic.List<Null> stuff = new global::System.Collections.Generic.List<Null>();
            stuff.Add(new Null<string>());

            global::System.Collections.Generic.List<Null<string>> stuff2 = new global::System.Collections.Generic.List<Null<string>>();
            stuff2.Add(new Null());

            var stuff3 = new global::System.Collections.Generic.List<Nullable2<string>>();
            stuff3.Add("asdf");
            stuff3.Add(new Null<string>());
            ///stuff3.Add(new Null());

            var stuff4 = new global::System.Collections.Generic.List<NewEither<string, Exception>>();
            stuff4.Add(new Exception());
            stuff4.Add("sadf");

            var stuff5 = new global::System.Collections.Generic.List<IEither<string, Exception>>();
            ////stuff5.Add(new Exception());

            var stuff6 = new global::System.Collections.Generic.List<NewEither<string, NewEither<InvalidOperationException, ArgumentException>>>();
            ////stuff6.Add(new ArgumentException());
            stuff6.Add("dsaf");
            ////stuff6.Add(new InvalidOperationException());
            ////stuff6.Add(EitherHelper.Create(new ArgumentException()));
            NewEither<InvalidOperationException, ArgumentException> right = new ArgumentException();
            stuff6.Add(right);
            stuff6.Add(new InvalidOperationException().Left().Right<ArgumentException>());
        }

        public static bool TryGetValue<T>(this T? nullable, [MaybeNullWhen(false)] out T value)
        {
            // this overload doesn't make the distinction that T might already be nullable, while nullable2 handles that situation; the c# compiler does the either.selectmany automatically for the new nullable syntax
            if (nullable == null)
            {
                value = nullable;
                return false;
            }
            else
            {
                value = nullable;
                return true;
            }
        }

        public static bool TryGetValue<T>(this Nullable2<T> nullable, [MaybeNullWhen(false)] out T value)
        {
            string? something = null;

            TryGetValue<string>(something, out something);

            if (something.TryGetValue(out var anotherThing))
            {
                var @char = anotherThing[0];
            }

            return nullable.TryGetLeft(out value);
        }

        public sealed class Nullable2<T> : IEither<T, Null<T>>
        {
            public Nullable2()
            {
            }

            public TResult Apply<TResult, TContext>(Func<T, TContext, TResult> leftAccept, Func<Null<T>, TContext, TResult> rightAccept, TContext context)
            {
                throw new NotImplementedException();
            }

            public Task<TResult> Apply<TResult, TContext>(Func<T, TContext, Task<TResult>> leftMap, Func<Null<T>, TContext, Task<TResult>> rightMap, TContext context)
            {
                throw new NotImplementedException();
            }

            public static implicit operator Nullable2<T>(T value)
            {
                return new Nullable2<T>();
            }

            public static implicit operator Nullable2<T>(Null<T> @null)
            {
                return new Nullable2<T>();
            }
        }
        public static class EitherHelper
        {
            public static EitherHelper<TValue> Create<TValue>(TValue value)
            {
                return new EitherHelper<TValue>();
            }
        }

        public sealed class EitherHelper<TValue>
        {

        }

        public sealed class NewEither<TLeft, TRight> : IEither<TLeft, TRight>
        {
            private readonly Nested nested;

            private NewEither(Nested nested)
            {
                this.nested = nested;
            }

            public TResult Apply<TResult, TContext>(Func<TLeft, TContext, TResult> leftAccept, Func<TRight, TContext, TResult> rightAccept, TContext context)
            {
                return this.nested.Apply(leftAccept, rightAccept, context);
            }

            public Task<TResult> Apply<TResult, TContext>(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap, TContext context)
            {
                throw new NotImplementedException();
            }

            public static implicit operator NewEither<TLeft, TRight>(EitherHelper<TLeft> left)
            {
                return new NewEither<TLeft, TRight>(new Nested.Left(default!));
            }

            public static implicit operator NewEither<TLeft, TRight>(EitherHelper<TRight> right)
            {
                return new NewEither<TLeft, TRight>(new Nested.Right(default!));
            }

            public static implicit operator NewEither<TLeft, TRight>(TLeft value)
            {
                //// TODO TOPIC there's something to this; if you can't figure it out after discussion, go ahead and just add it, making a note that maybe it's useless, but if you don't have it implemented, you will never accidentally discover the utility
                return null!;
                ////return new NewEither<TLeft, TRight>(new Nested.Left(value));
            }

            public static implicit operator NewEither<TLeft, TRight>(TRight value)
            {
                return null!;
                ////return new NewEither<TLeft, TRight>(new Nested.Right(value));
            }

            private abstract class Nested : IEither<TLeft, TRight>
            {
                private Nested()
                {
                }

                public TResult Apply<TResult, TContext>(Func<TLeft, TContext, TResult> leftAccept, Func<TRight, TContext, TResult> rightAccept, TContext context)
                {
                    throw new NotImplementedException();
                }

                public Task<TResult> Apply<TResult, TContext>(Func<TLeft, TContext, Task<TResult>> leftMap, Func<TRight, TContext, Task<TResult>> rightMap, TContext context)
                {
                    throw new NotImplementedException();
                }

                public sealed class Left : Nested
                {
                    public Left(TLeft value)
                    {
                        Value = value;
                    }

                    public TLeft Value { get; }
                }

                public sealed class Right : Nested
                {
                    public Right(TRight value)
                    {
                        Value = value;
                    }

                    public TRight Value { get; }
                }
            }
        }
    }
}
