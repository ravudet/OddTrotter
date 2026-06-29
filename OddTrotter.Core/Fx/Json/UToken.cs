namespace Fx.Json
{
    public readonly ref struct UToken
    {
        public static bool TryCreate(byte u, out UToken uToken)
        {
            uToken = default;
            return u == 0x75;
        }
    }
}
