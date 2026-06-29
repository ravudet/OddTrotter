namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class EscapedCharReader<TNextReader> : IMoveReader<EscapedCharReader<TNextReader>, EscapeCharacterReader<EscapableCharReader<TNextReader>>>
    {
        public static bool TryMove(Context context, out EscapeCharacterReader<EscapableCharReader<TNextReader>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
