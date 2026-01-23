/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    public readonly struct Optional<T>
    {
        private readonly T value;

        private readonly bool hasValue;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        public Optional(T value)
        {
            this.value = value;

            this.hasValue = true;
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(out T value)
        {
            value = this.value;
            return this.hasValue;
        }
    }
}
