/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    public sealed class Error<TError> : IError<TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        public Error(TError value)
        {
            this.Value = value;
        }

        /// <inheritdoc/>
        public TError Value { get; }
    }
}
