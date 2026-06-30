using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public static class ValueReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TNextReader, TValue>(
            this IValueReader<TCurrentReader, TNextReader, TValue>? valueReader,
            Context context,
            out TNextReader? nextReader,
            [MaybeNullWhen(false)] out TValue value)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
            where TValue : allows ref struct
        {
            return TCurrentReader.TryMove(context, out nextReader, out value);
        }
    }
}
