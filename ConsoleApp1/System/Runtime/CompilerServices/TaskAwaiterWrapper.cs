/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices
{
    public sealed class TaskAwaiterWrapper<T> : ITaskAwaiter<T>
    {
        private readonly TaskAwaiter<T> taskAwaiter;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="taskAwaiter"></param>
        public TaskAwaiterWrapper(TaskAwaiter<T> taskAwaiter)
        {
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
            this.taskAwaiter.OnCompleted(continuation);
        }

        /// <inheritdoc/>
        public void UnsafeOnCompleted(Action continuation)
        {
            this.taskAwaiter.UnsafeOnCompleted(continuation);
        }
    }
}
