namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class SignReader<TNextReader> : IValueReader<SignReader<TNextReader>, TNextReader, SignToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out SignToken value)
        {
            if (Helpers.NeedsMoreBytes(context, out nextReader, out value))
            {
                return false;
            }

            if (context.ValidBytes == 0 || context.Buffer[context.CurrentByteIndex] != '-')
            {
                value = SignToken.Absent();
            }
            else
            {
                value = SignToken.Negative();
                ++context.CurrentByteIndex;
            }

            return true;
        }
    }
}
