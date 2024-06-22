namespace OddTrotter.TodoList
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="TodoListResult"/>
    /// </summary>
    [TestClass]
    public sealed class TodoListResultUnitTests
    {
        /// <summary>
        /// Creates a <see cref="TodoListResult"/> with a <see langword="null"/> todo list
        /// </summary>
        [TestMethod]
        public void NullTodoList()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListResult(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>(),
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListResult"/> with a <see langword="null"/> sequence of events without starts
        /// </summary>
        [TestMethod]
        public void NullWithoutStarts()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListResult(
                "the list",
                null,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Enumerable.Empty<(CalendarEvent, Exception)>(),
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListResult"/> with a <see langword="null"/> sequence of events that failed to parse starts
        /// </summary>
        [TestMethod]
        public void NullStartParsing()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListResult(
                "the list",
                null,
                Enumerable.Empty<CalendarEvent>(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListResult"/> with a <see langword="null"/> sequence of events without bodies
        /// </summary>
        [TestMethod]
        public void NullWithoutBodies()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListResult(
                "the list",
                null,
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Enumerable.Empty<(CalendarEvent, Exception)>()));
        }

        /// <summary>
        /// Creates a <see cref="TodoListResult"/> with a <see langword="null"/> sequence of events that failed to parse bodies
        /// </summary>
        [TestMethod]
        public void NullBodyParsing()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TodoListResult(
                "the list",
                null,
                Enumerable.Empty<CalendarEvent>(),
                Enumerable.Empty<(CalendarEvent, Exception)>(),
                Enumerable.Empty<CalendarEvent>(),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ));
        }
    }
}
