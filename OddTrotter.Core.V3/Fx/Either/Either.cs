/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
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
    /// This avoids the both the runtime and development overhead of option 1. However, the "rule" for which factory the caller writes first is more abstract, which could lead to usability issues.
    /// 
    /// ### Option 3: "any" comes first
    /// 
    /// With this option, the caller can write whichever factory they want first. 
    /// 
    /// ```
    /// Either.Left<int>().Right("asdf");
    /// Either.Right<string>().Left(42);
    /// Either.Left(42).Right<string>();
    /// Either.Right("asdf").Left<int>();
    /// ```
    /// 
    /// The approach for the implementation is similar to option 1 and has the same issues presented there about runtime and development overhead. However, there is increased discoverability with option 3 and there is also increased usability. These are negated by there being no preference for ordering to use. This quickly devolves into a "curly braces on the same line or on a new line" stylistic argument. Because of the additional overhead and the fact that each individual developer will likely need to choose for themselves what convention they will follow, option 1 at least is certainly the better option.
    /// 
    /// ### Conclusion
    /// 
    /// We have decided on option 2. The compiler will guide callers who are confused by the more abstract "rule" such that those callers can only do the "right" thing anyway. This mitigates the only downside of option 2, while preserving the upsides of no additional runtime overhead and no additional development overhead in needing to check the semantics of the potentially thrown (in theory but almost never in practice) exception during a `Full.Right` call.
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
}
