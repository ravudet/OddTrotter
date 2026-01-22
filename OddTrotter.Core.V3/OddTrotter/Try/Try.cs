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
    /// <param name="output"></param>
    /// <returns></returns>
    /// <remarks>
    /// The idea behind this pattern is to remove <see cref="System.Exception"/>s from the control flow logic, so instances of
    /// this <see langword="delegate"> "shouldn't" throw; however, some APIs still use <see cref="System.Exception"/>s for things
    /// like <see langword="null"/> checks on <paramref name="input"/>. Be care what you pass as a
    /// <see cref="Try{TInput, TOutput}"/>.
    /// </remarks>
    public delegate bool Try<in TInput, TOutput>(TInput input, [MaybeNullWhen(false)] out TOutput output);
}
