namespace Fx.Either
{
    public static class RefEither
    {
        public readonly ref struct EmptyLeft<TLeft>
        {
            public RefEither<TLeft, TRight> Right<TRight>(TRight value)
            {
                return new RefEither<TLeft, TRight>(value);
            }
        }

        public static EmptyLeft<TLeft> Left<TLeft>()
        {
            return new EmptyLeft<TLeft>();
        }

        public readonly ref struct EmptyRight<TRight>
        {
            public RefEither<TLeft, TRight> Left<TLeft>(TLeft value)
            {
                return new RefEither<TLeft, TRight>(value);
            }
        }

        public static EmptyRight<TRight> Right<TRight>()
        {
            return new EmptyRight<TRight>();
        }
    }
}
