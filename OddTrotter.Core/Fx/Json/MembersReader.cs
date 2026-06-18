using System;

namespace Fx.Json
{
    public sealed class MembersReader<TNextReader>
    {
    }

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
            Func<FirstMemberReader<TNextReader>?, TResult> someReader)
        {
            switch (this.type)
            {
                case Type.None:
                    return noneReader(default);
            }
        }
    }


    public sealed class FirstMemberReader<TNextReader>
    {
    }

}
