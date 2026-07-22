using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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

        public static async ValueTask<(TNextReader?, TValue)> Move<TCurrentReader, TNextReader, TValue>(
            this IValueReader<TCurrentReader, TNextReader, TValue>? valueReader,
            Context context)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            TNextReader? nextReader;
            TValue? value;
            while (!valueReader.TryMove(context, out nextReader, out value))
            {
                await context.Read().ConfigureAwait(false);
            }

            return (nextReader, value);
        }
    }
}
