namespace OddTrotter.TodoListService
{
    using System.Threading.Tasks;

    internal interface ITodoListService<T>
    {
        Task<TodoListResult<T>> Retrieve();
    }

    internal sealed class TodoListResult<T>
    {
        internal TodoListResult(TodoList todoList, T errors)
        {
            TodoList = todoList;
            Errors = errors;
        }

        public TodoList TodoList { get; }
        public T Errors { get; }
    }
}
