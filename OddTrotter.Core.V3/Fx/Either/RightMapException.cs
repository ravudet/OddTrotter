/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    public sealed class RightMapException : Exception
    {
        public RightMapException(Exception exception)
            : base(null, exception)
        {
        }
    }
}
