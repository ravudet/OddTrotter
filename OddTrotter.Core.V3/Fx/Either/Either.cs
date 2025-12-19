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
                return new Either<TLeft, TRight>.Left(left);
            }
        }

        public static Full<TLeft> Left<TLeft>(TLeft value)
        {
            return new Full<TLeft>(value);
        }
    }
}
