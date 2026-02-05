namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System;

    internal sealed class WeakConventionException : Exception
    {
        public WeakConventionException(string message)
            : base(message)
        {
        }

        public WeakConventionException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
