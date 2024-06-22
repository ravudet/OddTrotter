namespace OddTrotter.TodoList
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    /// <summary>
    /// Unit tests for <see cref="TodoListServiceSettings"/>
    /// </summary>
    [TestClass]
    public sealed class TodoListServiceSettingsUnitTests
    {
        /// <summary>
        /// Creates default <see cref="TodoListServiceSettings"/>
        /// </summary>
        [TestMethod]
        public void DefaultSettings()
        {
            var builder = new TodoListServiceSettings.Builder();
            var settings = builder.Build();
            Assert.AreEqual(builder.CalendarEventPageSize, settings.CalendarEventPageSize);
            Assert.AreEqual(builder.TodoListDataBlobName, settings.TodoListDataBlobName);
        }

        /// <summary>
        /// Creates <see cref="TodoListServiceSettings"/> with a <see langword="null"/> todo list data blob name
        /// </summary>
        [TestMethod]
        public void NullTodoListDataBlobName()
        {
            var builder = new TodoListServiceSettings.Builder()
            {
                TodoListDataBlobName =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            Assert.ThrowsException<ArgumentNullException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="TodoListServiceSettings"/> with an empty todo list data blob name
        /// </summary>
        [TestMethod]
        public void EmptyTodoListDataBlobName()
        {
            var builder = new TodoListServiceSettings.Builder()
            {
                TodoListDataBlobName = string.Empty,
            };

            Assert.ThrowsException<ArgumentException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="TodoListServiceSettings"/> with a zero page size
        /// </summary>
        [TestMethod]
        public void ZeroPageSize()
        {
            var builder = new TodoListServiceSettings.Builder()
            {
                CalendarEventPageSize = 0,
            };

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => builder.Build());
        }

        /// <summary>
        /// Creates <see cref="TodoListServiceSettings"/> with a negative page size
        /// </summary>
        [TestMethod]
        public void NegativePageSize()
        {
            var builder = new TodoListServiceSettings.Builder()
            {
                CalendarEventPageSize = -5,
            };

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => builder.Build());
        }
    }
}
