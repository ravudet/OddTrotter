/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// There were several options to choose from in designing these factories:
    /// 
    /// ### Option 1: "left" is always first
    /// 
    /// With this option, the caller would always write `Either.Left` first. For example, they would write:
    /// 
    /// ```
    /// Either.Left(42).Right&lt;string>();
    /// Either.Left&lt;int>().Right("asdf");
    /// ```
    /// 
    /// but they cannot write:
    /// 
    /// ```
    /// Either.Right&lt;string>().Left(42);
    /// Either.Right("asdf").Left&lt;int>();
    /// ```
    /// 
    /// The issue with this approach is in the implementation for the factory methods:
    /// 
    /// ```
    /// public static class Either
    /// {
    ///     public readonly ref struct Empty<TLeft>
    ///     {
    ///         public Either<TLeft, TRight> Right<TRight>(TRight value)
    ///         {
    ///             return new Either<TLeft, TRight>.Right(value);
    ///         }
    ///     }
    /// 
    ///     public static Empty<TLeft> Left<TLeft>()
    ///     {
    ///         return new Empty<TLeft>();
    ///     }
    /// 
    ///     public readonly ref struct Full<TLeft>
    ///     {
    ///         private readonly TLeft left;
    /// 
    ///         private readonly bool initialized;
    ///
    ///         public Full(TLeft left)
    ///         {
    ///             this.left = left;
    /// 
    ///             this.initialized = true;
    ///         }
    /// 
    ///         public Full()
    ///         {
    ///             throw new System.Exception();
    ///         }
    /// 
    ///         public Either<TLeft, TRight> Right<TRight>()
    ///         {
    ///             if (!initialized)
    ///             {
    ///                 throw new System.Exception();
    ///             }
    /// 
    ///             return new Either<TLeft, TRight>.Left(this.left);
    ///         }
    ///     }
    /// 
    ///     public static Full<TLeft> Left<TLeft>(TLeft value)
    ///     {
    ///         return new Full<TLeft>(value);
    ///     }
    /// }
    /// ```
    /// 
    /// The `Full` type stores the `TLeft` temporarily so that type inference is allowed on it in the `static` factory method. But, being a `struct`, we have no way to ensure that a constructor is called; `default` can always be used for a caller to get an instance of it. As a result, if a caller creates their own instance of `Full`, we have to do some additional validation whenever `Right` is called. To perform this validation, we need the extra `initializaed` field. Ultimately, all of this means that the `Left<TLeft>(TLeft value)` method uses slightly more memory, and the `Full.Right` method needs to be documented to throw, and this always needs to be verified semantically by the caller for their own documentation. This adds both runtime and development overhead. 
    /// 
    /// ### Option 2: "empty" is always first
    /// 
    /// With this option, the caller always writes whichever "side" is empty first. For example, they would write:
    /// 
    /// ```
    /// Either.Left<int>().Right("asdf");
    /// Either.Right<string>().Left(42);
    /// ```
    /// 
    /// but they cannot write:
    /// 
    /// ```
    /// Either.Left(42).Right<string>();
    /// Either.Right("asdf").Left<int>();
    /// ```
    /// 
    /// 
    /// </remarks>
    public static class Either
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
