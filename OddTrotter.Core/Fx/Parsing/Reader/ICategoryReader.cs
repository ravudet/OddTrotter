namespace Fx.Parsing.Reader
{
    using System.Diagnostics.CodeAnalysis;

    public interface ICategoryReader<out TSelf, TCategory>
        where TSelf : ICategoryReader<TSelf, TCategory>
        where TCategory : allows ref struct
    {
        /// <exception cref="InvalidPayloadException">Thrown if the payload in <paramref name="context"/> is not the format that the reader implementation understands</exception>
        static abstract bool TryMove(Context context, [MaybeNullWhen(false)] out TCategory category);
    }
}
