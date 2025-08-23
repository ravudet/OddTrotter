/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public sealed class ConfiguredTaskAwaiterWrapper<T> : ITaskAwaiter<T>
    {
        private readonly ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter configuredTaskAwaiter;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="configuredTaskAwaiter"></param>
        public ConfiguredTaskAwaiterWrapper(ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter configuredTaskAwaiter)
        {
            this.configuredTaskAwaiter = configuredTaskAwaiter;
        }

        /// <inheritdoc/>
        public bool IsCompleted
        {
            get
            {
                return this.configuredTaskAwaiter.IsCompleted;
            }
        }

        /// <inheritdoc/>
        public T GetResult()
        {
            return this.configuredTaskAwaiter.GetResult();
        }

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            this.configuredTaskAwaiter.OnCompleted(continuation);
        }

        /// <inheritdoc/>
        public void UnsafeOnCompleted(Action continuation)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            this.configuredTaskAwaiter.UnsafeOnCompleted(continuation);
        }
    }
}
