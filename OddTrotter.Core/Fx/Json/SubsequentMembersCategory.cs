namespace Fx.Json
{
    using System;

    public readonly ref struct SubsequentMembersCategory<TNextReader>
    {
        private enum Type
        {
            None = 1,
            Some,
        }

        private Type type { get; init; }

        public static SubsequentMembersCategory<TNextReader> None()
        {
            return new SubsequentMembersCategory<TNextReader>()
            {
                type = Type.None,
            };
        }

        public static SubsequentMembersCategory<TNextReader> Some()
        {
            return new SubsequentMembersCategory<TNextReader>()
            {
                type = Type.Some,
            };
        }

        public TResult Apply<TResult>(
            Func<TNextReader?, TResult> noneReader,
            Func<MemberReader<SubsequentMembersReader<TNextReader>>?, TResult> someReader)
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
