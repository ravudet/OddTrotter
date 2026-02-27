namespace Fx.QueryContext
{
    using System;

    internal sealed class EvaluationException<T> : Exception
    {
        public EvaluationException(string message)
            : base(message)
        {
        }

        public EvaluationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
