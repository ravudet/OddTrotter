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






        //// TODO is it ok to put `task` and `itask` extensions in the same class?
        




        public static ITask<TResult> ContinueWith2<TSource, TResult>(this ITask<TSource> task)
        {
            throw new Exception("TODO");
        }


    }

    public interface IFuture<out T> //// TODO `itask` should be called `iawaitable` and this should be called `itask` (though, this doesn't have a `continuewith`, so maybe this shouldn't be called `itask`; i'm nervous about calling it `ifuture` though, because that's a name with a real mathematical meaning, and i didn't do any diligence to ensure i followed that meaning)
        where T : allows ref struct
    {
    }
}
