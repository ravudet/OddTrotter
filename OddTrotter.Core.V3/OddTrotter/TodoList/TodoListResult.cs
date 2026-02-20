namespace OddTrotter.TodoList
{
    using System;
    
    public sealed class TodoListResult
    {
        public TodoListResult(
            string todoList,
            DateTime startTimestamp,
            DateTime endTimestamp)
        {
            if (todoList == null)
            {
                throw new ArgumentNullException(nameof(todoList));
            }

            this.TodoList = todoList;
            this.StartTimestamp = startTimestamp;
            this.EndTimestamp = endTimestamp;
        }

        public string TodoList { get; }

        public DateTime StartTimestamp { get; }

        public DateTime EndTimestamp { get; }
    }
}