namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static TaskWrapper<T> ToTaskWrapper<T>(this Task<T> task)
        {
            return new TaskWrapper<T>(task);
        }

        public static TaskWrapper<Nothing> ToTaskWrapper(this Task task)
        {
            //// TODO is this really the best way to accomplish this?
            return task.ContinueWith(_ => new Nothing()).ToTaskWrapper();
        }
    }
}
