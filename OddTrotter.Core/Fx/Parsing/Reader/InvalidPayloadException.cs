namespace Fx.Parsing.Reader
{
    using System;

    public sealed class InvalidPayloadException : Exception
    {
        public InvalidPayloadException()
            : base()
        {
        }

        public InvalidPayloadException(string message)
            : base(message)
        {
        }

        public InvalidPayloadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
