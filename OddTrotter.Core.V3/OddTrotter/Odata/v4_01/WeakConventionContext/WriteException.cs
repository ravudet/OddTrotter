namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System;

    internal sealed class WriteException : Exception
    {
        public WriteException(string message)
            : base(message)
        {
        }

        public WriteException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
