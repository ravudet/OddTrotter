/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    public sealed class RightGenerationException : Exception //// TODO naming
    {
        public RightGenerationException(Exception exception)
            : base(null, exception)
        {
        }
    }
}
