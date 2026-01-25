namespace OddTrotter.Odata.v4_01.Reader
{
    using System;

    internal sealed class OdataException : Exception
    {
        public OdataException(string message)
            : base(message)
        {
        }

        public OdataException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
