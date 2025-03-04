/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public sealed class TaskAwaiterWrapper<T> : ITaskAwaiter<T>
    {
        private readonly TaskAwaiter<T> taskAwaiter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskAwaiter"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="taskAwaiter"/> is <see langword="null"/>
        /// </exception>
        public TaskAwaiterWrapper(TaskAwaiter<T> taskAwaiter)
        {
            ArgumentNullException.ThrowIfNull(taskAwaiter);

            this.taskAwaiter = taskAwaiter;
        }

        /// <inheritdoc/>
        public bool IsCompleted
        {
            get
            {
                return this.taskAwaiter.IsCompleted;
            }
        }

        /// <inheritdoc/>
        public T GetResult()
        {
            return this.taskAwaiter.GetResult();
        }

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            this.taskAwaiter.OnCompleted(continuation);
        }

        /// <inheritdoc/>
        public void UnsafeOnCompleted(Action continuation)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            this.taskAwaiter.UnsafeOnCompleted(continuation);
        }
    }
}
