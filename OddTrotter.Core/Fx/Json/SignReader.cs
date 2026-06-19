namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class SignReader<TNextReader> : IValueReader<SignReader<TNextReader>, TNextReader, SignToken>
    {
        public static bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out SignToken value)
        {
            throw new System.NotImplementedException();
        }
    }

    public readonly ref struct SignToken
    {
        private enum Type
        {
            Absent = 1,
            Present,
        }

        private Type type { get; init; }

        public static SignToken Absent()
        {
            return new SignToken()
            {
                type = Type.Absent,
            };
        }

        public static SignToken Present()
        {
            return new SignToken()
            {
                type = Type.Present,
            };
        }
    }
}
