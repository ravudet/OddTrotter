namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ArrayStartReader<TNextReader> : IValueReader<ArrayStartReader<TNextReader>, TNextReader, ArrayStartToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out ArrayStartToken value)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            return Helpers.TryReadChar(context, '[');
        }
    }
}
