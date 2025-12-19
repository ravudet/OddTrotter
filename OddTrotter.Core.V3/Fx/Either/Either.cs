/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    public static class Either
    {
        public readonly ref struct Empty<TLeft>
        {
            public Either<TLeft, TRight> Right<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static Empty<TLeft> Left<TLeft>()
        {
            return new Empty<TLeft>();
        }

        public readonly ref struct Full<TLeft>
        {
            private readonly TLeft left;

            public Full(TLeft left)
            {
                this.left = left;
            }

            public Either<TLeft, TRight> Right<TRight>()
            {
                return new Either<TLeft, TRight>.Left(this.left);
            }
        }

        public static Full<TLeft> Left<TLeft>(TLeft value)
        {
            return new Full<TLeft>(value);
        }
    }

    public static class Either2
    {
        public readonly ref struct EmptyLeft<TLeft>
        {
            public Either<TLeft, TRight> Right<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static EmptyLeft<TLeft> Left<TLeft>()
        {
            return new EmptyLeft<TLeft>();
        }

        public readonly ref struct EmptyRight<TRight>
        {
            public Either<TLeft, TRight> Left<TLeft>(TLeft value)
            {
                return new Either<TLeft, TRight>.Left(value);
            }
        }

        public static EmptyRight<TRight> Right<TRight>()
        {
            return new EmptyRight<TRight>();
        }
    }

    public static class Either3
    {
        public readonly ref struct EmptyLeft<TLeft>
        {
            public Either<TLeft, TRight> Right<TRight>(TRight value)
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static EmptyLeft<TLeft> Left<TLeft>()
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

            public Either<TLeft, TRight> Right<TRight>()
            {
                return new Either<TLeft, TRight>.Left(this.left);
            }
        }

        public static FullLeft<TLeft> Left<TLeft>(TLeft value)
        {
            return new FullLeft<TLeft>(value);
        }

        public readonly ref struct EmptyRight<TRight>
        {
            public Either<TLeft, TRight> Left<TLeft>(TLeft value)
            {
                return new Either<TLeft, TRight>.Left(value);
            }
        }

        public static EmptyRight<TRight> Right<TRight>()
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

            public Either<TLeft, TRight> Left<TLeft>()
            {
                return new Either<TLeft, TRight>.Right(value);
            }
        }

        public static FullRight<TRight> Right<TRight>(TRight value)
        {
            return new FullRight<TRight>(value);
        }
    }
}
