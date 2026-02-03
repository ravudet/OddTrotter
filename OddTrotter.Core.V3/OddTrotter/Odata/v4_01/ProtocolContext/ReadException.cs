namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;

    internal sealed class ReadException : Exception
    {
        public ReadException(string message)
            : base(message)
        {
        }

        public ReadException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
