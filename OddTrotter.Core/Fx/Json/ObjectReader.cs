namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class ObjectReader<TNextReader> : IMoveReader<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(Context context, out ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>? nextReader)
        {
            nextReader = default;
            return true;
        }
    }
}
