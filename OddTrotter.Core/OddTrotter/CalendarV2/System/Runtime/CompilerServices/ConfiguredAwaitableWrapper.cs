/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public sealed class ConfiguredAwaitableWrapper<T> : IConfiguredAwaitable<T>
    {
        private readonly ConfiguredTaskAwaitable<T> configuredTaskAwaitable;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="configuredTaskAwaitable"></param>
        public ConfiguredAwaitableWrapper(ConfiguredTaskAwaitable<T> configuredTaskAwaitable)
        {
            this.configuredTaskAwaitable = configuredTaskAwaitable;
        }

        /// <inheritdoc/>
        public ITaskAwaiter<T> GetAwaiter()
        {
            return new ConfiguredTaskAwaiterWrapper<T>(this.configuredTaskAwaitable.GetAwaiter());
        }
    }
}
