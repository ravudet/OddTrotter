namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ExpSignReader<TNextReader> : IValueReader<ExpSignReader<TNextReader>, TNextReader, ExpSignToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out ExpSignToken value)
        {
            if (context.ValidBytes == 0)
            {
                nextReader = default;
                value = ExpSignToken.Absent();
                return true;
            }

            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            var currentByte = context.Buffer[context.CurrentByteIndex];
            if (currentByte == '+')
            {
                value = ExpSignToken.Positive();
            }
            else if (currentByte == '-')
            {
                value = ExpSignToken.Negative();
            }
            else
            {
                value = ExpSignToken.Absent();
            }

            return true;
        }
    }
}
