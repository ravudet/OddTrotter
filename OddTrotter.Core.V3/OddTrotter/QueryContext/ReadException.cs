namespace Fx.QueryContext
{
    using System;

    internal sealed class EvaluationException<T> : Exception
    {
        public EvaluationException(T error)
        {
            Error = error;
        }

        public T Error { get; }
    }
}
