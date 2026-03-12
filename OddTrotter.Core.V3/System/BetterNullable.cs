/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Diagnostics.CodeAnalysis;

    public readonly struct BetterNullable<T> //// TODO i like the "optional" name from previous iterations
    {
        private readonly bool hasValue;
        private readonly T value;

        public BetterNullable(T value)
        {
            this.value = value;

            hasValue = true;
        }

        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            value = this.value;
            return hasValue;
        }
    }
}
