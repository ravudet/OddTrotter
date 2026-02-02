namespace OddTrotter.Odata.v4_01.Reader
{
    using System;

    /// <summary>
    /// indicates that an payload that is being read is not a valid odata payload
    /// 
    /// TODO i don't think there is an analogous "write" exception, but maybe there is
    /// </summary>
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
