/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    public sealed class LeftMapException : Exception
    {
        public LeftMapException(Exception exception)
            : base(null, exception)
        {
        }
    }
}
