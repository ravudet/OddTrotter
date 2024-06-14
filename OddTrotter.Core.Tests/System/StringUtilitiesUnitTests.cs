namespace System
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="StringUtilities"/>
    /// </summary>
    [TestClass]
    public sealed class StringUtilitiesUnitTests
    {
        /// <summary>
        /// Creates a <see cref="string"/> of maximum size and then creates a string that is too big
        /// </summary>
        [TestMethod]
        public void MaxStringSize()
        {
            var value = new string(' ', StringUtilities.MaxLength);
            Assert.ThrowsException<OutOfMemoryException>(() => new string(' ', StringUtilities.MaxLength + 1));
        }
    }
}
