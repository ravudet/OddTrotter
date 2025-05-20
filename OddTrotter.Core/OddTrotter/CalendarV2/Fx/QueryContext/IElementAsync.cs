/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IElementAsync<out TValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <remarks>
        /// This method should not throw. In the event of an error, a <see cref="IError{TError}"/> should be returned instead.
        /// </remarks>
        ITask<IQueryResultNodeAsync<TValue, TError>> NextAsync();
    }
}
