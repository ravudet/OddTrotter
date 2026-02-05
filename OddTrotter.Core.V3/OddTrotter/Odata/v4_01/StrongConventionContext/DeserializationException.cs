namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;

    internal sealed class DeserializationException : Exception
    {
        public DeserializationException(string message)
            : base(message)
        {
        }

        public DeserializationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
