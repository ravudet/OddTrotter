/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;

    public sealed class LeftGenerationException : Exception //// TODO naming
    {
        public LeftGenerationException(Exception exception)
            : base(null, exception)
        {
        }
    }
}
