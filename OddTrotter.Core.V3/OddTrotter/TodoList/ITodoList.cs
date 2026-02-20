namespace OddTrotter.TodoList
{
    using System.Threading.Tasks;

    internal interface ITodoList<T>
    {
        Task<(TodoListResult, T)> Retrieve(); //// TODO probably strongly type the return value
    }
}
