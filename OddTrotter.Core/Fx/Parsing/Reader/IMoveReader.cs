namespace Fx.Parsing.Reader
{
    public interface IMoveReader<out TSelf, TNextReader>
        where TSelf : IMoveReader<TSelf, TNextReader>
    {
        /// <exception cref="InvalidPayloadException">Thrown if the payload in <paramref name="context"/> is not the format that the reader implementation understands</exception>
        static abstract bool TryMove(Context context, out TNextReader? nextReader);
    }
}
