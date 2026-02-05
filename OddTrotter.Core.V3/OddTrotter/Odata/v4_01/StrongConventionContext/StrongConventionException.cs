namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;

    internal sealed class StrongConventionException : Exception
    {
        public StrongConventionException(string message)
            : base(message)
        {
        }

        public StrongConventionException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
