namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static TaskWrapper<T> ToTaskWrapper<T>(this Task<T> task)
        {
            return new TaskWrapper<T>(task);
        }
    }
}
