namespace Fx.Json
{
    using System;

    using Fx.Parsing.Reader;

    public sealed class Reader : IMoveReader<Reader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        public static bool TryMove(Context context, out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>? nextReader)
        {
            nextReader = null;
            return true;
        }
    }

    public sealed class WhitespaceReader<TNextReader>
    {
    }

    public sealed class ValueReader<TNextReader>
    {
    }
}
