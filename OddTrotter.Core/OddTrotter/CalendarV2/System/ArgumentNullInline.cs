////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace
#if OddTrotterCoreTestsInternal
    _.Foo //// TODO do  you need this extra namespace?
#else
    System
#endif
{
#if OddTrotterCoreTestsInternal
    using System; //// TODO you shouldn't need this if you are in the system namespace
#endif

    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Design decisions:
    /// 1. I considered making these methods extension methods. However, doing this pollutes intellisense. So the tradeoff is
    /// between the constant intellisense pollution versus the fluent API calls for inline checks. Comparing fluent API to the
    /// non-fluent API, we have something that looks like:
    /// ```
    /// ArgumentNullInline.ThrowIfNull(ArgumentNullInline.ThrowIfNull(settings).Nested).Value
    /// ```
    /// compared to
    /// ```
    /// settings.ThrowIfNull().Nested.ThrowIfNull().Value;
    /// ```
    /// To me, the fluent API definitely wins out, *but* precondition checks only happen at the beginning of the method (which is
    /// a small portion of the entire method), and inline checks only happen for cases where expressions are required due to
    /// certain restrictions while calling overloads (such as constructors) (and these cases are extremely rare themselves), and
    /// the fluent API only really helps for cases where nested checks are needed (which is absurdly rare because, using the
    /// above example, why hasn't `settings` already assert itself that `Nested` is not `null`?), so as a result I have chosen to
    /// not introduce the fluent API because the use cases seem to be so rare that the benefit would be far exceeded by the
    /// detriment of almost every intellisense being polluted by a method that is not applicable.
    /// 
    /// 2. I considered following the precedent of .NET and using `object?` as the input parameter for these methods. However, I
    /// didn't want to do this because it allows for `struct`s to be passed in. This is the result of implicit boxing which can
    /// have performance impacts. Further, we *know* that those structs will *never* be `null`, so this becomes a very expensive
    /// no-op. Of all of the decision, this is the one that is keeping this class marked `internal`.
    /// 
    /// 3. I considered leveraging the existing .NET methods to perform the actual `null` checks and did some pseudo
    /// micro-benchmarks which indicated that, for the `struct`, boxing *is* introducing a performance impact. Similar benchmarks
    /// also demonstrated that either there's *no* difference for the `class` case, or that the custom implementation wins out
    /// slightly.
    /// </remarks>
#if OddTrotterCoreTestsInternal
    public
#else
    internal
#endif
        static class

#if OddTrotterCoreTestsInternal
        ArgumentNullInline2 //// TODO hopefully you can re-use the right type name
#else
        ArgumentNullInline
#endif
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argument"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="argument"/> is <see langword="null"/></exception>
        public static T ThrowIfNull<T>(
            [NotNull] T? argument, 
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            where T : class
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            return argument;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argument"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="argument"/> is <see langword="null"/></exception>
        public static T ThrowIfNull<T>(
            [NotNull] T? argument, 
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            where T : struct
        {
            if (!argument.HasValue)
            {
                throw new ArgumentNullException(paramName);
            }

            return argument.Value;
        }
    }
}
