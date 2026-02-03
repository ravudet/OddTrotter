namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;

    internal sealed class ProtocolException : Exception
    {
        public ProtocolException(string message)
            : base(message)
        {
        }

        public ProtocolException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
