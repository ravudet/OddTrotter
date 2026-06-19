namespace Fx.Json
{
    using Fx.Parsing.Reader;

    public sealed class NumberReader<TNextReader> : IMoveReader<NumberReader<TNextReader>, SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>>
    {
        public static bool TryMove(Context context, out SignReader<IntReader<FracReader<ExpReader<TNextReader>>>>? nextReader)
        {
            throw new System.NotImplementedException();
        }
    }
}
