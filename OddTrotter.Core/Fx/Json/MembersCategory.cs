namespace Fx.Json
{
    using System;

    public readonly ref struct MembersCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static MembersCategory<TNextReader> None()
        {
            return new MembersCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static MembersCategory<TNextReader> Some()
        {
            return new MembersCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> noneReader,
            Func<MemberReader<MembersReader<TNextReader>>?, TResult> someReader)
        {
            switch (this.type)
            {
                case Type.None:
                    return noneReader(default);
                case Type.Some:
                    return someReader(default);
                default:
                    throw new Exception("TODO bug");
            }
        }
    }
}
