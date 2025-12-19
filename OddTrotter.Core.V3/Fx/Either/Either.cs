/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    //// TODO which of these options do you want to use?

    public static class Play
    {
        public static void DoWork()
        {
            // always start with left
            var either = Either1.Success<int>().Failure("asdf");
            either = Either1.Success(42).Failure<string>();




            // always start with the empty side
            Either2.Success(42).Failure<string>();
            Either2.Success<int>().Failure("asdf");
            Either2.Failure<string>().Success(42);





            
            // start however you want
            Either3.Success(42).Failure<string>();
            Either3.Failure<string>().Success(42);
            Either3.Success<int>().Failure("adsf");
            Either3.Failure("asdf").Success<int>();

            Either3.Success<int>().Failure<Exception>();
            Either3.Success(42).Failure<string>("asdf");



            Either5.Builder().



            var thing = "asdf";
            thing.MakeEither<string, Exception>();
            thing.MakeEither2<string, Exception>();
        }
    }

    public static class Either5
    {
        public readonly ref struct Builder2
        {
        }

        public static Builder2 Builder()
        {
        }
    }

    public static class Either4
    {
        public static Either<TLeft, TRight> MakeEither<TLeft, TRight>(this object obj)
        {
        }

        public static Either<TLeft, TRight> MakeEither2<TLeft, TRight>(this TLeft obj)
        {
        }
    }

    public static class Either1
    {
        public readonly ref struct Empty<TLeft>
        {
            public Either<TLeft, TRight> Failure<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static Empty<TLeft> Success<TLeft>()
        {
            return new Empty<TLeft>();
        }

        public readonly ref struct Full<TLeft>
        {
            private readonly TLeft left;

            private readonly bool initialized;

            public Full(TLeft left)
            {
                this.left = left;

                this.initialized = true;
            }

            public Full()
            {
                throw new System.Exception("TODO");
            }

            public Either<TLeft, TRight> Failure<TRight>()
            {
                if (!initialized)
                {
                    throw new System.Exception("TODO");
                }

                return new Either<TLeft, TRight>.Left(this.left);
            }
        }

        public static Full<TLeft> Success<TLeft>(TLeft value)
        {
            return new Full<TLeft>(value);
        }
    }

    public static class Either2
    {
        //// TODO you could also go the other direction and always do "full" first

        public readonly ref struct EmptyLeft<TLeft>
        {
            public Either<TLeft, TRight> Failure<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static EmptyLeft<TLeft> Success<TLeft>()
        {
            return new EmptyLeft<TLeft>();
        }

        public readonly ref struct EmptyRight<TRight>
        {
            public Either<TLeft, TRight> Success<TLeft>(TLeft value)
            {
                return new Either<TLeft, TRight>.Left(value);
            }
        }

        public static EmptyRight<TRight> Failure<TRight>()
        {
            return new EmptyRight<TRight>();
        }
    }

    public static class Either3
    {
        public readonly ref struct EmptyLeft<TLeft>
        {
            public Either<TLeft, TRight> Failure<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static EmptyLeft<TLeft> Success<TLeft>()
        {
            return new EmptyLeft<TLeft>();
        }

        public readonly ref struct FullLeft<TLeft>
        {
            private readonly TLeft left;

            public FullLeft(TLeft left)
            {
                this.left = left;
            }

            public Either<TLeft, TRight> Failure<TRight>()
            {
                return new Either<TLeft, TRight>.Left(this.left);
            }
        }

        public static FullLeft<TLeft> Success<TLeft>(TLeft value)
        {
            return new FullLeft<TLeft>(value);
        }

        public readonly ref struct EmptyRight<TRight>
        {
            public Either<TLeft, TRight> Success<TLeft>(TLeft value)
            {
                return new Either<TLeft, TRight>.Left(value);
            }
        }

        public static EmptyRight<TRight> Failure<TRight>()
        {
            return new EmptyRight<TRight>();
        }

        public readonly ref struct FullRight<TRight>
        {
            private readonly TRight value;

            public FullRight(TRight value)
            {
                this.value = value;
            }

            public Either<TLeft, TRight> Success<TLeft>()
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static FullRight<TRight> Failure<TRight>(TRight value)
        {
            return new FullRight<TRight>(value);
        }
    }
}
