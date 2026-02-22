namespace OddTrotter.TodoListService
{
    using System;
    
    public sealed class TodoList
    {
        public TodoList(
            string value,
            DateTime startTimestamp,
            DateTime endTimestamp)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.Value = value;
            this.StartTimestamp = startTimestamp;
            this.EndTimestamp = endTimestamp;
        }

        public string Value { get; }

        public DateTime StartTimestamp { get; }

        public DateTime EndTimestamp { get; }
    }
}