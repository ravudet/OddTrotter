namespace Fx.Either
{
    public static class RefEither
    {
        public readonly ref struct EmptyLeft<TLeft>
            where TLeft : allows ref struct
        {
            public RefEither<TLeft, TRight> Right<TRight>(TRight value)
                where TRight : allows ref struct
            {
                return new RefEither<TLeft, TRight>(value);
            }
        }

        public static EmptyLeft<TLeft> Left<TLeft>()
            where TLeft : allows ref struct
        {
            return new EmptyLeft<TLeft>();
        }

        public readonly ref struct EmptyRight<TRight>
            where TRight : allows ref struct
        {
            public RefEither<TLeft, TRight> Left<TLeft>(TLeft value)
                where TLeft : allows ref struct
            {
                return new RefEither<TLeft, TRight>(value);
            }
        }

        public static EmptyRight<TRight> Right<TRight>()
            where TRight : allows ref struct
        {
            return new EmptyRight<TRight>();
        }
    }
}
