/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Try
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// placeholder
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="input"></param>
    /// <param name="tried"></param>
    /// <returns></returns>
    /// <remarks>
    /// Despite the increased functionality of <see cref="TryCovariant{TInput, TOutput}"/> over
    /// <see cref="Try{TInput, TOutput}"/>, we will only use <see cref="TryCovariant{TInput, TOutput}"/> where covariance is
    /// significant (meaning that we will not implement covenience overloads that allow
    /// <see cref="TryCovariant{TInput, TOutput}"/>). The "try" pattern is a c# idiom and is convenient within the language
    /// syntax. Using <see cref="TryCovariant{TInput, TOutput}"/>, we will need to adapt it to the "try" pattern anyway, and
    /// existing .NET APIs all use the current idiom, which matches the <see cref="Try{TInput, TOutput}"/> delegate. If the
    /// covariance becomes a more popular pattern, we may add convenience overloads at that time.
    /// </remarks>
    [return: MaybeNull]
    public delegate TOutput TryCovariant<in TInput, out TOutput>(TInput input, out bool tried);
}
